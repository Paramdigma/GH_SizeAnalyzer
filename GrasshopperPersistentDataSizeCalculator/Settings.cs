using System;
using Grasshopper.GUI;
using System.Windows.Forms;
using System.Collections.Generic;
using Grasshopper;
using SizeAnalyzer.UI;

namespace SizeAnalyzer
{
  public class Settings : IGH_SettingFrontend
  {
    public string Category => "Widgets";

    public string Name => "Size analyzer widget";

    public IEnumerable<string> Keywords => new List<string> { "Widget", "Size", "Profiler", "Analyzer" };

    public Control SettingsUI() => new SizeAnalyzerSettingsUI();
    
    public static event VisibleChangedEventHandler ShowChanged;
    public static event VisibleChangedEventHandler ShowParamWarningsChanged;
    public static event VisibleChangedEventHandler ShowGlobalWarningsChanged;
    public static event ThresholdChangedEventHandler ParamThresholdChanged;
    public static event ThresholdChangedEventHandler GlobalThresholdChanged;

    public delegate void VisibleChangedEventHandler(bool visible);
    public delegate void ThresholdChangedEventHandler(double threshold);
    
    static readonly string _prefix = "Widget.SizeAnalyzer";
    static string _paramThresholdKey => $"{_prefix}.Threshold";
    static string _globalThresholdKey => $"{_prefix}.GlobalThreshold";
    static string _showKey => $"{_prefix}.Show";
    static string _showParamKey => $"{_prefix}.ShowParam";
    static string _showGlobalKey => $"{_prefix}.ShowGlobal";

    static double _paramThreshold
    {
      get => Instances.Settings.GetValue(_paramThresholdKey, 0.5);
      set => Instances.Settings.SetValue(_paramThresholdKey, value);
    }

    private static double _globalThreshold
    {
      get => Instances.Settings.GetValue(_globalThresholdKey, 10);
      set => Instances.Settings.SetValue(_globalThresholdKey, value);
    }

    private static bool _showWidget
    {
      get => Instances.Settings.GetValue(_showKey, true);
      set => Instances.Settings.SetValue(_showKey, value);
    }

    private static bool _showGlobalWarnings
    {
      get => Instances.Settings.GetValue(_showParamKey, true);
      set => Instances.Settings.SetValue(_showParamKey, value);
    }

    private static bool _showParamWarnings
    {
      get => Instances.Settings.GetValue(_showGlobalKey, true);
      set => Instances.Settings.SetValue(_showGlobalKey, value);
    }

    public static bool Show
    {
      get => _showWidget;
      set
      {
        if (_showWidget == value)
          return;
        _showWidget = value;
        ShowChanged?.Invoke(value);
        Instances.InvalidateCanvas();
      }
    }

    public static bool ShowGlobalWarnings
    {
      get => _showGlobalWarnings;
      set
      {
        if (_showGlobalWarnings == value)
          return;
        _showGlobalWarnings = value;
        ShowGlobalWarningsChanged?.Invoke(value);
        Instances.InvalidateCanvas();
      }
    }

    public static bool ShowParamWarnings
    {
      get => _showParamWarnings;
      set
      {
        if (_showParamWarnings == value)
          return;
        _showParamWarnings = value;
        ShowParamWarningsChanged?.Invoke(value);
        Instances.InvalidateCanvas();
      }
    }

    public static double ParamThreshold
    {
      get => _paramThreshold;
      set
      {
        if (Math.Abs(_paramThreshold - value) < 0.01) return;
        _paramThreshold = value;
        ParamThresholdChanged?.Invoke(value);
        Instances.InvalidateCanvas();
      }
    }

    public static double GlobalThreshold
    {
      get => _globalThreshold;
      set
      {
        if (Math.Abs(_globalThreshold - value) < 0.01) return;
        _globalThreshold = value;
        GlobalThresholdChanged?.Invoke(value);
      }
    }
  }
}