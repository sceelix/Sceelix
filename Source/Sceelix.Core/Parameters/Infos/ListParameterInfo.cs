using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Sceelix.Conversion;
using Sceelix.Core.Code;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters.Infos
{
    public class ListParameterInfo : ParameterInfo
    {
        protected List<ParameterInfo> _itemModels;
        protected List<ParameterInfo> _items;



        public ListParameterInfo(string label)
            : base(label)
        {
            _itemModels = new List<ParameterInfo>();
            _items = new List<ParameterInfo>();

            Expanded = true;
        }



        public ListParameterInfo(ListParameter parameter)
            : base(parameter)
        {
            _itemModels = parameter.CreationFunctionsList.Select(x => x.Invoke(parameter).ToParameterInfo()).ToList();
            _items = parameter.Items.Select(x => x.ToParameterInfo()).ToList();
            MaxSize = parameter.MaxSize;
            Expanded = parameter.IsExpandedAsDefault;

            foreach (var parameterInfo in _itemModels)
                parameterInfo.Parent = this;

            foreach (var parameterInfo in _items)
                parameterInfo.Parent = this;
        }



        public ListParameterInfo(XmlNode xmlNode)
            : base(xmlNode)
        {
            _items = new List<ParameterInfo>();
            _itemModels = new List<ParameterInfo>();

            MaxSize = xmlNode.GetAttributeOrDefault("MaxSize", 0);
            Expanded = xmlNode.GetAttributeOrDefault("Expanded", true);

            //for the model
            foreach (XmlElement xmlModelNode in xmlNode["ParameterModels"].ChildNodes)
            {
                var xmlModelType = xmlModelNode.GetAttributeOrDefault<Type>("Type", null);

                var newParameterModel = (ParameterInfo) Activator.CreateInstance(xmlModelType, xmlModelNode);
                newParameterModel.Parent = this;

                _itemModels.Add(newParameterModel);
            }

            XmlNodeList parameterList = xmlNode["Items"].ChildNodes;
            foreach (XmlElement xmlItemNode in parameterList)
            {
                var xmlModelType = xmlItemNode.GetAttributeOrDefault<Type>("Type", null);

                var parameterInfo = (ParameterInfo) Activator.CreateInstance(xmlModelType, xmlItemNode);
                parameterInfo.Parent = this;

                _items.Add(parameterInfo);
            }
        }



        private int ActualMaxSize => MaxSize > 0 ? MaxSize : int.MaxValue;


        public override bool CanHaveRecursiveItem => true;


        public override bool CanHaveSubItems => true;


        public bool Expanded
        {
            get;
            set;
        }


        public override List<ParameterInfo> ItemModels => _itemModels;


        public List<ParameterInfo> Items => _items;


        public int MaxSize
        {
            get;
            set;
        }



        public override string MetaType
        {
            get
            {
                if (_itemModels.Count == 1 && MaxSize == 1)
                    return "Check";
                if (_itemModels.Count > 1 && MaxSize == 1)
                    return "Choice";
                if (_itemModels.Count == 1)
                    return "List";

                return "Multi-Type List";
            }
        }



        public override List<ParameterInfo> Parameters => _items;


        public bool ReachedLimit => MaxSize > 0 && _items.Count == MaxSize;



        public override IEnumerable<InputPort> SubInputPortTree
        {
            get
            {
                if (IsExpression)
                    yield break;

                foreach (var inputPort in _items.Take(ActualMaxSize).SelectMany(item => item.SubInputPortTree))
                    yield return inputPort;
            }
        }



        public override IEnumerable<OutputPort> SubOutputPortTree
        {
            get
            {
                if (IsExpression)
                    yield break;

                foreach (var outputPort in _items.Take(ActualMaxSize).SelectMany(item => item.SubOutputPortTree))
                    yield return outputPort;
            }
        }



        public override object Clone()
        {
            var clone = (ListParameterInfo) base.Clone();

            clone._itemModels = _itemModels.Select(x => (ParameterInfo) x.Clone()).ToList();
            foreach (var parameterInfo in clone._itemModels)
                parameterInfo.Parent = clone;

            clone._items = _items.Select(x => (ParameterInfo) x.Clone()).ToList();
            foreach (var parameterInfo in clone._items)
                parameterInfo.Parent = clone;

            MaxSize = clone.MaxSize;

            return clone;
        }



        public override object CloneModel(bool resolveRecursiveParameter)
        {
            var clone = (ListParameterInfo) base.Clone();

            clone._itemModels = _itemModels.Select(x => (ParameterInfo) x.CloneModel(resolveRecursiveParameter)).ToList();
            foreach (var parameterInfo in clone._itemModels)
                parameterInfo.Parent = clone;

            clone._items = new List<ParameterInfo>();

            MaxSize = clone.MaxSize;

            return clone;
        }



        protected override void CreateSpecificCSharpCode(CodeBuilder codeBuilder, string varName)
        {
            int index = 0;
            foreach (var subParameterInfo in Parameters.Where(x => x.IsPublic))
            {
                codeBuilder.AppendLine($"{varName}.Set({subParameterInfo.Label.Quote()});");
                subParameterInfo.CreateCSharpCode(codeBuilder, varName + $".Parameters[{index++}]", true);
            }
        }



        protected internal override string GetFullLabel(ParameterInfo parameterInfo)
        {
            if (parameterInfo == this)
                return Label;

            for (int index = 0; index < _items.Count; index++)
            {
                var item = _items[index];

                var fullLabel = item.GetFullLabel(parameterInfo);
                if (fullLabel != null)
                    return Label + "[" + index + "]." + fullLabel; //Label + "." + index + "." + fullNameOf
            }

            return null;
        }



        public override IEnumerable<string> GetReferencedPaths()
        {
            return _items.SelectMany(val => val.GetReferencedPaths());
        }



        public override IEnumerable<ParameterInfo> GetSubtree(bool stopAtExpressionParameters)
        {
            if (stopAtExpressionParameters && IsExpression)
                yield break;


            foreach (var item in _items)
            {
                yield return item;

                foreach (var subTreeItem in item.GetSubtree(stopAtExpressionParameters))
                    yield return subTreeItem;
            }
        }



        public override bool HasReferenceToParameter(string label)
        {
            if (base.HasReferenceToParameter(label))
                return true;

            return _items.Any(x => x.HasReferenceToParameter(label));
        }



        public override void ReadArgumentXML(XmlNode xmlNode, IProcedureEnvironment procedureEnvironment)
        {
            base.ReadArgumentXML(xmlNode, procedureEnvironment);

            Expanded = xmlNode.GetAttributeOrDefault("Expanded", true);

            _items = new List<ParameterInfo>();
            foreach (XmlNode childNode in xmlNode["Items"].ChildNodes)
                try
                {
                    var label = childNode.GetAttributeOrDefault<string>("Label", null);
                    var guidString = childNode.GetAttributeOrDefault<string>("Guid", null);
                    Guid? guid = !string.IsNullOrWhiteSpace(guidString) ? (Guid?) ConvertHelper.Convert<Guid>(guidString) : null;

                    var model = _itemModels.FirstOrDefault(val => guid.HasValue && val.Guid == guid || val.Label == label);

                    if (model != null)
                    {
                        //var parameterInfo = (ParameterInfo)model.CloneRecursive();
                        var parameterInfo = (ParameterInfo) model.CloneModel(true);
                        parameterInfo.Parent = this;
                        _items.Add(parameterInfo);

                        parameterInfo.ReadArgumentXML(childNode, procedureEnvironment);
                    }
                }
                catch (Exception)
                {
                    throw new Exception(); //"Could not load arguments of node '" + node.DefaultLabel + "'.");
                }
        }



        /*public override Object CloneModel()
        {
            var clone = (ListParameterInfo)base.CloneModel();*/

        //clone each subparameter info
        /*clone._itemModels = _itemModels.Select(x => (ParameterInfo)x.CloneDefinition()).ToList();
            foreach (var parameterInfo in clone._itemModels)
                parameterInfo.Parent = clone;*/
        /*clone._itemModels = _itemModels.Select(x => (ParameterInfo)x.CloneModel()).ToList();
            foreach (var parameterInfo in clone._itemModels)
                parameterInfo.Parent = clone;*/

        /*clone._items = _items.Select(x => (ParameterInfo)x.CloneModel()).ToList();
            foreach (var parameterInfo in clone._items)
                parameterInfo.Parent = clone;

            //clone._items = new List<ParameterInfo>();

            return clone;
        }*/



        public override void RefactorGlobalParameters(string oldLabel, string newLabel)
        {
            base.RefactorGlobalParameters(oldLabel, newLabel);

            foreach (var parameterInfo in _items)
                parameterInfo.RefactorGlobalParameters(oldLabel, newLabel);
        }



        public bool SupportsItemModel(ParameterInfo argument)
        {
            return _itemModels.Any(model => model.StructurallyEqual(argument));
        }



        public override Parameter ToParameter()
        {
            return new ListParameter(this,
                _itemModels.Select(x => x.Label).ToList(),
                _itemModels.Select(x => new Func<Parameter, Parameter>(p =>
                {
                    var parameter = x.ToParameter();
                    parameter.Parent = p;
                    return parameter;
                })),
                _items.Select(x => x.ToParameter()));
        }



        public override void WriteArgumentXML(XmlWriter writer)
        {
            writer.WriteAttributeString("Expanded", Convert.ToString(Expanded, CultureInfo.InvariantCulture));

            base.WriteArgumentXML(writer);

            writer.WriteStartElement("Items");

            foreach (ParameterInfo parameterInfo in _items)
            {
                writer.WriteStartElement("Item");

                parameterInfo.WriteArgumentXML(writer);

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }



        public override void WriteXMLDefinition(XmlWriter writer)
        {
            writer.WriteAttributeString("MaxSize", MaxSize.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Expanded", Convert.ToString(Expanded, CultureInfo.InvariantCulture));

            base.WriteXMLDefinition(writer);

            writer.WriteStartElement("ParameterModels");
            foreach (var parameterInfo in _itemModels)
            {
                writer.WriteStartElement("ParameterModel");
                parameterInfo.WriteXMLDefinition(writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            writer.WriteStartElement("Items");

            foreach (ParameterInfo parameterInfo in _items)
            {
                writer.WriteStartElement("Item");

                parameterInfo.WriteXMLDefinition(writer);

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }
}