using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GH_IO;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using SizeAnalyzer.Schedulers;

namespace SizeAnalyzer
{
  public class Calculator
  {
    private const int MaxDegreeOfParallelism = 4;

    private CancellationTokenSource _cancelSource;
    private TaskFactory<double> _factory;
    private Dictionary<Guid, Task<double>> _resultsCache;
    private SerializationType _serializationType;

    public EventHandler? ComputeTaskFinished;
    public EventHandler? ParamTaskFinished;

    public Calculator(SerializationType type)
    {
      _serializationType = type;
      _resultsCache = new Dictionary<Guid, Task<double>>();
      var scheduler = new LimitedConcurrencyLevelTaskScheduler(MaxDegreeOfParallelism);
      _cancelSource = new CancellationTokenSource();
      _factory = new TaskFactory<double>(
        _cancelSource.Token,
        TaskCreationOptions.None,
        TaskContinuationOptions.None,
        scheduler);
    }

    public SerializationType SerializationType
    {
      get => _serializationType;
      set
      {
        _serializationType = value;
        Reset();
      }
    }

    private Task AddParameter(IGH_Param param)
    {
      if (param == null)
        throw new ArgumentNullException(nameof(param));
      var task = _factory.StartNew(() => GetParamDataSize(param, _serializationType));
      var continuation = task.ContinueWith((t) => ParamTaskFinished?.Invoke(param, null), TaskScheduler.Default);
      if (_resultsCache.ContainsKey(param.InstanceGuid))
        _resultsCache[param.InstanceGuid] = task;
      else
        _resultsCache.Add(param.InstanceGuid, task);
      return continuation;
    }

    private void RemoveParameter(IGH_Param param)
    {
      if (param == null) throw new ArgumentNullException(nameof(param));
      if (_resultsCache.ContainsKey(param.InstanceGuid))
        _resultsCache.Remove(param.InstanceGuid);
    }

    public void Add(IGH_DocumentObject docObject) => ObjectAction(docObject, p => AddParameter(p), null);

    public void Remove(IGH_DocumentObject docObject) => ObjectAction(docObject, RemoveParameter, null);

    public Task Compute(GH_Document doc)
    {
      if (doc == null) throw new ArgumentNullException(nameof(doc));
      // Reset the cache
      _resultsCache = new Dictionary<Guid, Task<double>>();

      var tasks = GetAllDocumentObjectsWithLocalData(doc)
        .SelectMany(obj =>
        {
          _cancelSource.Token.ThrowIfCancellationRequested();

          switch (obj)
          {
            case IGH_Param param:
              return new List<Task> { AddParameter(param) };
            case IGH_Component component:
              return GetComponentParamsWithLocalData(component)
                .Select(AddParameter);
            default:
              return new List<Task>();
          }
        });
      return Task.WhenAll(tasks)
        .ContinueWith(res => ComputeTaskFinished?.Invoke(this, null), TaskScheduler.Default);
    }

    public void Cancel()
    {
      _cancelSource.Cancel();
    }

    public void Reset()
    {
      Cancel();
      _resultsCache = new Dictionary<Guid, Task<double>>();
      var scheduler = new LimitedConcurrencyLevelTaskScheduler(MaxDegreeOfParallelism);
      _cancelSource = new CancellationTokenSource();
      _factory = new TaskFactory<double>(
        _cancelSource.Token,
        TaskCreationOptions.None,
        TaskContinuationOptions.None,
        scheduler);
    }

    public ParamStatus GetParamStatus(IGH_Param p) => GetTaskStatus(Get(p));

    public IEnumerable<IGH_DocumentObject> GetParams(double threshold)
    {
      return _resultsCache
        .Where(pair => GetTaskStatus(pair.Value) == ParamStatus.OverThreshold)
        .Select(kv => Instances.ActiveCanvas.Document.FindObject(kv.Key, false));
    }

    public double GetTotal()
    {
      return _resultsCache.Where(pair => pair.Value.IsCompleted).Sum(pair => pair.Value.Result);
    }

    public Task<double>? Get(IGH_Param param)
    {
      return !_resultsCache.ContainsKey(param.InstanceGuid) ? null : _resultsCache[param.InstanceGuid];
    }

    private static ParamStatus GetTaskStatus(Task<double>? task)
    {
      if (task == null) return ParamStatus.Untracked;
      if (task.IsCanceled || task.IsFaulted) return ParamStatus.Error;
      if (task.IsCompleted)
        return task.Result >= Settings.ParamThreshold ? ParamStatus.OverThreshold : ParamStatus.UnderThreshold;
      return ParamStatus.Loading;
    }

    private static IEnumerable<IGH_Param> GetComponentParamsWithLocalData(IGH_Component component)
    {
      return component.Params.Input.Where(cParam => cParam.DataType == GH_ParamData.local);
    }

    private static IEnumerable<IGH_DocumentObject> GetAllDocumentObjectsWithLocalData(GH_Document doc)
    {
      foreach (var obj in doc.Objects)
        switch (obj)
        {
          case IGH_Param param:
            if (param.DataType == GH_ParamData.local)
              yield return param;
            break;
          case IGH_Component component:
          {
            if (component.Params.Input.Any(cParam => cParam.DataType == GH_ParamData.local))
              yield return component;
            break;
          }
        }
    }

    private static double GetParamDataSize(IGH_Param param,
      SerializationType serializationType = SerializationType.Binary)
    {
      var archive = new GH_Archive();
      archive.CreateTopLevelNode("param size archive");
      archive.AppendObject(param, param.InstanceGuid.ToString());

      double size;
      switch (serializationType)
      {
        case SerializationType.Xml:
          var xml = archive.Serialize_Xml();
          var byteSize = (double)Encoding.Unicode.GetByteCount(xml);
          size = byteSize / 1048576;
          break;

        case SerializationType.Binary:
          var byteArray = archive.Serialize_Binary();
          size = (double)byteArray.Length / 1048576;
          break;

        default:
          throw new ArgumentOutOfRangeException(nameof(serializationType), serializationType,
            "Incorrect Serialization Type was passed.");
      }

      return size;
    }

    public static void ObjectAction(IGH_DocumentObject docObject, Action<IGH_Param>? parameterAction,
      Action<IGH_Component>? componentAction)
    {
      if (docObject == null) throw new ArgumentNullException(nameof(docObject));
      switch (docObject)
      {
        case IGH_Param param:
          parameterAction?.Invoke(param);
          break;
        case IGH_Component component:
          componentAction?.Invoke(component);
          if (parameterAction != null)
            component.Params.Input.ForEach(parameterAction);
          break;
      }
    }
  }

  public enum SerializationType
  {
    Binary,
    Xml
  }
}