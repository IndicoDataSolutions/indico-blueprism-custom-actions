using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Indico.BluePrism.Connector.Helpers;
using Indico.BluePrism.Connector.Tests.TestData;
using IndicoV2.Extensions.Jobs;
using IndicoV2.Extensions.SubmissionResult;
using IndicoV2.Reviews;
using IndicoV2.Submissions;
using IndicoV2.Submissions.Models;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SubmissionFilterV2 = IndicoV2.Submissions.Models.SubmissionFilter;

namespace Indico.BluePrism.Connector.Tests
{
    public class IndicoConnectorTests
    {
        private Mock<ISubmissionsClient> _submissionsClientMock;
        private Mock<ISubmissionResultAwaiter> _submissionResultAwaiter;
        private IndicoConnector _connector;
        private Mock<IReviewsClient> _reviewsClientMock;
        private Mock<IJobAwaiter> _jobAwaiterMock;

        [SetUp]
        public void Setup()
        {
            _submissionsClientMock = new Mock<ISubmissionsClient>();
            _submissionResultAwaiter = new Mock<ISubmissionResultAwaiter>();
            _reviewsClientMock = new Mock<IReviewsClient>();
            _jobAwaiterMock = new Mock<IJobAwaiter>();
            _connector = new IndicoConnector(_submissionsClientMock.Object, _submissionResultAwaiter.Object, _reviewsClientMock.Object, _jobAwaiterMock.Object);
        }

        [TestCase(5000)]
        public void Timeout_ShouldSetTimeout(decimal timeoutMs)
        {
            _connector.TimeoutMs = timeoutMs;

            _connector.TimeoutMs.Should().Be(timeoutMs);
        }

        [Test]
        public void Timeout_ShouldHaveDefaultValue() => _connector.TimeoutMs.Should().Be(5000);

        [TestCase(300)]
        public void CheckInterval_ShouldSetValue(decimal checkIntervalMs)
        {
            _connector.CheckIntervalMs = checkIntervalMs;

            _connector.CheckIntervalMs.Should().Be(checkIntervalMs);
        }

        [Test]
        public void CheckInterval_ShouldHaveDefaultValue() => _connector.CheckIntervalMs.Should().Be(300);

        [TestCase("testToken", null)]
        [TestCase("testToken", "https://app.indico.io")]
        public void IndicoConnector_ShouldCreateConnector(string token, string uri) => new IndicoConnector(token, uri).Should().NotBeNull();

        [Test]
        public void WorkflowSubmission_ShouldReturnIds_WhenPostingFilepaths()
        {
            //Arrange
            var sources = new List<string> { "test", "test 2" };

            var sourcesDataTable = sources.ToIdDataTable("file");

            var submissionResult = new List<int> { 1, 2 };

            decimal workflowId = 3;

            _submissionsClientMock
                .Setup(s => s.CreateAsync(It.IsAny<int>(), sources, default))
                .ReturnsAsync(submissionResult);

            //Act
            var resultFilePaths = _connector.WorkflowSubmission(sourcesDataTable, null, workflowId);

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

            decimal workflowId = 3;

            _submissionsClientMock.Setup(s =>
                s.CreateAsync(It.IsAny<int>(), sourcesUris, default))
                    .ReturnsAsync(submissionResult);

            //Act
            var resultUris = _connector.WorkflowSubmission(null, sourcesDataTable, workflowId);

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

            //Act
            Action act = () => _connector.WorkflowSubmission(null, null, workflowId);

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

            var submissionList = submissionIdsList.Select(s => Mock.Of<ISubmission>(sm => sm.Id == s)).ToList();

            _submissionsClientMock.Setup(s =>
                s.ListAsync(submissionIdsList, null, It.IsAny<SubmissionFilterV2>(), submissionIdsList.Count, default))
                    .ReturnsAsync(submissionList);

            //Act
            var resultSubmissionsDataTable = _connector.ListSubmissions(submissionIdsDecimalDataTable, null, null, null, null, Convert.ToDecimal(submissionIdsList.Count));
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

            var submissionList = submissionIdsList.Select(s => Mock.Of<ISubmission>(sm => sm.Id == s && sm.WorkflowId == workflowId)).ToList();

            _submissionsClientMock.Setup(s =>
                s.ListAsync(null, workflowIdsList, It.IsAny<SubmissionFilterV2>(), submissionIdsList.Count, default))
                    .ReturnsAsync(submissionList);

            //Act
            var resultSubmissionsDataTable = _connector.ListSubmissions(null, workflowIdsDecimalDataTable, null, null, null, Convert.ToDecimal(submissionIdsList.Count));
            var resultSubmissionIdsList = resultSubmissionsDataTable.ToList<int>();
            var resultSubmissionWorkflowIdsList = resultSubmissionsDataTable.ToList<int>("WorkflowId");

            //Assert
            _submissionsClientMock.Verify(s => s.ListAsync(null, workflowIdsList, It.IsAny<SubmissionFilterV2>(), submissionIdsList.Count, default), Times.Once);

            resultSubmissionsDataTable.Rows.Should().HaveSameCount(submissionList);
            resultSubmissionIdsList.Should().BeEquivalentTo(submissionIdsList);
            resultSubmissionWorkflowIdsList.Should().AllBeEquivalentTo(workflowId);
        }

        [TestCase("test", "PROCESSING", "True")]
        [TestCase("test2", "FAILED", "true")]
        [TestCase("test3", "COMPLETE", "False")]
        [TestCase("test4", "PENDING_ADMIN_REVIEW", "false")]
        [TestCase("test4", "PENDING_REVIEW", "TRUE")]
        [TestCase("test5", "PENDING_AUTO_REVIEW", "FALSE")]
        public void ListSubmissions_ShouldBuildProperFilterObject(string inputFileName, string status, string retrieved)
        {
            //Arrange
            var statusParsed = Enum.Parse<SubmissionStatus>(status);
            var retrievedParsed = bool.Parse(retrieved);
            var limit = 1000;

            _submissionsClientMock.Setup(s =>
                s.ListAsync(null, null, It.IsAny<SubmissionFilterV2>(), limit, default))
                    .ReturnsAsync(new List<ISubmission>());

            //Act
            _connector.ListSubmissions(null, null, inputFileName, status, retrieved, Convert.ToDecimal(limit));

            //Assert
            _submissionsClientMock.Verify(s => s.ListAsync(null, null, It.Is<SubmissionFilterV2>
                (sf =>
                    sf.InputFilename == inputFileName &&
                    sf.Status == statusParsed &&
                    sf.Retrieved == retrievedParsed),
            limit, default), Times.Once);
        }

        [Test]
        public void SubmissionResult_ShouldGetSubmission()
        {
            // Arrange
            const int submissionId = 1;
            _submissionResultAwaiter
                .Setup(cli => cli.WaitReady(submissionId, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JObject.Parse(@"{""test"": 1 }"));

            // Act
            var submissionResult = _connector.SubmissionResult(submissionId, null);

            // Assert
            var row = submissionResult.Rows.OfType<DataRow>().Single();
            row["test"].Should().Be(1);
        }

        [Test]
        public void SubmissionResult_ShouldThrowWhenIntOverflow() =>
            this.Invoking(
                    _ => _connector.SubmissionResult(decimal.MaxValue, null))
                .Should().Throw<OverflowException>();

        [Theory]
        public void SubmitReview_ShouldCallReviewsClient(bool rejected, bool? forceComplete)
        {
            // Arrange
            var submissionId = 1;
            var changes = new DataTable();
            changes.Columns.AddRange(new[] { new DataColumn("name", typeof(string)), new DataColumn("id", typeof(int)), new DataColumn("boolCol", typeof(bool)), new DataColumn("CreatedFromJsonObject") });
            changes.Rows.Add("test", 12, true);
            const int idCellValue = 2;
            _jobAwaiterMock.Setup(cli =>
                    cli.WaitReadyAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JToken.FromObject(new { id = idCellValue }));

            // Act
            var result = forceComplete.HasValue
                ? _connector.SubmitReview(submissionId, changes, rejected, forceComplete.Value)
                : _connector.SubmitReview(submissionId, changes, rejected);

            // Assert
            var expectedChangesJson = JObject.FromObject(new {name = "test", id = 12, boolCol = true});
            _reviewsClientMock.Verify(cli => cli.SubmitReviewAsync(
                submissionId,
                It.Is<JObject>(changesJson => JToken.DeepEquals(expectedChangesJson, changesJson)),
                rejected,
                forceComplete,
                It.IsAny<CancellationToken>()),
                Times.Once);
            result.Should().NotBeNull();
            result.Rows.Count.Should().Be(1);
            result.Rows[0]["id"].Should().Be(idCellValue);
        }

        [Test]
        public async Task SubmitReview_ShouldAcceptChangesDataTable()
        {
            // Arrange
            var submissionId = 1m;
            var rejected = false;
            var jobId = "test";
            var submissionResult = new IndicoJsonUtility().ConvertToDataTable(await new JsonTestData().SubmissionResult());
            var resultsOuter = (DataTable)submissionResult.Rows[0]["results"];
            var document = (DataTable)resultsOuter.Rows[0]["document"];
            var changes = (DataTable)document.Rows[0]["results"];
            _jobAwaiterMock
                .Setup(cli => cli.WaitReadyAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JObject.Parse(@"{ ""test"": 1 }"));

            // Act
            var result = _connector.SubmitReview(submissionId, changes, false);

            // Assert
            _reviewsClientMock.Verify(cli =>
                cli.SubmitReviewAsync((int)submissionId, It.Is<JObject>(ch => ch.ContainsKey("Invoice Fields q2076 model")), rejected, null, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void SubmitReview_ShouldThrowWhenIntOverflow() =>
            this.Invoking(_ => _connector.SubmitReview(decimal.MaxValue, default, default, default))
                .Should().Throw<OverflowException>();
    }
}
