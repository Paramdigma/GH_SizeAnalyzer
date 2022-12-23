using System;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;

namespace SizeAnalyzer
{
  public class CalculatorDocumentWatcher
  {
    private GH_Document? _document;
    public Calculator Calculator { get; }
    public bool IsWatching { get; private set; }

    public GH_Document? Document
    {
      set
      {
        if (IsWatching)
          throw new InvalidOperationException("Document cannot be changed while it's being watched");
        _document = value;
        if (value != null)
          Calculator.Compute(value);
        else
          Calculator.Reset();
      }
    }

    public CalculatorDocumentWatcher()
    {
      Calculator = new Calculator();
      Calculator.ParamTaskFinished += (sender, args) => Instances.InvalidateCanvas();
      Calculator.ComputeTaskFinished += (sender, args) => Instances.InvalidateCanvas();
    }

    public void Start()
    {
      if (_document == null)
        throw new Exception("Cannot start watching on a null document");
      if (IsWatching)
        throw new Exception("Already watching this document, please just call once per doc");

      _document.ObjectsAdded += Document_OnObjectsAdded;
      _document.ObjectsDeleted += Document_OnObjectsDeleted;
      foreach (var obj in _document.Objects)
        Calculator.ObjectAction(obj, Param_AddEvents, Component_AddEvents);
      IsWatching = true;
    }

    public void Stop()
    {
      if (_document == null) throw new ArgumentNullException(nameof(_document));
      _document.ObjectsAdded -= Document_OnObjectsAdded;
      _document.ObjectsDeleted -= Document_OnObjectsDeleted;
      foreach (var obj in _document.Objects)
        Calculator.ObjectAction(obj, Param_RemoveEvents, Component_RemoveEvents);
      IsWatching = false;
    }

    #region Events

    private void Document_OnObjectsAdded(object sender, GH_DocObjectEventArgs e) =>
      e.Objects.ToList().ForEach(Calculator.Add);

    private void Document_OnObjectsDeleted(object sender, GH_DocObjectEventArgs e) =>
      e.Objects.ToList().ForEach(Calculator.Remove);
    
    private void Param_AddEvents(IGH_Param p) => p.ObjectChanged += Param_OnObjectChanged;
    private void Param_RemoveEvents(IGH_Param p) => p.ObjectChanged -= Param_OnObjectChanged;

    private void Param_OnObjectChanged(IGH_DocumentObject sender, GH_ObjectChangedEventArgs e)
    {
      if (sender is IGH_Param p && e.Type == GH_ObjectEventType.Sources)
        switch (p.DataType)
        {
          case GH_ParamData.local:
            Calculator.Add(p);
            break;
          case GH_ParamData.unknown:
          case GH_ParamData.@void:
          case GH_ParamData.remote:
          default:
            Calculator.Remove(p);
            break;
        }
    }

    private void Component_AddEvents(IGH_Component c) => c.AttributesChanged += Component_OnAttributesChanged;
    private void Component_RemoveEvents(IGH_Component c) => c.AttributesChanged -= Component_OnAttributesChanged;

    private void Component_OnAttributesChanged(IGH_DocumentObject sender, GH_AttributesChangedEventArgs e)
    {
      // Attributes changing means that the input count may have been modified.
      // In that case, we should re-register all parameter events and re-calculate their size.
      if (sender is IGH_Component component)
        component.Params.Input.ForEach(p =>
        {
          Param_RemoveEvents(p);
          Param_AddEvents(p);
          Calculator.Add(p);
        });
    }

    #endregion
  }
}