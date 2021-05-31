using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;

namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// This tutorial shows how to use structured parameters, namely compound and list parameters.
    /// </summary>
    [Procedure("d5451972-0300-41e0-b6ea-24ae403b339b", Label = "Hello More Parameters", Category = "MyTutorial")]
    public class HelloMoreParametersProcedure : SystemProcedure
    {
        //to create structured data, this is one way to do it
        private readonly CompoundParameter _parameterCompoundParameter = new CompoundParameter("Compound Parameter",
            new IntParameter("Parameter 1", 0) {Description = "Hello, this is a description."},
            new IntParameter("Parameter 2", 0){MinValue = 0, MaxValue = 100});

        /// <summary>
        /// this is another way to do it (see class declaration below)
        /// </summary>
        private readonly MyCompoundParameter _parameterMyCompoundParameter = new MyCompoundParameter("My Compound Parameter", false, true, true);

        /// <summary>
        /// now, lists allow you to create enumerations of parameters
        /// this is one way to do it
        /// you use lambda expressions to indicate how to instantiate the parameters
        /// </summary>
        private readonly ListParameter _parameterIntList = new ListParameter("List Parameter", () => new IntParameter("My Integer", 0));

        //for this list which has an abstract type defined, the user will have many options to choose from any derived types (located in this library or others)
        //Check below for details on its working.
        private readonly ListParameter<MyAbstractParameter> _parameterCompoundList = new ListParameter<MyAbstractParameter>("Compound List Parameter");

        //now, for the previous examples, there no item limit was applied
        //if you have a limit of 1 item, with only one possible option, a simple checkbox will appear to let you enable or disable the option
        private readonly ListParameter _parameterIntList2 = new ListParameter("No Limit List Parameter", () => new IntParameter("My Integer", 0)) { MaxSize = 1 };

        //if you have a limit of 1 item, with multiple possible options, it will materialize into a combobox
        //This selectListparameter is basically a list (with multiple options) with a Max item number of 1
        private readonly SelectListParameter<MyAbstractParameter> _parameterSelect = new SelectListParameter<MyAbstractParameter>("Combobox Parameter");


        protected override void Run()
        {
            //some examples on how to access the data
            var intValue = ((IntParameter) _parameterCompoundParameter["Parameter 1"]).Value;

            bool boolValue = _parameterMyCompoundParameter.ParameterMyCheck1.Value;

            foreach (var parameter in _parameterIntList.Items.OfType<IntParameter>())
                Logger.Log(parameter.Value.ToString());

            //regardless of the item number, we can always use the 'foreach' to iterate
            foreach (var parameter in _parameterCompoundList.Items)
            {
                if (parameter is MyDerivedParameter)
                {
                    //we could go recursively here
                }
                else if(parameter is MyOtherDerivedParameter)
                {
                    ((MyOtherDerivedParameter) parameter).DoSomething();
                    //do something else
                }
            }
        }

        /// <summary>
        /// This comment is read automatically.
        /// </summary>
        public class MyCompoundParameter : CompoundParameter
        {
            internal readonly BoolParameter ParameterMyCheck1 = new BoolParameter("Check 1", false);
            internal readonly BoolParameter ParameterMyCheck2 = new BoolParameter("Check 2", false);
            internal readonly BoolParameter ParameterMyCheck3 = new BoolParameter("Check 3", false);

            public MyCompoundParameter(string label, bool check1, bool check2, bool check3) 
                : base(label)
            {
                ParameterMyCheck1.Value = check1;
                ParameterMyCheck2.Value = check2;
                ParameterMyCheck3.Value = check3;
            }
        }

        /// <summary>
        /// This abstract class is used in the ListParameter above.
        /// </summary>
        public abstract class MyAbstractParameter : CompoundParameter
        {
            private readonly StringParameter _parameterString1 = new StringParameter("String 1", "Hello");
            private readonly StringParameter _parameterString2 = new StringParameter("String 2", "Hello, as well");

            public MyAbstractParameter(string label)
                : base(label)
            {
            }
        }

        public class MyDerivedParameter : MyAbstractParameter
        {
            /// <summary>
            /// We can build recursive lists like this!
            /// </summary>
            private readonly ListParameter<MyAbstractParameter> _parameterRecursiveList = new ListParameter<MyAbstractParameter>("Recursive List");

            /// <summary>
            /// It must contain a parameterless constructor with the label already defined!
            /// </summary>
            public MyDerivedParameter()
                : base("My Derived Parameter")
            {
            }
        }

        public class MyOtherDerivedParameter : MyAbstractParameter
        {
            /// <summary>
            /// We can build recursive lists like this!
            /// </summary>
            private readonly FileParameter _parameterFile = new FileParameter("File", "");
            private readonly DoubleParameter _parameterDouble = new DoubleParameter("Double", 2.0);

            //we can also define more inputs and outputs here!
            //when new instances of this parameter are added to the list, these inputs/outputs
            //are added to the node
            private readonly CollectiveInput<IEntity> _dataInputs = new CollectiveInput<IEntity>("Data Input");
            private readonly Output<IEntity> _dataOutput = new Output<IEntity>("Data Input");

            /// <summary>
            /// It must contain a parameterless constructor with the label already defined!
            /// </summary>
            public MyOtherDerivedParameter()
                : base("My Other Parameter")
            {
            }


            public void DoSomething()
            {
                var entities = _dataInputs.Read();
            }
        }
    }
}
