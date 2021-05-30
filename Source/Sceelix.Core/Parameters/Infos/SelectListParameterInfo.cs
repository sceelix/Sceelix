using System;
using System.Linq;
using System.Xml;
using Sceelix.Core.Code;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters.Infos
{
    public class SelectListParameterInfo : ListParameterInfo
    {
        public SelectListParameterInfo(string label)
            : base(label)
        {
        }



        public SelectListParameterInfo(SelectListParameter parameter)
            : base(parameter)
        {
        }



        public SelectListParameterInfo(XmlNode xmlNode)
            : base(xmlNode)
        {
        }



        public override string MetaType => "Select";


        public ParameterInfo SelectedItem => Items.FirstOrDefault();



        protected override void CreateSpecificCSharpCode(CodeBuilder codeBuilder, string varName)
        {
            codeBuilder.AppendLine($"{varName}.Set({SelectedItem.Label.Quote()});");

            SelectedItem.CreateCSharpCode(codeBuilder, varName);
        }



        public ParameterInfo Select(string label)
        {
            Items.Clear();

            var newSubItem = (ParameterInfo) ItemModels.Find(x => x.Label == label).CloneModel(true);
            newSubItem.Parent = this;
            Items.Add(newSubItem);

            return newSubItem;
        }



        public override Parameter ToParameter()
        {
            return new SelectListParameter(this,
                _itemModels.Select(x => x.Label).ToList(),
                _itemModels.Select(x => new Func<Parameter, Parameter>(p =>
                {
                    var parameter = x.ToParameter();
                    parameter.Parent = p;
                    return parameter;
                })),
                _items.Select(x => x.ToParameter()));
        }
    }
}