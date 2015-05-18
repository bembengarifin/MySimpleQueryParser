using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
//using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace MySimpleQueryParser.Tests
{
    [TestClass]
    public class RegexParserTests
    {
        IParser _parser;

        [TestInitialize]
        public void SetUp()
        {
            var entities = new Dictionary<string, EntityDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                { "ety", new EntityDefinition
                    { 
                        Name = "ety", 
                        Fields = new List<Field> () 
                        { 
                            new Field ("abc", FieldType.GeneralType),
                            new Field ("def", FieldType.DateType),
                            new Field ("ghi", FieldType.NumberType),
                            new Field ("jkl", FieldType.GeneralType),
                            new Field ("mno", FieldType.NumberType),
                        } 
                    }
                },
                { "xyz", new EntityDefinition
                    { 
                        Name = "xyz", 
                        Fields = new List<Field> () 
                        { 
                            new Field ("uvw", FieldType.GeneralType),
                            new Field ("rst", FieldType.DateType),
                            new Field ("opq", FieldType.NumberType),
                        } 
                    }
                },
            };

            _parser = new RegexParser(entities);
        }

        #region Parser Initialization Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null_EntityDefinitions_Will_Raise_Exception()
        {
            _parser = new RegexParser(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Empty_EntityDefinitions_Will_Raise_Exception()
        {
            _parser = new RegexParser(new Dictionary<string, EntityDefinition>());
        }

        [TestMethod]
        public void Verify_Valid_EntityDefinitions()
        {
            Assert.AreEqual(2, _parser.Entities.Count);
        }
        #endregion

        #region Basic Input Tests
        [TestMethod]
        public void Null_Input_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(null);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, Parser.FAILED_PARSE_INVALID_NULL_EMPTY_INPUT);
        }

        [TestMethod]
        public void Empty_Input_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(string.Empty);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, Parser.FAILED_PARSE_INVALID_NULL_EMPTY_INPUT);
        }

        [TestMethod]
        public void Whitespace_Input_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse("   ");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, Parser.FAILED_PARSE_INVALID_NULL_EMPTY_INPUT);
        }

        [TestMethod]
        public void Incomplete_Input_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select  ");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, Parser.FAILED_PARSE_INVALID_INPUT_QUERY);
            Assert.IsNull(result.Query);
        }

        [TestMethod]
        public void Unknown_Input_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" selectabc  abc from XYZ ");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_QUERY_TYPE));
            Assert.IsNull(result.Query);
        }
        #endregion

        #region Query Type Tests
        [TestMethod]
        public void Select_Input_Will_Return_Select_Query_Type()
        {
            var result = _parser.Parse(" select abc from XYZ ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.Type == QueryType.Select);
        }

        [TestMethod]
        public void Pivot_Input_Will_Return_Pivot_Query_Type()
        {
            var result = _parser.Parse(" pivot  abc from XYZ  ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.Type == QueryType.Pivot);
        }

        [TestMethod]
        public void Chart_Input_Will_Return_Chart_Query_Type()
        {
            var result = _parser.Parse(" chart  abc from XYZ  ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.Type == QueryType.Chart);
        }
        #endregion

        #region Entity Name Tests

        [TestMethod]
        public void Invalid_EntityName_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select abc from XYZ ");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_ENTITY_NAME));
        }

        [TestMethod]
        public void Valid_EntityName_Will_Return_Success_Parse_Result()
        {
            var result = _parser.Parse(" select rst from xYz ");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Query.EntityName == "xyz");
        }

        #endregion
    }
}
