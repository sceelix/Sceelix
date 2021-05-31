using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Collections;
using Sceelix.Core;
using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Core.Environments;
using Sceelix.Core.Procedures;
using Sceelix.Core.Resources;
using Sceelix.Loading;
using Sceelix.Meshes.Data;

namespace Sceelix.MyExternalApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            //Now, we want to include all the libraries that are deployed with the binaries
            //If they are not directly referenced by the project and its code, they won't be loaded into the default domain,
            //so we need to do it here.
            SceelixDomain.LoadAssembliesFrom("");

            //Call the engine initialization function at the beginning. 
            //This will initialize libraries, procedures, entities, etc. of all the libraries in the domain.
            //You can also call the overload to load assemblies individually
            //just make sure you initialize all the libraries that you'll be using ahead in your application.
            EngineManager.Initialize();

            //now you can use procedure calls in your application
            //system procedures are those programmed in C#
            CallSystemProcedureSample();

            //you can also use graph procedure calls in your application
            //graphs procedures are those created using the Sceelix Designer
            CallGraphProcedureSample();

            
            Console.WriteLine("Finished Execution.");
            Console.ReadLine();
        }

        


        private static void CallSystemProcedureSample()
        {
            //you should create a LoadEnvironment instance and pass it to your procedures, so that it can find resources (files, textures, etc.)
            ProcedureEnvironment environment = new ProcedureEnvironment(new ResourceManager(@"C:\MyProjectFolder"));

            //create an instance of the system procedure and set the loadenvironment
            LogProcedure procedure = new LogProcedure();
            procedure.Environment = environment;

            //parameters are set in this way, using the same labels as viewed in the Sceelix Designer
            procedure.Parameters["Inputs"].Set("Single");

            //for lists, there are several options for settings data
            //1) Set a string, which will add an item with that label to the list
            //alternatively, you could use the same Set function for lists, which does the same as Add
            procedure.Parameters["Messages"].Set("Text");
            procedure.Parameters["Messages"].Parameters.Last().Set("Hello!");

            //2) Set an enumerable of strings, which will add several items with those labels to the list
            procedure.Parameters["Messages"].Set(new[] { "Text", "Text"});
            procedure.Parameters["Messages"].Parameters[1].Set("This is a second message.");
            procedure.Parameters["Messages"].Parameters[2].Set("This is a third message.");

            //You can also set data within a loop
            var listParameter = procedure.Parameters["Messages"];
            for (int i = 0; i < 5; i++)
            {
                listParameter.Set("Text");

                //you can set expressions (and reference attribute values) using the overload of the set function.
                listParameter.Parameters[i].SetExpression((inputData, currentEntity) => String.Format("The value of the attribute is '{0}'", inputData.Get(new GlobalAttributeKey("MyAttribute"), true)));
            }

            //to see what's stored in the parameter, you can always get it
            var data = listParameter.Get();
            Console.WriteLine(data);

            //3) You can use Sceelix's associative array, the Sceelist
            //bu setting it, you'll replace all the above
            SceeList sceelist = new SceeList(new KeyValuePair<string, object>("Text", "Message 4."), new KeyValuePair<string, object>("Text", "Message 5."), new KeyValuePair<string, object>("Text", "Message 6."));
            //Uncomment to see the result
            //listParameter.Set(sceelist);

            

            //Now, to set the inputs
            //let's create a new entity
            var newEntity = new Entity();
            newEntity.Attributes.Add(new GlobalAttributeKey("MyAttribute"), 123);

            //in this case, the input is tied to "Single" parameter, so you would need to do this.
            procedure.Parameters["Inputs"].Parameters["Single"].Inputs["Single"].Enqueue(newEntity);

            //or, after you have set all the parameters, you can call this function
            //to set all the ports and then use the procedure.Inputs field
            procedure.UpdateParameterPorts();

            var otherEntity = new Entity();
            otherEntity.Attributes.Add(new GlobalAttributeKey("MyAttribute"), "This is another value!");
            procedure.Inputs[0].Enqueue(otherEntity);
            
            //once we have finished the parameterization, execute it!
            //it will execute twice since we have added 2 entities to the input
            procedure.Execute();

            //now we can get the data from the outputs
            //this peeks (but not removes) one item from the first ouput
            IEntity entity = procedure.Outputs[0].Peek();

            //this peeks all the data from all the output ports
            IEnumerable<IEntity> entities = procedure.Outputs.PeekAll();

            //this gets (and removes) all the data from all the output ports
            IEnumerable<IEntity> poppedEntities = procedure.Outputs.DequeueAll();
        }



        private static void CallGraphProcedureSample()
        {
            //for graph procedures, you must really create a LoadEnvironment first, indicating the base folder location
            //all inside graph references (files, textures, subgraphs) are relative to the project folder
            ProcedureEnvironment environment = new ProcedureEnvironment(new ResourceManager("SampleProject"));

            //create a graph procedure 
            GraphProcedure procedure = GraphProcedure.FromPath(environment,"Card.slxg");

            //set parameters and inputs the same way as before
            procedure.Parameters["Height"].Set(20);

            //once we have finished the parameterization, execute it!
            procedure.Execute();

            //you can get the data from the output ports like the previous example
            IEnumerable<MeshEntity> meshes = procedure.Outputs.DequeueAll().OfType<MeshEntity>();
        }
    }
}
