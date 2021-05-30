using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Collections;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Extensions;

namespace Sceelix.Core.IO
{
    /*public interface IAttributeDataSource
    {
        Object Get(String name);
    }*/

    /// <summary>
    /// Data structure that provides access to entity attributes
    /// currently waiting for processing by a procedure. 
    /// </summary>
    public class InputData //: IAttributeDataSource
    {
        private readonly Dictionary<Input, object> _dataDictionary;
        private readonly InputCollection _inputList;



        internal InputData(InputCollection inputList)
        {
            _inputList = inputList;

            //read the data once
            _dataDictionary = inputList.ToDictionary(val => val, val => val.Read());
        }



        /*internal object Read(Input key)
        {
            return _dataDictionary[key];
        }


        /// <summary>
        /// Reads the data from the given single input.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Read<T>(SingleInput<T> key) where T : IEntity
        {
            return (T)_dataDictionary[key];
        }

        
        public IEnumerable<T> Read<T>(CollectiveInput<T> key) where T : IEntity
        {
            return (IEnumerable<T>)_dataDictionary[key];
        }
        

        public IEntity this[String inputLabel]
        {
            get
            {
                object obj = _dataDictionary[_inputList[inputLabel]];

                if (obj is IEntity)
                    return (IEntity)_dataDictionary[_inputList[inputLabel]];
                else
                    return ((IEnumerable<IEntity>)_dataDictionary[_inputList[inputLabel]]).First();
                    
            }
        }*/



        /// <summary>
        /// Gets the attribute value (or list of values) with the given key.
        /// </summary>
        /// <param name="name">Key of the attribute.</param>
        /// <param name="allowNull">If false, an exception will be thrown if the key is not found for any of the input entities. If true, simply a null value will be returned.</param>
        /// <returns>Depending on the number of inputs, it may return one value (for one single input), a list of values (for one collective input), or a list of lists of values (for several single or collective inputs).</returns>
        public object Get(AttributeKey name, bool allowNull = false)
        {
            if (_inputList.Count == 1) return GetAttributesFromInput(_inputList[0], name, allowNull);

            if (_inputList.Count > 1)
            {
                SceeList list = new SceeList();

                foreach (var input in _inputList)
                    list.Add(input.Label, GetAttributesFromInput(input, name, allowNull));

                //list.Add(input.UniqueLabel, GetAttributesFromInput(input, name));

                /*for (int i = 0; i < _inputList.Count; i++)
                {
                    var input = _inputList[i];
                    list.Add(input.Label + " #" + i, GetAttributesFromInput(input, name));
                }*/

                return list;
            }

            throw new Exception("This node does not have any input data from which to extract the requested attribute '" + name + "'.");
        }



        private object GetAttributesFromInput(Input input, AttributeKey name, bool allowNull)
        {
            try
            {
                //if its an input that holds a list of entities
                if (input is CollectiveInput)
                {
                    var entities = (IEnumerable<IEntity>) _dataDictionary[input];


                    return new SceeList(entities.Select(x => new KeyValuePair<string, object>(name.ToString(), allowNull ? x.Attributes.TryGet(name) : x.Attributes[name])));
                }

                //otherwise, just return the value from the only entity
                var entity = (IEntity) _dataDictionary[input];

                return allowNull ? entity.Attributes.TryGet(name) : entity.Attributes[name];
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("An attribute with the name '{0}' does not exist in one of more input entities. Use the ? operator after the attribute name to bypass this verification and use the default 'null' value.", name));
            }


            /*if(!allowNull)
            {
                var attributeValue = entity.Attributes.TryGet(name);
                if(attributeValue == null)
                    throw new KeyNotFoundException(String.Format("An attribute with the name '{0}' does not exist in the entity.", name));

                return attributeValue;
            }

            return entity.Attributes.TryGet(name);*/
        }



        /// <summary>
        /// Gets the first entity at the first available input port or null, 
        /// if there are no inputs or available data. 
        /// </summary>
        /// <returns>An entity if there is one available or null, otherwise.</returns>
        public IEntity GetFirst()
        {
            if (_inputList.Count > 0)
            {
                var firstInput = _inputList.First();
                var data = firstInput.Read();

                if (firstInput is CollectiveInput)
                    return data.CastTo<IEnumerable<IEntity>>().First();

                return (IEntity) data;
            }

            //otherwise, return null as initially planned
            return null;
        }
    }
}