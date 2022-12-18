using System.Linq;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using SizeAnalyzer.Widgets;

namespace SizeAnalyzer
{
  public class Loader : GH_AssemblyPriority
  {
    public readonly GH_SizeAnalyzerWidget Widget = new GH_SizeAnalyzerWidget();

    public override GH_LoadingInstruction PriorityLoad()
    {
      Instances.CanvasCreated += OnCanvasCreated;
      Instances.CanvasDestroyed += OnCanvasDestroyed;
      return GH_LoadingInstruction.Proceed;
    }

    private void OnCanvasDestroyed(GH_Canvas canvas)
    {
      Instances.ActiveCanvas.DocumentChanged -= OnDocumentChanged;
      Widget.Owner = null;
    }

    public void OnCanvasCreated(GH_Canvas canvas)
    {
      // Subscribe to all relevant events
      Instances.ActiveCanvas.DocumentChanged += OnDocumentChanged;

      // Set the canvas as the widget's owner
      Widget.Owner = canvas;
      // Finally, add the widget to the canvas.
      Instances.ActiveCanvas.Widgets.Add(Widget);
    }
    
    private void OnObjectsAdded(object sender, GH_DocObjectEventArgs e) => e.Objects.ToList().ForEach(Widget.Calculator.Add);
    private void OnObjectsDeleted(object sender, GH_DocObjectEventArgs e) => e.Objects.ToList().ForEach(Widget.Calculator.Remove);
    private void OnDocumentChanged(GH_Canvas c, GH_CanvasDocumentChangedEventArgs ce)
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
        
        Widget.Calculator.Compute(ce.NewDocument);
      }
    }
  }
}