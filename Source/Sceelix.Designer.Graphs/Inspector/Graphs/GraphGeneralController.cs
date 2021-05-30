using System;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.Inspector.Graphs
{
    public class GraphGeneralController : GroupBox
    {
        public GraphGeneralController(IServiceLocator services, Graph graph, FileItem fileItem)
        {
            Title = "General";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Margin = new Vector4F(0);

            //this layout control will keep the labels aligned and pretty
            LayoutControl layoutControl = new LayoutControl
            {
                Margin = new Vector4F(5)
            };


            //Control that allows the author of the graph to be changed
            var rowAuthor = layoutControl.Add("Author:", new ExtendedTextBox()
            {
                Text = graph.Author,
                HorizontalAlignment = HorizontalAlignment.Stretch
            });
            var textboxAuthorProperty = rowAuthor[1].Properties.Get<String>("Text");
            rowAuthor[0].ToolTip = rowAuthor[1].ToolTip = new ToolTipControl("The name of this graph's author.");
            textboxAuthorProperty.Changed += delegate(object sender, GamePropertyEventArgs<string> args)
            {
                graph.Author = args.NewValue;
                services.Get<MessageManager>().Publish(new GraphContentChanged(fileItem));
            };


            //Control that allows the description of the graph to be changed
            var rowDescription = layoutControl.Add("Description:", new ExtendedTextBox()
            {
                Text = graph.Description,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinLines = 2,
                MaxLines = 5,
            });
            var textboxDescriptionProperty = rowDescription[1].Properties.Get<String>("Text");
            rowDescription[0].ToolTip = rowDescription[1].ToolTip = new ToolTipControl("A description of the graph. Will appear as the description of the node, when instanced in another graph.");
            textboxDescriptionProperty.Changed += delegate(object sender, GamePropertyEventArgs<string> args)
            {
                graph.Description = args.NewValue;
                services.Get<MessageManager>().Publish(new GraphContentChanged(fileItem));
            };


            //Control that allows the category of the graph to be changed
            var rowCategory = layoutControl.Add("Category:", new ExtendedTextBox()
            {
                Text = graph.Category,
                HorizontalAlignment = HorizontalAlignment.Stretch
            });
            var textboxCategoryProperty = rowCategory[1].Properties.Get<String>("Text");
            rowCategory[0].ToolTip = rowCategory[1].ToolTip = new ToolTipControl("Category of the graph. Used to organize the corresponding entry in the node selection window.");
            textboxCategoryProperty.Changed += delegate(object sender, GamePropertyEventArgs<string> args)
            {
                graph.Category = args.NewValue;
                services.Get<MessageManager>().Publish(new GraphContentChanged(fileItem));
            };


            //Control that allows the tags of the graph to be changed
            var rowTags = layoutControl.Add("Tags:", new ExtendedTextBox()
            {
                Text = graph.Tags,
                HorizontalAlignment = HorizontalAlignment.Stretch
            });
            var textboxTagsProperty = rowTags[1].Properties.Get<String>("Text");
            rowTags[0].ToolTip = rowTags[1].ToolTip = new ToolTipControl("Tags associated to the graph. Used for searching purposes in the node selection window.");
            textboxTagsProperty.Changed += delegate(object sender, GamePropertyEventArgs<string> args)
            {
                graph.Tags = args.NewValue;
                services.Get<MessageManager>().Publish(new GraphContentChanged(fileItem));
            };

            Content = layoutControl;
        }
    }
}