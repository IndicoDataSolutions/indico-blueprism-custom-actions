﻿using System.Data;
using FluentAssertions;
using Indico.BluePrism.Connector.Helpers.Converters;
using Indico.BluePrism.Connector.Helpers.Converters.Exceptions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Indico.BluePrism.Connector.Tests.Helpers.Converters
{
    public class DataTableToJsonConverterTests
    {
        private readonly DataTableToJsonConverter _sut = new DataTableToJsonConverter();

        private readonly DataTableHelper _dataTableHelper = new DataTableHelper();

        [Test]
        public void ToJson_ShouldReturnObject_WhenProperMarker()
        {
            // Arrange
            var dataTable = new DataTable() { Columns = { _dataTableHelper.CreateFromJObjectMarker() } };
            dataTable.Columns.Add(new DataColumn("anything", typeof(int)));
            dataTable.Rows.Add(123);

            // Act
            var json = _sut.ToJson(dataTable);

            // Assert
            json.Should().BeEquivalentTo(JObject.Parse(@"{ ""test"": 123 }"));
        }

        [Test]
        public void ToJson_ShouldReturnArray_WhenProperMarker()
        {
            // Arrange
            var dataTable = new DataTable
            {
                Columns =
                {
                    new DataColumn(JsonToDataTableConverter.ArrayItemColumnName, typeof(int)),
                    _dataTableHelper.CreateFromJArrayMarker(),
                }
            };
            dataTable.Rows.Add(123);

            // Act
            var json = _sut.ToJson(dataTable);

            // Assert
            json.Should().BeEquivalentTo(JArray.Parse(@"[ 123 ]"));
        }

        [TestCase(null, JTokenType.Null)]
        [TestCase(12, JTokenType.Integer)]
        [TestCase(13.1232f, JTokenType.Float)]
        [TestCase("test", JTokenType.String)]
        public void ToJson_ShouldConvertValues(object value, JTokenType expectedTokenType)
        {
            // Arrange
            var dataTable = new DataTable
            {
                Columns =
                {
                    new DataColumn("test", value?.GetType() ?? typeof(object)),
                    _dataTableHelper.CreateFromJObjectMarker()
                }
            };
            dataTable.Rows.Add(value);

            // Act
            var json = _sut.ToJson(dataTable);

            // Assert
            bool isNumeric = value != null && typeof(decimal).IsAssignableFrom(value?.GetType());
            var valueJson = isNumeric ? value : $@"""{value}""";
            json.Should().BeEquivalentTo(JObject.Parse($@"{{ ""test"": {valueJson} }}"));
            var token = json["test"];
            token.Type.Should().Be(expectedTokenType);
        }

        [TestCase(null)]
        [TestCase("invalid")]
        public void ToJson_ShouldThrow_WhenNoMarker(string tableName) => this
            .Invoking(_ => _sut.ToJson(new DataTable(tableName))).Should().Throw<CannotDetermineTableSourceException>();

        [Test]
        public void ToJson_ShouldThrow_WhenInvalidColumnCount()
        {
            // Arrange
            var dt = new DataTable { Columns = { new DataColumn("test"), _dataTableHelper.CreateFromJObjectMarker(), } };
            dt.Columns.Add();
            dt.Rows.Add("test");
            dt.Rows.Add("test");

            // Act
            this.Invoking(_ => _sut.ToJson(dt)).Should().Throw<JObjectTableRowCountError>();
        }

        [Test]
        public void ToJson_ShouldConvertDecimalToInt_WhenNoFractionalPart()
        {
            // Arrange
            var dt = new DataTable {Columns = {new DataColumn("test", typeof(decimal)), _dataTableHelper.CreateFromJObjectMarker(), } };
            dt.Rows.Add(123);

            // Act
            var json = _sut.ToJson(dt);

            // Assert
            json["test"]!.Type.Should().Be(JTokenType.Integer);
        }
    }
}
