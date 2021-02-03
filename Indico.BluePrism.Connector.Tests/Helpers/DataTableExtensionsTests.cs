using NUnit.Framework;
using Indico.BluePrism.Connector.Helpers;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;

namespace Indico.BluePrism.Connector.Tests.Helpers
{
    public class DataTableExtensionsTests
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
    }
}
