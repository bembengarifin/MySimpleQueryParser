using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MySimpleQueryParser
{
    public class RegexParser : Parser
    {
        const string QueryPattern = @"^\s+(?<t>\w+)\s+(?<f>.+)\s+from\s+(?<e>\w+)\s*(?<w>where\s+(?<c>.+))*";

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

                if (!string.IsNullOrWhiteSpace(parsedWhere) && string.IsNullOrWhiteSpace( parsedCriteria))
                {
                    return new ParseResult(FAILED_PARSE_INVALID_WHERE_WITHOUT_FILTER);
                }

                var query = new Query();

                QueryType queryType;
                if (!Enum.TryParse<QueryType>(parsedQueryType, true, out queryType))
                {
                    return new ParseResult(FAILED_PARSE_INVALID_QUERY_TYPE + ":" + parsedQueryType);
                }
                query.Type = queryType;

                if (!_entities.ContainsKey(parsedEntityName))
                {
                    return new ParseResult(FAILED_PARSE_INVALID_ENTITY_NAME + ":" + parsedEntityName);
                }
                var entity = _entities[parsedEntityName];
                query.EntityName = entity.Name;

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

                return new ParseResult(query);
            }
            else
            {
                return new ParseResult(FAILED_PARSE_INVALID_INPUT_QUERY);
            }
        }
    }
}
