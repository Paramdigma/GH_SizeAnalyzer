using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Widgets;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using SizeAnalyzer.Properties;
using SizeAnalyzer.UI;
using GH_DigitScroller = Grasshopper.GUI.GH_DigitScroller;

namespace SizeAnalyzer.Widgets
{
  public class GH_SizeAnalyzerWidget : GH_CanvasWidget_FixedObject
  {
    private readonly int radius = 4;

    private List<IGH_Param> _drawnIcons = new List<IGH_Param>();

    private SizeAnalyzerSearchDialog _searchDialog;

    public Calculator Calculator;


    private Rectangle WidgetArea;

    public GH_SizeAnalyzerWidget()
    {
      Calculator = new Calculator();
    }

    public override bool Visible
    {
      get => Settings.Show;
      set => Settings.Show = value;
    }

    public override string Name => "Size Analyzer";

    public override string Description => "Alerts of the size of internal data when it exceeds a given threshold.";

    public override string TooltipText =>
      "Alerts of the size of any internalized data when it exceeds a given threshold.";

    public override Bitmap Icon_24x24 => Resources.CalculatorIcon;

    // Defines which corner the widget will be drawn in
    public override SizeF Ratio { get; set; } = new SizeF(0f, 1f);

    // Defines the size of the controlRectangle to draw the widget in.
    public override Size Size => new Size(Global_Proc.UiAdjust(200), Global_Proc.UiAdjust(60));

    public override int Padding => 10;

    public override bool TooltipEnabled => true;

    private SizeAnalyzerSearchDialog SearchDialog
    {
      get
      {
        if (_searchDialog != null) return _searchDialog;
        _searchDialog = new SizeAnalyzerSearchDialog(Calculator);
        _searchDialog.Canvas = Instances.ActiveCanvas;
        _searchDialog.FormClosed += (s, e) => _searchDialog = null;
        return _searchDialog;
      }
    }

    public override bool Contains(Point ptControl, PointF ptCanvas)
    {
      // Check if mouse is inside fixed widget area
      if (WidgetArea.Contains(ptControl))

        return true;

      // If not check the drawn icons.
      return _drawnIcons.Any(p => DrawUtils.GetParamIconRectangleF(p, radius).Contains(ptCanvas));
    }

    public override bool IsTooltipRegion(PointF canvas_coordinate)
    {
      var isTooltipRegion = base.IsTooltipRegion(canvas_coordinate);

      return isTooltipRegion;
    }

    public override void Render(GH_Canvas canvas)
    {
      // Call the base render to continue drawing the "fixed" part of the widget
      base.Render(canvas);

      if (canvas.Document == null) return;
      if (!Settings.ShowParamWarnings) return;

      // Draw here anything that has to be drawn "per-component" (i.e. the bubbles with the size)
      _drawnIcons = new List<IGH_Param>();
      var drawableParams = GetAllParamsWithLocalData(canvas.Document);
      foreach (var p in drawableParams)
      {
        var res = Calculator.Get(p);
        if (res == null) continue;
        switch (res.IsCompleted)
        {
          case false when !res.IsCanceled && !res.IsFaulted:
          {
            if (GH_Canvas.ZoomFadeLow != 0)
            {
              DrawUtils.DrawLoadingIcon(canvas, p, radius);
              _drawnIcons.Add(p);
            }

            break;
          }
          case true when res.Result > Settings.ParamThreshold:
          {
            if (GH_Canvas.ZoomFadeLow == 0)
            {
              DrawUtils.DrawParamIcon_ZoomedOut(canvas, p);
            }
            else
            {
              DrawUtils.DrawParamIcon(canvas, p, radius);
              _drawnIcons.Add(p);
            }

            break;
          }
        }
      }
    }

    public override void AppendToMenu(ToolStripDropDownMenu menu)
    {
      base.AppendToMenu(menu);

      GH_DocumentObject.Menu_AppendItem(menu, "Open search dialog", (o, e) =>
      {
        SearchDialog.Show();
        SearchDialog.Focus();
      });
      GH_DocumentObject.Menu_AppendSeparator(menu);

      var itemA = GH_DocumentObject.Menu_AppendItem(menu, "Document Threshold");
      itemA.Enabled = false;
      // Create fixed megabyte options
      var optionsGlobal = new List<double> { 10, 25, 50, 100 };
      foreach (var option in optionsGlobal)
        GH_DocumentObject.Menu_AppendItem(menu, $"{option}mb", (e, a) => Settings.GlobalThreshold = option,
          null, true, Math.Abs(Settings.GlobalThreshold - option) < 0.01);

      // Create custom option
      var customItemg = GH_DocumentObject.Menu_AppendItem(menu, "Custom");
      var digitScrollerg = new GH_DigitScroller
      {
        Height = 40,
        Width = 200,
        DecimalPlaces = 0,
        MaximumValue = 100,
        MinimumValue = Convert.ToDecimal(1),
        Value = Convert.ToDecimal(Settings.GlobalThreshold)
      };

      digitScrollerg.ValueChanged += (sender, args) => Settings.GlobalThreshold = Convert.ToDouble(args.Value);
      customItemg.Checked = !optionsGlobal.Any(option => Math.Abs(Settings.GlobalThreshold - option) < 0.01);
      GH_DocumentObject.Menu_AppendCustomItem(customItemg.DropDown, digitScrollerg);


      GH_DocumentObject.Menu_AppendSeparator(menu);
      var itemB = GH_DocumentObject.Menu_AppendItem(menu, "Params Threshold");
      itemB.Enabled = false;
      // Create fixed megabyte options
      var optionsParams = new List<double> { 1, 2, 5, 10 };
      foreach (var option in optionsParams)
        GH_DocumentObject.Menu_AppendItem(menu, $"{option}mb", (e, a) => Settings.ParamThreshold = option, null,
          true, Math.Abs(Settings.ParamThreshold - option) < 0.01);

      // Create custom option
      var customItem = GH_DocumentObject.Menu_AppendItem(menu, "Custom");
      var digitScroller = new GH_DigitScroller
      {
        Height = 40,
        Width = 200,
        DecimalPlaces = 0,
        MaximumValue = 100,
        MinimumValue = Convert.ToDecimal(1),
        Value = Convert.ToDecimal(Settings.ParamThreshold)
      };

      digitScroller.ValueChanged += (sender, args) => Settings.ParamThreshold = Convert.ToDouble(args.Value);
      customItem.Checked = !optionsParams.Any(option => Math.Abs(Settings.ParamThreshold - option) < 0.01);
      GH_DocumentObject.Menu_AppendCustomItem(customItem.DropDown, digitScroller);
    }

    /// <summary>
    ///   Draws the fixed position part of the Widget
    /// </summary>
    protected override void Render_Internal(GH_Canvas canvas, Point controlAnchor, PointF canvasAnchor,
      Rectangle controlFrame, RectangleF canvasFrame)
    {
      if (Instances.ActiveCanvas.Document == null) return; // Skip if no document
      if (!Settings.ShowGlobalWarnings) return;
      var total = Calculator.GetTotal();
      if (total < Settings.GlobalThreshold) return;

      WidgetArea = controlFrame; // Update the WidgetArea

      var graphics = canvas.Graphics; // Get the graphics instance

      // Setup brushes and pens
      var solidBrush = new SolidBrush(Color.Red);
      var blackPen = new Pen(Color.Black);

      // To get it to draw fixed on the screen we must reset the canvas transform, and store it for later.
      var transform = canvas.Graphics.Transform;
      var textCapsule = GH_Capsule.CreateTextCapsule(
        controlFrame,
        controlFrame,
        GH_Palette.Warning, "Total size:\n" + Math.Round(total, 1) + "mb", new Font(FontFamily.GenericSansSerif, 15));
      graphics.ResetTransform();

      textCapsule.Render(graphics, Color.Red);

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
            var localDataParams =
              component.Params.Input.Where(cParam => cParam.DataType == GH_ParamData.local);
            foreach (var cParam in localDataParams)
              yield return cParam;
            break;
          }
        }
    }

    public override void SetupTooltip(PointF canvasPoint, GH_TooltipDisplayEventArgs e)
    {
      var param = _drawnIcons.FirstOrDefault(p => DrawUtils.GetParamIconRectangleF(p, radius).Contains(canvasPoint));
      base.SetupTooltip(canvasPoint, e);

      if (param == null)
      {
        e.Description = $"Document Threshold = {Settings.GlobalThreshold}mb";
        return;
      }

      var task = Calculator.Get(param);
      if (!task.IsCompleted) return;

      e.Title = "Warning: Internal data is too big";
      e.Text = "This parameter's data is TOO BIG.";
      e.Description = $"Data size = {Math.Round(task.Result, 2)}mb\nThreshold = {Settings.ParamThreshold}mb";
    }
  }
}