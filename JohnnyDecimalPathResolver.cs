using System.IO;

namespace Community.PowerToys.Run.Plugin.JohnnyDecimal
{
    public record SearchResult(DirectoryInfo[]? Directories = null, string ErrorMessage = "")
    {
        public readonly DirectoryInfo[] Directories = Directories ?? [];
    }

    public class JohnnyDecimalPathResolver(string? rootFolderPath)
    {
        private readonly string _rootFolderPath = string.IsNullOrEmpty(rootFolderPath) ? "" : rootFolderPath;

        public SearchResult FindPaths(JohnnyDecimalId id)
        {
            // ROOT_DIR_NOT_SET
            if (string.IsNullOrEmpty(_rootFolderPath))
            {
                return new SearchResult(ErrorMessage: "Please set the root folder in the plugin settings");
            }
            // ROOT_DIR_NOT_FOUND
            var rootDirectory = new DirectoryInfo(_rootFolderPath);
            if (!rootDirectory.Exists)
            {
                return new SearchResult(ErrorMessage: "Root folder does not exist");
            }
            // AREA_NOT_FOUND
            if (!id.HasArea)
            {
                return new SearchResult(ErrorMessage: $"Please provide a valid Area");
            }
            // AREA_DIR_EMPTY
            var areas = rootDirectory.GetDirectories(id.AreaGlob, SearchOption.TopDirectoryOnly);
            if (areas.Length == 0)
            {
                return new SearchResult(ErrorMessage: $"Area '{id.AreaGlob[..^1]}' is empty");
            }
            // CAT_NOT_FOUND
            if (!id.HasCategory)
            {
                return new SearchResult(Directories: areas);
            }
            // CAT_DIR_EMPTY
            var categories = areas[0].GetDirectories(id.CategoryGlob, SearchOption.TopDirectoryOnly);
            if (categories.Length == 0)
            {
                return new SearchResult(ErrorMessage: $"Category '{id.CategoryGlob[..^1]}' is empty");
            }
            // ID_NOT_FOUND
            if (!id.HasId)
            {
                return new SearchResult(Directories: categories);
            }
            // ID_DIR_EMPTY
            var ids = categories[0].GetDirectories(id.IdGlob, SearchOption.TopDirectoryOnly);
            if (ids.Length == 0)
            {
                return new SearchResult(ErrorMessage: $"ID '{id.IdGlob[..^1]}' is empty");
            }
            // SUCCESS
            return new SearchResult(Directories: ids);
        }
    }
}
