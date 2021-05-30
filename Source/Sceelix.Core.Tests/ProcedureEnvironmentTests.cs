using NUnit.Framework;
using Sceelix.Core.Bindings;
using Sceelix.Core.Environments;
using Sceelix.Core.Messages;
using Sceelix.Core.Resources;
using Sceelix.Logging;

namespace Sceelix.Core.Tests
{
    public class ProcedureEnvironmentTests
    {
        [Test]
        public void TestDefaultEnvironmentServices()
        {
            ProcedureEnvironment procedureEnvironment = new ProcedureEnvironment();

            Assert.IsInstanceOf(typeof(EmptyBinding), procedureEnvironment.ExecutionBinding);
            Assert.IsInstanceOf(typeof(ConsoleLogger), procedureEnvironment.Logger);
            Assert.IsInstanceOf(typeof(EmptyMessenger), procedureEnvironment.Messenger);
            Assert.IsInstanceOf(typeof(ResourceManager), procedureEnvironment.Resources);
        }
    }
}