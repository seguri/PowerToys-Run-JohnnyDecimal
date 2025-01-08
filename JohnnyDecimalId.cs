using System.Text.RegularExpressions;

namespace Community.PowerToys.Run.Plugin.JohnnyDecimal
{
    public class JohnnyDecimalId
    {
        private static readonly Regex AREA_REGEX = new(@"^(?<area>\d)");
        private static readonly Regex CAT_REGEX = new(@"^(?<area>\d)(?<category>\d{1})");
        private static readonly Regex ID_REGEX = new(@"^(?<area>\d)(?<category>\d{1})[ ,./]?(?<id>\d{1,2})");

        public string Area { get; } = string.Empty;
        public string Category { get; } = string.Empty;
        public string Id { get; } = string.Empty;

        public bool HasArea => !string.IsNullOrWhiteSpace(Area);
        public bool HasCategory => !string.IsNullOrWhiteSpace(Category);
        public bool HasId => !string.IsNullOrWhiteSpace(Id);

        private JohnnyDecimalId(string area, string category, string id)
        {
            Area = area;
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
                    result = new JohnnyDecimalId(match.Groups["area"].Value, match.Groups["area"].Value + match.Groups["category"].Value, match.Groups["id"].Value);
                    return true;
                }
                match = CAT_REGEX.Match(input);
                if (match.Success)
                {
                    result = new JohnnyDecimalId(match.Groups["area"].Value, match.Groups["area"].Value + match.Groups["category"].Value, string.Empty);
                    return true;
                }
                match = AREA_REGEX.Match(input);
                if (match.Success)
                {
                    result = new JohnnyDecimalId(match.Groups["area"].Value, string.Empty, string.Empty);
                    return true;
                }
            }
            result = new JohnnyDecimalId(string.Empty, string.Empty, string.Empty);
            return false;
        }

        public string AreaGlob => HasArea ? $"{Area}0-{Area}9*" : "";

        public string CategoryGlob => HasCategory ? $"{Category}*" : "";

        public string IdGlob => HasId ? $"{Category}.{Id}*" : "";
    }
}
