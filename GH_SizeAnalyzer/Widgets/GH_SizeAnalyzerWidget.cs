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
using SizeAnalyzer.Properties;
using SizeAnalyzer.UI;

namespace SizeAnalyzer.Widgets
{
  public class GH_SizeAnalyzerWidget : GH_CanvasWidget_FixedObject
  {
    public readonly Calculator Calculator;
    private const int Radius = 4;

    private List<IGH_Param> _drawnIcons = new List<IGH_Param>();

    private SizeAnalyzerSearchDialog _searchDialog;

    private Rectangle _widgetArea;

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
      return _widgetArea.Contains(ptControl) ||
             // If not check the drawn icons.
             _drawnIcons.Any(p => DrawUtils.GetParamIconRectangleF(p, Radius).Contains(ptCanvas));
    }

    public override bool IsTooltipRegion(PointF canvasCoordinate)
    {
      var isTooltipRegion = base.IsTooltipRegion(canvasCoordinate);

      return isTooltipRegion;
    }

    public override void AppendToMenu(ToolStripDropDownMenu menu)
    {
      base.AppendToMenu(menu);

      GH_DocumentObject.Menu_AppendItem(menu, "Open search dialog", (o, e) => { ShowSearchDialog(); });

      GH_DocumentObject.Menu_AppendSeparator(menu);

      CreateContextMenuSettings(
        menu,
        "Document threshold",
        Settings.GlobalThreshold,
        new List<double> { 10, 20, 50, 100 },
        (value) => Settings.GlobalThreshold = value
      );

      CreateContextMenuSettings(
        menu,
        "Parameter threshold",
        Settings.ParamThreshold,
        new List<double> { 1, 2, 5, 10 },
        (value) => Settings.ParamThreshold = value
      );
    }
    
    private static void CreateContextMenuSettings(ToolStripDropDownMenu menu, string name,
      double value, List<double> options, Action<double> onClick)
    {
      var item = GH_DocumentObject.Menu_AppendItem(menu, name);

      foreach (var option in options)
        GH_DocumentObject.Menu_AppendItem(
          item.DropDown,
          $"{option}mb", (e, a) => onClick(option),
          null,
          true,
          Math.Abs(value - option) < 0.01);

      // Create custom option
      var custom = GH_DocumentObject.Menu_AppendItem(item.DropDown, "Custom");
      var digitScroller = DrawUtils.SetupMenuDigitScroller();
      digitScroller.Value = Convert.ToDecimal(value);
      digitScroller.ValueChanged += (sender, args) => onClick(Convert.ToDouble(args.Value));
      custom.Checked = !options.Any(option => Math.Abs(value - option) < 0.01);
      GH_DocumentObject.Menu_AppendCustomItem(custom.DropDown, digitScroller);
    }

    private void ShowSearchDialog()
    {
      SearchDialog.Show();
      if (!SearchDialog.Focused)
        SearchDialog.Focus();
    }

    public override void SetupTooltip(PointF canvasPoint, GH_TooltipDisplayEventArgs e)
    {
      var param = _drawnIcons.FirstOrDefault(p => DrawUtils.GetParamIconRectangleF(p, Radius).Contains(canvasPoint));
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

    public override void Render(GH_Canvas canvas)
    {
      // Call the base render to continue drawing the "fixed" part of the widget
      base.Render(canvas);

      if (canvas.Document == null) return;
      if (!Settings.ShowParamWarnings) return;

      // Draw here anything that has to be drawn "per-component" (i.e. the bubbles with the size)
      _drawnIcons = new List<IGH_Param>();
      DrawUtils.GetAllParamsWithLocalData(canvas.Document)
        .ToList()
        .ForEach(p => DrawParamIcon(canvas, p));
    }

    protected override void Render_Internal(GH_Canvas canvas, Point controlAnchor, PointF canvasAnchor,
      Rectangle controlFrame, RectangleF canvasFrame)
    {
      if (Instances.ActiveCanvas.Document == null) return; // Skip if no document
      if (!Settings.ShowGlobalWarnings) return;
      var total = Calculator.GetTotal();
      if (total < Settings.GlobalThreshold) return;

      _widgetArea = controlFrame; // Update the WidgetArea

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

    private void DrawParamIcon(GH_Canvas canvas, IGH_Param p)
    {
      var status = Calculator.GetParamStatus(p);
      switch (status)
      {
        case ParamStatus.Loading:
          DrawLoadingIcon(canvas, p);
          break;
        case ParamStatus.OverThreshold:
          DrawWarningIcon(canvas, p);
          break;
        case ParamStatus.UnderThreshold:
        case ParamStatus.Untracked:
        case ParamStatus.Error:
        default:
          break;
      }
    }

    private void DrawWarningIcon(GH_Canvas canvas, IGH_Param p)
    {
      if (GH_Canvas.ZoomFadeLow == 0)
      {
        DrawUtils.DrawParamIcon_ZoomedOut(canvas, p);
      }
      else
      {
        DrawUtils.DrawParamIcon(canvas, p, Radius);
        _drawnIcons.Add(p);
      }
    }

    private void DrawLoadingIcon(GH_Canvas canvas, IGH_Param p)
    {
      if (GH_Canvas.ZoomFadeLow != 0)
      {
        DrawUtils.DrawLoadingIcon(canvas, p, Radius);
        _drawnIcons.Add(p);
      }
    }
  }
}