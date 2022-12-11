﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Widgets;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.UI;
using GH_DigitScroller = Grasshopper.GUI.GH_DigitScroller;
using SizeAnalyzer.UI;

namespace SizeAnalyzer.Widgets
{
  public class SizeAnalyzerWidget : GH_CanvasWidget_FixedObject
  {

    private static int radius = 5;

    public override bool Visible
    {
      get => Settings.Show;
      set => Settings.Show = value;
    }

    public override string Name => "Internal Data Size Analyzer";

    public override string Description => "Shows the size of internal data";

    public override string TooltipText => "Shows the size of any internalized data";

    public override Bitmap Icon_24x24 => null;

    // Defines which corner the widget will be drawn in
    public override SizeF Ratio { get; set; } = new SizeF(0f, 1f);

    // Defines the size of the controlRectangle to draw the widget in.
    public override Size Size => new Size(Global_Proc.UiAdjust(200), Global_Proc.UiAdjust(60));

    public override int Padding => 10;

    public override bool Contains(Point ptControl, PointF ptCanvas)
    {
      // Check if mouse is inside fixed widget area
      if (WidgetArea.Contains(ptControl))
        return true;

      // If not check the drawn icons.
      return DrawnIcons.Any(p => GetParamIconRectangleF(p, radius).Contains(ptCanvas));
    }
    
    public override bool IsTooltipRegion(PointF canvas_coordinate)
    {
      var isTooltipRegion = base.IsTooltipRegion(canvas_coordinate);

      return isTooltipRegion;
    }

    public override bool TooltipEnabled => true;

    public static SizeAnalyzerSearchDialog SearchDialog
    {
      get
      {
        if (_searchDialog == null)
          _searchDialog = new SizeAnalyzerSearchDialog();
        _searchDialog.Canvas = Instances.ActiveCanvas;
        _searchDialog.FormClosed += (s, e) => _searchDialog = null;
        return _searchDialog;
      }
    }

    public List<IGH_Param> DrawnIcons = new List<IGH_Param>();

    public override void Render(GH_Canvas canvas)
    {
      // Call the base render to continue drawing the "fixed" part of the widget
      base.Render(canvas);

      if (canvas.Document == null) return;
      if (!Settings.ShowParamWarnings) return;
      
      // Draw here anything that has to be drawn "per-component" (i.e. the bubbles with the size)
      DrawnIcons = new List<IGH_Param>();
      var drawableParams = GetAllParamsWithLocalData(canvas.Document);
      foreach (var p in drawableParams)
      {
        var res = Calculator.Get(p);
        if (res == null) continue;
        if (!res.IsCompleted && !res.IsCanceled && !res.IsFaulted)
        {
          if (GH_Canvas.ZoomFadeLow != 0)
            DrawLoadingIcon(canvas, p);
        }
        else if (res.IsCompleted && res.Result > Settings.ParamThreshold)
        {
          if (GH_Canvas.ZoomFadeLow == 0)
            DrawParamIcon_ZoomedOut(canvas, p);
          else
            DrawParamIcon(canvas, p);
        }
      }
    }

    private void DrawLoadingIcon(GH_Canvas canvas, IGH_Param p)
    {
      var radius = 5;
      var brush = Brushes.Blue;
      var bounds = p.Attributes.Bounds;
      var center = new PointF(bounds.Right - radius, bounds.Top - radius);

      if (p.Kind == GH_ParamKind.input)
      {
        center.X += 4;
        center.Y += 2;
      }

      var r = new RectangleF(center, new SizeF(radius * 2, radius * 2));
      var whitesmoke = new Pen(Color.WhiteSmoke);
      whitesmoke.Width = 2;
      canvas.Graphics.DrawEllipse(whitesmoke, r);
      canvas.Graphics.FillEllipse(brush, r);

      DrawnIcons.Add(p);
    }

    private void DrawParamIcon(GH_Canvas canvas, IGH_Param p)
    {
      var radius = 5;
      var brush = Brushes.Red;
      var r = GetParamIconRectangleF(p, radius);
      var whitesmoke = new Pen(Color.WhiteSmoke)
      {
        Width = 2
      };
      canvas.Graphics.DrawEllipse(whitesmoke, r);
      canvas.Graphics.FillEllipse(brush, r);

      var font = new Font(FontFamily.GenericMonospace, radius, FontStyle.Bold);
      var textPosition = new PointF(r.Location.X, r.Location.Y + 1 - radius / 2);
      canvas.Graphics.DrawString("!", font, Brushes.WhiteSmoke, textPosition);
      DrawnIcons.Add(p);
    }

    private static RectangleF GetParamIconRectangleF(IGH_Param p, int radius)
    {
      var bounds = p.Attributes.Bounds;
      var center = new PointF(bounds.Right - radius, bounds.Top - radius);

      if (p.Kind == GH_ParamKind.input)
      {
        center.X += 4;
        center.Y += 2;
      }

      var r = new RectangleF(center, new SizeF(radius * 2, radius * 2));
      return r;
    }

    private static void DrawParamIcon_ZoomedOut(GH_Canvas canvas, IGH_Param p)
    {
      var rect = GH_Convert.ToRectangle(p.Attributes.Bounds);
      canvas.Graphics.FillRectangle(Brushes.Red, rect);
    }

    private Rectangle WidgetArea;

    public override void AppendToMenu(ToolStripDropDownMenu menu)
    {
      base.AppendToMenu(menu);
      GH_DocumentObject.Menu_AppendSeparator(menu);

      // Create fixed megabyte options
      var options = new List<double> { 0.5, 1, 2, 5, 10 };
      foreach (var option in options)
        GH_DocumentObject.Menu_AppendItem(menu, $"{option}mb", (e, a) => Settings.ParamThreshold = option, null, true, Math.Abs(Settings.ParamThreshold - option) < 0.01);

      GH_DocumentObject.Menu_AppendSeparator(menu);

      // Create custom option
      var customItem = GH_DocumentObject.Menu_AppendItem(menu, "Custom");
      var digitScroller = new GH_DigitScroller
      {
        Height = 40,
        Width = 200,
        MaximumValue = 100,
        MinimumValue = Convert.ToDecimal(0.01),
        Value = Convert.ToDecimal(Settings.ParamThreshold)
      };

      //digitScroller.ValueChanged += (sender, args) => Settings.ParamThreshold = Convert.ToDouble(args.Value);
      customItem.Checked = !options.Any(option => Math.Abs(Settings.ParamThreshold - option) < 0.01);
      GH_DocumentObject.Menu_AppendCustomItem(customItem.DropDown, digitScroller);

      GH_DocumentObject.Menu_AppendSeparator(menu);

      GH_DocumentObject.Menu_AppendItem(menu, "Open search dialog", (o, e) =>
      {
        SearchDialog.Show();
        SearchDialog.Focus();
      });

    }

    private static SizeAnalyzerSearchDialog _searchDialog;

    /// <summary>
    /// Draws the fixed position part of the Widget
    /// </summary>
    protected override void Render_Internal(GH_Canvas canvas, Point controlAnchor, PointF canvasAnchor,
          Rectangle controlFrame, RectangleF canvasFrame)
    {
      if (Instances.ActiveCanvas.Document == null) return; // Skip if no document
      if(!Settings.ShowGlobalWarnings) return;
      WidgetArea = controlFrame; // Update the WidgetArea

      var graphics = canvas.Graphics; // Get the graphics instance

      // Setup brushes and pens
      var solidBrush = new SolidBrush(Color.Red);
      var blackBrush = new SolidBrush(Color.Black);
      var blackPen = new Pen(Color.Black);

      // To get it to draw fixed on the screen we must reset the canvas transform, and store it for later.
      var transform = canvas.Graphics.Transform;
      graphics.ResetTransform();

      // Draw the background rectangle
      graphics.DrawRectangle(blackPen, controlFrame);
      graphics.FillRectangle(solidBrush, controlFrame);

      // Get x,y coords for text
      var x = controlAnchor.X - controlFrame.Width / 2;
      var y = controlAnchor.Y - controlFrame.Height / 2;

      // Draw the text
      graphics.DrawString("Total internal size", new Font(FontFamily.GenericSansSerif, 10), blackBrush, x,
        y += Global_Proc.UiAdjust(5));
      graphics.DrawString(Math.Round(Calculator.GetTotal(), 3) + "mb", new Font(FontFamily.GenericSansSerif, 20), blackBrush, x,
        y + Global_Proc.UiAdjust(15));

      // Once done, we reset the transform of the canvas.
      graphics.Transform = transform;

      // Dispose all of our pens when done
      blackPen.Dispose();
      solidBrush.Dispose();
      blackPen.Dispose();
    }

    public static IEnumerable<IGH_Param> GetAllParamsWithLocalData(GH_Document doc)
    {
      foreach (var obj in doc.Objects)
      {
        switch (obj)
        {
          case IGH_Param param:
            var paramType = param.GetType();

            // In general, skip any type from the `Special` namespace
            var specialNamespace = typeof(GH_NumberSlider).Namespace;
            var shouldSkipNamespace = paramType.Namespace == specialNamespace;

            // With some exceptions
            var isException = new List<Type> { typeof(GH_Panel) }.Contains(paramType);

            var shouldSkip = shouldSkipNamespace && !isException;

            if (!shouldSkip && param.DataType == GH_ParamData.local)
              yield return param;
            break;
          case IGH_Component component:
            {
              var localDataParams = component.Params.Input.Where(cParam => cParam.DataType == GH_ParamData.local);
              foreach (var cParam in localDataParams)
                yield return cParam;
              break;
            }
        }
      }
    }

    public override void SetupTooltip(PointF canvasPoint, GH_TooltipDisplayEventArgs e)
    {
      var param = DrawnIcons.FirstOrDefault(p => GetParamIconRectangleF(p, radius).Contains(canvasPoint));
      base.SetupTooltip(canvasPoint, e);
      if (param == null) return;

      var task = Calculator.Get(param);
      if (!task.IsCompleted) return;

      e.Title = "Warning: Internal data is too big";
      e.Text = $"This parameter's data is TOO BIG.";
      e.Description = $"Data size = {Math.Round(task.Result, 3)}mb\nThreshold = {Settings.ParamThreshold}mb";
    }
  }
}