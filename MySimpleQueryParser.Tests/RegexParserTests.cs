using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MySimpleQueryParser.Tests
{
    [TestClass]
    public class RegexParserTests
    {
        IParser _parser;

        [TestInitialize]
        public void SetUp()
        {
            _parser = new RegexParser();
        }

        [TestMethod]
        public void Null_Input_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(null);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, RegexParser.FAILED_PARSE_INVALID_NULL_EMPTY_INPUT);
        }

        [TestMethod]
        public void Empty_Input_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(string.Empty);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, RegexParser.FAILED_PARSE_INVALID_NULL_EMPTY_INPUT);
        }

        [TestMethod]
        public void Whitespace_Input_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse("   ");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, RegexParser.FAILED_PARSE_INVALID_NULL_EMPTY_INPUT);
        }

        [TestMethod]
        public void Incomplete_Input_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select  ");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, RegexParser.FAILED_PARSE_INVALID_INPUT_QUERY);
            Assert.IsNull(result.Query);
        }

        [TestMethod]
        public void Unknown_Input_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" selectabc  abc from def ");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(RegexParser.FAILED_PARSE_INVALID_QUERY_TYPE));
            Assert.IsNull(result.Query);
        }

        [TestMethod]
        public void Select_Input_Will_Return_Select_Query_Type()
        {
            var result = _parser.Parse(" select abc from def ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.Type == QueryType.Select);
        }

        [TestMethod]
        public void Pivot_Input_Will_Return_Pivot_Query_Type()
        {
            var result = _parser.Parse(" pivot  abc from def  ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.Type == QueryType.Pivot);
        }

        [TestMethod]
        public void Chart_Input_Will_Return_Chart_Query_Type()
        {
            var result = _parser.Parse(" chart  abc from def  ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.Type == QueryType.Chart);
        }

    }
}
