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
            Instances.ActiveCanvas.DocumentChanged -= Calculator.OnDocumentChanged;
            Widget.Owner = null;
        }

        public void OnCanvasCreated(GH_Canvas canvas)
        {
            // Subscribe to all relevant events
            Instances.ActiveCanvas.DocumentChanged += Calculator.OnDocumentChanged;

            // Set the canvas as the widget's owner
            Widget.Owner = canvas;
            // Finally, add the widget to the canvas.
            Instances.ActiveCanvas.Widgets.Add(Widget);
        }
    }
}