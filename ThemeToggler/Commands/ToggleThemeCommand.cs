namespace ThemeToggler
{
    [Command(PackageIds.MyCommand)]
    internal sealed class ToggleThemeCommand : BaseCommand<ToggleThemeCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            ThemeStore.ToggleThemeAsync().FireAndForget();
        }
    }
}
