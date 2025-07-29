using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using ThemeToggler;

namespace ThemeToggler.Commands
{
    [Command(PackageIds.FirstTheme)]
    internal sealed class ThemeTogglerMenuCommand : BaseCommand<ThemeTogglerMenuCommand>
    {
        public static readonly List<OleMenuCommand> _commands = new();
        protected override void BeforeQueryStatus(EventArgs e)
        {
            if (_commands.Any())
            {
                return; // The commands were already set up
            }

            IEnumerable<Theme> themes = ThemeStore.Themes.Value;
            var selected = new HashSet<Guid>(ThemeStore.LoadSelectedThemeGuids());
            if (!selected.Any())
            {
                selected.Add(ThemeStore.GetActiveTheme());
            }
            OleMenuCommandService mcs = Package.GetService<IMenuCommandService, OleMenuCommandService>();
            var i = 1;

            SetupCommand(Command, themes.First(), selected.Contains(themes.First().Guid));

            foreach (Theme theme in themes.Skip(1))
            {
                CommandID cmdId = new(PackageGuids.ThemeToggler, PackageIds.FirstTheme + i++);
                OleMenuCommand command = new(Execute, cmdId);
                SetupCommand(command, theme, selected.Contains(theme.Guid));
                mcs.AddCommand(command);
            }
        }

        private void SetupCommand(OleMenuCommand command, Theme theme, bool isChecked)
        {
            command.Enabled = command.Visible = true;
            command.Text = theme.Name;
            command.Checked = isChecked;
            command.Properties["guid"] = theme.Guid;
            _commands.Add(command);
        }

        protected override void Execute(object sender, EventArgs e)
        {
            var command = (OleMenuCommand)sender;

            if (command.Properties.Contains("guid"))
            {

                var guid = (Guid)command.Properties["guid"];
                foreach (OleMenuCommand cmd in _commands)
                {
                    if (cmd == command)
                    {
                        cmd.Checked = !cmd.Checked;
                    }
                }
                ThemeStore.SaveSelectedThemeGuids(_commands.Where(c => c.Checked).Select(c => (Guid)c.Properties["guid"]));
                ThemeStore.RefreshTogglingThemes();
            }
        }
    }
}
