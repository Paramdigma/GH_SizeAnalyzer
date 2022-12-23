using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Base;

namespace SizeAnalyzer.UI
{
  public class SizeAnalyzerSettingsUI : UserControl
  {
    private CheckBox? _checkShow;
    private CheckBox? _checkShowGlobal;
    private CheckBox? _checkShowParams;

    private GH_DigitScroller? _digitThreshold;
    private GH_DigitScroller? _digitThresholdGlobal;
    private IContainer? components;
    private GH_Label? label;
    private TableLayoutPanel? panel;
    private ToolTip? toolTip;

    public SizeAnalyzerSettingsUI()
    {
      Load += SizeAnalyzerWidgetFrontEnd_Load;
      Disposed += SizeAnalyzerWidgetFrontEnd_HandleDestroyed;
      InitializeComponent();
    }

    private GH_DigitScroller? digitThresholdGlobal
    {
      get => _digitThresholdGlobal;
      set
      {
        if (_digitThresholdGlobal != null)
          _digitThresholdGlobal.ValueChanged -= digitThresholdGlobal_ValueChanged;
        _digitThresholdGlobal = value;
        if (_digitThresholdGlobal == null)
          return;
        _digitThresholdGlobal.ValueChanged += digitThresholdGlobal_ValueChanged;
      }
    }


    private CheckBox? checkShow
    {
      get => _checkShow;
      set
      {
        if (_checkShow != null)
          _checkShow.CheckedChanged -= checkShow_ValueChanged;
        _checkShow = value;
        if (_checkShow == null)
          return;
        _checkShow.CheckedChanged += checkShow_ValueChanged;
      }
    }

    private CheckBox? checkShowGlobal
    {
      get => _checkShowGlobal;
      set
      {
        if (_checkShowGlobal != null)
          _checkShowGlobal.CheckedChanged -= checkShowGlobal_ValueChanged;
        _checkShowGlobal = value;
        if (_checkShowGlobal == null)
          return;
        _checkShowGlobal.CheckedChanged += checkShowGlobal_ValueChanged;
      }
    }

    private CheckBox? checkShowParams
    {
      get => _checkShowParams;
      set
      {
        if (_checkShowParams != null)
          _checkShowParams.CheckedChanged -= checkShowParams_ValueChanged;
        _checkShowParams = value;
        if (_checkShowParams == null)
          return;
        _checkShowParams.CheckedChanged += checkShowParams_ValueChanged;
      }
    }

    internal virtual GH_DigitScroller? digitThreshold
    {
      get => _digitThreshold;
      set
      {
        if (_digitThreshold != null)
          _digitThreshold.ValueChanged -= digitThreshold_ValueChanged;
        _digitThreshold = value;
        if (_digitThreshold == null)
          return;
        _digitThreshold.ValueChanged += digitThreshold_ValueChanged;
      }
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
      components = new Container();
      panel = new TableLayoutPanel();

      // Start the layout
      panel.SuspendLayout();
      SuspendLayout();

      // Initialize inner controls
      checkShow = SetupVisibilityCheckbox();
      checkShowGlobal = SetupCheckboxShowGlobal();
      checkShowParams = SetupCheckboxShowParams();
      digitThreshold = DrawUtils.SetupDigitScroller("digitThreshold", "Node size Threshold", "megabytes");
      digitThreshold.Value = new decimal(Settings.ParamThreshold);
      digitThresholdGlobal =
        DrawUtils.SetupDigitScroller("digitThresholdGlobal", "Document size threshold", "megabytes");
      digitThresholdGlobal.Value = new decimal(Settings.GlobalThreshold);
      label = DrawUtils.SetupLabel();

      // Setup tooltips
      toolTip = new ToolTip(components);
      SetupToolTip(digitThreshold, "Set the size in mb under which size analyzer warnings are not displayed");

      // Setup panel
      SetupPanel();
      panel.Controls.Add(checkShow, 1, 0);
      panel.Controls.Add(checkShowGlobal, 1, 1);
      panel.Controls.Add(digitThresholdGlobal, 1, 2);
      panel.Controls.Add(checkShowParams, 1, 3);
      panel.Controls.Add(digitThreshold, 1, 4);
      panel.Controls.Add(label, 0, 0);
      Controls.Add(panel);

      // Set instance properties
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      Margin = new Padding(24, 23, 24, 23);
      Name = nameof(Settings);
      Size = new Size(450, 138);

      // Resume layout
      panel.ResumeLayout(false);
      ResumeLayout(false);
    }

    private void SetupPanel()
    {
      panel.ColumnCount = 2;
      panel.ColumnStyles.Add(new ColumnStyle());
      panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
      panel.Dock = DockStyle.Fill;
      panel.Location = new Point(0, 0);
      panel.Margin = new Padding(24, 23, 24, 23);
      panel.Name = "Panel";
      panel.RowCount = 5;
      panel.RowStyles.Add(new RowStyle());
      panel.TabIndex = 2;
      panel.Size = new Size(450, 125);
    }

    private void SetupToolTip(Control control, string text)
    {
      toolTip.AutoPopDelay = 32000;
      toolTip.InitialDelay = 500;
      toolTip.ReshowDelay = 100;
      toolTip.SetToolTip(control, text);
    }

    private static CheckBox SetupVisibilityCheckbox()
    {
      var box = DrawUtils.SetupCheckbox("checkVisibility", "Show Size Analyzer widget");
      box.Checked = Settings.Show;
      return box;
    }

    private static CheckBox SetupCheckboxShowParams()
    {
      var box = DrawUtils.SetupCheckbox("checkShowParams", "Display warnings on each parameter");
      box.Checked = Settings.ShowParamWarnings;
      return box;
    }

    private static CheckBox SetupCheckboxShowGlobal()
    {
      var box = DrawUtils.SetupCheckbox("checkShowGlobal", "Display global warnings");
      box.Checked = Settings.ShowGlobalWarnings;
      return box;
    }

    private void OnWidgetGlobalThresholdChanged(double value)
    {
      if (digitThresholdGlobal == null) return;
      digitThresholdGlobal.ValueChanged -= OnGlobalThresholdChanged;
      digitThresholdGlobal.Value = Convert.ToDecimal(value);
      digitThresholdGlobal.ValueChanged += OnGlobalThresholdChanged;
    }

    private void OnWidgetShowParamWarningsChanged(bool show)
    {
      checkShowParams.CheckedChanged -= checkShowParams_ValueChanged;
      checkShowParams.Checked = show;
      checkShowParams.CheckedChanged += checkShowParams_ValueChanged;
    }

    private void OnWidgetShowGlobalWarningsChanged(bool show)
    {
      checkShowGlobal.CheckedChanged -= checkShowGlobal_ValueChanged;
      checkShowGlobal.Checked = show;
      checkShowGlobal.CheckedChanged += checkShowGlobal_ValueChanged;
    }

    private void OnVisibleChanged(bool visible)
    {
      checkShow.Checked = visible;
    }

    private void OnThresholdChanged(double value)
    {
      digitThreshold.Value = new decimal(value);
    }

    private void OnGlobalThresholdChanged(object sender, GH_DigitScrollerEventArgs e)
    {
      if (e.Intermediate) return;
      Settings.GlobalThresholdChanged -= OnWidgetGlobalThresholdChanged;
      Settings.GlobalThreshold = Convert.ToDouble(e.Value);
      Settings.GlobalThresholdChanged += OnWidgetGlobalThresholdChanged;
    }

    private void checkShowParams_ValueChanged(object sender, EventArgs e)
    {
      Settings.ShowParamWarningsChanged -= OnWidgetShowParamWarningsChanged;
      Settings.ShowParamWarnings = checkShowParams.Checked;
      Settings.ShowParamWarningsChanged += OnWidgetShowParamWarningsChanged;
    }

    private void checkShowGlobal_ValueChanged(object sender, EventArgs e)
    {
      Settings.ShowGlobalWarningsChanged -= OnWidgetShowGlobalWarningsChanged;
      Settings.ShowGlobalWarnings = checkShowGlobal.Checked;
      Settings.ShowGlobalWarningsChanged += OnWidgetShowGlobalWarningsChanged;
    }

    private void checkShow_ValueChanged(object sender, EventArgs e)
    {
      Settings.Show = checkShow.Checked;
      Instances.RedrawCanvas();
    }

    private void digitThreshold_ValueChanged(object sender, GH_DigitScrollerEventArgs e)
    {
      if (e.Intermediate) return;
      Settings.ParamThreshold = Convert.ToDouble(digitThreshold.Value);
      Instances.RedrawCanvas();
    }

    private void digitThresholdGlobal_ValueChanged(object sender, GH_DigitScrollerEventArgs e)
    {
      if (e.Intermediate) return;
      Settings.GlobalThreshold = Convert.ToDouble(digitThresholdGlobal.Value);
      Instances.RedrawCanvas();
    }

    private void SizeAnalyzerWidgetFrontEnd_Load(object sender, EventArgs e)
    {
      //OnVisibleChanged(Settings.ShowChanged);
      //OnThresholdChanged();
      Settings.ShowChanged += OnVisibleChanged;
      Settings.ShowGlobalWarningsChanged += OnWidgetShowGlobalWarningsChanged;
      Settings.ShowParamWarningsChanged += OnWidgetShowParamWarningsChanged;
      Settings.ParamThresholdChanged += OnThresholdChanged;
      Settings.GlobalThresholdChanged += OnWidgetGlobalThresholdChanged;
    }

    private void SizeAnalyzerWidgetFrontEnd_HandleDestroyed(object sender, EventArgs e)
    {
      Settings.ShowChanged -= OnVisibleChanged;
      Settings.ShowGlobalWarningsChanged -= OnWidgetShowGlobalWarningsChanged;
      Settings.ShowParamWarningsChanged -= OnWidgetShowParamWarningsChanged;
      Settings.ParamThresholdChanged -= OnThresholdChanged;
      Settings.GlobalThresholdChanged -= OnWidgetGlobalThresholdChanged;
    }
  }
}