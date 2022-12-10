

using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry;
using Grasshopper.Kernel.Geometry.Delaunay;
using Grasshopper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Grasshopper.GUI;
using System.Linq;

namespace SizeAnalyzer
{
  /// <exclude />
  public class GH_LocateBigInternalDataObjectsDialog : Form
  {
    private IContainer components;
    private GH_DoubleBufferedPanel _pnlList;
    private Panel _pnlContent;
    private Panel _Panel2;
    private GH_Canvas _canvas;
    private readonly List<SearchHit> _hits;
    private static readonly int ItemHeight;
    private int m_selected;

    static GH_LocateBigInternalDataObjectsDialog()
    {
      ItemHeight = Global_Proc.UiAdjust(27);
    }

    public GH_LocateBigInternalDataObjectsDialog()
    {
      Load += GH_FindObjectDialog_Load;
      FormClosed += GH_FindObjectDialog_FormClosed;
      LocationChanged += GH_FindObjectDialog_LocationChanged;
      _hits = new List<SearchHit>();
      m_selected = -1;
      InitializeComponent();
    }

    protected override void Dispose(bool disposing)
    {
      try
      {
        if (!disposing || components == null)
          return;
        components.Dispose();
      }
      finally
      {
        base.Dispose(disposing);
      }
    }

    private void InitializeComponent()
    {
      pnlContent = new Panel();
      pnlList = new GH_DoubleBufferedPanel();
      Panel2 = new Panel();
      pnlContent.SuspendLayout();
      SuspendLayout();
      pnlContent.AutoScroll = true;
      pnlContent.BackgroundImageLayout = ImageLayout.Center;
      pnlContent.BorderStyle = BorderStyle.Fixed3D;
      pnlContent.Controls.Add((Control)pnlList);
      pnlContent.Dock = DockStyle.Fill;
      pnlContent.Location = new Point(0, 32);
      pnlContent.Name = "pnlContent";
      pnlContent.Size = new Size(193, 209);
      pnlContent.TabIndex = 4;
      pnlList.Dock = DockStyle.Top;
      pnlList.Location = new Point(0, 0);
      pnlList.Name = "pnlList";
      pnlList.Size = new Size(189, 41);
      pnlList.TabIndex = 0;
      Panel2.Dock = DockStyle.Top;
      Panel2.Location = new Point(0, 22);
      Panel2.Name = "Panel2";
      Panel2.Size = new Size(193, 10);
      Panel2.TabIndex = 5;
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(193, 241);
      Controls.Add((Control)pnlContent);
      Controls.Add((Control)Panel2);
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      MaximizeBox = false;
      MinimizeBox = false;
      Name = nameof(GH_LocateBigInternalDataObjectsDialog);
      ShowIcon = true;
      TopMost = true;
      // Get an Hicon for myBitmap.
      IntPtr Hicon = Properties.Resources.CalculatorIcon.GetHicon();
      // Create a new icon from the handle. 
      Icon = Icon.FromHandle(Hicon);
      ShowInTaskbar = false;
      SizeGripStyle = SizeGripStyle.Hide;
      StartPosition = FormStartPosition.Manual;
      Text = "Size Analyzer - Find";
      pnlContent.ResumeLayout(false);
      ResumeLayout(false);
      PerformLayout();
    }

    internal virtual GH_DoubleBufferedPanel pnlList
    {
      get
      {
        return _pnlList;
      }
      set
      {
        MouseEventHandler mouseEventHandler1 = pnlList_MouseClick;
        MouseEventHandler mouseEventHandler2 = pnlList_MouseMove;
        PaintEventHandler paintEventHandler = pnlList_Paint;
        GH_DoubleBufferedPanel pnlList1 = _pnlList;
        if (pnlList1 != null)
        {
          pnlList1.MouseClick -= mouseEventHandler1;
          pnlList1.MouseMove -= mouseEventHandler2;
          pnlList1.Paint -= paintEventHandler;
        }
        _pnlList = value;
        GH_DoubleBufferedPanel pnlList2 = _pnlList;
        if (pnlList2 == null)
          return;
        pnlList2.MouseClick += mouseEventHandler1;
        pnlList2.MouseMove += mouseEventHandler2;
        pnlList2.Paint += paintEventHandler;
      }
    }

    internal virtual Panel pnlContent
    {
      get
      {
        return _pnlContent;
      }
      set
      {
        _pnlContent = value;
      }
    }

    internal virtual Panel Panel2
    {
      get
      {
        return _Panel2;
      }
      set
      {
        _Panel2 = value;
      }
    }

    private void GH_FindObjectDialog_Load(object sender, EventArgs e)
    {
      GH_WindowsControlUtil.FixTextRenderingDefault(Controls);
    }

    private void GH_FindObjectDialog_FormClosed(object sender, FormClosedEventArgs e)
    {
      UnregisterAllEvents();
      if (_canvas == null)
        return;
      _canvas.Refresh();
    }

    public GH_Canvas Canvas
    {
      get
      {
        return _canvas;
      }
      set
      {
        if (_canvas != null)
        {
          UnregisterAllEvents();
          _canvas = (GH_Canvas)null;
        }
        _canvas = value;
        RegisterAllEvents();
        NewSearch();
      }
    }

    private void RegisterAllEvents()
    {
      if (_canvas == null)
        return;
      Form form = _canvas.FindForm();
      if (form != null)
      {
        form.LocationChanged += RemoteControl_LayoutChanged;
      }
      _canvas.SizeChanged += RemoteControl_LayoutChanged;
      _canvas.CanvasPrePaintWidgets += Canvas_PaintOverlay;
      _canvas.Document_ObjectsAdded += DocumentObjectsChanged;
      _canvas.Document_ObjectsDeleted += DocumentObjectsChanged;
    }

    private void UnregisterAllEvents()
    {
      if (_canvas == null)
        return;
      Form form = _canvas.FindForm();
      if (form != null)
      {
        form.LocationChanged -= RemoteControl_LayoutChanged;
      }
      _canvas.SizeChanged -= RemoteControl_LayoutChanged;
      _canvas.CanvasPrePaintWidgets -= Canvas_PaintOverlay;
      _canvas.Document_ObjectsAdded -= DocumentObjectsChanged;
      _canvas.Document_ObjectsDeleted -= DocumentObjectsChanged;
    }

    private void DocumentObjectsChanged(GH_Document sender, EventArgs e)
    {
      NewSearch();
    }

    private void NewSearch()
    {
      _hits.Clear();
      var objects = Calculator.GetParams(SizeAnalyzerWidget.SharedThreshold).ToList();
      if (_canvas == null || !_canvas.IsDocument || objects.Count == 0)
      {
        SetNoResults();
      }
      else
      {
        if (objects != null)
        {
          if (objects.Count != 0)
          {
            try
            {
              foreach (IGH_DocumentObject ghDocumentObject in objects)
                _hits.Add(new SearchHit(ghDocumentObject));
            }
            finally
            {

            }

            LayoutHits();
            pnlList.Height = _hits[_hits.Count - 1].Box.Bottom + ItemHeight;
            pnlContent.BackgroundImage = (Image)null;
            pnlList.Refresh();
            Instances.RedrawCanvas();
            return;
          }
        }
        SetNoResults();
      }
    }

    private void SetNoResults()
    {
      pnlList.Height = 0;
      pnlContent.BackgroundImageLayout = ImageLayout.Center;
      pnlContent.BackgroundImage = (Image)Properties.Resources.CalculatorIcon;
      Instances.RedrawCanvas();
    }

    private void LayoutHits()
    {
      if (_hits == null)
        return;
      int y = 0;
      int num = _hits.Count - 1;
      for (int index = 0; index <= num; ++index)
      {
        _hits[index].SetBox(new Rectangle(0, y, pnlList.Width, ItemHeight));
        y = _hits[index].Box.Bottom;
      }
    }

    private void GH_FindObjectDialog_LocationChanged(object sender, EventArgs e)
    {
      pnlList.Refresh();
    }

    private void RemoteControl_LayoutChanged(object sender, EventArgs e)
    {
      pnlList.Refresh();
    }

    private GraphicsPath HitOutline()
    {
      GraphicsPath graphicsPath;
      if (_hits == null)
        graphicsPath = (GraphicsPath)null;
      else if (_hits.Count == 0)
      {
        graphicsPath = (GraphicsPath)null;
      }
      else
      {
        List<RectangleF> content = new List<RectangleF>(_hits.Count);
        int num = _hits.Count - 1;
        for (int index = 0; index <= num; ++index)
          content.Add(_hits[index].DocObject.Attributes.Bounds);
        graphicsPath = GH_GDI_Util.Freeform_Blob(content, 20, 3.0);
      }
      return graphicsPath;
    }

    private GraphicsPath RemotePoints()
    {
      GraphicsPath graphicsPath1;
      if (_hits == null)
        graphicsPath1 = null;
      else if (_hits.Count < 3)
      {
        graphicsPath1 = null;
      }
      else
      {
        Node2List nodes = new Node2List();
        try
        {
          foreach (SearchHit hit in _hits)
          {
            RectangleF bounds = hit.DocObject.Attributes.Bounds;
            double nx = 0.5 * ((double)bounds.Left + (double)bounds.Right);
            double ny = 0.5 * ((double)bounds.Top + (double)bounds.Bottom);
            nodes.Append(new Node2(nx, ny));
          }
        }
        finally
        {
          List<SearchHit>.Enumerator enumerator;
          //enumerator.Dispose();
        }
        List<Face> faceList = Solver.Solve_Faces(nodes, 2.0);
        if (faceList == null)
          graphicsPath1 = null;
        else if (faceList.Count == 0)
        {
          graphicsPath1 = null;
        }
        else
        {
          GraphicsPath graphicsPath2 = new GraphicsPath();
          try
          {
            foreach (FaceEx faceEx in faceList)
              graphicsPath2.AddEllipse(Convert.ToSingle(faceEx.center.x) - 3f, Convert.ToSingle(faceEx.center.y) - 3f, 6f, 6f);
          }
          finally
          {
            List<Face>.Enumerator enumerator;
            //enumerator.Dispose();
          }
          graphicsPath1 = graphicsPath2;
        }
      }
      return graphicsPath1;
    }

    private void Canvas_PaintOverlay(GH_Canvas sender)
    {
      GraphicsPath path = HitOutline();
      if (path == null)
        return;
      Region region = new Region(sender.Viewport.VisibleRegion);
      region.Exclude(path);
      SolidBrush solidBrush = new SolidBrush(Color.FromArgb(150, Color.White));
      sender.Graphics.FillRegion((Brush)solidBrush, region);
      solidBrush.Dispose();
      region.Dispose();
      Pen pen = new Pen(Color.Black, 1.5f);
      pen.DashPattern = new float[2] { 3f, 2f };
      pen.DashCap = DashCap.Round;
      sender.Graphics.DrawPath(pen, path);
      pen.Dispose();
      path.Dispose();
      pnlList.Refresh();
    }

    public int SelectedHit
    {
      get
      {
        return m_selected;
      }
      set
      {
        if (m_selected == value)
          return;
        m_selected = value;
        pnlList.Refresh();
      }
    }

    private void pnlList_MouseClick(object sender, MouseEventArgs e)
    {
      if (SelectedHit < 0)
        return;
      RectangleF rectangle = (RectangleF)GH_Convert.ToRectangle(_hits[SelectedHit].DocObject.Attributes.Bounds);
      Rectangle client = _canvas.RectangleToClient(pnlContent.RectangleToScreen(_hits[SelectedHit].Box));
      Point point;
      PointF target;
      if (Math.Abs(client.Left - _canvas.Left) > Math.Abs(client.Right - _canvas.Right))
      {
        point = new Point(client.Left - 15, client.Top + client.Height / 2);
        target = new PointF(rectangle.Right + 50, (float)(0.5 * ((double)rectangle.Top + (double)rectangle.Bottom)));
      }
      else
      {
        point = new Point(client.Right + 15, client.Top + client.Height / 2);
        target = new PointF(rectangle.X - 50, (float)(0.5 * ((double)rectangle.Top + (double)rectangle.Bottom)));
      }
      new GH_NamedView(_canvas.Viewport, point, target).SetToViewport(_canvas, 200);
    }

    private void pnlList_MouseMove(object sender, MouseEventArgs e)
    {
      if (_hits == null || _hits.Count == 0)
      {
        SelectedHit = -1;
      }
      else
      {
        int num = _hits.Count - 1;
        for (int index = 0; index <= num; ++index)
        {
          if (_hits[index].Box.Contains(e.Location))
          {
            SelectedHit = index;
            break;
          }
        }
      }
    }

    private void pnlList_Paint(object sender, PaintEventArgs e)
    {
      e.Graphics.Clear(BackColor);
      if (_hits == null || _hits.Count == 0)
        return;
      LayoutHits();
      e.Graphics.TextRenderingHint = GH_TextRenderingConstants.GH_CrispText;
      e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
      e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
      int num = _hits.Count - 1;
      for (int index = 0; index <= num; ++index)
      {
        if (index != SelectedHit)
          RenderHit(e.Graphics, index);
      }
      if (SelectedHit < 0)
        return;
      RenderHit(e.Graphics, SelectedHit);
    }

    private void RenderHit(Graphics G, int index)
    {
      if (index >= _hits.Count)
        return;
      Color color1 = BackColor;
      Color color2 = Color.FromArgb(150, ForeColor);
      if (index == SelectedHit)
      {
        color1 = GH_GraphicsUtil.ScaleColour(BackColor, 1.15);
        color2 = ForeColor;
      }
      else if (index % 2 == 1)
        color1 = GH_GraphicsUtil.ScaleColour(BackColor, 0.85);
      if (_hits[index].DocObject.Attributes.Selected)
        color1 = GH_GraphicsUtil.BlendColour(color1, Color.LawnGreen, 0.5);
      SolidBrush solidBrush1 = new SolidBrush(color1);
      SolidBrush solidBrush2 = new SolidBrush(color2);
      StringFormat format = new StringFormat();
      format.Alignment = StringAlignment.Near;
      format.LineAlignment = StringAlignment.Center;
      format.Trimming = StringTrimming.EllipsisCharacter;
      format.FormatFlags = StringFormatFlags.NoWrap;
      Rectangle box = _hits[index].Box;
      if (index == SelectedHit)
      {
        GH_GraphicsUtil.ShadowHorizontal(G, 0, Width, box.Y, Global_Proc.UiAdjust(15), false, 80);
        GH_GraphicsUtil.ShadowHorizontal(G, 0, Width, box.Bottom, Global_Proc.UiAdjust(15), true, 80);
        G.FillRectangle((Brush)solidBrush1, box);
        G.DrawLine(Pens.Black, 0, box.Y, Width, box.Y);
        G.DrawLine(Pens.Black, 0, box.Bottom, Width, box.Bottom);
      }
      else
        G.FillRectangle((Brush)solidBrush1, box);
      Rectangle boxIcon = _hits[index].BoxIcon;
      ++boxIcon.Y;
      Bitmap icon = _hits[index].DocObject.Icon_24x24 ?? Properties.Resources.CalculatorIcon;
      GH_GraphicsUtil.RenderIcon(G, (RectangleF)boxIcon, (Image)icon);
      Rectangle boxText = _hits[index].BoxText;
      boxText.Inflate(-2, -1);
      string nickName = _hits[index].DocObject.NickName;
      string name = _hits[index].DocObject.Name;
      var task = Calculator.Get(_hits[index].DocObject as IGH_Param);
      var size = task.IsCompleted ? $"{Math.Round(task.Result, 2)} mb" : "loading";

      if (string.IsNullOrEmpty(nickName) || nickName.Equals(name, StringComparison.OrdinalIgnoreCase))
        G.DrawString(string.Format("{0}", (object)name), Font, (Brush)solidBrush2, (RectangleF)boxText, format);
      else
        G.DrawString(string.Format("{0} [{1}]", (object)name, (object)size), Font, (Brush)solidBrush2, (RectangleF)boxText, format);
      Rectangle boxDirection = _hits[index].BoxDirection;
      RectangleF client = (RectangleF)pnlList.RectangleToClient(GH_Convert.ToRectangle((RectangleF)_canvas.RectangleToScreen(GH_Convert.ToRectangle(_canvas.Viewport.ProjectRectangle(_hits[index].DocObject.Attributes.Bounds)))));
      DrawDirArrow(G, (RectangleF)boxDirection, client);
    }

    private void DrawDirArrow(Graphics G, RectangleF arrowBox, RectangleF targetBox)
    {
      float x = arrowBox.X + 0.5f * arrowBox.Width;
      float y = arrowBox.Y + 0.5f * arrowBox.Height;
      PointF head = GH_GraphicsUtil.BoxClosestPoint(new PointF(x, y), targetBox);
      DrawDirArrow(G, new PointF(x, y), head);
    }

    private void DrawDirArrow(Graphics G, PointF tail, PointF head)
    {
      float x = head.X - tail.X;
      double num = Math.Atan2((double)head.Y - (double)tail.Y, (double)x);
      Matrix transform = G.Transform;
      transform.Translate(tail.X, tail.Y);
      transform.Rotate(Convert.ToSingle(180.0 * num / Math.PI) - 90f);
      G.Transform = transform;
      DrawDirArrow(G);
      G.ResetTransform();
    }

    private void DrawDirArrow(Graphics G)
    {
      GraphicsPath path = new GraphicsPath();
      int num = Global_Proc.UiAdjust(3);
      path.AddLines(new Point[7]
      {
        new Point(0, 3 * num),
        new Point(2 * num, 0),
        new Point(1 * num, 0),
        new Point(1 * num, -3 * num),
        new Point(-1 * num, -3 * num),
        new Point(-1 * num, 0),
        new Point(-2 * num, 0)
      });
      path.CloseAllFigures();
      LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Point(0, 3 * num), new Point(0, -2 * num), Color.FromArgb(80, 80, 80), Color.FromArgb(0, 0, 0));
      linearGradientBrush.WrapMode = WrapMode.TileFlipXY;
      G.FillPath((Brush)linearGradientBrush, path);
      G.DrawPath(Pens.Black, path);
      path.Dispose();
      linearGradientBrush.Dispose();
    }

    private class SearchHit
    {
      public readonly IGH_DocumentObject DocObject;
      public Rectangle Box;
      public Rectangle BoxIcon;
      public Rectangle BoxText;
      public Rectangle BoxDirection;

      public SearchHit(IGH_DocumentObject obj)
      {
        DocObject = obj;
      }

      public void SetBox(Rectangle targetBox)
      {
        Box = targetBox;
        BoxIcon = targetBox;
        Rectangle local = new Rectangle(BoxIcon.Location, BoxIcon.Size);
        // ISSUE: explicit reference operation
        int num = local.X + Global_Proc.UiAdjust(3);
        local.X = num;
        BoxIcon.Width = BoxIcon.Height;
        BoxDirection = targetBox;
        BoxDirection.Width = BoxDirection.Height;
        BoxDirection.X = targetBox.Right - BoxDirection.Width;
        BoxText = Rectangle.FromLTRB(BoxIcon.Right, targetBox.Top, BoxDirection.Left, targetBox.Bottom);
      }
    }
  }
}
