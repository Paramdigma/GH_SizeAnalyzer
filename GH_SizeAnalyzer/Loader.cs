using System.Linq;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using SizeAnalyzer.Widgets;

namespace SizeAnalyzer
{
  public class Loader : GH_AssemblyPriority
  {
    private static readonly GH_SizeAnalyzerWidget Widget = new GH_SizeAnalyzerWidget();

    public override GH_LoadingInstruction PriorityLoad()
    {
      Instances.CanvasCreated += OnCanvasCreated;
      Instances.CanvasDestroyed += OnCanvasDestroyed;
      return GH_LoadingInstruction.Proceed;
    }

    private static void OnCanvasDestroyed(GH_Canvas canvas)
    {
      Instances.ActiveCanvas.DocumentChanged -= Widget.OnDocumentChanged;
      Widget.Owner = null;
    }

    private static void OnCanvasCreated(GH_Canvas canvas)
    {
      // Subscribe to all relevant events
      Instances.ActiveCanvas.DocumentChanged += Widget.OnDocumentChanged;

      // Set the canvas as the widget's owner
      Widget.Owner = canvas;
      
      // Finally, add the widget to the canvas.
      Instances.ActiveCanvas.Widgets.Add(Widget);
    }
  }
}