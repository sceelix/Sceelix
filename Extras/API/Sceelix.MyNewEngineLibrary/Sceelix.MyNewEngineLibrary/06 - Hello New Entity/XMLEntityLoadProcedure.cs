using System.Xml;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;

namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// This is just a simple example on how to make a procedure that uses out new entity type.
    /// </summary>
    [Procedure("c1dc238d-ae56-4bee-934f-3b002b557396", Label = "XMLEntity Load", Category = "MyTutorial")]
    public class XMLEntityLoadProcedure : SystemProcedure
    {
        private readonly Output<IEntity> _output = new Output<IEntity>("Output");

        private readonly FileParameter _fileParameter = new FileParameter("Path", "",".xml");

        protected override void Run()
        {
            //we'll just use the XMLDocument class to load the xml from the file
            var document = new XmlDocument();

            //load the content
            document.Load(_fileParameter.Value);

            //create the entity and send it to the output
            _output.Write(new XMLEntity(document));
        }
    }
}
