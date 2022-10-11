using Eto.Forms;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using GH_IO;
using Grasshopper.Kernel.Parameters;

namespace PersistentDataSizeCalculator
{
    public class PersistentDataSizeCalculatorComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PersistentDataSizeCalculatorComponent()
          : base("PersistentDataSizeCalculator", "PDC",
            "Calculates all persistent data size",
            "Params", "Scripts")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Data name", "DN", "DN", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var persistentParams = GetAllParamsInDoc().Where(param=> param.DataType == GH_ParamData.local).ToList();
            var paramSizes = persistentParams.Select(param => GetParamSize(param)).ToList();
            // if()
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < paramSizes.Count; i++)
            {
                stringBuilder.AppendFormat(persistentParams[i].Name +"(" + persistentParams[i].TypeName + ")" + ":" + paramSizes[i].ToString() + "Mb\n");
            }
            DA.SetData(0, stringBuilder.ToString());
        }

        private GH_Document _document = Grasshopper.Instances.ActiveCanvas.Document;
        public IEnumerable<IGH_Param> GetAllParamsInDoc()
        {
            var objects = _document.Objects;
            foreach(var obj in objects){
                if(obj is IGH_Param param){
                    yield return param;
                }
                if(obj is GH_Component component){
                    foreach(var cParam in component.Params.Input){
                        yield return cParam;
                    }
                }
            }
        }
        public double GetParamSize(IGH_Param param){
            var archive = new GH_Archive();
            archive.CreateTopLevelNode("size check");
            archive.AppendObject(param, param.InstanceGuid.ToString());
            var xml = archive.Serialize_Xml();
            double megabyteSize = (Math.Round((double)Encoding.Unicode.GetByteCount(xml) / 1048576, 3));
            return megabyteSize;
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("BA7BB4D9-22B3-45D3-A71A-22B61EE59FC4");
    }
}