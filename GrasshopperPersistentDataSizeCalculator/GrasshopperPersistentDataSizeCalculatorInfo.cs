using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace GrasshopperPersistentDataSizeCalculator
{
    public class GrasshopperPersistentDataSizeCalculatorInfo : GH_AssemblyInfo
    {
        public override string Name => "GrasshopperPersistentDataSizeCalculator";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("95323AB1-7AD7-48FB-8583-AEEB21987B89");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}