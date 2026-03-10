using H.NotifyIcon.Core;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using SmartCapture.Pages;
namespace SmartCapture;

public class App : Application
{
    private TrayIcon _trayIcon;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    public App()
    {
        MainPage = new NavigationPage(new WelcomePage());
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);

        window.HandlerChanged += (s, e) =>
        {
#if WINDOWS
            var nativeWindow = window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            var appWin = nativeWindow?.GetAppWindow();

            if (appWin != null)
            {
                appWin.Closing += (sender, args) =>
                {
                    args.Cancel = true; 
                    appWin.Hide();      
                };
            }

            string path = Path.Combine(AppContext.BaseDirectory, "applogo.png");
            using var bitmap = new System.Drawing.Bitmap(path);
            _trayIcon = new TrayIcon
            {
                Icon = bitmap.GetHicon(),
                ToolTip = "Smart Capture AI"
            };
            
            _trayIcon.Create();

            _trayIcon.MessageWindow.MouseEventReceived += (sender, args) =>
            {
                if (args.MouseEvent == MouseEvent.IconLeftMouseUp)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        appWin?.Show();
                        nativeWindow?.Activate();
                    });
                }
            };

            GlobalHotkey.Start(
                onAltSPressed: () =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var capturedImage = ScreenCapturer.CaptureEntireScreen();
                        var selectionPage = new SelectionPage(capturedImage);
                        var newWindow = new Window(selectionPage);

                        newWindow.HandlerChanged += (sender, args) =>
                        {
                            if (newWindow.Handler?.PlatformView is Microsoft.UI.Xaml.Window winUIWindow)
                            {
                                var subAppWin = winUIWindow.GetAppWindow();
                                subAppWin.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen);
                            }
                        };

                        Application.Current.OpenWindow(newWindow);
                    });
                },
                onAltKPressed: () =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        var capturedImage = ScreenCapturer.CaptureEntireScreen();
                        
                        var hwnd = GetForegroundWindow();
                        
                        // Hatanın çözümü: 'global::' ekleyerek karışıklığı giderdik
                        var savePicker = new global::Windows.Storage.Pickers.FileSavePicker();
                        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
                        
                        savePicker.SuggestedStartLocation = global::Windows.Storage.Pickers.PickerLocationId.Desktop;
                        savePicker.FileTypeChoices.Add("PNG Resmi", new List<string>() { ".png" });
                        savePicker.SuggestedFileName = "TamEkran_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

                        var file = await savePicker.PickSaveFileAsync();
                        
                        if (file != null)
                        {
                            using var stream = await file.OpenStreamForWriteAsync();
                            capturedImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        }
                        
                        capturedImage.Dispose();
                    });
                }
            );
#endif
        };

        return window;
    }
}