using System;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;

namespace Sceelix.MyNewEngineLibrary
{
    public enum Animal
    {
        Dog,
        Cat,
        Donkey
    }

    /// <summary>
    /// This tutorial shows the many basic parameter types that you can use and customize.
    /// 
    /// There are already multiple types of parameters available for use. Like inputs and outputs, parameters
    /// are read automatically into the node's signature.
    /// 
    /// Again, a good programming practice is to hide these fields (either private, protected or internal) and leave them readonly.
    /// 
    /// The actual field name is not important, but the label is - and must be unique within the class.
    /// </summary>
    [Procedure("c7051799-118d-4ac9-83ae-19d5fd55d0fc", Label = "Hello Parameters", Category = "MyTutorial")]
    public class HelloParametersProcedure : SystemProcedure
    {
        private readonly Output<IEntity> _output = new Output<IEntity>("Output");

        /// <summary>
        /// These are the basic parameter types.
        /// 
        /// By the way, these comments are also read automatically into parameter descriptions.
        /// </summary>
        private readonly BoolParameter _parameterBool = new BoolParameter("Bool Parameter", true);
        private readonly StringParameter _parameterString = new StringParameter("String Parameter", "");
        private readonly IntParameter _parameterInt = new IntParameter("Int Parameter", 0) { MinValue = 0, MaxValue = 10 };
        private readonly FloatParameter _parameterFloat = new FloatParameter("Float Parameter", 0) { MinValue = 0, MaxValue = 10 };
        private readonly DoubleParameter _parameterDouble = new DoubleParameter("Double Parameter", 0) { MinValue = 0, MaxValue = 10 };
        private readonly ChoiceParameter _parameterChoice = new ChoiceParameter("Choice Parameter", "Choice 2", "Choice 1", "Choice 2", "Choice 3");
        private readonly FileParameter _parameterFile = new FileParameter("File Parameter", "", ".jpg", ".jpeg", ".bmp", ".png");
        private readonly FolderParameter _parameterFolder = new FolderParameter("Folder Parameter", "");
        private readonly EnumChoiceParameter<Animal> _parameterEnum = new EnumChoiceParameter<Animal>("Enum Parameter", Animal.Dog);

        //The Geometry3D library introduces these new parameters
        //Oh, these types of comments are not read automatically - in some cases, you may prefer to use them
        private readonly Vector2DParameter _parameterVector2D = new Vector2DParameter("Vector2D Parameter", new Vector2D(1, 0));
        private readonly Vector3DParameter _parameterVector3D = new Vector3DParameter("Vector3D Parameter", new Vector3D(1, 0, 1));
        private readonly ColorParameter _parameterColor = new ColorParameter("Color Parameter", Color.Red);

        
        protected override void Run()
        {
            //some examples on how to access the parameter values
            bool boolValue = _parameterBool.Value;
            string stringValue = _parameterString.Value;
            string folderValue = _parameterFolder.Value;
            Animal animalValue = _parameterEnum.Value;

            //file loading is abstracted
            //you should define as type of data you are loading - String, String[], byte[], Stream are supported by default.
            var loadedText = Resources.Load<String>(_parameterFile.Value);
            
            Vector3D vector3DValue = _parameterVector3D.Value;
            Color colorValue = _parameterColor.Value;
        }
    }
}
