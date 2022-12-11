using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace SizeAnalyzer.UI
{
  public class SizeAnalyzerHit
  {
    public readonly IGH_DocumentObject DocObject;
    public Rectangle Box;
    public Rectangle BoxIcon;
    public Rectangle BoxText;
    public Rectangle BoxDirection;

    public SizeAnalyzerHit(IGH_DocumentObject obj)
    {
      DocObject = obj;
    }

    public void SetBox(Rectangle targetBox)
    {
      Box = targetBox;
      BoxIcon = targetBox;
      Rectangle local = new Rectangle(BoxIcon.Location, BoxIcon.Size);
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