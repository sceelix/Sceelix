using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using Sceelix.Designer.Actors.Components;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Paths.Components;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Renderer3D.Settings;
using Sceelix.Designer.Renderer3D.Translators;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Logging;
using Sceelix.Paths.Data;

namespace Sceelix.Designer.Paths.Translators
{
    [EntityTranslator(typeof(PathEntity))]
    public class PathTranslator : EntityTranslator<PathEntity>, IServiceable
    {
        public static readonly Vector3F DefaultStrokeColor = new Vector3F(0, 0, 0);
        public static readonly float DefaultStrokeThickness = 5;
        public static readonly Vector3F SelectedStrokeColor = new Vector3F(1, 1, 0);
        public static readonly float SelectedStrokeThickness = 10;
        
        private Renderer3DSettings _render3DSettings;
        private RenderTargetControl _renderTargetControl;


        public void Initialize(IServiceLocator services)
        {
            _render3DSettings = services.Get<SettingsManager>().Get<Renderer3DSettings>();
            _render3DSettings.ShowPathConnectivity.Changed += UpdateRender;
            _render3DSettings.ShowPathDirection.Changed += UpdateRender;

            _renderTargetControl = services.Get<RenderTargetControl>();
        }



        private void UpdateRender(ApplicationField<bool> field, bool oldValue, bool newValue)
        {
            _renderTargetControl.ShouldRender = true;
        }



        public override void Process(List<PathEntity> entities, EntityObjectDomain entityObjectDomain)
        {
            foreach (PathEntity pathEntity in entities)
            {
                if (pathEntity.Edges.Any())
                {
                    var figureNode = CalculateFigureNode(pathEntity);
                    //figureNode.PoseWorld = new Pose(DigitalRuneUtils.ZUpRotation);

                    EntityObject componentObject = new EntityObject(entityObjectDomain);
                    componentObject.AddComponent(new SceneComponent(figureNode));
                    componentObject.AddComponent(new CollisionComponent(figureNode.Figure.HitShape));//, new Pose(DigitalRuneUtils.ZUpRotation)
                    componentObject.AddComponent(new SelectableEntityComponent(pathEntity));
                    componentObject.AddComponent(new CollisionHighlightComponent());
                    componentObject.AddComponent(new ScopeHighlightComponent(pathEntity.BoxScope));
                    componentObject.AddComponent(new PathDrawComponent(pathEntity));

                    entityObjectDomain.ComponentObjects.Add(componentObject);
                }
                else
                {
                    entityObjectDomain.Logger.Log("Could not display Path because it contains no edges.", LogType.Warning);
                }
                /*try
                {*/

                /*}
                catch (Exception ex)
                {
                    entityObjectDomain.Environment.Log("Could not load pathentity. " + ex.Message,LogType.Error);
                }*/
            }
        }



        private FigureNode CalculateFigureNode(PathEntity entity)
        {
            var majorGridLines = new PathFigure3F();

            foreach (PathEdge line in entity.Edges)
            {
                majorGridLines.Segments.Add(new LineSegment3F
                {
                    Point1 = line.Source.Position.ToVector3F(), //DigitalRuneConvert.ZUpRotation * 
                    Point2 = line.Target.Position.ToVector3F() //DigitalRuneConvert.ZUpRotation * 
                });
            }

            var edgeLinesNode = new FigureNode(majorGridLines)
            {
                Name = "Path Lines",
                PoseLocal = Pose.Identity,
                StrokeThickness = DefaultStrokeThickness,
                StrokeColor = DefaultStrokeColor,
                StrokeAlpha = 1f,
            };

            return edgeLinesNode;
        }
    }
}