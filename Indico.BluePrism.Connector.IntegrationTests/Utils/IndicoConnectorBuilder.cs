using System;

namespace Indico.BluePrism.Connector.IntegrationTests.Utils
{
    public class IndicoConnectorBuilder
    {
        private string BaseUrl { get; set; } = Environment.GetEnvironmentVariable("INDICO_HOST");
        private string ApiToken { get; set; } = Environment.GetEnvironmentVariable("INDICO_TOKEN");

        public IndicoConnector Build() => new IndicoConnector(ApiToken, BaseUrl);

        #region fluent

        public IndicoConnectorBuilder WithBaseUrl(string baseUrl) => FluentWrapper(() => BaseUrl = baseUrl);

        public IndicoConnectorBuilder WithToken(string token) => FluentWrapper(() => ApiToken = token);

        private IndicoConnectorBuilder FluentWrapper(Action action)
        {
            action();

            return this;
        }
        #endregion fluent
    }
}
