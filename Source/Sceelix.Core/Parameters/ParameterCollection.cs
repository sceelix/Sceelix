using System.Collections;
using System.Collections.Generic;

namespace Sceelix.Core.Parameters
{
    /// <summary>
    /// A read-only collection of Parameters.
    /// </summary>
    public class ParameterCollection : IEnumerable<Parameter>
    {
        private readonly List<Parameter> _list;



        internal ParameterCollection()
        {
            //this should still be accessible to add items
            _list = new List<Parameter>();
        }



        internal ParameterCollection(List<Parameter> list)
        {
            //this should be readonly from now on
            _list = list;
        }



        public int Count => _list.Count;



        public Parameter this[string label]
        {
            get
            {
                var result = _list.Find(x => x.Label == label);
                if (result == null)
                    throw new KeyNotFoundException("There is no parameter with label '" + label + "'.");

                return result;
            }
        }



        public Parameter this[int index]
        {
            get
            {
                try
                {
                    return _list[index];
                }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException("There is no parameter at index " + index + ".");
                }
            }
        }



        /*
        internal Parameter TryGetByFullNameOld(String label)
        {
            //string[] strings = label.Split('.');
            var nameQueue = new Queue<String>(label.Split('.'));

            var currentParameterList = _list;

            while (nameQueue.Count > 0)
            {
                var currentParameterName = nameQueue.Dequeue();

                int index;
                if (Int32.TryParse(currentParameterName, out index))
                {
                    if (index < 0 || index >= currentParameterList.Count)
                        return null;

                    var parameter = currentParameterList[index];
                    if (nameQueue.Count == 0)
                        return parameter;

                    currentParameterList = parameter.SubParameters.ToList();
                }
                else
                {
                    var parameter = currentParameterList.Find(x => x.Label == currentParameterName);
                    if (parameter == null)
                        return null;
                    if (nameQueue.Count == 0)
                        return parameter;

                    currentParameterList = parameter.SubParameters.ToList();
                }
            }

            return null;


            //return _list.Find(x => x.Label.StartsWith(label));
        }*/



        public bool Contains(Parameter parameter)
        {
            return _list.Contains(parameter);
        }



        public IEnumerator<Parameter> GetEnumerator()
        {
            return _list.GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }



        internal Parameter TryGetByFullName(string label)
        {
            foreach (var subParameter in _list)
                if (subParameter.FullLabel == label)
                    return subParameter;
                else if (label.StartsWith(subParameter.FullLabel)) return TryGetByFullName(label, subParameter);

            return null;
        }



        private Parameter TryGetByFullName(string label, Parameter parameter)
        {
            foreach (var subParameter in parameter.SubParameters)
                if (subParameter.FullLabel == label)
                    return subParameter;
                else if (label.StartsWith(subParameter.FullLabel)) return TryGetByFullName(label, subParameter);

            return null;
        }
    }
}