using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySimpleQueryParser
{
    public interface IParser
    {
        ParseResult Parse(string inputQuery);
    }

    public class ParseResult
    {
        private ParseResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public ParseResult(Query query) : this(true)
        {
            Query = query;
        }

        public ParseResult(string failMessage) : this(false)
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
        public IList<QueryField> Fields { get; set; }
        public string EntityName { get; set; }
        public IList<QueryFilter> Filters { get; set; }
    }

    public enum QueryType
    {
        Select,
        Pivot,
        Chart
    }

    public class QueryField
    {
        public string Name { get; set; }
        public QueryFieldType FieldType { get; set; }
    }

    public class QueryFilter
    {
        public QueryField Field { get; set; }
        public IList<string> FilterValues { get; set; }
    }

    public class PivotQueryField : QueryField
    {
        public PivotArea PivotLocation { get; set; }
    }

    public class ChartQueryField : QueryField
    {
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

    public enum QueryFieldType
    {
        GeneralType,
        DateType,
        NumberType,
    }
}
