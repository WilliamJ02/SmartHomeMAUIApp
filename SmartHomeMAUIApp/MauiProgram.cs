using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Shared.Azure.Communications;
using Shared.Database;
using Shared.Handlers;
using SmartHomeMAUIApp.ViewModels;

namespace SmartHomeMAUIApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });
            
            
            builder.Services.AddMauiBlazorWebView();

    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();

            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();

            builder.Services.AddTransient<EmailCommunication>();
            builder.Services.AddTransient<DatabaseService>();
            builder.Services.AddTransient<IotHub>();


            return builder.Build();
        }
    }
}
