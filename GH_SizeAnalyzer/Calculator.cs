using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino;
using SizeAnalyzer.Schedulers;

namespace SizeAnalyzer
{
  public class Calculator
  {
    private Dictionary<Guid, Task<double>> _resultsCache = new Dictionary<Guid, Task<double>>();
    private TaskFactory<double> factory;
    public CancellationTokenSource CancelTokenSource = new CancellationTokenSource();

    public SerializationType SerializationType = SerializationType.Xml;

    public Calculator()
    {
      var scheduler = new LimitedConcurrencyLevelTaskScheduler(4);
      factory = new TaskFactory<double>(scheduler);
    }
    private void AddParameter(IGH_Param param)
    {
      if (param == null)
        return;
      var task = factory.StartNew(() => GetParamDataSize(param, SerializationType));
      if (_resultsCache.ContainsKey(param.InstanceGuid))
        _resultsCache[param.InstanceGuid] = task;
      else
        _resultsCache.Add(param.InstanceGuid, task);
    }

    private bool RemoveParameter(IGH_Param param)
    {
      return param != null
             && _resultsCache.ContainsKey(param.InstanceGuid)
             && _resultsCache.Remove(param.InstanceGuid);
    }

    public void Add(IGH_DocumentObject ghDocumentObject)
    {
      switch (ghDocumentObject)
      {
        case IGH_Param param:
          AddParamEvents(param);
          AddParameter(param);
          break;
        case IGH_Component component:
          // A component's attributes change when a param is added/deleted from the component
          AddComponentEvents(component);
          component.Params.Input.ForEach(p =>
          {
            AddParamEvents(p);
            AddParameter(p);
          });
          break;
      }
    }

    public void Remove(IGH_DocumentObject obj)
    {
      switch (obj)
      {
        case IGH_Param param:
          RemoveParamEvents(param);
          RemoveParameter(param);
          break;
        case IGH_Component component:
          // A component's attributes change when a param is added/deleted from the component
          RemoveComponentEvents(component);
          component.Params.Input.ForEach(p =>
          {
            RemoveParamEvents(p);
            RemoveParameter(p);
          });
          break;
      }
    }
    
    public void Compute(GH_Document doc)
    {
      CancelTokenSource.Cancel();
      CancelTokenSource = new CancellationTokenSource();
      _resultsCache = new Dictionary<Guid, Task<double>>();

      GetAllDocumentObjectsWithLocalData(doc)
        .ToList()
        .ForEach(obj =>
        {
          switch (obj)
          {
            case IGH_Param param:
              AddParameter(param);
              break;
            case IGH_Component component:
              GetComponentParamsWithLocalData(component)
                .ToList()
                .ForEach(AddParameter);
              break;
          }
        });
    }

    private ParamStatus GetParamStatus(Task<double> task)
    {
      if (task == null) return ParamStatus.Untracked;
      if (task.IsCanceled || task.IsFaulted) return ParamStatus.Error;
      if (task.IsCompleted)
        return task.Result >= Settings.ParamThreshold ? ParamStatus.OverThreshold : ParamStatus.UnderThreshold;
      return ParamStatus.Loading;
    }

    public Dictionary<Guid, Task<double>> Results => _resultsCache;
    
    public ParamStatus GetParamStatus(IGH_Param p) => GetParamStatus(Get(p));

    public IEnumerable<IGH_DocumentObject> GetParams(double threshold)
    {
      return _resultsCache
        .Where(pair => GetParamStatus(pair.Value) == ParamStatus.OverThreshold)
        .Select(kv => Instances.ActiveCanvas.Document.FindObject(kv.Key, false));
    }

    public double GetTotal()
    {
      return _resultsCache.Where(pair => pair.Value.IsCompleted).Sum(pair => pair.Value.Result);
    }

    public Task<double> Get(IGH_Param param)
    {
      return !_resultsCache.ContainsKey(param.InstanceGuid) ? null : _resultsCache[param.InstanceGuid];
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

      var size = 0.0;
      switch (serializationType)
      {
        case SerializationType.Xml:
          var xml = archive.Serialize_Xml();
          var byteSize = (double)Encoding.Unicode.GetByteCount(xml);
          size = byteSize / 1048576;
          break;

        case SerializationType.Binary:
          var byteArray = archive.Serialize_Binary();
          size = ((double)byteArray.Length) / 1048576; 
          break;

        default:
          throw new ArgumentOutOfRangeException(nameof(serializationType), serializationType,
            "Incorrect Serialization Type was passed.");
      }
      RhinoApp.InvokeOnUiThread((Action)Instances.InvalidateCanvas);
      return size;
    }


    private void AddParamEvents(IGH_Param p)
    {
      p.ObjectChanged += OnObjectChanged;
    }

    private void RemoveParamEvents(IGH_Param p)
    {
      p.ObjectChanged -= OnObjectChanged;
    }

    private void AddComponentEvents(IGH_Component c)
    {
      c.AttributesChanged += OnAttributesChanged;
    }

    private void RemoveComponentEvents(IGH_Component c)
    {
      c.AttributesChanged -= OnAttributesChanged;
    }

    private void OnAttributesChanged(IGH_DocumentObject sender, GH_AttributesChangedEventArgs e)
    {
      if (sender is IGH_Component component)
        component.Params.Input.ForEach(p =>
        {
          // Re-register `OnObjectChanged` on all params of that component
          RemoveParamEvents(p);
          AddParamEvents(p);
          AddParameter(p);
        });
    }

    private void OnObjectChanged(IGH_DocumentObject sender, GH_ObjectChangedEventArgs e)
    {
      if (sender is IGH_Param p && e.Type == GH_ObjectEventType.Sources)
        switch (p.DataType)
        {
          case GH_ParamData.local:
            AddParameter(p);
            break;
          case GH_ParamData.unknown:
          case GH_ParamData.@void:
          case GH_ParamData.remote:
          default:
            RemoveParameter(p);
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