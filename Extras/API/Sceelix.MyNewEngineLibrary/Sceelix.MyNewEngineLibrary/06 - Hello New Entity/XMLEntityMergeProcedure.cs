using System.Linq;
using System.Xml;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;

namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// This is just a simple example on how to make a procedure that uses out new entity type.
    /// </summary>
    [Procedure("824f6c7b-d96e-406b-b4dd-812745b2904c", Label = "XMLEntity Merge", Category = "MyTutorial")]
    public class XMLEntityMergeProcedure : SystemProcedure
    {
        private readonly SingleInput<XMLEntity> _input = new SingleInput<XMLEntity>("Parent");
        private readonly SingleInput<XMLEntity> _input2 = new SingleInput<XMLEntity>("Child Node");
        private readonly Output<XMLEntity> _output = new Output<XMLEntity>("Output");

        /// <summary>
        /// This will determine if the attributes from the merged item should be passed to the parent.
        /// </summary>
        private readonly ChoiceParameter _choiceParameter = new ChoiceParameter("Attributes", "Complement", "None", "Replace", "Complement");

        protected override void Run()
        {
            var parentEntity = _input.Read();
            var childEntity = _input2.Read();

            //first we merge the 
            if(_choiceParameter.Value == "Complement")
                childEntity.Attributes.ComplementAttributesTo(parentEntity.Attributes);
            else if(_choiceParameter.Value == "Replace")
                childEntity.Attributes.ReplaceAttributesOn(parentEntity.Attributes);

            //now, we append the child
            foreach (var newChild in childEntity.Document.ChildNodes.OfType<XmlNode>())
                parentEntity.Document.ImportNode(newChild, true);
            
            _output.Write(parentEntity);
        }
    }
}
