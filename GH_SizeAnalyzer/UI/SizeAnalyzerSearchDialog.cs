using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using SizeAnalyzer.Properties;
using SizeAnalyzer.Widgets;

namespace SizeAnalyzer.UI
{
  public class SizeAnalyzerSearchDialog : Form
  {
    private static readonly int ItemHeight;
    private readonly List<SizeAnalyzerHit> _hits;
    private GH_Canvas _canvas;
    private GH_DoubleBufferedPanel _listPanel;
    private int _selected;

    static SizeAnalyzerSearchDialog()
    {
      ItemHeight = Global_Proc.UiAdjust(27);
    }

    public SizeAnalyzerSearchDialog()
    {
      Load += OnLoad;
      FormClosed += OnFormClosed;
      LocationChanged += OnLocationChanged;
      _hits = new List<SizeAnalyzerHit>();
      _selected = -1;
      InitializeComponent();
    }

    private GH_DoubleBufferedPanel ListPanel
    {
      get => _listPanel;
      set
      {
        if (_listPanel != null)
        {
          _listPanel.MouseClick -= ListPanelMouseClick;
          _listPanel.MouseMove -= ListPanelMouseMove;
          _listPanel.Paint -= ListPanelPaint;
        }

        _listPanel = value;

        if (_listPanel == null)
          return;

        _listPanel.MouseClick += ListPanelMouseClick;
        _listPanel.MouseMove += ListPanelMouseMove;
        _listPanel.Paint += ListPanelPaint;
      }
    }

    private Panel ContentPanel { get; set; }

    private Panel SeparatorPanel { get; set; }

    public GH_Canvas Canvas
    {
      get => _canvas;
      set
      {
        if (_canvas != null)
        {
          UnregisterAllEvents();
          _canvas = null;
        }

        _canvas = value;
        RegisterAllEvents();
        SetupResults();
      }
    }

    public int SelectedHit
    {
      get => _selected;
      set
      {
        if (_selected == value)
          return;
        _selected = value;
        ListPanel.Refresh();
      }
    }

    private void InitializeComponent()
    {
      ContentPanel = new Panel();
      ListPanel = new GH_DoubleBufferedPanel();
      SeparatorPanel = new Panel();
      ContentPanel.SuspendLayout();
      SuspendLayout();
      ContentPanel.AutoScroll = true;
      ContentPanel.BackgroundImageLayout = ImageLayout.Center;
      ContentPanel.BorderStyle = BorderStyle.Fixed3D;
      ContentPanel.Controls.Add(ListPanel);
      ContentPanel.Dock = DockStyle.Fill;
      ContentPanel.Location = new Point(0, 32);
      ContentPanel.Name = "pnlContent";
      ContentPanel.Size = new Size(193, 209);
      ContentPanel.TabIndex = 4;
      ListPanel.Dock = DockStyle.Top;
      ListPanel.Location = new Point(0, 0);
      ListPanel.Name = "pnlList";
      ListPanel.Size = new Size(189, 41);
      ListPanel.TabIndex = 0;
      SeparatorPanel.Dock = DockStyle.Top;
      SeparatorPanel.Location = new Point(0, 22);
      SeparatorPanel.Name = "Panel2";
      SeparatorPanel.Size = new Size(193, 10);
      SeparatorPanel.TabIndex = 5;
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(193, 241);
      Controls.Add(ContentPanel);
      Controls.Add(SeparatorPanel);
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      MaximizeBox = false;
      MinimizeBox = false;
      Name = nameof(SizeAnalyzerSearchDialog);
      ShowIcon = true;
      TopMost = true;
      var Hicon = Resources.CalculatorIcon.GetHicon();
      Icon = Icon.FromHandle(Hicon);
      ShowInTaskbar = false;
      SizeGripStyle = SizeGripStyle.Hide;
      StartPosition = FormStartPosition.Manual;
      Text = "Size Analyzer - Find";
      ContentPanel.ResumeLayout(false);
      ResumeLayout(false);
      PerformLayout();
    }

    private void OnLoad(object sender, EventArgs e)
    {
      GH_WindowsControlUtil.FixTextRenderingDefault(Controls);
    }

    private void OnFormClosed(object sender, FormClosedEventArgs e)
    {
      UnregisterAllEvents();
      _canvas?.Refresh();
    }

    private void RegisterAllEvents()
    {
      if (_canvas == null) return;
      var form = _canvas.FindForm();
      if (form != null) form.LocationChanged += OnLayoutChanged;
      _canvas.SizeChanged += OnLayoutChanged;
      _canvas.CanvasPrePaintWidgets += Canvas_PaintOverlay;
      _canvas.Document_ObjectsAdded += DocumentObjectsChanged;
      _canvas.Document_ObjectsDeleted += DocumentObjectsChanged;
    }

    private void UnregisterAllEvents()
    {
      if (_canvas == null) return;
      var form = _canvas.FindForm();
      if (form != null) form.LocationChanged -= OnLayoutChanged;
      _canvas.SizeChanged -= OnLayoutChanged;
      _canvas.CanvasPrePaintWidgets -= Canvas_PaintOverlay;
      _canvas.Document_ObjectsAdded -= DocumentObjectsChanged;
      _canvas.Document_ObjectsDeleted -= DocumentObjectsChanged;
    }

    private void DocumentObjectsChanged(GH_Document sender, EventArgs e)
    {
      SetupResults();
    }

    private void SetupResults()
    {
      _hits.Clear();
      var objects = Calculator.GetParams(Settings.ParamThreshold).ToList();
      if (!(_canvas is { IsDocument: true }) || objects.Count == 0)
      {
        SetNoResults();
      }
      else
      {
        if (objects.Count != 0)
        {
          foreach (var ghDocumentObject in objects)
            _hits.Add(new SizeAnalyzerHit(ghDocumentObject));

          LayoutHits();
          ListPanel.Height = _hits[_hits.Count - 1].Box.Bottom + ItemHeight;
          ContentPanel.BackgroundImage = null;
          ListPanel.Refresh();
          Instances.RedrawCanvas();
          return;
        }

        SetNoResults();
      }
    }

    private void SetNoResults()
    {
      ListPanel.Height = 0;
      ContentPanel.BackgroundImageLayout = ImageLayout.Center;
      ContentPanel.BackgroundImage = Resources.CalculatorIcon;
      Instances.RedrawCanvas();
    }

    private void LayoutHits()
    {
      if (_hits == null)
        return;
      var y = 0;
      var num = _hits.Count - 1;
      for (var index = 0; index <= num; ++index)
      {
        _hits[index].SetBox(new Rectangle(0, y, ListPanel.Width, ItemHeight));
        y = _hits[index].Box.Bottom;
      }
    }

    private void OnLocationChanged(object sender, EventArgs e) => ListPanel.Refresh();

    private void OnLayoutChanged(object sender, EventArgs e) => ListPanel.Refresh();

    private GraphicsPath HitOutlineSelected()
    {
      if (_hits == null || _hits.Count == 0 || _selected < 0)
        return null;

      var bounds = _hits[_selected].DocObject.Attributes.Bounds;
      return GH_GDI_Util.Freeform_Blob(new List<RectangleF> { bounds }, 10, 3.0);
    }

    private GraphicsPath HitOutline()
    {
      GraphicsPath graphicsPath;
      if (_hits == null)
      {
        graphicsPath = null;
      }
      else if (_hits.Count == 0)
      {
        graphicsPath = null;
      }
      else
      {
        var content = new List<RectangleF>(_hits.Count);
        var num = _hits.Count - 1;
        for (var index = 0; index <= num; ++index)
          content.Add(_hits[index].DocObject.Attributes.Bounds);
        graphicsPath = GH_GDI_Util.Freeform_Blob(content, 20, 3.0);
      }

      return graphicsPath;
    }

    private void Canvas_PaintOverlay(GH_Canvas sender)
    {
      var path = HitOutline();
      var selectedPath = HitOutlineSelected();

      if (!(path?.Clone() is GraphicsPath combined)) return;
      combined.FillMode = FillMode.Winding;

      if (selectedPath != null)
        combined.AddPath(selectedPath, false);
      
      // Draw the excluded region
      var region = new Region(sender.Viewport.VisibleRegion);
      region.Exclude(combined);
      var solidBrush = new SolidBrush(Color.FromArgb(150, Color.White));
      sender.Graphics.FillRegion(solidBrush, region);
      solidBrush.Dispose();
      region.Dispose();

      // Draw the black outline
      var pen = new Pen(Color.Black, 1.5f);
      pen.DashPattern = new float[2] { 3f, 2f };
      pen.DashCap = DashCap.Round;
      sender.Graphics.DrawPath(pen, path);
      pen.Dispose();
      path.Dispose();
      
      // Draw the selected outline blob
      if (selectedPath != null)
      {
        pen = new Pen(Color.Red, 3f);
        sender.Graphics.DrawPath(pen, selectedPath);
        pen.Dispose();
        selectedPath.Dispose();
      }

      ListPanel.Refresh();
    }

    private void ListPanelMouseClick(object sender, MouseEventArgs e)
    {
      if (SelectedHit < 0)
        return;
      RectangleF rectangle = GH_Convert.ToRectangle(_hits[SelectedHit].DocObject.Attributes.Bounds);
      var client = _canvas.RectangleToClient(ContentPanel.RectangleToScreen(_hits[SelectedHit].Box));
      Point point = new Point(_canvas.ClientRectangle.Left + _canvas.ClientRectangle.Width / 2,
        _canvas.ClientRectangle.Top + _canvas.ClientRectangle.Height / 2);
      PointF target;
      if (Math.Abs(client.Left - _canvas.Left) > Math.Abs(client.Right - _canvas.Right))
      {
        //point = new Point(client.Left - 15, client.Top + client.Height / 2);
        target = new PointF(rectangle.Right + 50, (float)(0.5 * (rectangle.Top + (double)rectangle.Bottom)));
      }
      else
      {
        //point = new Point(client.Right + 15, client.Top + client.Height / 2);
        target = new PointF(rectangle.X - 50, (float)(0.5 * (rectangle.Top + (double)rectangle.Bottom)));
      }

      new GH_NamedView(_canvas.Viewport, point, target).SetToViewport(_canvas, 200);
    }

    private void ListPanelMouseMove(object sender, MouseEventArgs e)
    {
      if (_hits == null || _hits.Count == 0)
      {
        SelectedHit = -1;
      }
      else
      {
        var num = _hits.Count - 1;
        for (var index = 0; index <= num; ++index)
          if (_hits[index].Box.Contains(e.Location))
          {
            SelectedHit = index;
            break;
          }
      }
    }

    private void ListPanelPaint(object sender, PaintEventArgs e)
    {
      e.Graphics.Clear(BackColor);
      if (_hits == null || _hits.Count == 0)
        return;
      LayoutHits();
      e.Graphics.TextRenderingHint = GH_TextRenderingConstants.GH_CrispText;
      e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
      e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
      var num = _hits.Count - 1;
      for (var index = 0; index <= num; ++index)
        if (index != SelectedHit)
          RenderHit(e.Graphics, index);
      if (SelectedHit < 0)
        return;
      RenderHit(e.Graphics, SelectedHit);
    }

    private void RenderHit(Graphics g, int index)
    {
      if (index >= _hits.Count)
        return;
      var color1 = BackColor;
      var color2 = Color.FromArgb(150, ForeColor);
      if (index == SelectedHit)
      {
        color1 = GH_GraphicsUtil.ScaleColour(BackColor, 1.15);
        color2 = ForeColor;
      }
      else if (index % 2 == 1)
      {
        color1 = GH_GraphicsUtil.ScaleColour(BackColor, 0.85);
      }

      if (_hits[index].DocObject.Attributes.Selected)
        color1 = GH_GraphicsUtil.BlendColour(color1, Color.LawnGreen, 0.5);
      var solidBrush1 = new SolidBrush(color1);
      var solidBrush2 = new SolidBrush(color2);
      var format = new StringFormat();
      format.Alignment = StringAlignment.Near;
      format.LineAlignment = StringAlignment.Center;
      format.Trimming = StringTrimming.EllipsisCharacter;
      format.FormatFlags = StringFormatFlags.NoWrap;
      var box = _hits[index].Box;
      if (index == SelectedHit)
      {
        GH_GraphicsUtil.ShadowHorizontal(g, 0, Width, box.Y, Global_Proc.UiAdjust(15), false, 80);
        GH_GraphicsUtil.ShadowHorizontal(g, 0, Width, box.Bottom, Global_Proc.UiAdjust(15), true, 80);
        g.FillRectangle(solidBrush1, box);
        g.DrawLine(Pens.Black, 0, box.Y, Width, box.Y);
        g.DrawLine(Pens.Black, 0, box.Bottom, Width, box.Bottom);
      }
      else
      {
        g.FillRectangle(solidBrush1, box);
      }

      var boxIcon = _hits[index].BoxIcon;
      ++boxIcon.Y;
      var icon = _hits[index].DocObject.Icon_24x24 ?? Resources.CalculatorIcon;
      GH_GraphicsUtil.RenderIcon(g, boxIcon, icon);
      var boxText = _hits[index].BoxText;
      boxText.Inflate(-2, -1);
      var nickName = _hits[index].DocObject.NickName;
      var name = _hits[index].DocObject.Name;
      var task = Calculator.Get(_hits[index].DocObject as IGH_Param);
      var size = task.IsCompleted ? $"{Math.Round(task.Result, 2)} mb" : "loading";

      if (string.IsNullOrEmpty(nickName) || nickName.Equals(name, StringComparison.OrdinalIgnoreCase))
        g.DrawString($"{name}", Font, solidBrush2, boxText, format);
      else
        g.DrawString($"{name} [{size}]", Font, solidBrush2, boxText, format);
      var boxDirection = _hits[index].BoxDirection;
      RectangleF client = ListPanel.RectangleToClient(GH_Convert.ToRectangle(
        _canvas.RectangleToScreen(
          GH_Convert.ToRectangle(_canvas.Viewport.ProjectRectangle(_hits[index].DocObject.Attributes.Bounds)))));
      DrawUtils.DrawDirArrow(g, boxDirection, client);
    }
  }
}