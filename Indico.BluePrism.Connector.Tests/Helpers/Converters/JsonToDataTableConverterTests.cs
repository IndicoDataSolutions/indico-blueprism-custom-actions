using FluentAssertions;
using Indico.BluePrism.Connector.Helpers.Converters;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Indico.BluePrism.Connector.Tests.Helpers.Converters
{
    public class JsonToDataTableConverterTests
    {
        private readonly JsonToDataTableConverter _sut = new JsonToDataTableConverter();
        private readonly DataTableToJsonConverter _dtToJson = new DataTableToJsonConverter();

        [TestCase(@"{ ""test"" : 123 }")]
        [TestCase(@"{ ""test"" : 1.25 }")]
        [TestCase(@"{ ""test"" : ""test"" }")]
        [TestCase(@"{ ""test"" : null }")]
        [TestCase(@"{ ""test"" : ""c0134b41-f579-42d3-badd-8accc2fb3eae"" }")]
        [TestCase(@"{ ""test"" : ""https://indico.org"" }")]
        [TestCase(@"{ ""test"" : ""2017-05-17T23:09:14.000000Z"" }")]
        [TestCase(@"{ ""test"" : ""00:00:01"" }")]
        public void ToDataTable_ConvertsSimpleObjects(string json)
        {
            // Arrange
            var token = JObject.Parse(json);

            // Act
            var dt = _sut.ToDataTable(token);

            // Assert
            _dtToJson.ToJson(dt).Should().BeEquivalentTo(token);
        }

        [TestCase(@"[{ ""test"" : null }]")]
        public void ToDataTable_ShouldUseStringColumn_WhenNull(string json)
        {
            // Arrange
            var array = JArray.Parse(json);

            // Act
            var dt = _sut.ToDataTable(array);

            // Assert
            dt.Columns[0].DataType.Should().Be(typeof(string));
        }

        [TestCase(@"[{ ""test"" : null }, { ""test"": ""asdf"" }]")]
        public void ToDataTable_ShouldReplaceDbNullWithAcctualType(string json)
        {
            // Arrange
            var array = JArray.Parse(json);

            // Act
            var dt = _sut.ToDataTable(array);

            // Assert
            dt.Columns[0].DataType.Should().Be(typeof(string));
        }

        [TestCase(@"[ { ""test"" : 123 } ]")]
        public void ToDataTable_ShouldConvertArray(string arrayJson)
        {
            // Arrange
            var array = JArray.Parse(arrayJson);

            // Act
            var dt = _sut.ToDataTable(array);

            // Assert
            _dtToJson.ToJson(dt).Should().BeEquivalentTo(array);
        }

        [TestCase(@"{ ""test"" : { ""inner"" : 123 } }")]
        public void ToDataTable_ShouldConvertInnerObject(string json)
        {
            // Arrange
            var obj = JObject.Parse(json);

            // Act
            var dt = _sut.ToDataTable(obj);

            // Assert
            _dtToJson.ToJson(dt).Should().BeEquivalentTo(obj);
        }

        [TestCase(@"{ ""test"": [ 123 ]}")]
        public void ToDataTable_ShouldConvertInnerArray(string json)
        {
            // Arrange
            var obj = JObject.Parse(json);

            // Act
            var dt = _sut.ToDataTable(obj);

            // Assert
            _dtToJson.ToJson(dt).Should().BeEquivalentTo(obj);
        }
    }
}
