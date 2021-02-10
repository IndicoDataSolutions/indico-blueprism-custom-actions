using System.Data;
using NUnit.Framework;
using Indico.BluePrism.Connector.Helpers;
using FluentAssertions;

namespace Indico.BluePrism.Connector.Tests.Helpers
{
    public class DataTableExtensionsTests
    {
        private const string _id = "Id";

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(178)]
        public void ToListInt_ShouldReturnRightNumberOfElements(int elementsCount)
        {
            //Arrange
            var dataTable = new DataTable();

            PopulateDataTable(dataTable, elementsCount);

            //Act
            var list = dataTable.ToList<int>(0);

            //Assert
            list.Should().NotBeNull();
            list.Should().HaveCount(elementsCount);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(178)]
        public void ToListString_ShouldReturnRightNumberOfElements(int elementsCount)
        {
            //Arrange
            var dataTable = new DataTable();

            PopulateDataTable(dataTable, elementsCount);

            //Act
            var list = dataTable.ToList<int>(_id);

            //Assert
            list.Should().NotBeNull();
            list.Should().HaveCount(elementsCount);
        }

        private static void PopulateDataTable(DataTable dataTable, int elementCount)
        {
            dataTable.Columns.Add(new DataColumn(_id, typeof(int)));

            for (int i = 0; i < elementCount; i++)
            {
                dataTable.Rows.Add(i);
            }
        }
    }
}
