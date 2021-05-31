using System;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;

namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// This sample will focus on how to read/write from/to entity attributes. 
    /// 
    /// Attributes, as you may know, are custom data that each entity may have. Each entity has an 'Attributes' field
    /// - a Dictionary that may have new Key-Value pairs defined.
    /// </summary>
    [Procedure("a1c53733-c1b7-4a24-963b-08161047d014", Label = "Hello Attributes", Category = "MyTutorial")]
    public class HelloAttributesProcedure : SystemProcedure
    {
        private readonly CollectiveInput<IEntity> _input = new CollectiveInput<IEntity>("Input");
        private readonly Output<IEntity> _output = new Output<IEntity>("Output");

        /// <summary>
        /// Attributes can have one of three types of access. This is merely an indication for the user, as this choice does not
        /// prohibit you to actually read or write from the attributes.
        /// 
        /// AttributeAccess.Read: We are only going to read from the attribute (so it should exist!)
        /// AttributeAccess.Write: We are only going to write to the attribute (so it should NOT exist, unless the /replace is used)
        /// AttributeAccess.ReadWrite: We are going to read and write to the attribute, so the attribute can either exist or not.
        /// </summary>
        private readonly AttributeParameter<bool> _boolAttribute = new AttributeParameter<bool>("Bool Attribute", AttributeAccess.Read);
        private readonly AttributeParameter<int> _intAttribute = new AttributeParameter<int>("Int Attribute", AttributeAccess.ReadWrite);
        private readonly AttributeParameter<String> _stringAttribute = new AttributeParameter<String>("String Attribute", AttributeAccess.Write);

        
        protected override void Run()
        {
            var entities = _input.Read().ToList();

            int modifiedEntries = 0;

            foreach (var entity in entities)
            {
                //you can verify is an actual attribute name has been set
                var isMapped = _boolAttribute.IsMapped;

                //or if the attribute actually exists
                var hasAttribute = _boolAttribute.HasAttribute(entity);

                //otherwise, getting the value of an attribute that is not defined will return the default value
                bool boolValue = _boolAttribute[entity];
                if (boolValue)
                {
                    //for the ReadWrite attribute, we are going to read the value
                    _intAttribute[entity] = _intAttribute[entity] * 2;
                    _stringAttribute[entity] = "This value was modified!";
                    modifiedEntries++;
                }
                
                //you can also get the values in the following way
                //still, we need to check if the user has defined an attribute name 
                if (_stringAttribute.IsMapped)
                {
                    string currentValue = (String)entity.Attributes.TryGet(_stringAttribute.AttributeKey);
                    if (String.IsNullOrWhiteSpace(currentValue))
                        entity.Attributes.TrySet(_stringAttribute.AttributeKey, "This value was not modified!", true);
                }
            }

            Logger.Log(modifiedEntries + " entities were modified!");

            _output.Write(entities);
        }
    }
}
