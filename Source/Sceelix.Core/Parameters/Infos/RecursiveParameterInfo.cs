using System.Collections.Generic;
using System.Xml;
using Sceelix.Core.Parameters.Infos;

namespace Sceelix.Core.Parameters
{
    public class RecursiveParameterInfo : ParameterInfo
    {
        public RecursiveParameterInfo(string label)
            : base(label)
        {
        }



        public RecursiveParameterInfo(Parameter parameter)
            : base(parameter)
        {
        }



        public RecursiveParameterInfo(XmlNode xmlNode)
            : base(xmlNode)
        {
        }



        public override string MetaType => "Recursive (" + Label + ")";



        public override object CloneModel(bool resolveRecursiveParameter)
        {
            if (resolveRecursiveParameter)
            {
                var ancestor = GetParameterInfoAncestor(Label);
                var clonedModel = (ParameterInfo) ancestor.CloneModel(false);

                return clonedModel;
            }

            return base.CloneModel(false);
        }



        public ParameterInfo GetActualParameterInfo()
        {
            return GetParameterInfoAncestor(Label);
        }



        protected ParameterInfo GetParameterInfoAncestor(string label)
        {
            var parent = Parent;

            while (parent != null)
            {
                if (parent.Label == label)
                    return parent;

                parent = parent.Parent;
            }

            throw new KeyNotFoundException(string.Format("Could not find the parameter {0} referred by the recursive parameter. Please check if you have typed the label correctly.", label));
        }



        public override Parameter ToParameter()
        {
            return GetParameterInfoAncestor(Label).ToParameter();
        }



        /*public override object Clone()
        {
            return (RecursiveParameterInfo)base.Clone();
        }*/



        public override void WriteArgumentXML(XmlWriter writer)
        {
            //we forcibly do not write anything, because the recursive item comes naturally
        }
    }
}