using System;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace GrasshopperPersistentDataSizeCalculator
{
  public class Loader : GH_AssemblyPriority
  {
    public GH_Document Doc => Instances.ActiveCanvas.Document;

    public override GH_LoadingInstruction PriorityLoad()
    {
      Instances.CanvasCreated += OnCanvasCreated;
      Instances.CanvasDestroyed += OnCanvasDestroyed;
      return GH_LoadingInstruction.Proceed;
    }

    private void OnCanvasDestroyed(GH_Canvas canvas)
    {
      Instances.DocumentEditor.Load -= OnDocumentEditorOnLoad;
      Instances.ActiveCanvas.DocumentChanged -= OnDocumentChanged;
    }

    public void OnCanvasCreated(GH_Canvas canvas)
    {
      // When the canvas is created (i.e. when Grasshopper first loads)
      // Setup event to wait for the document editor to be loaded.
      Instances.DocumentEditor.Load += OnDocumentEditorOnLoad;

      // Subscribe to all relevant events
      Instances.ActiveCanvas.DocumentChanged += OnDocumentChanged;

      // Set the canvas as the widget's owner
      Widget.Owner = canvas;
      // Finally, add the widget to the canvas.
      Instances.ActiveCanvas.Widgets.Add(Widget);
    }

    private void OnDocumentEditorOnLoad(object o, EventArgs e)
    {
      // Create the top menu if/when necessary. It's always better to use the `settings` panel.
      // CreateTopMenu(Instances.DocumentEditor.MainMenuStrip);
    }

    private void OnDocumentChanged(GH_Canvas c, GH_CanvasDocumentChangedEventArgs ce)
    {
      if (ce.OldDocument != null)
      {
        ce.OldDocument.ObjectsAdded -= DocOnObjectsAdded;
        ce.OldDocument.ObjectsDeleted -= DocOnObjectsDeleted;
      }

      if (ce.NewDocument != null)
      {
        ce.NewDocument.ObjectsAdded += DocOnObjectsAdded;
        ce.NewDocument.ObjectsDeleted += DocOnObjectsDeleted;
        
        InternalDataCalculator.Compute(ce.NewDocument);
      }
      
    }

    private void DocOnObjectsDeleted(object sender, GH_DocObjectEventArgs e)
    {
      e.Objects.ToList().ForEach(obj =>
      {
        switch (obj)
        {
          case IGH_Param param:
            param.ObjectChanged -= OnObjectChanged;
            InternalDataCalculator.Remove(param);
            break;
          case IGH_Component component:
            // A component's attributes change when a param is added/deleted from the component
            component.AttributesChanged -= OnAttributesChanged;
            component.Params.Input.ForEach(p =>
            {
              p.ObjectChanged -= OnObjectChanged;
              InternalDataCalculator.Remove(p);
            });
            break;
        }
      });
    }

    private void DocOnObjectsAdded(object sender, GH_DocObjectEventArgs e)
    {
      foreach (var ghDocumentObject in e.Objects)
      {
        switch (ghDocumentObject)
        {
          case IGH_Param param:
            param.ObjectChanged += OnObjectChanged;
            InternalDataCalculator.Add(param);
            break;
          case IGH_Component component:
            // A component's attributes change when a param is added/deleted from the component
            component.AttributesChanged += OnAttributesChanged;
            component.Params.Input.ForEach(p =>
            {
              p.ObjectChanged += OnObjectChanged;
              InternalDataCalculator.Add(p);
            });
            break;
        }
      }
    }

    private void OnAttributesChanged(IGH_DocumentObject sender, GH_AttributesChangedEventArgs e)
    {
      if (sender is IGH_Component component)
      {
        component.Params.Input.ForEach(p =>
        {
          // Re-register `OnObjectChanged` on all params of that component
          p.ObjectChanged -= OnObjectChanged;
          p.ObjectChanged += OnObjectChanged;
          InternalDataCalculator.Add(p);
        });
      }
    }

    private void OnObjectChanged(IGH_DocumentObject sender, GH_ObjectChangedEventArgs e)
    {
      if (sender is IGH_Param p && e.Type == GH_ObjectEventType.Sources)
          InternalDataCalculator.Add(p);
    }

    public readonly SizeAnalyzerWidget Widget = new SizeAnalyzerWidget();

    public ToolStripMenuItem CreateTopMenu(MenuStrip parent)
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

      parent.Items.Add(menu);
      return menu;
    }
  }
}