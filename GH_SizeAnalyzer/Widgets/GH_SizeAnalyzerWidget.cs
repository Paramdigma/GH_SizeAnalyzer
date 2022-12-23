using System;
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
using SizeAnalyzer.Properties;
using SizeAnalyzer.UI;

namespace SizeAnalyzer.Widgets
{
  public class GH_SizeAnalyzerWidget : GH_CanvasWidget_FixedObject
  {
    public CalculatorDocumentWatcher Watcher = new CalculatorDocumentWatcher();
    private const int Radius = 4;

    private List<IGH_Param> _drawnIcons = new List<IGH_Param>();

    private SizeAnalyzerSearchDialog? _searchDialog;

    private Rectangle _widgetArea;

    public GH_SizeAnalyzerWidget()
    {
      WidgetCornerChanged(Settings.Corner);
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
    public override SizeF Ratio { get; set; } = new SizeF(1f, 0f);

    // Defines the size of the controlRectangle to draw the widget in.
    public override Size Size => new Size(Global_Proc.UiAdjust(80), (int)Global_Proc.UiAdjust(80 * (float)Math.Sqrt(3)/2f));

    public override int Padding => 50;

    public override bool TooltipEnabled => true;

    protected SizeAnalyzerSearchDialog SearchDialog
    {
      get
      {
        if (_searchDialog != null) return _searchDialog;
        _searchDialog = new SizeAnalyzerSearchDialog(Watcher.Calculator);
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
      
      GH_DocumentObject.Menu_AppendSeparator(menu);
      var posMenu = GH_DocumentObject.Menu_AppendItem(menu, "Doc Warning Position");

      GH_DocumentObject.Menu_AppendItem(posMenu.DropDown, "Top Left",
        (sender, args) => WidgetCornerChanged(Corner.TopLeft));
      GH_DocumentObject.Menu_AppendItem(posMenu.DropDown, "Top Rigth",
        (sender, args) => WidgetCornerChanged(Corner.TopRight));
      GH_DocumentObject.Menu_AppendItem(posMenu.DropDown, "Bottom Left",
        (sender, args) => WidgetCornerChanged(Corner.BottomLeft));
      GH_DocumentObject.Menu_AppendItem(posMenu.DropDown, "Bottom Right",
        (sender, args) => WidgetCornerChanged(Corner.BottomRight));
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

      var task = Watcher.Calculator.Get(param);
      if (task is { IsCompleted: false }) return;

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
      var total = Watcher.Calculator.GetTotal();
      if (total < Settings.GlobalThreshold) return;

      _widgetArea = controlFrame; // Update the WidgetArea

      var graphics = canvas.Graphics; // Get the graphics instance
      
      // To get it to draw fixed on the screen we must reset the canvas transform, and store it for later.
      var transform = canvas.Graphics.Transform;
      
      graphics.ResetTransform();

      //textCapsule.Render(graphics, Color.Red);
      DrawWarningTriangle(controlFrame, graphics);

      // Once done, we reset the transform of the canvas.
      graphics.Transform = transform;
    }

    private static void DrawWarningTriangle(Rectangle controlFrame, Graphics graphics)
    {
      var outlineWidth = 5;
      var textWidth = 10;
      var solidBrush = new SolidBrush(Color.Red);
      var nearBlack = Color.FromArgb(100, 0, 0, 0);
      var shadowBrush = new SolidBrush(nearBlack);
      var shadowPen = new Pen(nearBlack)
      {
        Width = Global_Proc.UiAdjust(outlineWidth),
        StartCap = LineCap.Round,
        EndCap = LineCap.Round,
        LineJoin = LineJoin.Round,
      };
      var whitePen = new Pen(Color.White)
      {
        Width = Global_Proc.UiAdjust(outlineWidth),
        StartCap = LineCap.Round,
        EndCap = LineCap.Round,
        LineJoin = LineJoin.Round,
      };
      var bottomLeft = new PointF(controlFrame.Left, controlFrame.Bottom);
      var bottomRight = new PointF(controlFrame.Right, controlFrame.Bottom);
      var height = Convert.ToSingle(controlFrame.Width * Math.Sqrt(3) / 2);
      var top = new PointF(controlFrame.Left + controlFrame.Width / 2, controlFrame.Bottom - height);

      var path = new GraphicsPath();
      path.AddPolygon(new[] { bottomLeft, top, bottomRight });
      graphics.TranslateTransform(12, 7);
      graphics.DrawPath(shadowPen, path);
      graphics.ResetTransform();
      graphics.FillPath(solidBrush, path);
      graphics.DrawPath(whitePen, path);

      var step = height / 10f;
      var topExcl = new PointF(controlFrame.Left + controlFrame.Width / 2, controlFrame.Bottom - height + step * 3);
      var bottomExcl = new PointF(controlFrame.Left + controlFrame.Width / 2,
        controlFrame.Bottom - height + step * 6.5f);

      var topExclPt = new PointF(controlFrame.Left + controlFrame.Width / 2, controlFrame.Bottom - step * 1.6f);
      var bottomExclPt = new PointF(controlFrame.Left + controlFrame.Width / 2, controlFrame.Bottom - step * 1.5f);

      whitePen.Width = Global_Proc.UiAdjust(7);
      graphics.DrawLine(whitePen, topExcl, bottomExcl);
      graphics.DrawLine(whitePen, topExclPt, bottomExclPt);

      solidBrush.Dispose();
      whitePen.Dispose();
      shadowBrush.Dispose();
    }

    private void DrawParamIcon(GH_Canvas canvas, IGH_Param p)
    {
      var status = Watcher.Calculator.GetParamStatus(p);
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

    private void WidgetCornerChanged(Corner corner)
    {
      switch (corner)
      {
        case Corner.TopLeft:
          Ratio = new SizeF(0f, 0f);
          break;
        case Corner.TopRight:
          Ratio = new SizeF(1f, 0f);
          break;
        case Corner.BottomLeft:
          Ratio = new SizeF(0f, 1f);
          break;
        case Corner.BottomRight:
          Ratio = new SizeF(1f, 1f);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
      }

      Settings.Corner = corner;
      Instances.InvalidateCanvas();
    }
  }
  
  public enum Corner
  {
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
  }
}