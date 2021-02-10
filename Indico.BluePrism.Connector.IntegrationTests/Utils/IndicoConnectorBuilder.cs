using System;

namespace Indico.BluePrism.Connector.IntegrationTests.Utils
{
    public class IndicoConnectorBuilder
    {
        private string BaseUrl => Environment.GetEnvironmentVariable("INDICO_HOST");
        private string ApiToken => Environment.GetEnvironmentVariable("INDICO_TOKEN");

        public IndicoConnector Build() => new IndicoConnector(ApiToken, BaseUrl);
    }
}
