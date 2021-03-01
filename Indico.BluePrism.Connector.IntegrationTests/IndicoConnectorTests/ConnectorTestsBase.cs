using Indico.BluePrism.Connector.IntegrationTests.Utils;
using NUnit.Framework;

namespace Indico.BluePrism.Connector.IntegrationTests.IndicoConnectorTests
{
    public abstract class ConnectorTestsBase
    {
        protected IndicoConnector _connector;
        protected TestDataHelper _dataHelper;


        [SetUp]
        public void SetUp()
        {
            _connector = new IndicoConnectorBuilder().Build();
            _dataHelper = new TestDataHelper(_connector);
        }

    }
}
