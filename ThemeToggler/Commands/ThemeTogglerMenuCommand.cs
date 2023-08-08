using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            OleMenuCommandService mcs = Package.GetService<IMenuCommandService, OleMenuCommandService>();
            var i = 1;

            SetupCommand(Command, themes.First());

            foreach (Theme theme in themes.Skip(1))
            {
                CommandID cmdId = new(PackageGuids.ThemeToggler, PackageIds.FirstTheme + i++);
                OleMenuCommand command = new(Execute, cmdId);
                SetupCommand(command, theme);
                mcs.AddCommand(command);
            }
        }

        private void SetupCommand(OleMenuCommand command, Theme theme)
        {
            command.Enabled = command.Visible = true;
            command.Text = theme.Name;
            command.Checked = theme.IsActive;
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
                        if (cmd.Checked == true)
                        {
                            cmd.Checked = false;
                        }
                        else
                        {
                            cmd.Checked = true;
                        }
                    }
                }
                
            }
        }
    }
}
