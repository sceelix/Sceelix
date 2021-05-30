using System;
using System.Linq;
using Newtonsoft.Json;
using Sceelix.Core.Data;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Serialization;
using Sceelix.Unity.Annotations;

namespace Sceelix.Designer.Unity3D.Serialization
{
    [UnityJsonConverter(typeof(GraphExecutionFinished))]
    public class GraphExecutionFinishedConverter : JsonConverter
    {
        public override bool CanRead
        {
            get { return false; }
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var graphExecutionFinished = (GraphExecutionFinished) value;

            serializer.AddObject("ProcedureEnvironment",graphExecutionFinished.Procedure.Environment);

            writer.WriteStartObject();

            writer.WritePropertyName("Name");
            writer.WriteValue(graphExecutionFinished.Procedure.Name);

            //writer.WritePropertyName("Graph");
            //WriteProcedureInfoObject(writer, graphExecutionFinished.Procedure, serializer);

            writer.WritePropertyName("Entities");
            WriteEntities(writer, graphExecutionFinished.Data, serializer);

            writer.WriteEndObject();
        }



        private void WriteProcedureInfoObject(JsonWriter writer, GraphProcedure procedure, JsonSerializer serializer)
        {
            //writer.WriteStartObject();


            writer.WritePropertyName("Parameters");
            writer.WriteStartArray();
            foreach (ParameterReference parameterReference in procedure.Parameters)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("Label");
                writer.WriteValue(parameterReference.Label);
                writer.WritePropertyName("Type");
                writer.WriteValue(parameterReference.ParameterInfo.MetaType);
                writer.WritePropertyName("Value");
                serializer.Serialize(writer, parameterReference.Get());

                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            /*writer.WritePropertyName("Inputs");
            writer.WriteStartArray();
            foreach (InputReference inputReference in procedure.Inputs)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("Label");
                writer.WriteValue(inputReference.Label);
                writer.WritePropertyName("Type");
                writer.WriteValue(inputReference.EntityType.Name);

                writer.WriteEndObject();
            }
            writer.WriteEndArray();*/

            //writer.WriteEndObject();
        }



        private void WriteEntities(JsonWriter writer, IEntity[] entities, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            foreach (IEntity entity in entities) //.OfType<UnityEntity>()
                serializer.Serialize(writer, entity);

            writer.WriteEndArray();
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //not going to be used
            throw new NotImplementedException();
        }



        public override bool CanConvert(Type objectType)
        {
            //not going to be used
            throw new NotImplementedException();
        }
    }
}