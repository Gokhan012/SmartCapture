using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using Microsoft.Extensions.Logging;
using H.NotifyIcon;

namespace SmartCapture;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseNotifyIcon()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureLifecycleEvents(events =>
            {
#if WINDOWS
                events.AddWindows(windows => windows
                    .OnWindowCreated(window =>
                    {
                        var nativeWindow = window as Microsoft.UI.Xaml.Window;
                        if (nativeWindow == null) return;

                        var appWindow = nativeWindow.GetAppWindow();

                        if (appWindow != null)
                        {
                            appWindow.Closing += (sender, args) =>
                            {
                                args.Cancel = true;
                                appWindow.Hide();
                            };
                        }
                    }));
#endif
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}