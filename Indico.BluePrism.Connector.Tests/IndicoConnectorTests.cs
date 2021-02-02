using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Indico.BluePrism.Connector.Helpers;
using IndicoV2.Submissions;
using Moq;
using NUnit.Framework;

namespace Indico.BluePrism.Connector.Tests
{
    public class IndicoConnectorTests
    {
        private Mock<ISubmissionsClient> _submissionsClientMock;

        [SetUp]
        public void Setup()
        {
#pragma warning disable IDE0022 // Use expression body for methods
            _submissionsClientMock = new Mock<ISubmissionsClient>();
#pragma warning restore IDE0022 // Use expression body for methods
        }

        [Test]
        public void WorkflowSubmission_ShouldReturnIds_WhenPostingFilepaths()
        {
            //Arrange
            var sources = new List<string> { "test", "test 2" };

            var sourcesDataTable = sources.ToDataTable("file");

            var submissionResult = new List<int> { 1, 2 };
            var submissionDataTableResult = submissionResult.ToDataTable("Id");
            
            decimal workflowId = 3;

            _submissionsClientMock.Setup(s =>
                s.CreateAsync(It.IsAny<int>(), sources, default))
                    .Returns(Task.FromResult((IEnumerable<int>)submissionResult));

            var connector = new IndicoConnector(_submissionsClientMock.Object);

            //Act
            var resultFilePaths = connector.WorkflowSubmission(sourcesDataTable, null, workflowId);
            
            var resultFilePathsList = resultFilePaths.ToList<int>();

            //Assert
            _submissionsClientMock.Verify(s => s.CreateAsync(Convert.ToInt32(workflowId), sources, default), Times.Once);

            resultFilePathsList.Should().HaveCount(submissionResult.Count);
            resultFilePathsList.Should().BeEquivalentTo(submissionResult);
        }

        [Test]
        public void WorkflowSubmission_ShouldReturnIds_WhenPostingUris()
        {
            //Arrange
            var sources = new List<string> { "http://test", "http://test2" };
            var sourcesUris = sources.Select(s => new Uri(s));

            var sourcesDataTable = sources.ToDataTable("file");

            var submissionResult = new List<int> { 1, 2 };
            var submissionDataTableResult = submissionResult.ToDataTable("Id");

            decimal workflowId = 3;

            _submissionsClientMock.Setup(s =>
                s.CreateAsync(It.IsAny<int>(), sourcesUris, default))
                    .Returns(Task.FromResult((IEnumerable<int>)submissionResult));

            var connector = new IndicoConnector(_submissionsClientMock.Object);

            //Act
            var resultUris = connector.WorkflowSubmission(null, sourcesDataTable, workflowId);

            var resultUrisList = resultUris.ToList<int>();

            //Assert
            _submissionsClientMock.Verify(s => s.CreateAsync(Convert.ToInt32(workflowId), sourcesUris, default), Times.Once);

            resultUrisList.Should().HaveCount(submissionResult.Count);
            resultUrisList.Should().BeEquivalentTo(submissionResult);
        }

        [Test]
        public void WorkflowFileSubmission_ShouldThrowArgumentNull_WhenEmptyData()
        {
            //Arrange
            decimal workflowId = 3;

            var connector = new IndicoConnector(_submissionsClientMock.Object);

            //Act
            Action act = () => connector.WorkflowSubmission(null, null, workflowId);

            //Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}