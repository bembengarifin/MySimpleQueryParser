using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MySimpleQueryParser
{
    public class RegexParser : IParser
    {
        const string QueryPattern = @"^\s+(?<t>\w+)\s+(?<f>.+)\s+from\s+(?<e>\w+)\s*(?:where\s+(?<c>.+))*";

        public const string FAILED_PARSE_INVALID_NULL_EMPTY_INPUT = "Invalid null/empty/whitespace input";
        public const string FAILED_PARSE_INVALID_INPUT_QUERY = "Invalid or incomplete input query parameter";
        public const string FAILED_PARSE_INVALID_QUERY_TYPE = "Invalid query type was found";

        public ParseResult Parse(string inputQuery)
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

                var query = new Query();

                QueryType queryType;
                if (!Enum.TryParse<QueryType>(parsedQueryType, true, out queryType))
                {
                    return new ParseResult(FAILED_PARSE_INVALID_QUERY_TYPE + ":" + parsedQueryType);
                }
                else
                {
                    query.Type = queryType;
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
