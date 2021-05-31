using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Procedures;
using Sceelix.Meshes.Data;

namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// This sample demonstrates how to define and use input and output ports in a node.
    /// 
    /// All that needs to be done is define them in the body of the class and they will be read automatically
    /// into the node's signature.
    /// 
    /// The type of input or output can be of any type that implements IEntity.
    /// 
    /// A good programming practice is to hide these fields (either private, protected or internal) and leave them readonly.
    /// 
    /// Inputs can be of 2 natures: single or collective, meaning that they accept one entity 
    /// or a collection of entities at once, respectively.
    /// 
    /// You can define multiple inputs, but their labels must have unique names so that they do 
    /// not conflict. Same goes for outputs.
    /// </summary>
    [Procedure("cac682ce-f0d1-45c4-97db-8eea46709864", Label = "Hello IO", Category = "MyTutorial")]
    public class HelloIOProcedure : SystemProcedure
    {
        /// <summary>
        /// Description are also read automatically from these comment blocks.
        /// </summary>
        private readonly SingleInput<IEntity> _input = new SingleInput<IEntity>("Entity");

        /// <summary>
        /// This is a collective port that will accept a collection of meshes at one. 
        /// </summary>
        private readonly CollectiveInput<MeshEntity> _inputMesh = new CollectiveInput<MeshEntity>("Meshes");

        /// <summary>
        /// We could also have multiple outputs, but for this example we'll have just one.
        /// </summary>
        private readonly Output<IEntity> _output = new Output<IEntity>("All Entities");


        protected override void Run()
        {
            var entity = _input.Read();
            var meshEnumerable = _inputMesh.Read();

            //we could do something with our entities here

            //and here we output them.
            _output.Write(entity);
            _output.Write(meshEnumerable);
        }
    }
}
