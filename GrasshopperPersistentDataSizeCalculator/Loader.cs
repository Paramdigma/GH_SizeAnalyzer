using System;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace GrasshopperPersistentDataSizeCalculator
{
    public class Loader : GH_AssemblyPriority
    {
        public GH_Document Doc => Instances.ActiveCanvas.Document;

        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.CanvasCreated += OnCanvasCreated;

            return GH_LoadingInstruction.Proceed;
        }

        public void OnCanvasCreated(GH_Canvas canvas)
        {
            Instances.DocumentEditor.Load += (o, e) =>
            {
                var mainMenu = Instances.DocumentEditor.MainMenuStrip;
                var menu = CreateTopMenu();
                mainMenu.Items.Add(menu);

                Instances.ActiveCanvas.DocumentChanged += (c, ce) =>
                {
                    Console.WriteLine("Doc changed");
                };

                Instances.ActiveCanvas.Widgets.Add(Widget);
            };
        }

        public SizeAnalyzerWidget Widget = new SizeAnalyzerWidget();

        public ToolStripMenuItem CreateTopMenu()
        {
            var menu = new ToolStripMenuItem("GH_PDSC");

            // TODO: Add menu items here
            var enabled = new ToolStripMenuItem("Enabled");
            enabled.CheckOnClick = true;

            enabled.Click += (o, e) => { Widget.Visible = !Widget.Visible; };
            enabled.CheckState = Widget.Visible ? CheckState.Checked : CheckState.Unchecked;

            menu.DropDown.Items.Add(enabled);
            menu.DropDown.Items.Add(new ToolStripSeparator());
            menu.DropDown.Items.Add("Help");
            return menu;
        }

    }
}
