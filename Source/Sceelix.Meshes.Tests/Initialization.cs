using NUnit.Framework;
using Sceelix.Core;
using Sceelix.Loading;
using Sceelix.Logging;

namespace Sceelix.Meshes.Tests
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