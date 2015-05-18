using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySimpleQueryParser
{
    public interface IParser
    {
        ParseResult Parse(string inputQuery);
        IDictionary<string, EntityDefinition> Entities { get; }
    }

    public abstract class Parser : IParser
    {
        public const string FAILED_PARSE_INVALID_NULL_EMPTY_INPUT = "Invalid null/empty/whitespace input";
        public const string FAILED_PARSE_INVALID_INPUT_QUERY = "Invalid or incomplete input query parameter";
        public const string FAILED_PARSE_INVALID_QUERY_TYPE = "Invalid query type was found";
        public const string FAILED_PARSE_INVALID_ENTITY_NAME = "Invalid entity name was found";

        protected IDictionary<string, EntityDefinition> _entities;
        public Parser(IDictionary<string, EntityDefinition> entities)
        {
            if (entities == null) throw new ArgumentNullException();
            if (entities.Count == 0) throw new ArgumentException();

            _entities = entities;
        }

        public IDictionary<string, EntityDefinition> Entities { get { return _entities; } }
        public abstract ParseResult Parse(string inputQuery);
    }

    public class EntityDefinition
    {
        public string Name { get; set; }
        public IList<Field> Fields { get; set; }
    }

    public class ParseResult
    {
        private ParseResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public ParseResult(Query query)
            : this(true)
        {
            Query = query;
        }

        public ParseResult(string failMessage)
            : this(false)
        {
            Message = failMessage;
        }

        public bool IsSuccess { get; private set; }
        public Query Query { get; private set; }
        public String Message { get; private set; }
    }

    public class Query
    {
        public QueryType Type { get; set; }
        public IList<Field> Fields { get; set; }
        public string EntityName { get; set; }
        public IList<QueryFilter> Filters { get; set; }
    }

    public enum QueryType
    {
        Select,
        Pivot,
        Chart
    }

    public class Field
    {
        public Field(string name, FieldType fieldType)
        {
            Name = name;
            FieldType = fieldType;
        }
        public string Name { get; set; }
        public FieldType FieldType { get; set; }
    }

    public class QueryFilter
    {
        public Field Field { get; set; }
        public IList<string> FilterValues { get; set; }
    }

    public class PivotField : Field
    {
        public PivotField(string name, FieldType fieldType, PivotArea pivotArea)
            : base(name, fieldType)
        {
            PivotArea = pivotArea;
        }
        public PivotArea PivotArea { get; set; }
    }

    public class ChartField : Field
    {
        public ChartField(string name, FieldType fieldType, ChartArea chartArea, ChartType chartType)
            : base(name, fieldType)
        {
            ChartArea = chartArea;
            ChartType = chartType;
        }

        public ChartArea ChartArea { get; set; }
        public ChartType ChartType { get; set; }
    }

    public enum PivotArea
    {
        Row,
        Column,
        Data,
        Filter
    }

    public enum ChartArea
    {
        XAxis,
        YAxis,
        ZAxis,
    }

    public enum ChartType
    {
        Bar,
        Line,
        Pie,
    }

    public enum FieldType
    {
        GeneralType,
        DateType,
        NumberType,
    }
}
