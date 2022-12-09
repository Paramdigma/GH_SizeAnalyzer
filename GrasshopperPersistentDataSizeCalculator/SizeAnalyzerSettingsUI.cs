
using Grasshopper.GUI;
using Grasshopper.GUI.Base;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SizeAnalyzer
{
  public class SizeAnalyzerSettingsUI : IGH_SettingFrontend
  {
    public string Category => "Widgets";

    public string Name => "Size analyzer widget";

    public IEnumerable<string> Keywords => new List<string> { "Widget", "Size", "Profiler", "Analyzer" };

    public Control SettingsUI() => new SizeAnalyzerSettingsFrontEnd();
  }

  public class SizeAnalyzerSettingsFrontEnd : UserControl
  {
    private IContainer components;
    [CompilerGenerated]
    [AccessedThroughProperty("TableLayoutPanel1")]
    private TableLayoutPanel _TableLayoutPanel1;
    [CompilerGenerated]
    [AccessedThroughProperty("checkShow")]
    private CheckBox _checkShow;
    [CompilerGenerated]
    [AccessedThroughProperty("digitThreshold")]
    private GH_DigitScroller _digitThreshold;
    [CompilerGenerated]
    [AccessedThroughProperty("ToolTip")]
    private ToolTip _ToolTip;
    [CompilerGenerated]
    [AccessedThroughProperty("GH_Label1")]
    private GH_Label _GH_Label1;

    public SizeAnalyzerSettingsFrontEnd()
    {
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
      components = (IContainer)new System.ComponentModel.Container();
      TableLayoutPanel1 = new TableLayoutPanel();
      checkShow = new CheckBox();
      digitThreshold = new GH_DigitScroller();
      GH_Label1 = new GH_Label();
      ToolTip = new ToolTip(components);
      TableLayoutPanel1.SuspendLayout();
      SuspendLayout();
      TableLayoutPanel1.ColumnCount = 2;
      TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
      TableLayoutPanel1.Controls.Add((Control)checkShow, 1, 0);
      TableLayoutPanel1.Controls.Add((Control)digitThreshold, 1, 1);
      TableLayoutPanel1.Controls.Add((Control)GH_Label1, 0, 0);
      TableLayoutPanel1.Dock = DockStyle.Fill;
      TableLayoutPanel1.Location = new Point(0, 0);
      TableLayoutPanel1.Margin = new Padding(24, 23, 24, 23);
      TableLayoutPanel1.Name = "TableLayoutPanel1";
      TableLayoutPanel1.RowCount = 3;
      TableLayoutPanel1.RowStyles.Add(new RowStyle());
      TableLayoutPanel1.RowStyles.Add(new RowStyle());
      TableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
      TableLayoutPanel1.Size = new Size(514, 75);
      TableLayoutPanel1.TabIndex = 2;
      checkShow.Dock = DockStyle.Fill;
      checkShow.Location = new Point(38, 0);
      checkShow.Margin = new Padding(6, 0, 0, 0);
      checkShow.Name = "checkShow";
      checkShow.Size = new Size(476, 32);
      checkShow.TabIndex = 0;
      checkShow.Text = "Show Profiler widget";
      checkShow.UseVisualStyleBackColor = true;
      digitThreshold.AllowRadixDrag = true;
      digitThreshold.AllowTextInput = true;
      digitThreshold.DecimalPlaces = 0;
      digitThreshold.Digits = 5;
      digitThreshold.Dock = DockStyle.Fill;
      digitThreshold.Location = new Point(38, 32);
      digitThreshold.Margin = new Padding(6, 0, 0, 0);
      digitThreshold.MaximumValue = new Decimal(new int[4]
      {
        10000,
        0,
        0,
        0
      });
      digitThreshold.MinimumValue = new Decimal(new int[4]);
      digitThreshold.Name = "digitThreshold";
      digitThreshold.Prefix = "Threshold";
      digitThreshold.Radix = -1;
      digitThreshold.ShowTextInputOnDoubleClick = true;
      digitThreshold.ShowTextInputOnKeyDown = true;
      digitThreshold.Size = new Size(450, 28);
      digitThreshold.Suffix = "megabytes";
      digitThreshold.TabIndex = 5;
      ToolTip.SetToolTip((Control)digitThreshold, "Set the duration in milliseconds under which profiler tags are not displayed");
      digitThreshold.Value = new Decimal(new int[4]);
      GH_Label1.Dock = DockStyle.Fill;
      GH_Label1.Image = Properties.Resources.CalculatorIcon;
      GH_Label1.ImageAlign = ContentAlignment.MiddleCenter;
      GH_Label1.Location = new Point(0, 0);
      GH_Label1.Margin = new Padding(0);
      GH_Label1.Name = "GH_Label1";
      GH_Label1.Size = new Size(32, 32);
      GH_Label1.TabIndex = 6;
      GH_Label1.Text = (string)null;
      GH_Label1.TextAlign = ContentAlignment.MiddleCenter;
      ToolTip.AutoPopDelay = 32000;
      ToolTip.InitialDelay = 500;
      ToolTip.ReshowDelay = 100;
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      Controls.Add((Control)TableLayoutPanel1);
      Margin = new Padding(24, 23, 24, 23);
      Name = nameof(SizeAnalyzerSettingsUI);
      Size = new Size(514, 75);
      TableLayoutPanel1.ResumeLayout(false);
      ResumeLayout(false);
    }

    internal virtual TableLayoutPanel TableLayoutPanel1
    {
      [CompilerGenerated]
      get
      {
        return _TableLayoutPanel1;
      }
      [CompilerGenerated, MethodImpl(MethodImplOptions.Synchronized)]
      set
      {
        _TableLayoutPanel1 = value;
      }
    }

    internal virtual CheckBox checkShow
    {
      [CompilerGenerated]
      get
      {
        return _checkShow;
      }
      [CompilerGenerated, MethodImpl(MethodImplOptions.Synchronized)]
      set
      {
        CheckBox checkShow1 = _checkShow;
        if (checkShow1 != null)
          checkShow1.CheckedChanged -= checkShow_CheckedChanged;
        _checkShow = value;
        CheckBox checkShow2 = _checkShow;
        if (checkShow2 == null)
          return;
        checkShow2.CheckedChanged += checkShow_CheckedChanged;
      }
    }

    internal virtual GH_DigitScroller digitThreshold
    {
      [CompilerGenerated]
      get
      {
        return _digitThreshold;
      }
      [CompilerGenerated, MethodImpl(MethodImplOptions.Synchronized)]
      set
      {
        GH_DigitScroller digitThreshold1 = _digitThreshold;
        if (digitThreshold1 != null)
          digitThreshold1.ValueChanged -= digitThreshold_ValueChanged;
        _digitThreshold = value;
        GH_DigitScroller digitThreshold2 = _digitThreshold;
        if (digitThreshold2 == null)
          return;
        digitThreshold2.ValueChanged += digitThreshold_ValueChanged;
      }
    }

    internal virtual ToolTip ToolTip
    {
      [CompilerGenerated]
      get
      {
        return _ToolTip;
      }
      [CompilerGenerated, MethodImpl(MethodImplOptions.Synchronized)]
      set
      {
        _ToolTip = value;
      }
    }

    internal virtual GH_Label GH_Label1
    {
      [CompilerGenerated]
      get
      {
        return _GH_Label1;
      }
      [CompilerGenerated, MethodImpl(MethodImplOptions.Synchronized)]
      set
      {
        _GH_Label1 = value;
      }
    }

    private void SizeAnalyzerWidgetFrontEnd_Load(object sender, EventArgs e)
    {
      ProfilerVisibleChanged();
      ProfilerThresholdChanged();
      SizeAnalyzerWidget.WidgetVisibleChanged += ProfilerVisibleChanged;
      SizeAnalyzerWidget.WidgetThresholdChanged += ProfilerThresholdChanged;
    }

    private void SizeAnalyzerWidgetFrontEnd_HandleDestroyed(object sender, EventArgs e)
    {
      SizeAnalyzerWidget.WidgetVisibleChanged -= ProfilerVisibleChanged;
      SizeAnalyzerWidget.WidgetThresholdChanged -= ProfilerThresholdChanged;
    }

    private void ProfilerVisibleChanged()
    {
      checkShow.Checked = SizeAnalyzerWidget.SharedVisible;
    }

    private void ProfilerThresholdChanged()
    {
      digitThreshold.Value = new decimal(SizeAnalyzerWidget.SharedThreshold);
    }

    private void checkShow_CheckedChanged(object sender, EventArgs e)
    {
      SizeAnalyzerWidget.SharedVisible = checkShow.Checked;
      Grasshopper.Instances.RedrawCanvas();
    }

    private void digitThreshold_ValueChanged(object sender, GH_DigitScrollerEventArgs e)
    {
      SizeAnalyzerWidget.SharedThreshold = Convert.ToDouble(digitThreshold.Value);
      Grasshopper.Instances.RedrawCanvas();
    }
  }
}
