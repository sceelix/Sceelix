using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Resources;
using Sceelix.Extensions;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// Creates random values according to certain types, range 
    /// and specifications. Stores the result into attributes 
    /// of the flowing entities.
    /// </summary>
    [Procedure("92d65cb9-ef76-40fa-ad69-32707b893e36", Label = "Random", Category = "Basic")]
    public class RandomProcedure : SystemProcedure
    {
        /// <summary>
        /// The type of input port. <br/>
        /// Setting a <b>Single</b> (circle) input means that the node will be executed once per entity. Useful to choose a different seed for each entity. <br/>
        /// Setting a <b>Collective</b> (square) input means that the node will be executed once for all entities. Useful to choose one starting seed for the whole set of entities (yet each entity will have a different random value).
        /// </summary>
        private readonly SingleOrCollectiveInputChoiceParameter<IEntity> _parameterInput = new SingleOrCollectiveInputChoiceParameter<IEntity>("Inputs", "Collective");

        /// <summary>
        /// Entities that were sent to the input.
        /// </summary>
        private readonly Output<IEntity> _output = new Output<IEntity>("Output");

        /// <summary>
        /// List of random values to create. There are many types of random values that can be created, all of which are stored to entity attributes so as to be used in later nodes.
        /// </summary>
        private readonly ListParameter<RandomParameter> _parameterRandomAttributes = new ListParameter<RandomParameter>("Attributes");

        /// <summary>
        /// Seed that controls the random generator.
        /// </summary>
        private readonly IntParameter _parameterSeed = new IntParameter("Seed", 1000);



        protected override void Run()
        {
            var entitites = _parameterInput.Read().ToList();

            //_randomGenerator = _randomGenerator ?? new Random(SeedParameter.Value);
            Random randomGenerator = new Random(_parameterSeed.Value);

            //passes the same list to all the 
            foreach (var randomAttributeParameter in _parameterRandomAttributes.Items)
                randomAttributeParameter.Execute(randomGenerator, entitites);

            _output.Write(entitites);
        }



        #region Abstract Random Parameter

        public abstract class RandomParameter : CompoundParameter
        {
            protected RandomParameter(string label)
                : base(label)
            {
            }



            public abstract void Execute(Random random, List<IEntity> entities);
        }

        #endregion

        #region Int

        /// <summary>
        /// Generates random integer numbers within a specified range.
        /// </summary>
        public class IntRandomParameter : RandomParameter
        {
            /// <summary>
            /// Inclusive lower bound of the random number returned.
            /// </summary>
            private readonly IntParameter _parameterMin = new IntParameter("Minimum", 0);

            /// <summary>
            /// Exclusive upper bound of the random number returned.
            /// </summary>
            private readonly IntParameter _parameterMax = new IntParameter("Maximum", 10);

            /// <summary>
            /// Attribute where to store the random value 
            /// </summary>
            private readonly AttributeParameter<int> _attributeValue = new AttributeParameter<int>("Value", AttributeAccess.Write);



            public IntRandomParameter()
                : base("Integer")
            {
            }



            public override void Execute(Random random, List<IEntity> entities)
            {
                foreach (var entity in entities)
                    _attributeValue[entity] = random.Next(_parameterMin.Value, _parameterMax.Value);
            }
        }

        #endregion

        #region Bool

        /// <summary>
        /// Generates boolean values, either true or false.
        /// </summary>
        /// <seealso cref="Sceelix.Core.Procedures.RandomProcedure.RandomParameter" />
        public class BoolRandomParameter : RandomParameter
        {
            /// <summary>
            /// Attribute where to store the random boolean.
            /// </summary>
            private readonly AttributeParameter<bool> _attributeValue = new AttributeParameter<bool>("Value", AttributeAccess.Write);



            public BoolRandomParameter()
                : base("Bool")
            {
            }



            public override void Execute(Random random, List<IEntity> entities)
            {
                foreach (var entity in entities)
                    _attributeValue[entity] = random.NextDouble() > 0.5f;
            }
        }

        #endregion

        #region Float

        /// <summary>
        /// Generates random floating-point numbers within a specified range.
        /// </summary>
        public class FloatRandomParameter : RandomParameter
        {
            /// <summary>
            /// Inclusive lower bound of the random number returned.
            /// </summary>
            private readonly FloatParameter _parameterMin = new FloatParameter("Minimum", 0);

            /// <summary>
            /// Exclusive upper bound of the random number returned.
            /// </summary>
            private readonly FloatParameter _parameterMax = new FloatParameter("Maximum", 10);

            /// <summary>
            /// Attribute where to store the random value 
            /// </summary>
            private readonly AttributeParameter<float> _attributeValue = new AttributeParameter<float>("Value", AttributeAccess.Write);



            public FloatRandomParameter()
                : base("Float")
            {
            }



            public override void Execute(Random random, List<IEntity> entities)
            {
                var difference = _parameterMax.Value - _parameterMin.Value;

                foreach (var entity in entities)
                    _attributeValue[entity] = Convert.ToSingle(_parameterMin.Value + random.NextDouble() * difference);
            }
        }

        #endregion

        #region Double

        /// <summary>
        /// Generates random double-precision numbers within a specified range.
        /// </summary>
        public class DoubleRandomParameter : RandomParameter
        {
            /// <summary>
            /// Inclusive lower bound of the random number returned.
            /// </summary>
            private readonly DoubleParameter _parameterMin = new DoubleParameter("Minimum", 0);

            /// <summary>
            /// Exclusive upper bound of the random number returned.
            /// </summary>
            private readonly DoubleParameter _parameterMax = new DoubleParameter("Maximum", 10);


            /// <summary>
            /// Attribute where to store the random value 
            /// </summary>
            private readonly AttributeParameter<double> _attributeValue = new AttributeParameter<double>("Value", AttributeAccess.Write);



            public DoubleRandomParameter()
                : base("Double")
            {
            }



            public override void Execute(Random random, List<IEntity> entities)
            {
                var difference = _parameterMax.Value - _parameterMin.Value;

                foreach (var entity in entities)
                    _attributeValue[entity] = Convert.ToSingle(_parameterMin.Value + random.NextDouble() * difference);
            }
        }

        #endregion

        #region File

        /// <summary>
        /// Selects a random file path existing inside a given folder.
        /// </summary>
        public class FileRandomParameter : RandomParameter
        {
            /// <summary>
            /// Folder from which to extract the random file names.
            /// </summary>
            private readonly FolderParameter _parameterFolder = new FolderParameter("Folder", "");

            /// <summary>
            /// Attribute where to store the random file name in.
            /// </summary>
            private readonly AttributeParameter<string> _attributeValue = new AttributeParameter<string>("File Name", AttributeAccess.Write);


            private string[] _fileNames;
            private string _lastPathToFolder;



            public FileRandomParameter()
                : base("File")
            {
            }



            public override void Execute(Random random, List<IEntity> entities)
            {
                string pathToFolder = _parameterFolder.Value; //Path.Combine(loadEnvironment.projectFolder, FolderParameter.Value);

                //avoid calling the heavy Directory.GetFiles function every time
                if (_lastPathToFolder != pathToFolder)
                {
                    _fileNames = ProcedureEnvironment.GetService<IResourceManager>().GetFilePaths(pathToFolder);
                    //if(Path.IsPathRooted(pathToFolder))

                    _lastPathToFolder = pathToFolder;
                }

                foreach (var entity in entities)
                {
                    string selectedItem = Path.GetFileName(_fileNames[random.Next(0, _fileNames.Length)]);

                    _attributeValue[entity] = Path.Combine(_parameterFolder.Value, selectedItem);
                }
            }
        }

        #endregion

        #region From List

        /// <summary>
        /// Selects a random item from a give list of items.
        /// </summary>
        public class ListRandomParameter : RandomParameter
        {
            /// <summary>
            /// List from which to extract the random value.
            /// </summary>
            private readonly ListParameter _listParameter = new ListParameter("List", () => new ObjectParameter("Item"));


            /// <summary>
            /// Attribute where to write the random value to.
            /// </summary>
            private readonly AttributeParameter<object> _attributeValue = new AttributeParameter<object>("Value", AttributeAccess.Write);



            public ListRandomParameter()
                : base("List")
            {
            }



            public override void Execute(Random random, List<IEntity> entities)
            {
                var list = _listParameter.Get() as SceeList;
                if (list != null && list.Any())
                {
                    foreach (var entity in entities)
                    {
                        _attributeValue[entity] = list[random.Int(list.Count)];
                    }
                }
            }
        }

        #endregion
    }
}