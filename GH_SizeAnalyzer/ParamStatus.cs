using System;

namespace SizeAnalyzer
{
  [Flags]
  public enum ParamStatus
  {
    OverThreshold = 1,
    Loading = 2,
    UnderThreshold = 4,
    Untracked = 8,
    Error = 16
  }
}