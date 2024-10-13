using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;


namespace SmartHomeMAUIApp.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
