using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Core.Data;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.Inspector.Entities
{
    public class EntityInspectorControl : ContentControl
    {
        private readonly IEntity _entity;
        private readonly FlexibleStackPanel _mainPanel;



        public EntityInspectorControl(IServiceLocator services, IEntity entity)
        {
            _entity = entity;

            _mainPanel = new FlexibleStackPanel()
            {
                Margin = new Vector4F(5),
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            _mainPanel.Children.Add(new GeneralController(entity));
            _mainPanel.Children.Add(new PropertyController(entity));

            Content = _mainPanel;
        }



        public IEntity Entity
        {
            get { return _entity; }
        }
    }
}