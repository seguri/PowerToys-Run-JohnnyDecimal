using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.JohnnyDecimal
{
    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    public class Main : IPlugin, IContextMenu, ISettingProvider, IDisposable
    {
        /// <summary>
        /// ID of the plugin.
        /// </summary>
        public static string PluginID => "FEAFCFE212C04174A6CA07DB644DDB86";

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "JohnnyDecimal";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "Navigate to the target Johnny.Decimal folder";

        /// <summary>
        /// Additional options for the plugin.
        /// </summary>
        public IEnumerable<PluginAdditionalOption> AdditionalOptions => [
            new()
            {
                Key = nameof(CountSpaces),
                DisplayLabel = "Count spaces",
                DisplayDescription = "Count spaces as characters",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Checkbox,
                Value = CountSpaces,
            },
            new()
            {
                Key = nameof(RootFolder),
                DisplayLabel = "Root folder",
                DisplayDescription = "The path to the Johnny.Decimal folder",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
                TextValue = RootFolder,
            },
        ];

        private bool CountSpaces { get; set; }

        private string? RootFolder { get; set; }

        private PluginInitContext? Context { get; set; }

        private string? IconPath { get; set; }

        private bool Disposed { get; set; }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            Log.Info("Query: " + query.Search, GetType());
            if (!JohnnyDecimalId.TryParse(query.Search, out var johnnyDecimalId))
            {
                Log.Debug("Query doesn't contain a valid Johnny.Decimal ID", GetType());
                return [];
            }
            if (string.IsNullOrEmpty(RootFolder) || !Directory.Exists(RootFolder))
            {
                Log.Debug("Root folder is not set or does not exist. Current value:" + RootFolder, GetType());
                return [];
            }

            var words = query.Terms.Count;
            // Average rate for transcription: 32.5 words per minute
            // https://en.wikipedia.org/wiki/Words_per_minute
            var transcription = TimeSpan.FromMinutes(words / 32.5);
            var minutes = $"{(int)transcription.TotalMinutes}:{transcription.Seconds:00}";

            var charactersWithSpaces = query.Search.Length;
            var charactersWithoutSpaces = query.Terms.Sum(x => x.Length);

            return [
                new()
                {
                    QueryTextDisplay = query.Search,
                    IcoPath = IconPath,
                    Title = $"Words: {words}",
                    SubTitle = $"Transcription: {minutes} minutes",
                    ToolTipData = new ToolTipData("Words", $"{words} words\n{minutes} minutes for transcription\nAverage rate for transcription: 32.5 words per minute"),
                    ContextData = (words, transcription),
                },
                new()
                {
                    QueryTextDisplay = query.Search,
                    IcoPath = IconPath,
                    Title = $"Characters: {(CountSpaces ? charactersWithSpaces : charactersWithoutSpaces)}",
                    SubTitle = CountSpaces ? "With spaces" : "Without spaces",
                    ToolTipData = new ToolTipData("Characters", $"{charactersWithSpaces} characters (with spaces)\n{charactersWithoutSpaces} characters (without spaces)"),
                    ContextData = CountSpaces ? charactersWithSpaces : charactersWithoutSpaces,
                },
            ];
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Log.Info("Init", GetType());

            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            Log.Info("LoadContextMenus", GetType());

            if (selectedResult?.ContextData is (int words, TimeSpan transcription))
            {
                return
                [
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy (Enter)",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        Glyph = "\xE8C8", // Copy
                        AcceleratorKey = Key.Enter,
                        Action = _ => CopyToClipboard(words.ToString()),
                    },
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy time (Ctrl+Enter)",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        Glyph = "\xE916", // Stopwatch
                        AcceleratorKey = Key.Enter,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ => CopyToClipboard(transcription.ToString()),
                    },
                ];
            }

            if (selectedResult?.ContextData is int characters)
            {
                return
                [
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy (Enter)",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        Glyph = "\xE8C8", // Copy
                        AcceleratorKey = Key.Enter,
                        Action = _ => CopyToClipboard(characters.ToString()),
                    },
                ];
            }

            return [];
        }

        /// <summary>
        /// Creates setting panel.
        /// </summary>
        /// <returns>The control.</returns>
        /// <exception cref="NotImplementedException">method is not implemented.</exception>
        public Control CreateSettingPanel() => throw new NotImplementedException();

        /// <summary>
        /// Updates settings.
        /// </summary>
        /// <param name="settings">The plugin settings.</param>
        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            Log.Info("UpdateSettings", GetType());

            CountSpaces = settings.AdditionalOptions.SingleOrDefault(x => x.Key == nameof(CountSpaces))?.Value ?? false;
            RootFolder = settings.AdditionalOptions.SingleOrDefault(x => x.Key == nameof(RootFolder))?.TextValue ?? string.Empty;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Log.Info("Dispose", GetType());

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            Disposed = true;
        }

        private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? Context?.CurrentPluginMetadata.IcoPathLight : Context?.CurrentPluginMetadata.IcoPathDark;

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);

        private static bool CopyToClipboard(string? value)
        {
            if (value != null)
            {
                Clipboard.SetText(value);
            }

            return true;
        }
    }

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
            var match = ID_REGEX.Match(input);
            if (match.Success)
            {
                var category = match.Groups["category"].Value;
                var id = match.Groups["id"].Value;
                result = new JohnnyDecimalId(category, id);
                return true;
            }
            result = new JohnnyDecimalId(string.Empty, string.Empty);
            return false;
        }

        public char GetArea() => string.IsNullOrWhiteSpace(Category) ? '\0' : Category[0];

        public string GetCategory() => Category;

        public string GetId() => Id;
    }
}