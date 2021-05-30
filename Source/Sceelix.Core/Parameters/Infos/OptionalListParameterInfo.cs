using System;
using System.Linq;
using System.Xml;
using Sceelix.Core.Code;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters.Infos
{
    public class OptionalListParameterInfo : ListParameterInfo
    {
        public OptionalListParameterInfo(string label) : base(label)
        {
        }



        public OptionalListParameterInfo(OptionalListParameter parameter) : base(parameter)
        {
        }



        public OptionalListParameterInfo(XmlNode xmlNode) : base(xmlNode)
        {
        }



        public bool HasItem => Items.Count > 0;


        public override string MetaType => "Optional";


        public ParameterInfo SelectedItem => Items.FirstOrDefault();



        protected override void CreateSpecificCSharpCode(CodeBuilder codeBuilder, string varName)
        {
            codeBuilder.AppendLine($"{varName}.Set({SelectedItem.Label.Quote()});");

            if (HasItem)
                SelectedItem.CreateCSharpCode(codeBuilder, varName);
        }



        public override Parameter ToParameter()
        {
            return new OptionalListParameter(this,
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