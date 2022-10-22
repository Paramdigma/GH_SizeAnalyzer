using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Grasshopper.GUI;

namespace GrasshopperPersistentDataSizeCalculator
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
        public SizeAnalyzerSettingsFrontEnd()
        {
            this.Load += new EventHandler(this.SizeAnalyzerSettingsFrontEnd_Load);
            this.HandleDestroyed += new EventHandler(this.SizeAnalyzerSettingsFrontEnd_HandleDestroyed);
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.checkShow = new CheckBox();
            this.Panel1 = new Panel();
            this.GH_Label1 = new GH_Label();
            this.SuspendLayout();
            this.checkShow.Dock = DockStyle.Fill;
            this.checkShow.Location = new Point(58, 0);
            this.checkShow.Margin = new Padding(0);
            this.checkShow.Name = "checkShow";
            this.checkShow.Size = new Size(608, 46);
            this.checkShow.TabIndex = 1;
            this.checkShow.Text = "Show Size analyzer widget";
            this.checkShow.UseVisualStyleBackColor = true;
            this.checkShow.AutoCheck = true;
            this.checkShow.Click += checkShowOnClick;
            this.Panel1.Dock = DockStyle.Left;
            this.Panel1.Location = new Point(48, 0);
            this.Panel1.Margin = new Padding(6, 6, 6, 6);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new Size(10, 46);
            this.Panel1.TabIndex = 3;
            this.GH_Label1.Dock = DockStyle.Left;
            this.GH_Label1.Image = null;
            this.GH_Label1.ImageAlign = ContentAlignment.MiddleCenter;
            this.GH_Label1.Location = new Point(0, 0);
            this.GH_Label1.Name = "GH_Label1";
            this.GH_Label1.Size = new Size(48, 46);
            this.GH_Label1.TabIndex = 4;
            this.GH_Label1.Text = (string)null;
            this.GH_Label1.TextAlign = ContentAlignment.MiddleCenter;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(checkShow);
            this.Controls.Add(Panel1);
            this.Margin = new Padding(6, 6, 6, 6);
            this.Name = nameof(SizeAnalyzerSettingsFrontEnd);
            this.Size = new Size(666, 46);
            this.ResumeLayout(false);
        }

        private void checkShowOnClick(object o, EventArgs e)
        {
            SizeAnalyzerWidget.SharedVisible = this.checkShow.Checked;
        }

        private void SizeAnalyzerSettingsFrontEnd_HandleDestroyed(object sender, EventArgs e)
        {
        }

        private void SizeAnalyzerSettingsFrontEnd_Load(object sender, EventArgs e)
        {
            checkShow.Checked = SizeAnalyzerWidget.SharedVisible;
        }

        internal virtual CheckBox checkShow { get; [MethodImpl(MethodImplOptions.Synchronized)] set; }

        [field: AccessedThroughProperty("Panel1")]
        internal virtual Panel Panel1 { get; [MethodImpl(MethodImplOptions.Synchronized)] set; }

        [field: AccessedThroughProperty("GH_Label1")]
        internal virtual GH_Label GH_Label1 { get; [MethodImpl(MethodImplOptions.Synchronized)] set; }
    }
}
