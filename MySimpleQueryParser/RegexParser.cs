using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MySimpleQueryParser
{
    public class RegexParser : Parser
    {
        const string QueryPattern = @"^\s*(?<t>\w+)\s+(?<f>.+)\s+from\s+(?<e>\w+)\s*(?<w>where\s*(?<c>.+)*)*";
        const string ConditionPattern = @"\s*(?<f>\w+?)\s+(?<o>.+?)\s+?(?<v>.+)\s*?";

        public RegexParser(IList<EntityDefinition> entities)
            : base(entities)
        {
        }

        public override ParseResult Parse(string inputQuery)
        {
            if (string.IsNullOrWhiteSpace(inputQuery)) return new ParseResult(FAILED_PARSE_INVALID_NULL_EMPTY_INPUT);

            /* there are 4 data set to be captured
             * 1. query type
             * 2. field(s)
             * 3. entity name
             * 4. filter(s) (OPTIONAL)
             */

            var regex = new Regex(QueryPattern, RegexOptions.IgnoreCase);
            var match = regex.Match(inputQuery);

            if (match.Success)
            {
                var parsedQueryType = match.Groups["t"].Value;
                var parsedFields = match.Groups["f"].Value;
                var parsedEntityName = match.Groups["e"].Value;
                var parsedCriteria = match.Groups["c"].Value;
                var parsedWhere = match.Groups["w"].Value;

                var query = new Query();

                #region Query Type
                QueryType queryType;
                if (!Enum.TryParse<QueryType>(parsedQueryType, true, out queryType))
                {
                    return new ParseResult(FAILED_PARSE_INVALID_QUERY_TYPE + ":" + parsedQueryType);
                }
                query.Type = queryType;
                #endregion

                #region Entity Name
                if (!_entities.ContainsKey(parsedEntityName))
                {
                    return new ParseResult(FAILED_PARSE_INVALID_ENTITY_NAME + ":" + parsedEntityName);
                }
                var entity = _entities[parsedEntityName];
                query.EntityName = entity.Name;
                #endregion

                #region Fields
                query.Fields = new List<Field>();
                var splitFields = parsedFields.Split(',');
                var invalidFields = new List<string>();
                foreach (var fieldName in splitFields)
                {
                    var field = entity[fieldName.Trim()];
                    if (field == null)
                        invalidFields.Add(fieldName);

                    query.Fields.Add(field);
                }
                if (invalidFields.Count > 0)
                {
                    return new ParseResult(FAILED_PARSE_INVALID_FIELD_NAME + ":" + string.Join(",", invalidFields.ToArray()));
                }
                #endregion

                #region Filters

                if (!string.IsNullOrWhiteSpace(parsedWhere))
                {
                    if (string.IsNullOrWhiteSpace(parsedCriteria))
                    {
                        return new ParseResult(FAILED_PARSE_INVALID_WHERE_WITHOUT_FILTER);
                    }

                    query.Filters = new List<QueryFilter>();
                    var conditions = Regex.Split(parsedCriteria, " and ", RegexOptions.IgnoreCase);
                    foreach (var condition in conditions)
                    {
                        var matchCondition = Regex.Match(condition, ConditionPattern);
                        if (matchCondition.Success)
                        {
                            var conditionField = matchCondition.Groups["f"].Value.Trim();
                            var conditionOperator = matchCondition.Groups["o"].Value.ToUpperInvariant();
                            var conditionValue = matchCondition.Groups["v"].Value.Trim('(', ')', ' ', '[', ']');

                            var filter = new QueryFilter();

                            var field = entity[conditionField];
                            if (field == null)
                            {
                                return new ParseResult(FAILED_PARSE_INVALID_FIELD_NAME);
                            }
                            filter.Field = field;

                            switch (conditionOperator)
                            {
                                case "=":
                                    filter.Operator = FilterOperator.Equal;
                                    filter.FilterValues = new List<string> { conditionValue };
                                    break;

                                case "IN":
                                    filter.Operator = FilterOperator.InList;
                                    filter.FilterValues = conditionValue.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

                                    if (filter.FilterValues == null || filter.FilterValues.Count == 0)
                                    {
                                        return new ParseResult(FAILED_PARSE_INVALID_WHERE_EMPTY_IN_LIST);
                                    }
                                    //else
                                    //{
                                    //    return new ParseResult(FAILED_PARSE_INVALID_WHERE_INCORRECT_FORMAT_IN_LIST);
                                    //}
                                    break;

                                case "BETWEEN":
                                    filter.Operator = FilterOperator.Between;
                                    filter.FilterValues = conditionValue.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
                                    if (filter.FilterValues == null || filter.FilterValues.Count != 2)
                                    {
                                        return new ParseResult(FAILED_PARSE_INVALID_WHERE_BETWEEN_FORMAT);
                                    }

                                    break;

                                default:
                                    return new ParseResult(FAILED_PARSE_INVALID_WHERE_INVALID_OPERATOR + ":" + conditionOperator);
                            }

                            query.Filters.Add(filter);
                        }
                        else
                        {
                            return new ParseResult(FAILED_PARSE_INVALID_WHERE_INCORRECT_FORMAT);
                        }

                    }

                    // validate filter data type
                    var invalidFilterValues = new List<String>();
                    foreach (var f in query.Filters)
                    {
                        var lv = new List<String>();
                        switch (f.Field.FieldType)
                        {
                            case FieldType.DateType:
                                foreach (var v in f.FilterValues)
                                {
                                    DateTime result;
                                    if (!DateTime.TryParseExact(v, "dd-MMM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                                    {
                                        lv.Add(v);
                                    }
                                }
                                break;
                            case FieldType.NumberType:
                                foreach (var v in f.FilterValues)
                                {
                                    Decimal result;
                                    if (!Decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                                    {
                                        lv.Add(v);
                                    }
                                }
                                break;
                        }
                        if (lv.Count > 0)
                        {
                            invalidFilterValues.Add(string.Format("{0}({1}): {2}", f.Field.Name, f.Field.FieldType, string.Join(",", lv.ToArray())));
                        }
                    }

                    if (invalidFilterValues.Count > 0)
                    {
                        return new ParseResult(Parser.FAILED_PARSE_INVALID_WHERE_INVALID_DATA_TYPE + ", Details:" + string.Join(";", invalidFilterValues.ToArray()));
                    }
                }
                #endregion

                return new ParseResult(query);
            }
            else
            {
                return new ParseResult(FAILED_PARSE_INVALID_INPUT_QUERY);
            }
        }
    }
}

