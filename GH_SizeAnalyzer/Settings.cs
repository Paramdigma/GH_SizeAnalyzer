using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using SizeAnalyzer.UI;
using SizeAnalyzer.Widgets;

namespace SizeAnalyzer
{
  public class Settings : IGH_SettingFrontend
  {
    public delegate void ThresholdChangedEventHandler(double threshold);
    public delegate void VisibleChangedEventHandler(bool visible);
    public delegate void WidgetCornerChangedEventHandler(Corner corner);
    public delegate void SerialisationTypeChangedEventHandler(SerializationType type);

    

    private static readonly string _prefix = "Widget.SizeAnalyzer";
    private static string _paramThresholdKey => $"{_prefix}.Threshold";
    private static string _globalThresholdKey => $"{_prefix}.GlobalThreshold";
    private static string _showKey => $"{_prefix}.Show";
    private static string _showParamKey => $"{_prefix}.ShowParam";
    private static string _showGlobalKey => $"{_prefix}.ShowGlobal";

    private static string _widgetCornerKey => $"{_prefix}.Corner";
    private static string _serialisationTypeKey => $"{_prefix}.Serialisation.Type";

    private static double _paramThreshold
    {
      get => Instances.Settings.GetValue(_paramThresholdKey, 1.00);
      set => Instances.Settings.SetValue(_paramThresholdKey, value);
    }

    private static double _globalThreshold
    {
      get => Instances.Settings.GetValue(_globalThresholdKey, 10.00);
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
    
    private static Corner _widgetCorner
    {
      get => (Corner)Instances.Settings.GetValue(_widgetCornerKey, 0);
      set => Instances.Settings.SetValue(_widgetCornerKey, (int)value);
    }

    private static SerializationType _serialisationType
    {
      get => (SerializationType)Instances.Settings.GetValue(_serialisationTypeKey, 0);
      set => Instances.Settings.SetValue(_serialisationTypeKey, (int)value);
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
        Instances.RedrawCanvas();
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
        Instances.RedrawCanvas();
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
        Instances.RedrawCanvas();
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
        Instances.RedrawCanvas();
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
        Instances.RedrawCanvas();
      }
    }

    public static Corner Corner
    {
      get => _widgetCorner;
      set
      {
        if (_widgetCorner == value)
          return;
        _widgetCorner = value;
        WidgetCornerChanged?.Invoke(value);
        Instances.RedrawCanvas();
      }
    }
    
    public static SerializationType SerializationType
    {
      get => _serialisationType;
      set
      {
        if (_serialisationType == value)
          return;
        _serialisationType = value;
        SerialisationTypeChanged?.Invoke(value);
        Instances.RedrawCanvas();
      }
    }
    public string Category => "Widgets";

    public string Name => "Size analyzer widget";

    public IEnumerable<string> Keywords => new List<string> { "Widget", "Size", "Profiler", "Analyzer" };

    public Control SettingsUI()
    {
      return new SizeAnalyzerSettingsUI();
    }

    public static event VisibleChangedEventHandler ShowChanged;
    public static event VisibleChangedEventHandler ShowParamWarningsChanged;
    public static event VisibleChangedEventHandler ShowGlobalWarningsChanged;
    public static event ThresholdChangedEventHandler ParamThresholdChanged;
    public static event ThresholdChangedEventHandler GlobalThresholdChanged;
    public static event WidgetCornerChangedEventHandler WidgetCornerChanged;
    public static event SerialisationTypeChangedEventHandler SerialisationTypeChanged;
  }
}