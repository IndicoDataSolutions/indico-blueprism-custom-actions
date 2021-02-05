using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Indico.BluePrism.Connector.Helpers;
using Indico.Entity;
using IndicoV2.Submissions;
using IndicoV2.Submissions.Models;
using Moq;
using NUnit.Framework;
using IndicoV2.V1Adapters.Submissions.Models;
using SubmissionFilterV2 = IndicoV2.Submissions.Models.SubmissionFilter;

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

            var sourcesDataTable = sources.ToIdDataTable("file");

            var submissionResult = new List<int> { 1, 2 };
            var submissionDataTableResult = submissionResult.ToIdDataTable("Id");

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

            var sourcesDataTable = sources.ToIdDataTable("file");

            var submissionResult = new List<int> { 1, 2 };
            var submissionDataTableResult = submissionResult.ToIdDataTable("Id");

            decimal workflowId = 3;

            _submissionsClientMock.Setup(s =>
                s.CreateAsync(It.IsAny<int>(), sourcesUris, default))
                    .ReturnsAsync(submissionResult);

            var connector = new IndicoConnector(_submissionsClientMock.Object);

            //Act
            var resultUris = connector.WorkflowSubmission(null, sourcesDataTable, workflowId);

            var resultUrisList = resultUris.ToList<int>();

            //Assert
            _submissionsClientMock.Verify(s => s.CreateAsync(Convert.ToInt32(workflowId), sourcesUris, default), Times.Once);

            resultUrisList.Should().HaveSameCount(submissionResult);
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

        [Test]
        public void ListSubmissions_ShouldReturnSubmissionsById()
        {
            //Arrange
            var submissionIdsList = new List<int> { 1, 2, 3, 4 };
            var submissionIdsDecimalList = submissionIdsList.Select(id => Convert.ToDecimal(id));
            var submissionIdsDecimalDataTable = submissionIdsDecimalList.ToIdDataTable();

            var submissionList = submissionIdsList.Select(s => new V1SubmissionAdapter(new Submission { Id = s })).ToList();

            var submissionDataTable = submissionList.ToDetailedDataTable();

            _submissionsClientMock.Setup(s =>
                s.ListAsync(submissionIdsList, null, It.IsAny<SubmissionFilterV2>(), submissionIdsList.Count, default))
                    .ReturnsAsync(submissionList);

            var connector = new IndicoConnector(_submissionsClientMock.Object);

            //Act
            var resultSubmissionsDataTable = connector.ListSubmissions(submissionIdsDecimalDataTable, null, null, null, null, Convert.ToDecimal(submissionIdsList.Count));
            var resultSubmissionIdsList = resultSubmissionsDataTable.ToList<int>();

            //Assert
            _submissionsClientMock.Verify(s => s.ListAsync(submissionIdsList, null, It.IsAny<SubmissionFilterV2>(), submissionIdsList.Count, default), Times.Once);

            resultSubmissionsDataTable.Rows.Should().HaveSameCount(submissionList);
            resultSubmissionIdsList.Should().BeEquivalentTo(submissionIdsList);
        }

        [Test]
        public void ListSubmissions_ShouldReturnSubmissionsByWorkflowId()
        {
            //Arrange
            var workflowId = 5;
            var workflowIdsList = new List<int> { workflowId };
            var workflowIdsDecimalList = workflowIdsList.Select(id => Convert.ToDecimal(id));
            var workflowIdsDecimalDataTable = workflowIdsDecimalList.ToIdDataTable();

            var submissionIdsList = new List<int> { 1, 2, 3, 4 };
            var submissionIdsDecimalList = submissionIdsList.Select(id => Convert.ToDecimal(id));
            var submissionIdsDecimalDataTable = submissionIdsDecimalList.ToIdDataTable();

            var submissionList = submissionIdsList.Select(s => new V1SubmissionAdapter(new Submission { Id = s, WorkflowId = workflowId })).ToList();

            var submissionDataTable = submissionList.ToDetailedDataTable();

            _submissionsClientMock.Setup(s =>
                s.ListAsync(null, workflowIdsList, It.IsAny<SubmissionFilterV2>(), submissionIdsList.Count, default))
                    .ReturnsAsync(submissionList);

            var connector = new IndicoConnector(_submissionsClientMock.Object);

            //Act
            var resultSubmissionsDataTable = connector.ListSubmissions(null, workflowIdsDecimalDataTable, null, null, null, Convert.ToDecimal(submissionIdsList.Count));
            var resultSubmissionIdsList = resultSubmissionsDataTable.ToList<int>();
            var resultSubmissionWorkflowIdsList = resultSubmissionsDataTable.ToList<int>("WorkflowId");

            //Assert
            _submissionsClientMock.Verify(s => s.ListAsync(null, workflowIdsList, It.IsAny<SubmissionFilterV2>(), submissionIdsList.Count, default), Times.Once);

            resultSubmissionsDataTable.Rows.Should().HaveSameCount(submissionList);
            resultSubmissionIdsList.Should().BeEquivalentTo(submissionIdsList);
            resultSubmissionWorkflowIdsList.Should().AllBeEquivalentTo(workflowId);
        }
    }
}