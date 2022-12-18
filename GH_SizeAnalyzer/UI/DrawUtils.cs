using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using SizeAnalyzer.Properties;
using GH_DigitScroller = Grasshopper.GUI.GH_DigitScroller;

namespace SizeAnalyzer.UI
{
  public static class DrawUtils
  {
    public static void DrawLoadingIcon(GH_Canvas canvas, IGH_Param p, int radius)
    {
      var brush = Brushes.Blue;
      var bounds = p.Attributes.Bounds;
      var center = new PointF(bounds.Right - radius, bounds.Top - radius);

      if (p.Kind == GH_ParamKind.input)
      {
        center.X += 4;
        center.Y += 2;
      }

      var r = new RectangleF(center, new SizeF(radius * 2, radius * 2));
      var whitesmoke = new Pen(Color.WhiteSmoke)
      {
        Width = 2
      };
      canvas.Graphics.DrawEllipse(whitesmoke, r);
      canvas.Graphics.FillEllipse(brush, r);
    }

    public static void DrawParamIcon(GH_Canvas canvas, IGH_Param p, int radius)
    {
      var brush = Brushes.Red;
      var r = GetParamIconRectangleF(p, radius);
      var whitesmoke = new Pen(Color.WhiteSmoke)
      {
        Width = 2
      };
      canvas.Graphics.DrawEllipse(whitesmoke, r);
      canvas.Graphics.FillEllipse(brush, r);
      whitesmoke.Width = 1;
      canvas.Graphics.DrawLine(whitesmoke, r.Left + r.Width / 2, r.Top + 1, r.Left + r.Width / 2, r.Bottom - 3);
      canvas.Graphics.DrawLine(whitesmoke, r.Left + r.Width / 2, r.Bottom - 2, r.Left + r.Width / 2, r.Bottom - 1);
    }

    public static RectangleF GetParamIconRectangleF(IGH_Param p, int radius)
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

    public static void DrawParamIcon_ZoomedOut(GH_Canvas canvas, IGH_Param p)
    {
      var rect = GH_Convert.ToRectangle(p.Attributes.Bounds);
      canvas.Graphics.FillRectangle(Brushes.Red, rect);
    }

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
        Image = Resources.CalculatorIcon,
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
        MaximumValue = 100,
        MinimumValue = 1,
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

    public static GH_DigitScroller SetupMenuDigitScroller()
    {
      return new GH_DigitScroller
      {
        Height = 40,
        Width = 200,
        DecimalPlaces = 0,
        MaximumValue = 100,
        MinimumValue = 1
      };
    }
  }
}