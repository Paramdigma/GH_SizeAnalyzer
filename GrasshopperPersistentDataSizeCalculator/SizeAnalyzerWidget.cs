using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Widgets;
using Grasshopper.Kernel;

namespace GrasshopperPersistentDataSizeCalculator
{
    public class SizeAnalyzerWidget : GH_CanvasWidget_FixedObject
    {
        public static bool SharedVisible { get; set; } = true;

        public override bool Visible { get => SharedVisible; set => SharedVisible = value; }

        public override string Name => "Internal Data Size Analyzer";

        public override string Description => "Shows the size of internal data";

        public override string TooltipText => "Shows the size of any internalized data";

        public override Bitmap Icon_24x24 => null;

        // Defines which corner the widget will be drawn in
        public override SizeF Ratio { get; set; } = new SizeF(0f, 1f);

        // Defines the size of the controlRectangle to draw the widget in.
        public override Size Size => new Size(200, 60);

        public override int Padding => 10;

        public override bool Contains(Point pt_control, PointF pt_canvas)
        {
            if (bbox == null) return false;
            return bbox.Contains(pt_control);
        }
        
        public override void SetupTooltip(PointF canvasPoint, GH_TooltipDisplayEventArgs e)
        {
            base.SetupTooltip(canvasPoint, e);
        }
        
        public override bool TooltipEnabled => true;

        public override void Render(GH_Canvas canvas)
        {
            // Call the base render to continue drawing the "fixed" part of the widget
            base.Render(canvas);

            // Draw here anything that has to be drawn "per-component" (i.e. the bubbles with the size)
        }

        protected Rectangle bbox;

        protected override void Render_Internal(GH_Canvas canvas, Point controlAnchor, PointF canvasAnchor, Rectangle controlFrame, RectangleF canvasFrame)
        {
            //if (Instances.ActiveCanvas.Document == null) return;

            var graphics = canvas.Graphics;
            var solidBrush = new SolidBrush(Color.Red);
            var blackBrush = new SolidBrush(Color.Black);
            var blackPen = new Pen(Color.Black);
            var rect = controlFrame;
            bbox = rect;
            // To get it to draw fixed on the screen we must reset the canvas transform, and store it for later.
            var transform = canvas.Graphics.Transform;
            graphics.ResetTransform();

            // Draw your stuff here!
            graphics.DrawRectangle(blackPen, rect);
            graphics.FillRectangle(solidBrush, rect);

            var x = controlAnchor.X - controlFrame.Width/2;
            var y = controlAnchor.Y - controlFrame.Height/2;

            graphics.DrawString("Total internal size", new Font(FontFamily.GenericSansSerif, 10), blackBrush, x, y);
            graphics.DrawString("Total internal size", new Font(FontFamily.GenericSansSerif, 20), blackBrush, x, y + 15);
            // Once done, we reset the transform of the canvas.
            graphics.Transform = transform;

            // Dispose all of our pens when done
            blackPen.Dispose();
            solidBrush.Dispose();
        }
    }
}
