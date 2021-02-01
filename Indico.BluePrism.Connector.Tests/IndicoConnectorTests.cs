using System;
using System.Collections.Generic;
using System.Data;
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
        public void WorkflowFileSubmission_ShouldReturnIdForEachFile()
        {
            //Arrange
            var filePathList = new List<string> { "test file path", "test file path 2" };
            var filePathDataTable = filePathList.ToDataTable("filepath");
            var submissionResult = new List<int> { 1, 2 };
            var submissionDataTableResult = submissionResult.ToDataTable("Id");
            decimal workflowId = 3;

            _submissionsClientMock.Setup(s =>
                s.CreateAsync(It.IsAny<int>(), filePathList, default))
                    .Returns(Task.FromResult((IEnumerable<int>)submissionResult));

            var connector = new IndicoConnector(_submissionsClientMock.Object);

            //Act
            var result = connector.WorkflowFileSubmission(filePathDataTable, workflowId);
            var resultList = result.ToList<int>();

            //Assert
            _submissionsClientMock.Verify(s => s.CreateAsync(Convert.ToInt32(workflowId), filePathList, default), Times.Once);

            resultList.Should().HaveCount(submissionResult.Count);
            resultList.Should().BeEquivalentTo(submissionResult);
        }

        [Test]
        public void WorkflowFileSubmission_ShouldThrowArgumentNull_WhenFilepathsDataTableNull()
        {
            //Arrange
            decimal workflowId = 3;

            var connector = new IndicoConnector(_submissionsClientMock.Object);

            //Act
            Action act = () => connector.WorkflowFileSubmission(null, workflowId);

            //Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void WorkflowFileSubmission_ShouldThrowArgument_WhenFilepathsDataTableEmpty()
        {
            //Arrange
            decimal workflowId = 3;

            var connector = new IndicoConnector(_submissionsClientMock.Object);

            //Act
            Action act = () => connector.WorkflowFileSubmission(new DataTable(), workflowId);

            //Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}