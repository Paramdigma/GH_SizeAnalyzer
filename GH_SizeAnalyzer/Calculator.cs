using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Rhino;

namespace SizeAnalyzer
{
  public class Calculator
  {
    [Flags]
    public enum ParamType
    {
      OverThreshold = 1,
      Loading = 2,
      UnderThreshold = 4,
      All = 7
    }

    private Dictionary<Guid, Task<double>> _resultsCache = new Dictionary<Guid, Task<double>>();

    public CancellationTokenSource CancelTokenSource = new CancellationTokenSource();

    public SerializationType SerializationType = SerializationType.Xml;

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
              Add(param);
              break;
            case IGH_Component component:
              GetComponentParamsWithLocalData(component)
                .ToList()
                .ForEach(Add);
              break;
          }
        });
    }

    public IEnumerable<IGH_DocumentObject> GetParams(double threshold)
    {
      return _resultsCache
        .Where(pair => pair.Value.IsCompleted && pair.Value.Result >= threshold)
        .Select(kv => Instances.ActiveCanvas.Document.FindObject(kv.Key, false));
    }

    public double GetTotal()
    {
      return _resultsCache.Where(pair => pair.Value.IsCompleted).Sum(pair => pair.Value.Result);
    }

    private void Add(IGH_Param param)
    {
      if (_resultsCache.ContainsKey(param.InstanceGuid))
        _resultsCache[param.InstanceGuid] = GetParamDataSizeAsync(param, SerializationType);
      else
        _resultsCache.Add(param.InstanceGuid, GetParamDataSizeAsync(param, SerializationType));
    }

    private void Remove(IGH_Param param)
    {
      if (_resultsCache.ContainsKey(param.InstanceGuid))
        _resultsCache.Remove(param.InstanceGuid);
    }

    public Task<double> Get(IGH_Param param)
    {
      return !_resultsCache.ContainsKey(param.InstanceGuid) ? null : _resultsCache[param.InstanceGuid];
    }

    private IEnumerable<IGH_Param> GetComponentParamsWithLocalData(IGH_Component component)
    {
      return component.Params.Input.Where(cParam => cParam.DataType == GH_ParamData.local);
    }

    private IEnumerable<IGH_DocumentObject> GetAllDocumentObjectsWithLocalData(GH_Document doc)
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

    private async Task<double> GetParamDataSizeAsync(IGH_Param param,
      SerializationType serializationType = SerializationType.Binary)
    {
      var task = Task.Run(() => GetParamDataSize(param, serializationType), CancelTokenSource.Token);
      await task.ContinueWith(_ => RhinoApp.InvokeOnUiThread((Action)Instances.InvalidateCanvas));
      return await task;
    }

    private double GetParamDataSize(IGH_Param param,
      SerializationType serializationType = SerializationType.Binary)
    {
      var archive = new GH_Archive();
      archive.CreateTopLevelNode("param size archive");
      archive.AppendObject(param, param.InstanceGuid.ToString());

      switch (serializationType)
      {
        case SerializationType.Xml:
          var xml = archive.Serialize_Xml();
          var byteSize = (double)Encoding.Unicode.GetByteCount(xml);
          return byteSize / 1048576;

        case SerializationType.Binary:
          var byteArray = archive.Serialize_Binary();
          return byteArray.Length / 1048576;

        default:
          throw new ArgumentOutOfRangeException(nameof(serializationType), serializationType,
            "Incorrect Serialization Type was passed.");
      }
    }

    public void OnDocumentChanged(GH_Canvas c, GH_CanvasDocumentChangedEventArgs ce)
    {
      if (ce.OldDocument != null)
      {
        ce.OldDocument.ObjectsAdded -= OnObjectsAdded;
        ce.OldDocument.ObjectsDeleted -= OnObjectsDeleted;
      }

      if (ce.NewDocument != null)
      {
        ce.NewDocument.ObjectsAdded += OnObjectsAdded;
        ce.NewDocument.ObjectsDeleted += OnObjectsDeleted;

        Compute(ce.NewDocument);
      }
    }

    private void OnObjectsDeleted(object sender, GH_DocObjectEventArgs e)
    {
      e.Objects.ToList().ForEach(RemoveObject);
    }

    private void RemoveObject(IGH_DocumentObject obj)
    {
      switch (obj)
      {
        case IGH_Param param:
          param.ObjectChanged -= OnObjectChanged;
          Remove(param);
          break;
        case IGH_Component component:
          // A component's attributes change when a param is added/deleted from the component
          component.AttributesChanged -= OnAttributesChanged;
          component.Params.Input.ForEach(p =>
          {
            p.ObjectChanged -= OnObjectChanged;
            Remove(p);
          });
          break;
      }
    }

    private void OnObjectsAdded(object sender, GH_DocObjectEventArgs e)
    {
      foreach (var ghDocumentObject in e.Objects) AddObject(ghDocumentObject);
    }

    private void AddObject(IGH_DocumentObject ghDocumentObject)
    {
      switch (ghDocumentObject)
      {
        case IGH_Param param:
          param.ObjectChanged += OnObjectChanged;
          Add(param);
          break;
        case IGH_Component component:
          // A component's attributes change when a param is added/deleted from the component
          component.AttributesChanged += OnAttributesChanged;
          component.Params.Input.ForEach(p =>
          {
            p.ObjectChanged += OnObjectChanged;
            Add(p);
          });
          break;
      }
    }

    private void OnAttributesChanged(IGH_DocumentObject sender, GH_AttributesChangedEventArgs e)
    {
      if (sender is IGH_Component component)
        component.Params.Input.ForEach(p =>
        {
          // Re-register `OnObjectChanged` on all params of that component
          p.ObjectChanged -= OnObjectChanged;
          p.ObjectChanged += OnObjectChanged;
          Add(p);
        });
    }

    private void OnObjectChanged(IGH_DocumentObject sender, GH_ObjectChangedEventArgs e)
    {
      if (sender is IGH_Param p && e.Type == GH_ObjectEventType.Sources)
        switch (p.DataType)
        {
          case GH_ParamData.local:
            Add(p);
            break;
          default:
            Remove(p);
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