using Assets.Sceelix.Contexts;
using Newtonsoft.Json.Linq;

namespace Assets.Sceelix.Processors.Messages
{
    public abstract class MessageProcessor
    {
        public abstract void Process(IGenerationContext context, JToken data);
    }
}
