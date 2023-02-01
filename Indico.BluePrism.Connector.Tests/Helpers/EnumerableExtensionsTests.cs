using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using Indico.BluePrism.Connector.Helpers;
using Indico.Entity;
using Indico.Types;
using NUnit.Framework;

namespace Indico.BluePrism.Connector.Tests.Helpers
{
    public class EnumerableExtensionsTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(178)]
        public void ToIdDataTable_ShouldReturnRightNumberOfElements(int elementCount)
        {
            //Arrange
            var list = new List<int>(Enumerable.Range(0, elementCount));

            //Act
            var dataTable = list.ToIdDataTable();

            //Assert
            dataTable.Should().NotBeNull();
            dataTable.Rows.Should().HaveCount(elementCount);
        }

        [TestCase(1, SubmissionStatus.COMPLETE, "test")]
        [TestCase(2, SubmissionStatus.FAILED, "test2")]
        public void ToDetailedDataTable_ShouldReturnValidObject(int id, SubmissionStatus status, string inputFile)
        {
            //Arrange
            var testObject = new Submission
            {
                Id = id,
                Status = status,
                InputFile = inputFile,
                SubmissionFiles = new List<SubmissionFiles>()
                {
                    new SubmissionFiles()
                    {
                        Filename = "test.pdf",
                        Id = 1,
                        NumPages = 1

                    }
                }
            };

            //Act
            var dataTable = new List<Submission>() { testObject }.ToDetailedDataTable();


            //Assert
            dataTable.Should().NotBeNull();
            dataTable.Rows[0][nameof(testObject.Id)].Should().Equals(id);
            dataTable.Rows[0][nameof(testObject.Status)].Should().Equals(status);
            dataTable.Rows[0][nameof(testObject.InputFile)].Should().Equals(inputFile);
            dataTable.Rows[0][nameof(testObject.SubmissionFiles)].Should().NotBeNull();
            var subfiles = dataTable.Rows[0][nameof(testObject.SubmissionFiles)];
            subfiles.Should().BeAssignableTo<DataTable>();
            (subfiles as DataTable).Rows[0]["Filename"].Should().NotBeNull();
            (subfiles as DataTable).Rows[0]["Id"].Should().Be(1);
        }

        [TestCase(1, SubmissionStatus.COMPLETE, "test")]
        [TestCase(2, SubmissionStatus.FAILED, "test2")]
        public void ToDetailedDataTable_ShouldReturnValidObjectOfPage(int id, SubmissionStatus status, string inputFile)
        {
            //Arrange
            var testObject = new Submission
            {
                Id = id,
                Status = status,
                InputFile = inputFile,
                SubmissionFiles = new List<SubmissionFiles>()
                {
                    new SubmissionFiles()
                    {
                        Filename = "test.pdf",
                        Id = 1,
                        NumPages = 1

                    }
                }
            };

            //Act
            var dataTable = new List<Submission>() { testObject }.ToDetailedDataTable();


            //Assert
            dataTable.Should().NotBeNull();
            dataTable.Rows[0][nameof(testObject.Id)].Should().Equals(id);
            dataTable.Rows[0][nameof(testObject.Status)].Should().Equals(status);
            dataTable.Rows[0][nameof(testObject.InputFile)].Should().Equals(inputFile);
            dataTable.Rows[0][nameof(testObject.SubmissionFiles)].Should().NotBeNull();
            var subfiles = dataTable.Rows[0][nameof(testObject.SubmissionFiles)];
            subfiles.Should().BeAssignableTo<DataTable>();
            (subfiles as DataTable).Rows[0]["Filename"].Should().NotBeNull();
            (subfiles as DataTable).Rows[0]["Id"].Should().Be(1);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(8)]
        public void ToDetailedDataTable_ShouldReturnAllElements(int count)
        {
            //Arrange
            var list = new List<Submission>();

            for (int i = 0; i < count; i++)
            {
                list.Add(new Submission { Id = i,
                  SubmissionFiles = new List<SubmissionFiles>()
                {
                    new SubmissionFiles()
                    {
                        Filename = "test.pdf",
                        Id = 1,
                        NumPages = 1

                    }
                  }
                });
            }

            //Act
            var dataTable = list.ToDetailedDataTable();

            //Assert
            dataTable.Should().NotBeNull();
            dataTable.Rows.Count.Should().Equals(count);
        }

        [Test]
        public void ToDetailedDataTable_ShouldThrow_WhenInvalidObject()
        {
            //Arrange
            var testObject = new
            {
                Id = 1,
                List = new List<int>()
            };

            var testObject2 = new
            {
                Id = 2,
                Class = new { Name = "Test" }
            };

            //Act
            Action act = () => new[] { testObject }.ToDetailedDataTable();
            Action act2 = () => new[] { testObject2 }.ToDetailedDataTable();

            //Assert
            act.Should().Throw<NotSupportedException>();
            act2.Should().Throw<NotSupportedException>();
        }
    }
}
