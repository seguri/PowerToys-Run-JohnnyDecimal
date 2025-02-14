using System.Diagnostics;
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
                Key = nameof(RootFolder),
                DisplayLabel = "Root folder",
                DisplayDescription = "The path to the Johnny.Decimal folder",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
                TextValue = RootFolder,
            },
        ];

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

            Result CreateResult(string title, string subTitle, string? contextData = null) =>
                new()
                {
                    QueryTextDisplay = query.Search,
                    IcoPath = IconPath,
                    Title = title,
                    SubTitle = subTitle,
                    ContextData = contextData,
                };

            if (!JohnnyDecimalId.TryParse(query.Search, out var johnnyDecimalId))
            {
                return [CreateResult("No results found", "Please insert a valid JohnnyDecimal ID")];
            }
            var resolver = new JohnnyDecimalPathResolver(RootFolder);
            var pathsResult = resolver.FindPaths(johnnyDecimalId);
            return string.IsNullOrEmpty(pathsResult.ErrorMessage)
                ? pathsResult.Directories.Select(x => CreateResult(x.Name, x.FullName, x.FullName)).ToList()
                : [CreateResult("Error", pathsResult.ErrorMessage)];
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

            if (selectedResult?.ContextData is string path) {
                return [
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Open (Enter)",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        Glyph = "\xE838", // FolderOpen
                        AcceleratorKey = Key.Enter,
                        Action = _ => OpenFolder(path),
                    },
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy (Ctrl+Enter)",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        Glyph = "\xE8C8", // Copy
                        AcceleratorKey = Key.Enter,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ => CopyToClipboard(path),
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

        private static bool OpenFolder(string? value)
        {
            if (value != null)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = value,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }

            return true;
        }

        private static bool CopyToClipboard(string? value)
        {
            if (value != null)
            {
                Clipboard.SetText(value);
            }

            return true;
        }
    }
}