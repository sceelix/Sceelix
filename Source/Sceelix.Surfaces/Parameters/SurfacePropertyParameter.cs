using System.Linq;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Parameters
{
    /// <summary>
    /// Reads/calculates properties from surface entities.
    /// </summary>
    /// <seealso cref="Sceelix.Core.Procedures.PropertyProcedure.PropertyParameter" />
    public class SurfacePropertyParameter : PropertyProcedure.PropertyParameter
    {
        /// <summary>
        /// Surface entity from which to read the properties.
        /// </summary>
        private readonly SingleInput<SurfaceEntity> _input = new SingleInput<SurfaceEntity>("Input");

        /// <summary>
        /// Surface entity from which the properties were read.
        /// </summary>
        private readonly Output<SurfaceEntity> _output = new Output<SurfaceEntity>("Output");

        /// <summary>
        /// Size of the each surface (square) cell.
        /// </summary>
        private readonly AttributeParameter<float> _parameterCellSize = new AttributeParameter<float>("Cell Size", AttributeAccess.Write);

        /// <summary>
        /// Number of columns of the surface.
        /// </summary>
        private readonly AttributeParameter<int> _parameterNumColumns = new AttributeParameter<int>("Num. Columns", AttributeAccess.Write);

        /// <summary>
        /// Number of rows of the surface.
        /// </summary>
        private readonly AttributeParameter<int> _parameterNumRows = new AttributeParameter<int>("Num. Rows", AttributeAccess.Write);

        /// <summary>
        /// Number of layers in the surface.
        /// </summary>
        private readonly AttributeParameter<float> _parameterNumLayers = new AttributeParameter<float>("Num. Layers", AttributeAccess.Write);



        public SurfacePropertyParameter()
            : base("Surface")
        {
        }



        public override void Run()
        {
            var surfaceEntity = _input.Read();

            //no heavy calculations are needed in this case, to try the assignment anyway 
            //(the attributeparameter does the check for mapping inside)
            _parameterNumColumns[surfaceEntity] = surfaceEntity.NumColumns;
            _parameterNumRows[surfaceEntity] = surfaceEntity.NumRows;
            _parameterCellSize[surfaceEntity] = surfaceEntity.CellSize;
            _parameterNumLayers[surfaceEntity] = surfaceEntity.Layers.Count();

            _output.Write(surfaceEntity);
        }
    }
}