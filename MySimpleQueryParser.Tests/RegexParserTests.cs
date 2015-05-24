using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
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
        public void Null_EntityDefinitions_Should_Raise_Exception()
        {
            _parser = new RegexParser(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Empty_EntityDefinitions_Should_Raise_Exception()
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
        public void Null_Input_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(null);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, Parser.FAILED_PARSE_INVALID_NULL_EMPTY_INPUT);
        }

        [TestMethod]
        public void Empty_Input_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(string.Empty);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, Parser.FAILED_PARSE_INVALID_NULL_EMPTY_INPUT);
        }

        [TestMethod]
        public void Whitespace_Input_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse("   ");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, Parser.FAILED_PARSE_INVALID_NULL_EMPTY_INPUT);
        }

        [TestMethod]
        public void Incomplete_Input_Only_Query_Type_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select  ");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, Parser.FAILED_PARSE_INVALID_INPUT_QUERY);
            Assert.IsNull(result.Query);
        }

        [TestMethod]
        public void Incomplete_Input_Only_Query_Type_And_Fields_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select abc, def ");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, Parser.FAILED_PARSE_INVALID_INPUT_QUERY);
            Assert.IsNull(result.Query);
        }

        [TestMethod]
        public void Incomplete_Input_Where_Without_Condition_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select abc, def from ety where ");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(result.Message, Parser.FAILED_PARSE_INVALID_WHERE_WITHOUT_FILTER);
            Assert.IsNull(result.Query);
        }

        [TestMethod]
        public void Unknown_Input_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" selectabc  abc from XYZ ");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_QUERY_TYPE));
            Assert.IsNull(result.Query);
        }
        #endregion

        #region Query Type Tests
        [TestMethod]
        public void Select_Input_Should_Return_Select_Query_Type()
        {
            var result = _parser.Parse(" sEleCt abc from ety ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.Type == QueryType.Select);
        }

        [TestMethod]
        public void Pivot_Input_Should_Return_Pivot_Query_Type()
        {
            var result = _parser.Parse(" pivot  abc from ety ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.Type == QueryType.Pivot);
        }

        [TestMethod]
        public void Chart_Input_Should_Return_Chart_Query_Type()
        {
            var result = _parser.Parse(" chart  abc from ety  ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.Type == QueryType.Chart);
        }
        #endregion

        #region Entity Name Tests

        [TestMethod]
        public void Invalid_EntityName_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select abc from qwe ");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_ENTITY_NAME));
        }

        [TestMethod]
        public void Valid_EntityName_Should_Return_Success_Parse_Result()
        {
            var result = _parser.Parse(" select rst fRoM xYz ");
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Query.EntityName == "xyz");
        }

        #endregion-

        #region Field Tests

        [TestMethod]
        public void Invalid_Field_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select abc from XyZ ");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_FIELD_NAME));
        }

        [TestMethod]
        public void Mixed_Valid_And_Invalid_Fields_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select rsT, abc, uvw from XyZ ");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_FIELD_NAME));
        }

        [TestMethod]
        public void Valid_FieldName_Should_Return_Success_Parse_Result()
        {
            var result = _parser.Parse(" select rst from xYz ");
            Assert.IsTrue(result.IsSuccess);
            CollectionAssert.AreEqual(new List<Field>() { new Field("rSt", FieldType.DateType) }, result.Query.Fields.ToList());
        }

        [TestMethod]
        public void Valid_FieldNames_Should_Return_Success_Parse_Result()
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

        #region Where Condition Tests
        
        void AssertFilter(QueryFilter filter, string name, FilterOperator filterOperator, params string[] values)
        {
            Assert.AreEqual(name, filter.Field.Name);
            Assert.AreEqual(filterOperator, filter.Operator);
            CollectionAssert.AreEquivalent(values.ToList(), filter.FilterValues.ToList());
        }

        [TestMethod]
        public void Valid_Equal_Condition_Should_Return_Success_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz where uvw = 123");
            Assert.IsTrue(result.IsSuccess);

            var filter = result.Query.Filters[0];
            AssertFilter(filter, "uvw", FilterOperator.Equal, "123" );
        }

        [TestMethod]
        public void More_Than_Three_Resultset_In_Condition_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz where uvw = 123 456");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_WHERE_INCORRECT_FORMAT));
        }

        [TestMethod]
        public void Less_Than_Three_Resultset_In_Condition_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz where uvw =");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_WHERE_INCORRECT_FORMAT));
        }

        [TestMethod]
        public void Invalid_Operator_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz where uvw <> 123");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_WHERE_INVALID_OPERATOR));
        }

        [TestMethod]
        public void Valid_InList_Condition_Should_Return_Success_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz where uvw in ( 123, 456 )");
            Assert.IsTrue(result.IsSuccess);

            var filter = result.Query.Filters[0];
            AssertFilter(filter, "uvw", FilterOperator.Equal, "123", "456");
        }

        [TestMethod]
        public void Empty_InList_Condition_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz where uvw in ()");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_WHERE_EMPTY_IN_LIST));
        }

        [TestMethod]
        public void Invalid_InList_Condition_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz where uvw in 1,2");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_WHERE_INCORRECT_FORMAT_IN_LIST));
        }
        
        [TestMethod]
        public void Valid_Between_Condition_Should_Return_Success_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz where uvw between ( 123, 456 )");
            Assert.IsTrue(result.IsSuccess);

            var filter = result.Query.Filters[0];
            AssertFilter(filter, "uvw", FilterOperator.Equal, "123", "456");
        }

        [TestMethod]
        public void Empty_Between_Condition_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz where uvw between ()");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_WHERE_BETWEEN_FORMAT));
        }

        [TestMethod]
        public void One_Empty_Between_Condition_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz where uvw in (1,)");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_WHERE_BETWEEN_FORMAT));
        }

        [TestMethod]
        public void Valid_Multiple_Conditions_Should_Return_Success_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz where uvw in ( 123, 456 ) and opq = 789 and rst between (01-Jan-2015, 05-Feb-2015)");
            Assert.IsTrue(result.IsSuccess);

            Assert.AreEqual(3, result.Query.Filters.Count);

            var filter = result.Query.Filters[0];
            AssertFilter(filter, "uvw", FilterOperator.InList, "123", "456");

            var filter2 = result.Query.Filters[1];
            AssertFilter(filter, "opq", FilterOperator.Equal, "789");

            var filter3 = result.Query.Filters[2];
            AssertFilter(filter, "rst", FilterOperator.Between, "01-Jan-2015", "05-Feb-2015");
        }

        [TestMethod]
        public void Invalid_Date_Filter_Value_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz where rst = 2015");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_WHERE_INVALID_DATE_VALUE));
        }

        [TestMethod]
        public void Invalid_Numeric_Filter_Value_Should_Return_Failed_Parse_Result()
        {
            var result = _parser.Parse(" select uvw, rst,opq  from xYz where opq = 12a");
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.StartsWith(Parser.FAILED_PARSE_INVALID_WHERE_INVALID_NUMERIC_VALUE));
        }

        [TestMethod]
        public void test_regex()
        {
            string input = "(abc ,cde ,efg)";
            string pattern = " ANd ";            // Split on hyphens 

            string[] substrings = Regex.Split(input, pattern, RegexOptions.IgnoreCase);
            foreach (string match in substrings)
            {
                Console.WriteLine("'{0}'", match);
            }
        }

        #endregion
    }
}
