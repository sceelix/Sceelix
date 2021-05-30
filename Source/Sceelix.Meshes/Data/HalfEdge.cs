namespace Sceelix.Meshes.Data
{
    public class HalfEdge
    {
        public HalfEdge(Vertex destination, Face face)
        {
            Destination = destination;
            Face = face;
        }



        public Vertex Destination
        {
            get;
        }


        public Face Face
        {
            get;
        }


        public HalfEdge Next => new HalfEdge(Destination[Face].Next, Face);


        public HalfEdge Previous => new HalfEdge(Destination[Face].Previous, Face);


        /*struct HE_edge
        {

            HE_vert* vert;   // vertex at the end of the half-edge
            HE_edge* pair;   // oppositely oriented adjacent half-edge 
            HE_face* face;   // face the half-edge borders
            HE_edge* next;   // next half-edge around the face

        };*/
    }
}