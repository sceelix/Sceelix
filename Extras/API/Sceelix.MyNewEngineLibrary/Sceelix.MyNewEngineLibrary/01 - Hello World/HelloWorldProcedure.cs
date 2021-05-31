using Sceelix.Core.Annotations;
using Sceelix.Core.Procedures;

namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// Definition of a procedure, which is read into a node.
    /// 
    /// All procedures must inherit from SystemProcedure and carry the "Procedure" attribute.
    /// The attribute contains a Guid, which uniquely identifies this procedure.
    /// Other details can also be defined in this attribute.
    /// 
    /// If the description field of the attribute is not defined, this comment block will automatically be set as the procedure description.
    /// </summary>
    [Procedure("f24271aa-a14d-47d4-98c0-5d32e2ab3dc4", Label = "Hello World", Author = "MyName", Category = "MyTutorial", Tags = "Hello, Other")]
    public class HelloWorldProcedure : SystemProcedure
    {
        /// <summary>
        /// When a procedure is executed, this function is called.
        /// In this particular case, since no inputs have been defined, 
        /// this function will be called only once.
        /// </summary>
        protected override void Run()
        {
            //The load environment is specific of the platform 
            //it has information of the environment where the procedure is being run
            //there is only one instance 
            Logger.Log("Hello World");
        }
    }
}
