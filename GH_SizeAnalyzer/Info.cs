using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace SizeAnalyzer
{
    public class Info : GH_AssemblyInfo
    {
        public override string Name => "GH_SizeAnalyzer";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => Properties.Resources.CalculatorIcon;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Grasshopper Tool to warn of big internalized data in GH documents.";

        public override Guid Id => new Guid("95323AB1-7AD7-48FB-8583-AEEB21987B89");

        //Return a string identifying you or your company.
        public override string AuthorName => "Paramdigma";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "https://github.com/Paramdigma";

        public override string Version => System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

        public override string AssemblyVersion => System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

        public override Bitmap AssemblyIcon => Properties.Resources.CalculatorIcon;
    }
}