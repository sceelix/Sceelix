using NUnit.Framework;
using Sceelix.Loading;
using Sceelix.Logging;

namespace Sceelix.Core.Tests
{
    [SetUpFixture]
    public class TestLogging
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SceelixDomain.Logger = new EmptyLogger();
            SceelixDomain.LoadAssembliesFrom(TestContext.CurrentContext.TestDirectory);
            EngineManager.Initialize();
        }
    }
}