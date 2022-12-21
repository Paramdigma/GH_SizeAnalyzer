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
    
    private void OnDocumentChanged(GH_Canvas c, GH_CanvasDocumentChangedEventArgs ce)
    {
      if (ce.OldDocument != null)
      {
        if (Widget.Watcher.IsWatching)
          Widget.Watcher.Stop();
        Widget.Watcher.Document = null;
      }

      if (ce.NewDocument == null) return;
      
      Widget.Watcher.Document = ce.NewDocument;
      Widget.Watcher.Start();
    }
  }
}