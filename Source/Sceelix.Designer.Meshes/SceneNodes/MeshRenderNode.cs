using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Meshes.Translators.Materials;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Utils;
using Sceelix.Meshes.Data;
using Material = Sceelix.Actors.Data.Material;

namespace Sceelix.Designer.Meshes.SceneNodes
{
    public class MeshRenderNode : SceneNode
    {
        private readonly FigureNode _faceEdgesNode;
        

        public MeshRenderNode(IEnumerable<MeshEntity> meshEntities, Dictionary<Type, IMeshMaterialTranslator> meshCreators, ContentLoader contentLoader)
        {
            var meshList = meshEntities.ToList();

            ProcessFaces(meshList.SelectMany(x => x.Faces), meshCreators, contentLoader);
            
            if (BuildPlatform.IsWindows)
            {

                List<Edge> faceEdges = meshList.SelectMany(val => val.FaceEdges.Distinct()).ToList();
                if (faceEdges.Any())
                {
                    Children.Add(_faceEdgesNode = LoadEdgeNode(faceEdges, 3, new Vector3F(0)));
                    _faceEdgesNode.IsEnabled = false;
                }
            }
            else
            {
                _faceEdgesNode = new FigureNode(new PathFigure3F());
            }
        }


        private void ProcessFaces(IEnumerable<Face> allFaces, Dictionary<Type, IMeshMaterialTranslator> meshCreators, ContentLoader contentLoader)
        {
            this.Children = new SceneNodeCollection();

            //okay, now that we have all the objects, we can group them by material
            List<Face> faces = allFaces.ToList();
            if (faces.Any())
            {
                IEnumerable<IGrouping<Material, Face>> facesByMaterial = faces.GroupBy(val => val.Material);

                foreach (IGrouping<Material, Face> grouping in facesByMaterial)
                {
                    IMeshMaterialTranslator materialTranslator = meshCreators[grouping.Key.GetType()];

                    var sceneNode = materialTranslator.CreateSceneNode(contentLoader, grouping.Key, grouping.ToList());

                    Children.Add(sceneNode);
                }
            }
        }



        private MeshRenderNode(MeshRenderNode meshRenderNode)
        {
            this.Children = new SceneNodeCollection();
            this.PoseWorld = meshRenderNode.PoseWorld;
            this.ScaleLocal = meshRenderNode.ScaleLocal;

            foreach (SceneNode sceneNode in meshRenderNode.Children)
            {
                Children.Add(sceneNode.Clone());
            }
        }

        



        public bool IsSticky
        {
            get;
            set;
        }



        public bool EdgesVisible
        {
            get { return _faceEdgesNode.IsEnabled; }
            set
            {
                _faceEdgesNode.IsEnabled = value;
                //_faceBoundaryEdgesNode.IsEnabled = value;
            }
        }



        private FigureNode LoadEdgeNode(IEnumerable<Edge> lines, float stokeThickness, Vector3F strokeColor)
        {
            var majorGridLines = new PathFigure3F();

            foreach (Edge line in lines)
            {
                majorGridLines.Segments.Add(new LineSegment3F
                {
                    Point1 = line.V0.Position.ToVector3F(), //DigitalRuneConvert.ZUpRotation * 
                    Point2 = line.V1.Position.ToVector3F() //DigitalRuneConvert.ZUpRotation * 
                });
            }

            var edgeLinesNode = new FigureNode(majorGridLines)
            {
                Name = "Major grid lines",
                PoseLocal = Pose.Identity,
                StrokeThickness = stokeThickness,
                StrokeColor = strokeColor,
                StrokeAlpha = 1f,
            };

            return edgeLinesNode;
        }


        protected override void CloneCore(SceneNode source)
        {
            base.CloneCore(source);
        }



        protected override SceneNode CreateInstanceCore()
        {
            return new MeshRenderNode(this);
        }


        /// <summary>
        /// Releases the unmanaged resources used by an instance of the DigitalRune.Graphics.SceneGraph.SceneNode class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"> true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        /// <param name="disposeData"> true to dispose scene nodes including data objects; false to dispose only scene nodes but preserve the data objects.</param>
        protected override void Dispose(bool disposing, bool disposeData)
        {
            foreach (SceneNode sceneNode in Children)
                sceneNode.Dispose(disposeData);


            base.Dispose(disposing, disposeData);
        }
    }
}