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
              Color.FromArgb(80, 80, 80), Color.FromArgb(0, 0, 0))
            {
                WrapMode = WrapMode.TileFlipXY
            };
            g.FillPath(linearGradientBrush, path);
            g.DrawPath(Pens.Black, path);
            path.Dispose();
            linearGradientBrush.Dispose();
        }

        public static CheckBox SetupCheckbox(string name, string text)
        {
            var checkbox = new CheckBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 4, 0, 0),
                Name = name,
                AutoSize = true,
                Text = text,
                UseVisualStyleBackColor = true
            };
            return checkbox;
        }

        public static GH_Label SetupLabel()
        {
            var label = new GH_Label
            {
                Dock = DockStyle.Fill,
                Image = Properties.Resources.CalculatorIcon,
                ImageAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 0),
                Margin = new Padding(0),
                Name = "icon",
                Size = new Size(32, 32),
                Text = null,
                TextAlign = ContentAlignment.MiddleCenter
            };
            return label;
        }

        public static GH_DigitScroller SetupDigitScroller(string name, string prefix, string suffix)
        {
            var scroller = new GH_DigitScroller
            {
                AllowRadixDrag = true,
                AllowTextInput = true,
                DecimalPlaces = 2,
                Digits = 3,
                Dock = DockStyle.Fill,
                Margin = new Padding(24, 0, 0, 0),
                MaximumValue = new decimal(100.00),
                MinimumValue = new decimal(1.00),
                Name = name,
                Prefix = prefix,
                Radix = -1,
                ShowTextInputOnDoubleClick = true,
                ShowTextInputOnKeyDown = true,
                Size = new Size(450, 28),
                Suffix = suffix,
                TabIndex = 5,
                Value = new decimal(new int[4])
            };
            return scroller;
        }

    }
}