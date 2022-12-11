using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;

namespace SizeAnalyzer.UI
{
  public static class DrawUtils
  {
    public static void DrawDirArrow(Graphics g, RectangleF arrowBox, RectangleF targetBox)
    {
      var x = arrowBox.X + 0.5f * arrowBox.Width;
      var y = arrowBox.Y + 0.5f * arrowBox.Height;
      var head = GH_GraphicsUtil.BoxClosestPoint(new PointF(x, y), targetBox);
      DrawDirArrow(g, new PointF(x, y), head);
    }

    public static void DrawDirArrow(Graphics g, PointF tail, PointF head)
    {
      var x = head.X - tail.X;
      var num = Math.Atan2(head.Y - (double)tail.Y, x);
      var transform = g.Transform;
      transform.Translate(tail.X, tail.Y);
      transform.Rotate(Convert.ToSingle(180.0 * num / Math.PI) - 90f);
      g.Transform = transform;
      DrawDirArrow(g);
      g.ResetTransform();
    }

    public static void DrawDirArrow(Graphics g)
    {
      var path = new GraphicsPath();
      var num = Global_Proc.UiAdjust(3);
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
      var linearGradientBrush = new LinearGradientBrush(new Point(0, 3 * num), new Point(0, -2 * num),
        Color.FromArgb(80, 80, 80), Color.FromArgb(0, 0, 0));
      linearGradientBrush.WrapMode = WrapMode.TileFlipXY;
      g.FillPath(linearGradientBrush, path);
      g.DrawPath(Pens.Black, path);
      path.Dispose();
      linearGradientBrush.Dispose();
    }
    
    public static CheckBox SetupCheckbox(string name, string text)
    {
      var checkbox = new CheckBox();
      checkbox.Dock = DockStyle.Fill;
      checkbox.Margin = new Padding(6, 4, 0, 0);
      checkbox.Name = name;
      checkbox.AutoSize = true;
      checkbox.Text = text;
      checkbox.UseVisualStyleBackColor = true;
      return checkbox;
    }

    public static GH_Label SetupLabel()
    {
      var label = new GH_Label();
      label.Dock = DockStyle.Fill;
      label.Image = Properties.Resources.CalculatorIcon;
      label.ImageAlign = ContentAlignment.MiddleCenter;
      label.Location = new Point(0, 0);
      label.Margin = new Padding(0);
      label.Name = "icon";
      label.Size = new Size(32, 32);
      label.Text = null;
      label.TextAlign = ContentAlignment.MiddleCenter;
      return label;
    }

    public static GH_DigitScroller SetupDigitScroller(string name, string prefix, string suffix)
    {
      var scroller = new GH_DigitScroller();
      scroller.AllowRadixDrag = true;
      scroller.AllowTextInput = true;
      scroller.DecimalPlaces = 2;
      scroller.Digits = 3;
      scroller.Dock = DockStyle.Fill;
      scroller.Margin = new Padding(24, 0, 0, 0);
      scroller.MaximumValue = new decimal(100.00);
      scroller.MinimumValue = new decimal(1.00);
      scroller.Name = name;
      scroller.Prefix = prefix;
      scroller.Radix = -1;
      scroller.ShowTextInputOnDoubleClick = true;
      scroller.ShowTextInputOnKeyDown = true;
      scroller.Size = new Size(450, 28);
      scroller.Suffix = suffix;
      scroller.TabIndex = 5;
      scroller.Value = new decimal(new int[4]);
      return scroller;
    }

  }
}