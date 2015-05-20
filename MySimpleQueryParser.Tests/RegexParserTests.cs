using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
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
            var entities = new List<EntityDefinition>
            {
                new EntityDefinition("ety", new List<Field> () 
                    { 
                        new Field ("abc", FieldType.GeneralType),
                        new Field ("def", FieldType.DateType),
                        new Field ("ghi", FieldType.NumberType),
                        new Field ("jkl", FieldType.GeneralType),
                        new Field ("mno", FieldType.NumberType),
                    }),
                new EntityDefinition("xyz", new List<Field> () 
                    { 
                        new Field ("uvw", FieldType.GeneralType),
                        new Field ("rst", FieldType.DateType),
                        new Field ("opq", FieldType.NumberType),
                    }),
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
            _parser = new RegexParser(new List<EntityDefinition>());
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
        public void Incomplete_Input_Only_Query_Type_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select  ");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, Parser.FAILED_PARSE_INVALID_INPUT_QUERY);
            Assert.IsNull(result.Query);
        }

        [TestMethod]
        public void Incomplete_Input_Only_Query_Type_And_Fields_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select abc, def ");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, Parser.FAILED_PARSE_INVALID_INPUT_QUERY);
            Assert.IsNull(result.Query);
        }

        [TestMethod]
        public void Incomplete_Input_Where_Without_Condition_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select abc, def from ety where ");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, Parser.FAILED_PARSE_INVALID_WHERE_WITHOUT_FILTER);
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
            var result = _parser.Parse(" sEleCt abc from ety ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.Type == QueryType.Select);
        }

        [TestMethod]
        public void Pivot_Input_Will_Return_Pivot_Query_Type()
        {
            var result = _parser.Parse(" pivot  abc from ety ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.Type == QueryType.Pivot);
        }

        [TestMethod]
        public void Chart_Input_Will_Return_Chart_Query_Type()
        {
            var result = _parser.Parse(" chart  abc from ety  ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.Type == QueryType.Chart);
        }
        #endregion

        #region Entity Name Tests

        [TestMethod]
        public void Invalid_EntityName_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select abc from qwe ");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_ENTITY_NAME));
        }

        [TestMethod]
        public void Valid_EntityName_Will_Return_Success_Parse_Result()
        {
            var result = _parser.Parse(" select rst fRoM xYz ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.EntityName == "xyz");
        }

        #endregion-

        #region Field Tests

        [TestMethod]
        public void Invalid_Field_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select abc from XyZ ");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_FIELD_NAME));
        }

        [TestMethod]
        public void Mixed_Valid_And_Invalid_Fields_Will_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select rsT, abc, uvw from XyZ ");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_FIELD_NAME));
        }

        [TestMethod]
        public void Valid_FieldName_Will_Return_Success_Parse_Result()
        {
            var result = _parser.Parse(" select rst from xYz ");
            Assert.IsTrue(result.IsSuccess);
            CollectionAssert.AreEqual(new List<Field>() { new Field("rSt", FieldType.DateType) }, result.Query.Fields.ToList());
        }

        [TestMethod]
        public void Valid_FieldNames_Will_Return_Success_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz ");
            Assert.IsTrue(result.IsSuccess);
            CollectionAssert.AreEqual(new List<Field>() { 
                                                            new Field("uVw", FieldType.GeneralType) ,
                                                            new Field("rSt", FieldType.DateType) ,
                                                            new Field("OPQ", FieldType.NumberType) ,
                                                        }
                , result.Query.Fields.ToList());
        }

        #endregion
    }
}
