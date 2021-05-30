using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Core.Graphs;
using Sceelix.Core.Graphs.Expressions;
using Sceelix.Core.Parameters;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.Inspector.Graphs;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using ParameterInfo = Sceelix.Core.Parameters.Infos.ParameterInfo;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    public abstract class ParameterEditor : IServiceable
    {
        private MessageManager _messageManager;
        
        

        public virtual void Initialize(IServiceLocator services)
        {
            _messageManager = services.Get<MessageManager>();
        }


        public virtual string ParameterInfoName
        {
            get { return ParameterInfoType.Name.Replace("ParameterInfo", ""); }
        }

        public abstract Type ParameterInfoType
        {
            get;
        }

        public abstract TreeViewItem CreateArgumentTreeViewItem(ParameterInfo argument, VisualNode visualNode, FileItem fileItem, string group = null);

        public virtual TreeViewItem CreateEditorTreeViewItem(GraphParameterEditorWindow info, ParameterInfo parameterInfo)
        {
            return new ParameterEditorTreeViewItem(parameterInfo, info);
        }



        public virtual bool SetValue(ArgumentTreeViewItem item, object value)
        {
            return false;
        }



        public virtual UIControl CreateInspectorContent(ParameterEditorTreeViewItem parameterEditorTreeViewItem)
        {
            var parameterInfo = parameterEditorTreeViewItem.ParameterInfo;

            var parameterInfoBrothers = parameterInfo.Parent != null ? parameterInfo.Parent.Parameters : parameterEditorTreeViewItem.EditorWindow.Graph.ParameterInfos.Where(x => x != parameterInfo);
            var siblingIdentifiers = parameterInfoBrothers.Where(x => x != parameterInfo).Select(x => Parameter.GetIdentifier(x.Label));

            LayoutControl layoutControl = new LayoutControl();

            //the label control that, when changed, changes the label of the parameterinfo 
            layoutControl.Add("Label:", new ExtendedTextBox() {Text = parameterInfo.Label, ToolTip = new ToolTipControl("Label of the parameter.") });
            var labelProperty = layoutControl.Last()[1].Properties.Get<String>("Text");

            //this label will show a whitespace-ridden name, which shall be used in expressions
            //TODO: Adjust this to the theme color
            TextBox uniqueLabelTextbox;
            layoutControl.Add("Identifier:", uniqueLabelTextbox = new ExtendedTextBox() {Text = Parameter.GetIdentifier(parameterInfo.Label), IsReadOnly = true, Foreground = Color.DarkGray, ToolTip = new ToolTipControl("Identifier, which allows you to reference the parameter. This is generated automatically from the chosen label.") });

            layoutControl.Add("Type:", new ExtendedTextBox() {Text = parameterInfo.GetType().Name.Replace("ParameterInfo", String.Empty), IsReadOnly = true, Foreground = Color.DarkGray, ToolTip = new ToolTipControl("Type of parameter.")});

            labelProperty.Changed += delegate(object sender, GamePropertyEventArgs<string> args)
            {
                var textbox = (TextBox) sender;

                var newIdentifier = Parameter.GetIdentifier(args.NewValue);

                if ( //!String.IsNullOrWhiteSpace(args.NewValue)
                    !String.IsNullOrWhiteSpace(newIdentifier) && !siblingIdentifiers.Contains(newIdentifier)) // && Regex.IsMatch(args.NewValue, "^[a-zA-Z][a-zA-Z0-9\\s]*$"))
                {
                    parameterInfo.Label = args.NewValue;
                    parameterEditorTreeViewItem.Text = args.NewValue + "(" + newIdentifier + ")"; //.ToSplitCamelCase();

                    //TODO: Adjust this to the theme color
                    textbox.Foreground = Color.White;

                    //refactor references to the parameter
                    RefactorParameterReference(parameterEditorTreeViewItem.EditorWindow.Graph, uniqueLabelTextbox.Text, newIdentifier);

                    uniqueLabelTextbox.Text = newIdentifier;

                    //uniqueLabelTextbox.Text = Parameter.GetIdentifier(parameterInfo.Label); //parameterInfo.Label.Replace(" ", "");
                }
                else
                {
                    //TODO: Adjust this to the actual theme color
                    textbox.Foreground = Color.Red;
                    //args.CoercedValue = args.OldValue;
                }
            };

            //same for the description textbox
            layoutControl.Add("Description:", new ExtendedTextBox() {Text = parameterInfo.Description, MinLines = 5, MaxLines = 5, ToolTip = new ToolTipControl("Description of the parameter.") });
            var property2 = layoutControl.Last()[1].Properties.Get<String>("Text");
            property2.Changed += (sender, args) => parameterInfo.Description = args.NewValue;

            //same for the section textbox
            layoutControl.Add("Section:", new ExtendedTextBox() { Text = parameterInfo.Section, ToolTip = new ToolTipControl("Section of the parameter. If it differs from the previous parameter, a titled separator will be introduced.") });
            var property5 = layoutControl.Last()[1].Properties.Get<String>("Text");
            property5.Changed += (sender, args) => parameterInfo.Section = args.NewValue;

            //and this for the ispublic field
            layoutControl.Add("Public:", new CheckBox() {IsChecked = parameterInfo.IsPublic, Margin = new Vector4F(0, 5, 0, 0), Height = 15, ToolTip = new ToolTipControl("Indicates if this parameter will be available externally, i.e. if it will be listed when used as a node.") });
            var property3 = layoutControl.Last()[1].Properties.Get<bool>("IsChecked");
            property3.Changed += (sender, args) => parameterInfo.IsPublic = args.NewValue;

            layoutControl.Add("Entity Evaluation:", new CheckBox() {IsChecked = parameterInfo.EntityEvaluation, Margin = new Vector4F(0, 5, 0, 0), Height = 15, ToolTip = new ToolTipControl("Indicates if this parameter is evaluated on particular entities, instead of per node round.") });
            var property4 = layoutControl.Last()[1].Properties.Get<bool>("IsChecked");
            property4.Changed += (sender, args) => parameterInfo.EntityEvaluation = args.NewValue;
            
            return layoutControl;
        }



        private void RefactorParameterReference(Graph graph, string oldParameterName, string newParameterName)
        {
            //go over all nodes of the graph
            //get a flat list of all arguments and subarguments
            //filter only the ones that have a parsedexpression
            //get a flat tree of the parsed expression nodes
            //filter the nodes of type ParameterExpressionNode 
            //and get only those that have this parameter name
            foreach (var expressionElement in 
                graph.Nodes.SelectMany(x => x.Parameters).SelectMany(x => x.GetThisAndSubtree(false)).
                    Where(z => z.ParsedExpression != null).
                    SelectMany(y => y.ParsedExpression.GetSubtree()).
                    OfType<ParameterExpressionNode>().Where(x => x.Name == oldParameterName))
            {
                expressionElement.RenameTo(newParameterName);
            }
        }



        protected void AlertGraphChange(FileItem fileItem, bool worthExecuting = true)
        {
            _messageManager.Publish(new GraphContentChanged(fileItem, worthExecuting));
        }





        /// <summary>
        /// Indicates whether ParameterInfos associated to this editor can exist at the element root. The recursive parameter, for instance, is one that can not.
        /// </summary>
        public virtual bool CanExistAtRoot
        {
            get { return true; }
        }



        public virtual UIControl CreateControl(ParameterInfo argument, FileItem fileItem, Action onChanged)
        {
            return null;
        }
    }


    public abstract class ParameterEditor<T> : ParameterEditor where T : ParameterInfo
    {
        public sealed override Type ParameterInfoType
        {
            get { return typeof(T); }
        }
    }
}