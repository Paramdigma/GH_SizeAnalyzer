using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace SizeAnalyzer
{
    public class Info : GH_AssemblyInfo
    {
        public override string Name => "GH_SizeAnalyzer";

        public override Bitmap Icon => Properties.Resources.CalculatorIcon;

        public override string Description => "Grasshopper Tool to warn of big internalized data in GH documents.";

        public override Guid Id => new Guid("95323AB1-7AD7-48FB-8583-AEEB21987B89");

        public override string AuthorName => "Paramdigma";

        public override string AuthorContact => "https://github.com/Paramdigma/GH_SizeAnalyzer";

        public override string Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override Bitmap AssemblyIcon => Properties.Resources.CalculatorIcon;

    }
}