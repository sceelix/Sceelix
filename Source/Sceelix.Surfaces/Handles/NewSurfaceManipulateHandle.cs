using Sceelix.Core.Handles;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Handles
{
    public class NewSurfaceManipulateHandle : VisualHandle
    {
        public NewSurfaceManipulateHandle(SurfaceEntity surfaceEntity, float[] originalValues, string fullPath)
        {
            SurfaceEntity = surfaceEntity;
            OriginalValues = originalValues;
            FullPath = fullPath;
        }



        public string FullPath
        {
            get;
        }


        public float[] OriginalValues
        {
            get;
        }


        public SurfaceEntity SurfaceEntity
        {
            get;
        }
    }
}