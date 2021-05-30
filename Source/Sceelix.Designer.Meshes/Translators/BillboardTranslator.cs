using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Renderer3D.Translators;
using Sceelix.Designer.Utils;
using Sceelix.Meshes.Data;

namespace Sceelix.Designer.Meshes.Translators
{
    [EntityTranslator(typeof(BillboardEntity))]
    public class BillboardTranslator : EntityTranslator<BillboardEntity>
    {

        public override void Process(List<BillboardEntity> entities, EntityObjectDomain entityObjectDomain)
        {
            foreach (var billboardGroup in entities.GroupBy(x => x.Image))
            {
                var texture = new PackedTexture(entityObjectDomain.ContentLoader.LoadTexture(billboardGroup.Key));

                foreach (BillboardEntity billboardEntity in billboardGroup)
                {
                    var vector3D = billboardEntity.BoxScope.Sizes;
                    var color = Vector3F.FromXna(billboardEntity.Color.ToXnaVector()/255f);
                    
                    color *= BuildPlatform.IsWindows ? new Vector3F(0.5f, 0.5f, 0.5f) : new Vector3F(2f, 2f, 2f);

                    var imageBillboard = new ImageBillboard(texture)
                    {
                        Size = new Vector2F(vector3D.X, vector3D.Z),
                        /*,Color = new Vector3F(0.1f, 0.1f, 0.1f), BlendMode = 1*/
                        Color = color, //*new Vector3F(0.5f, 0.5f, 0.5f),
                        BlendMode = 1
                    };

                    var billboardNode = new BillboardNode(imageBillboard)
                    {
                        PoseWorld = new Pose((billboardEntity.BoxScope.Translation + billboardEntity.BoxScope.SizedZAxis/2f).ToVector3F()),
                    };


                    EntityObject componentObject = new EntityObject(entityObjectDomain);
                    componentObject.AddComponent(new SceneComponent(billboardNode));
                    componentObject.AddComponent(new CollisionComponent(billboardNode.Shape, billboardNode.PoseLocal));
                    componentObject.AddComponent(new SelectableEntityComponent(billboardEntity));
                    componentObject.AddComponent(new CollisionHighlightComponent());

                    entityObjectDomain.ComponentObjects.Add(componentObject);
                }
            }
        }
    }
}