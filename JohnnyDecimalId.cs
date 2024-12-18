using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Community.PowerToys.Run.Plugin.JohnnyDecimal
{
    public class JohnnyDecimalId
    {
        private static readonly Regex ID_REGEX = new(@"^(?<category>\d{2})[ ,./]?(?<id>\d{2})");

        public string Category { get; } = string.Empty;
        public string Id { get; } = string.Empty;

        private JohnnyDecimalId(string category, string id)
        {
            Category = category;
            Id = id;
        }

        public static bool TryParse(string input, out JohnnyDecimalId result)
        {
            if (!string.IsNullOrEmpty(input))
            {
                var match = ID_REGEX.Match(input);
                if (match.Success)
                {
                    var category = match.Groups["category"].Value;
                    var id = match.Groups["id"].Value;
                    result = new JohnnyDecimalId(category, id);
                    return true;
                }
            }
            result = new JohnnyDecimalId(string.Empty, string.Empty);
            return false;
        }

        public char GetArea() => string.IsNullOrWhiteSpace(Category) ? '\0' : Category[0];

        public string GetCategory() => Category;

        public string GetId() => Id;
    }
}
