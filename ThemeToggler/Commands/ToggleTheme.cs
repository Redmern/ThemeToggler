namespace ThemeToggler
{
    [Command(PackageIds.MyCommand)]
    internal sealed class ToggleTheme : BaseCommand<ToggleTheme>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            ThemeStore.ToggleThemeAsync().FireAndForget();
        }
    }
}
