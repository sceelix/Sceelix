using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sceelix.Core.Annotations;
using Sceelix.Core.Extensions;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Extensions;
using Sceelix.Loading;

namespace Sceelix.Core.Utils
{
    internal class ProcedureParameterHelper
    {
        internal static void ReadParametersInputsOutputs(object parent, List<Parameter> fields, List<Input> inputs, List<Output> outputs)
        {
            var type = parent.GetType();

            var fieldInfos = type.GetInstancePublicPrivateFields().OrderBy(x => (x.GetCustomAttribute<OrderAttribute>() ?? new OrderAttribute(0)).OrderIndex);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                var description = CommentLoader.GetComment(fieldInfo).Summary;

                var value = fieldInfo.GetValue(parent);
                if (value is Parameter)
                {
                    var parameter = (Parameter) value;
                    parameter.Parent = parent;

                    if (fields.Any(x => x.Label == parameter.Label))
                        throw new Exception("More than one parameter with the name '" + parameter.Label + "' is being defined at class '" + type.Name + "'.");

                    //only overwrite with comment descriptions if a description is not already set
                    if (string.IsNullOrWhiteSpace(parameter.Description))
                        parameter.Description = description;

                    //read the section, so that we can present better present them
                    var sectionAttribute = fieldInfo.GetCustomAttribute<SectionAttribute>();
                    if (sectionAttribute != null)
                        parameter.Section = sectionAttribute.Name;

                    fields.Add(parameter);
                }
                else if (value is Input)
                {
                    var input = (Input) value;

                    if (inputs.Any(x => x.Label == input.Label))
                        throw new Exception("More than one input with the name '" + input.Label + "' is being defined at class '" + type.Name + "'.");

                    //only overwrite with comment descriptions if a description is not already set
                    if (string.IsNullOrWhiteSpace(input.Description))
                        input.Description = description;

                    input.Parent = parent;

                    inputs.Add(input);
                }
                else if (value is Output)
                {
                    var output = (Output) value;

                    if (outputs.Any(x => x.Label == output.Label))
                        throw new Exception("More than one output with the name '" + output.Label + "' is being defined at class '" + type.Name + "'.");

                    //only overwrite with comment descriptions if a description is not already set
                    if (string.IsNullOrWhiteSpace(output.Description))
                        output.Description = description;

                    output.Parent = parent;

                    outputs.Add(output);
                }
            }
        }
    }
}