using System.Collections.Generic;
using System.IO;
using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Resources;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// Generates empty entities, which can be used to carry Attribute data and serve as impulses 
    /// to source nodes.
    /// </summary>
    [Procedure("2ef7c2e5-c59f-464b-8557-c1e4e38216a9", Label = "Entity Create", Category = "Basic", Tags = "Entity")]
    public class EntityCreateProcedure : SystemProcedure
    {
        /// <summary>
        /// Created empty Entity instances.
        /// </summary>
        private readonly Output<IEntity> _output = new Output<IEntity>("Entities");

        /// <summary>
        /// Type of entity creation method to run.
        /// </summary>
        private readonly SelectListParameter<EntityCreateParameter> _parameterCreation = new SelectListParameter<EntityCreateParameter>("Method", "Standard");



        protected override void Run()
        {
            foreach (var parameter in _parameterCreation.Items)
                _output.Write(parameter.CreateEntities());
        }



        #region Abstract

        public abstract class EntityCreateParameter : CompoundParameter
        {
            protected EntityCreateParameter(string label)
                : base(label)
            {
            }



            protected internal abstract IEnumerable<IEntity> CreateEntities();
        }

        #endregion

        #region Standard

        /// <summary>
        /// Creates empty entities, with the possibility to assign an index value to each outputted.
        /// </summary>
        public class StandardEntityCreateParameter : EntityCreateParameter
        {
            /// <summary>
            /// Number of entities to create.
            /// </summary>
            private readonly IntParameter _parameterCount = new IntParameter("Count", 1);

            /// <summary>
            /// Attribute where to store the index to identify and enumerate the created entities.
            /// </summary>
            private readonly AttributeParameter<int> _attributeIndex = new AttributeParameter<int>("Index", AttributeAccess.Write); //{Group = "Attributes"};



            protected StandardEntityCreateParameter()
                : base("Standard")
            {
            }



            protected internal override IEnumerable<IEntity> CreateEntities()
            {
                for (int i = 0; i < _parameterCount.Value; i++)
                {
                    var entity = new Entity();

                    //store the index
                    _attributeIndex[entity] = i;

                    yield return entity;
                }
            }
        }

        #endregion

        #region Files

        /// <summary>
        /// Creates as many entities as the files in a given folder. Stores the name of the file in an attribute. 
        /// Useful to create an enumeration over files.
        /// </summary>
        /// <seealso cref="Sceelix.Core.Procedures.EntityCreateProcedure.EntityCreateParameter" />
        public class FilesEntityCreateParameter : EntityCreateParameter
        {
            /// <summary>
            /// Folder from which the file names should be read.
            /// </summary>
            private readonly FolderParameter _parameterFolder = new FolderParameter("Folder", "");

            /// <summary>
            /// Attribute where the file name will be stored.
            /// </summary>
            [Section("Attributes")] private readonly AttributeParameter<string> _attributeFileName = new AttributeParameter<string>("File Name", AttributeAccess.Write);

            private string[] _fileNames;

            private string _lastPathToFolder;



            protected FilesEntityCreateParameter()
                : base("Files")
            {
            }



            protected internal override IEnumerable<IEntity> CreateEntities()
            {
                string pathToFolder = _parameterFolder.Value; //Path.Combine(loadEnvironment.projectFolder, FolderParameter.Value);

                //avoid calling the heavy Directory.GetFiles function every time
                if (_lastPathToFolder != pathToFolder)
                {
                    _fileNames = ProcedureEnvironment.GetService<IResourceManager>().GetFilePaths(pathToFolder);
                    _lastPathToFolder = pathToFolder;
                }

                foreach (var fileName in _fileNames)
                {
                    Entity entity = new Entity();

                    _attributeFileName[entity] = Path.Combine(_parameterFolder.Value, Path.GetFileName(fileName));

                    yield return entity;
                }
            }
        }

        #endregion

        #region List

        /// <summary>
        /// Creates as many entities as the entries in a list. Stored the content of each list entry in an attribute.
        /// Useful to iterate over lists of numbers, strings, or sets of data.
        /// </summary>
        /// <seealso cref="Sceelix.Core.Procedures.EntityCreateProcedure.EntityCreateParameter" />
        public class ListEntityCreateParameter : EntityCreateParameter
        {
            /// <summary>
            /// List whose entries are to be read.
            /// </summary>
            private readonly ObjectParameter _parameterList = new ObjectParameter("List");

            /// <summary>
            /// Attribute where the list entries are to be stored.
            /// </summary>
            private readonly AttributeParameter<object> _parameterAttributeItem = new AttributeParameter<object>("Item", AttributeAccess.Write);



            protected ListEntityCreateParameter()
                : base("List")
            {
            }



            protected internal override IEnumerable<IEntity> CreateEntities()
            {
                var list = ConvertHelper.Convert<SceeList>(_parameterList.Value);

                foreach (object value in list.Values)
                {
                    Entity entity = new Entity();
                    _parameterAttributeItem[entity] = value;

                    yield return entity;
                }
            }
        }

        #endregion
    }
}