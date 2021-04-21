using System;
using System.Net.Http;
using FluentAssertions;
using Indico.BluePrism.Connector.IntegrationTests.Utils;
using NUnit.Framework;

namespace Indico.BluePrism.Connector.IntegrationTests.IndicoConnectorTests
{
    public abstract class ConnectorTestsBase<TActionResult>
    {
        protected TestDataHelper _dataHelper;

        [OneTimeSetUp]
        public virtual void SetUp() => _dataHelper = new TestDataHelper(new IndicoConnectorBuilder().Build());

        [Test]
        public void PerformAction_ShouldReturnResult()
        {
            // Arrange
            var connector = new IndicoConnectorBuilder().Build();

            //Act
            var result = PerformAction(connector);

            // Assert
            result.Should().NotBeNull();
            AssertResultCorrect(result);
        }

        [Test]
        public void PerformAction_ShouldThrow_WhenBaseUrlNotFound() =>
            new IndicoConnectorBuilder().WithBaseUrl("https://invalid.indico.io").Build()
                .Invoking(PerformAction)
                .Should().Throw<HttpRequestException>();

        [Test]
        public void PerformAction_ShouldThrow_WhenBaseUrlIncorrectFormat() =>
            new IndicoConnectorBuilder().WithBaseUrl("invalid").Build()
                .Invoking(PerformAction)
                .Should().Throw<UriFormatException>();

        [Test]
        public void PerformAction_ShouldThrow_WhenApiTokenIncorrect() =>
            new IndicoConnectorBuilder().WithToken("invalid").Build()
                .Invoking(PerformAction)
                .Should().Throw<HttpRequestException>();

        [Test]
        public void PerformAction_ShouldThrow_WhenApiTokenNull() =>
            new IndicoConnectorBuilder().WithToken(null)
                .Invoking(bld => PerformAction(bld.Build()))
                .Should().Throw<ArgumentNullException>();


        protected abstract TActionResult PerformAction(IndicoConnector connector);
        protected abstract void AssertResultCorrect(TActionResult result);
    }
}
