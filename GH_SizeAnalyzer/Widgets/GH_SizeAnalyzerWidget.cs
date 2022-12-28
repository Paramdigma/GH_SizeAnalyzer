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
    private CalculatorDocumentWatcher Watcher = new CalculatorDocumentWatcher();

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

    /// Defines which corner the widget will be drawn in
    public override SizeF Ratio { get; set; } = new SizeF(1f, 0f);

    /// Defines the size of the controlRectangle to draw the widget in.
    public override Size Size =>
      new Size(Global_Proc.UiAdjust(80), (int)Global_Proc.UiAdjust(80 * (float)Math.Sqrt(3) / 2f));

    public override int Padding => 50;

    public override bool TooltipEnabled => true;

    private SizeAnalyzerSearchDialog SearchDialog
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

    private void ShowSearchDialog()
    {
      SearchDialog.Show();
      if (!SearchDialog.Focused)
        SearchDialog.Focus();
    }

    public override bool Contains(Point ptControl, PointF ptCanvas)
    {
      // Check if mouse is inside fixed widget area
      return _widgetArea.Contains(ptControl) ||
             // If not check the drawn icons.
             _drawnIcons.Any(p => DrawUtils.GetParamIconRectangleF(p, Radius).Contains(ptCanvas));
    }

    public override void SetupTooltip(PointF canvasPoint, GH_TooltipDisplayEventArgs e)
    {
      var param = _drawnIcons.FirstOrDefault(p => DrawUtils.GetParamIconRectangleF(p, Radius).Contains(canvasPoint));
      base.SetupTooltip(canvasPoint, e);

      if (param == null)
      {
        var total = Watcher.Calculator.GetTotal();
        e.Description =
          $"Document Threshold = {Settings.GlobalThreshold}mb\nTotal Internal Data size: {Math.Round(total, 2)}mb";
        return;
      }

      var task = Watcher.Calculator.Get(param);
      if (task is { IsCompleted: false }) return;

      e.Title = "Warning: Internal data is too big";
      e.Text = "This parameter's data is TOO BIG.";
      e.Description = $"Data size = {Math.Round(task.Result, 2)}mb\nThreshold = {Settings.ParamThreshold}mb";
    }

    public override bool IsTooltipRegion(PointF canvasCoordinate)
    {
      var isTooltipRegion = base.IsTooltipRegion(canvasCoordinate);

      return isTooltipRegion;
    }

    #region Context-menu

    public override void AppendToMenu(ToolStripDropDownMenu menu)
    {
      base.AppendToMenu(menu);

      GH_DocumentObject.Menu_AppendItem(menu, "Open search dialog", (o, e) => { ShowSearchDialog(); });

      GH_DocumentObject.Menu_AppendSeparator(menu);

      CreateContextMenuSettings(
        menu,
        "Document threshold",
        Settings.GlobalThreshold,
        new List<double> { 1, 2, 5, 10 },
        (value) => Settings.GlobalThreshold = value
      );

      CreateContextMenuSettings(
        menu,
        "Parameter threshold",
        Settings.ParamThreshold,
        new List<double> { 0.1, 0.2, 0.5, 1 },
        (value) => Settings.ParamThreshold = value
      );

      GH_DocumentObject.Menu_AppendSeparator(menu);

      CreateContextMenuCornerSettings(menu);

      //CreateContextMenuSerialisationSettings(menu);
    }

    private static void CreateContextMenuSerialisationSettings(ToolStrip menu)
    {
      var parent = GH_DocumentObject.Menu_AppendItem(menu, "Serialisation Type");
      GH_DocumentObject.Menu_AppendItem(
        parent.DropDown,
        "Binary (.gh)",
        (sender, args) => Settings.SerializationType = SerializationType.Binary,
        null,
        true,
        Settings.SerializationType == SerializationType.Binary);
      GH_DocumentObject.Menu_AppendItem(
        parent.DropDown,
        "XML (.gha)",
        (sender, args) => Settings.SerializationType = SerializationType.Xml,
        null,
        true,
        Settings.SerializationType == SerializationType.Xml);
    }

    private void CreateContextMenuCornerSettings(ToolStrip menu)
    {
      var posMenu = GH_DocumentObject.Menu_AppendItem(menu, "Doc Warning Position");
      var options = new List<(string, Corner)>
      {
        ("Top Left", Corner.TopLeft),
        ("Top Right", Corner.TopRight),
        ("Bottom Left", Corner.BottomLeft),
        ("Bottom Right", Corner.BottomRight),
      };
      foreach (var (name, corner) in options)
      {
        GH_DocumentObject.Menu_AppendItem(posMenu.DropDown, name,
          (sender, args) => WidgetCornerChanged(corner),
          null,
          true,
          Settings.Corner == corner
        );
      }
    }

    private static void CreateContextMenuSettings(ToolStrip menu, string name,
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

    #endregion

    #region Render logic

    public override void Render(GH_Canvas canvas)
    {
      if (canvas.Document == null) return;
      if (!Settings.ShowParamWarnings) return;

      // Draw here anything that has to be drawn "per-component" (i.e. the bubbles with the size)
      _drawnIcons = new List<IGH_Param>();
      DrawUtils.GetAllParamsWithLocalData(canvas.Document)
        .ToList()
        .ForEach(p => DrawParam(canvas, p));

      // Call the base render to continue drawing the "fixed" part of the widget
      base.Render(canvas);
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
      DrawUtils.DrawWarningTriangle(controlFrame, graphics);

      // Once done, we reset the transform of the canvas.
      graphics.Transform = transform;
    }

    private void DrawParam(GH_Canvas canvas, IGH_Param p)
    {
      var status = Watcher.Calculator.GetParamStatus(p);
      switch (status)
      {
        case ParamStatus.Loading:
          DrawParam_Loading(canvas, p);
          break;
        case ParamStatus.OverThreshold:
          DrawParam_Warning(canvas, p);
          break;
        case ParamStatus.UnderThreshold:
        case ParamStatus.Untracked:
        case ParamStatus.Error:
        default:
          break;
      }
    }

    private void DrawParam_Warning(GH_Canvas canvas, IGH_Param p)
    {
      if (GH_Canvas.ZoomFadeLow < 255)
        DrawUtils.DrawParamIcon_ZoomedOut(canvas, p);

      DrawUtils.DrawParamIcon(canvas, p, Radius);
      _drawnIcons.Add(p);
    }

    private void DrawParam_Loading(GH_Canvas canvas, IGH_Param p)
    {
      if (GH_Canvas.ZoomFadeHigh == 0) return;

      DrawUtils.DrawLoadingIcon(canvas, p, Radius);
      _drawnIcons.Add(p);
    }

    #endregion

    #region Events

    private void WidgetCornerChanged(Corner corner)
    {
      Ratio = corner switch
      {
        Corner.TopLeft => new SizeF(0f, 0f),
        Corner.TopRight => new SizeF(1f, 0f),
        Corner.BottomLeft => new SizeF(0f, 1f),
        Corner.BottomRight => new SizeF(1f, 1f),
        _ => throw new ArgumentOutOfRangeException(nameof(corner), corner,
          "Corner should be an int from 0 to 3 (TL,TR,BL,BR)")
      };

      Settings.Corner = corner;
      Instances.InvalidateCanvas();
    }

    public void OnDocumentChanged(GH_Canvas c, GH_CanvasDocumentChangedEventArgs ce)
    {
      if (ce.OldDocument != null)
      {
        if (Watcher.IsWatching)
          Watcher.Stop();
        Watcher.Document = null;
      }


      Watcher.Document = ce.NewDocument;
      if (ce.NewDocument == null) return;
      Watcher.Start();
    }

    #endregion
  }

  public enum Corner
  {
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
  }
}