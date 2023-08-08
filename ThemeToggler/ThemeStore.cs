using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using ThemeToggler.Commands;

namespace ThemeToggler
{
    public static class ThemeStore
    {
        public static Guid highContrast = new("{a5c004b4-2d4b-494e-bf01-45fc492522c7}");
        public static Guid additionalContrast = new("{ce94d289-8481-498b-8ca9-9b6191a315b9}");
        public static Lazy<IEnumerable<Theme>> Themes = new(GetInstalledThemes);
        public static Lazy<IEnumerable<Theme>> TogglingThemes = new(GetTogglingThemes);

        public static Guid GetActiveTheme()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            const string COLLECTION_NAME = @"ApplicationPrivateSettings\Microsoft\VisualStudio";
            const string PROPERTY_NAME = "ColorTheme";

            SettingsStore store = new ShellSettingsManager(ServiceProvider.GlobalProvider).GetReadOnlySettingsStore(SettingsScope.UserSettings);

            if (store.CollectionExists(COLLECTION_NAME))
            {
                if (store.PropertyExists(COLLECTION_NAME, PROPERTY_NAME))
                {
                    var parts = store.GetString(COLLECTION_NAME, PROPERTY_NAME).Split('*');
                    if (parts.Length == 3)
                    {
                        if (Guid.TryParse(parts[2], out Guid value))
                        {
                            return value;
                        }
                    }
                }
            }

            return Guid.Empty;
        }

        public static async Task ToggleThemeAsync()
        {
            var activeTheme = GetActiveTheme();
            var selectedThemes = GetTogglingThemes();
            var newTheme = selectedThemes.ToList().SkipWhile(x => x.Guid != activeTheme).Skip(1).FirstOrDefault() ?? selectedThemes.ToList().FirstOrDefault();


            var settingsFile = string.Format(_vsSettings, "{" + newTheme.Guid + "}");
            var path = Path.Combine(Path.GetTempPath(), "temp.vssettings");


            System.IO.File.WriteAllText(path, settingsFile);

            await KnownCommands.Tools_ImportandExportSettings.ExecuteAsync($@"/import:""{path}""");

            foreach (Theme theme in TogglingThemes.Value.ToList())
            {
                theme.IsActive = false;

                if (theme.Guid == newTheme.Guid)
                {
                    theme.IsActive = true;
                }
            }
        }

        private static IEnumerable<Theme> GetDefaultThemes()
        {
            List<Theme> mainThemes = new();

            ThreadHelper.ThrowIfNotOnUIThread();
            SettingsStore store = new ShellSettingsManager(ServiceProvider.GlobalProvider).GetReadOnlySettingsStore(SettingsScope.Configuration);
            if (store.CollectionExists("Themes"))
            {
                IEnumerable<string> guids = store.GetSubCollectionNames("Themes");
                foreach (Guid guid in guids.Select(g => new Guid(g)).Where(g => g != highContrast))
                {
                    var collection = $@"Themes\{{{guid}}}";

                    if (store.PropertyExists(collection, ""))
                    {
                        var name = store.GetString(collection, "");
                        if (store.GetString(collection, "") == "Dark" || store.GetString(collection, "") == "Light")
                        {
                            mainThemes.Add(new Theme(name, guid));
                        }
                    }
                }
            }

            return mainThemes.OrderBy(t => t.Name);
        }

            private static IEnumerable<Theme> GetTogglingThemes()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SettingsStore store = new ShellSettingsManager(ServiceProvider.GlobalProvider).GetReadOnlySettingsStore(SettingsScope.Configuration);
            List<Theme> themes = new();
            List<Theme> mainThemes = new();

            if (store.CollectionExists("Themes"))
            {
                IEnumerable<string> guids = store.GetSubCollectionNames("Themes");

                foreach (Guid guid in guids.Select(g => new Guid(g)).Where(g => g != highContrast))
                {
                    var collection = $@"Themes\{{{guid}}}";

                    if (store.PropertyExists(collection, ""))
                    {
                        var name = store.GetString(collection, "");

                        foreach (var cmd in ThemeTogglerMenuCommand._commands)
                        {
                            if (cmd.Checked)
                            { 
                                var selectedGuid = (Guid)cmd.Properties["guid"];
                                if (selectedGuid == guid)
                                {
                                    themes.Add(new Theme(name, guid));
                                }
                            }
                        }
                    }
                }
            }
            Guid activeGuid = GetActiveTheme();
            Theme activeTheme = themes.SingleOrDefault(t => t.Guid == activeGuid);

            if (activeTheme != null)
            {
                activeTheme.IsActive = true;
            }

            if (themes.Count < 2)
            {
                var defaultThemes = GetDefaultThemes();

                //add the non duplicates from defaultThemes to themes list
                foreach (var theme in defaultThemes)
                {
                    if (!themes.Any(t => t.Guid == theme.Guid))
                    {
                        themes.Add(theme);
                    }
                }
            }

            return themes.OrderBy(t => t.Name);
        }

        private static IEnumerable<Theme> GetInstalledThemes()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SettingsStore store = new ShellSettingsManager(ServiceProvider.GlobalProvider).GetReadOnlySettingsStore(SettingsScope.Configuration);
            List<Theme> themes = new();

            if (store.CollectionExists("Themes"))
            {
                IEnumerable<string> guids = store.GetSubCollectionNames("Themes");

                foreach (Guid guid in guids.Select(g => new Guid(g)).Where(g => g != highContrast))
                {
                    var collection = $@"Themes\{{{guid}}}";

                    if (store.PropertyExists(collection, ""))
                    {
                        var name = store.GetString(collection, "");

                        if (guid == additionalContrast)
                        {
                            name = "Blue (Extra Contrast)";
                        }

                        themes.Add(new Theme(name, guid));
                    }
                }
            }

            Guid activeGuid = GetActiveTheme();
            Theme activeTheme = themes.SingleOrDefault(t => t.Guid == activeGuid);

            if (activeTheme != null)
            {
                activeTheme.IsActive = true;
            }

            return themes.OrderBy(t => t.Name);
        }

        public const string _vsSettings = @"<UserSettings>
    <ApplicationIdentity version=""16.0""/>
    <ToolsOptions>
        <ToolsOptionsCategory name=""Environment"" RegisteredName=""Environment""/>
    </ToolsOptions>
    <Category name=""Environment_Group"" RegisteredName=""Environment_Group"">
        <Category name=""Environment_FontsAndColors"" Category=""{{1EDA5DD4-927A-43a7-810E-7FD247D0DA1D}}"" Package=""{{DA9FB551-C724-11d0-AE1F-00A0C90FFFC3}}"" RegisteredName=""Environment_FontsAndColors"" PackageName=""Visual Studio Environment Package"">
            <PropertyValue name=""Version"">2</PropertyValue>
            <FontsAndColors Version=""2.0"">
                <Theme Id=""{0}""/>
            </FontsAndColors>
        </Category>
    </Category>
</UserSettings>";
    }
}
