using System;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;


namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// In the scope of your library maintenance, you may decide to add/modify/fix existing procedures.
    ///  
    /// The purpose of this sample is to give some insight on how you can perform changes to
    /// your procedures safely without completely breaking already existing graphs that may
    /// already been using those procedures.
    /// 
    /// In this sense, there are some do's and don'ts:
    /// 
    /// DO: You can change the name/label/namespace/location of the procedure class. Sceelix
    /// will look for the unique Guid if it fails to find by class/namespace.
    /// 
    /// DON'T: You should not change the Guid, for the reason above.
    /// </summary>
    [Obsolete("This was replaced by XXX")]
    [Procedure("7be13450-4a98-441a-b877-afdfb30eb397", Label = "My Obsolete Procedure", Category = "MyTutorial")]
    public class MyObsoleteProcedure : SystemProcedure
    {
        /// <summary>
        /// DO: You can change the variable name of a parameter, input or output. 
        ///     You can also change its order, i.e. you can put the _output before the _input, or put them in the end of the class definition.
        /// 
        /// DON'T: You should not change the LABEL of the parameter, as it is the key that the graph definition uses
        ///        to match the saved parameter value to the parameter. 
        /// </summary>
        private readonly SingleInput<IEntity> _input = new SingleInput<IEntity>("Input");
        private readonly Output<IEntity> _output = new Output<IEntity>("Output");

        /// <summary>
        /// DO: You can change the default value of a parameter. Existing values have their own copies of initial values so the changes
        ///     will only affect newly added parameters.
        ///
        /// DON'T: You should not change the type of parameter. Also, avoid changing any of its properties (max values, min values, etc.) unless
        ///        you are really sure that they won't break existing graphs.
        /// </summary>
        private readonly StringParameter _parameter = new StringParameter("Parameter", "");
        private readonly IntParameter _secondParameter = new IntParameter("Second Parameter", 0) { MinValue = 0, MaxValue = 10 };


        /// <summary>
        /// In other words, if you would wish to alter the procedure in the ways that are here described as DON'TS, you should
        /// create a new node and mark this one as Obsolete() by adding the ObsoleteAttribute at the class definition (see above).
        /// 
        /// Obsolete nodes are identified as such in the existing graphs and do not appear in the node list anymore.
        /// </summary>
        protected override void Run()
        {

        }
    }
}
