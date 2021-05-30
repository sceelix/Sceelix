using System.Linq;
using Microsoft.Xna.Framework;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.Graphs.Inspector.Entities;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.Entities
{
    [DesignerService]
    public class EntitySelectionManager : IServiceable
    {
        private IEntity _selectedEntity = null;
        private MessageManager _messageManager;
        private IServiceLocator _services;


        public void Initialize(IServiceLocator services)
        {
            _services = services;
            _messageManager = services.Get<MessageManager>();

            services.Get<MessageManager>().Register<EntitySelected>(OnEntitySelected);
            services.Get<MessageManager>().Register<EntityDeselected>(OnEntityDeselected);
        }



        private void OnEntityDeselected(EntityDeselected obj)
        {
            //we only care if the deselected object is the one we had in store
            if (obj.Entity == _selectedEntity)
            {
                _selectedEntity = null;

                _messageManager.Publish(new MarkNode(null));
                _messageManager.Publish(new MarkPort(null));
                _messageManager.Publish(new MarkEdge(null));

                _messageManager.Publish(new OwnerClosed(this));
            }

        }



        private void OnEntitySelected(EntitySelected obj)
        {
            /*if (_selectedEntity != null && obj.Entity == null)
            {
                _services.Get<MessageManager>().Publish(new MarkNode(null));
                _services.Get<MessageManager>().Publish(new MarkPort(null));
                _services.Get<MessageManager>().Publish(new MarkEdge(null));

                _services.Get<MessageManager>().Publish(new OwnerClosed(this));
            }
            else*/ if (obj.Entity != _selectedEntity)
            {
                _messageManager.Publish(new ShowPropertiesRequest(new EntityInspectorControl(_services, obj.Entity), this));

                var attributeValue = obj.Entity.Attributes.TryGet(new SystemKey("Node Sequence"));
                if (attributeValue != null)
                {
                    var sequence = (GraphTrail)attributeValue;

                    _messageManager.Publish(new MarkNodes(sequence.Objects.OfType<Node>().ToList(), new Color(0, 148, 255)));
                    _messageManager.Publish(new MarkPorts(sequence.Objects.OfType<Port>().ToList(), new Color(0, 148, 255)));
                }

                _selectedEntity = obj.Entity;
            }

            //_selectedEntity = obj.Entity;
        }
    }
}