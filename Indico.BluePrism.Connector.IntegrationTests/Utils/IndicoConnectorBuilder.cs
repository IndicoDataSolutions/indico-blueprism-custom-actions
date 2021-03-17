using System;

namespace Indico.BluePrism.Connector.IntegrationTests.Utils
{
    public class IndicoConnectorBuilder
    {
        private string _baseUrl = Environment.GetEnvironmentVariable("INDICO_HOST");
        private string _apiToken = Environment.GetEnvironmentVariable("INDICO_TOKEN");

        public IndicoConnector Build() => new IndicoConnector(_apiToken, _baseUrl);

        #region fluent

        public IndicoConnectorBuilder WithBaseUrl(string baseUrl) => FluentWrapper(() => _baseUrl = baseUrl);

        public IndicoConnectorBuilder WithToken(string token) => FluentWrapper(() => _apiToken = token);

        private IndicoConnectorBuilder FluentWrapper(Action action)
        {
            action();

            return this;
        }
        #endregion fluent
    }
}
