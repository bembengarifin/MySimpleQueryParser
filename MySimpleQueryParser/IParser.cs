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
        public const string FAILED_PARSE_INVALID_FIELD_NAME = "Invalid field name was found";
        public const string FAILED_PARSE_INVALID_WHERE_WITHOUT_FILTER = "Invalid query, where is specified without any filter condition";

        protected IDictionary<string, EntityDefinition> _entities;
        public Parser(IList<EntityDefinition> entities)
        {
            if (entities == null) throw new ArgumentNullException();
            if (entities.Count == 0) throw new ArgumentException();

            _entities = entities.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, EntityDefinition> Entities { get { return _entities; } }
        public abstract ParseResult Parse(string inputQuery);
    }

    public class EntityDefinition
    {
        public EntityDefinition(string name, IList<Field> fields)
        {
            Name = name;
            Fields = fields;

            _fieldDictionary = fields.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        }
        public string Name { get; private set; }
        public IList<Field> Fields { get; private set; }

        private IDictionary<string, Field> _fieldDictionary;

        public Field this[string fieldName]
        {
            get
            {
                return _fieldDictionary.ContainsKey(fieldName) ? _fieldDictionary[fieldName] : null;
            }
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", Name, Fields);
        }
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

        public override string ToString()
        {
            return string.Format("IsSuccess:{0},Message:{1},Query:{2}", IsSuccess, Message, Query);
        }
    }

    public class Query
    {
        public QueryType Type { get; set; }
        public IList<Field> Fields { get; set; }
        public string EntityName { get; set; }
        public IList<QueryFilter> Filters { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder(200);

            sb.AppendFormat("{0} {1} FROM {2}"
                , Type
                , string.Join(",", Fields.Select(x => x.ToString()).ToArray())
                , EntityName
                ); 
            
            if (Filters != null && Filters.Count > 0)
            {
                sb.AppendFormat(" WHERE {1}", string.Join(",", Filters.Select(x => x.ToString()).ToArray()));
            }

            return sb.ToString();
        }
    }

    public enum QueryType
    {
        Select,
        Pivot,
        Chart
    }

    public class Field : IEquatable<Field>
    {
        public Field(string name, FieldType fieldType)
        {
            Name = name;
            FieldType = fieldType;
        }
        public string Name { get; private set; }
        public FieldType FieldType { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, FieldType);
        }

        public bool Equals(Field other)
        {
            if (other == null)
                return false;

            return string.Compare(this.Name, other.Name, true) == 0
                && this.FieldType.Equals(other.FieldType);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Field);
        }
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
