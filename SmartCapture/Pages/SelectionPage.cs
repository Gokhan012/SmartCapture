using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
#if WINDOWS
using Microsoft.UI.Windowing;
#endif

namespace SmartCapture.Pages;

#if WINDOWS
public class SelectionOverlayDrawable : IDrawable
{
    public Point StartPoint { get; set; }
    public Point CurrentPoint { get; set; }
    public bool IsDragging { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Color.FromRgba(13, 13, 15, 200);
        canvas.FillRectangle(dirtyRect);

        if (IsDragging)
        {
            float x = Math.Min((float)StartPoint.X, (float)CurrentPoint.X);
            float y = Math.Min((float)StartPoint.Y, (float)CurrentPoint.Y);
            float w = Math.Abs((float)StartPoint.X - (float)CurrentPoint.X);
            float h = Math.Abs((float)StartPoint.Y - (float)CurrentPoint.Y);
            var selectionRect = new RectF(x, y, w, h);

            canvas.SubtractFromClip(selectionRect);
            canvas.ResetState();

            canvas.StrokeColor = Color.FromArgb("#5B6AF0");
            canvas.StrokeSize = 2;
            canvas.DrawRectangle(selectionRect);

            float dotSize = 7;
            float half = dotSize / 2;
            canvas.FillColor = Color.FromArgb("#5B6AF0");
            canvas.FillEllipse(x - half, y - half, dotSize, dotSize);
            canvas.FillEllipse(x + w - half, y - half, dotSize, dotSize);
            canvas.FillEllipse(x - half, y + h - half, dotSize, dotSize);
            canvas.FillEllipse(x + w - half, y + h - half, dotSize, dotSize);

            string sizeText = $"{(int)w} × {(int)h}";
            float fontSize = 11;
            float labelW = sizeText.Length * 6.8f + 16;
            float labelH = fontSize + 10;
            float labelX = x;
            float labelY = y - labelH - 5;
            if (labelY < 0) labelY = y + 5;

            canvas.FillColor = Color.FromRgba(13, 13, 15, 220);
            canvas.FillRoundedRectangle(labelX, labelY, labelW, labelH, 4);

            canvas.StrokeColor = Color.FromArgb("#5B6AF0");
            canvas.StrokeSize = 1;
            canvas.DrawRoundedRectangle(labelX, labelY, labelW, labelH, 4);

            canvas.FontColor = Color.FromArgb("#5B6AF0");
            canvas.FontSize = fontSize;
            canvas.DrawString(sizeText, labelX, labelY, labelW, labelH, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}

public class SelectionPage : ContentPage
{
    private System.Drawing.Bitmap _originalCapture;
    private GraphicsView _graphicsView;
    private SelectionOverlayDrawable _drawable;
    private bool _isClosing = false;
    private bool _framelessApplied = false;
    private System.Timers.Timer _inputTimer;

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_SHOWWINDOW = 0x0040;

    public SelectionPage(System.Drawing.Bitmap capture)
    {
        NavigationPage.SetHasNavigationBar(this, false);
        _originalCapture = capture;
        _drawable = new SelectionOverlayDrawable();
        _graphicsView = new GraphicsView { Drawable = _drawable };

        var ms = new MemoryStream();
        _originalCapture.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        ms.Position = 0;

        Content = new Grid
        {
            Children = {
                new Image { Source = ImageSource.FromStream(() => new MemoryStream(ms.ToArray())), Aspect = Aspect.Fill },
                _graphicsView
            }
        };

        var pointer = new PointerGestureRecognizer();

        pointer.PointerPressed += (s, e) => {
            if (_isClosing) return;
            var pos = e.GetPosition(_graphicsView).Value;
            _drawable.StartPoint = pos;
            _drawable.CurrentPoint = pos;
            _drawable.IsDragging = true;
        };

        pointer.PointerMoved += (s, e) => {
            if (_isClosing || !_drawable.IsDragging) return;
            _drawable.CurrentPoint = e.GetPosition(_graphicsView).Value;
            _graphicsView.Invalidate();
        };

        pointer.PointerReleased += async (s, e) => {
            if (!_drawable.IsDragging || _isClosing) return;
            _drawable.IsDragging = false;

            int w = (int)Math.Abs(_drawable.StartPoint.X - _drawable.CurrentPoint.X);
            int h = (int)Math.Abs(_drawable.StartPoint.Y - _drawable.CurrentPoint.Y);

            if (w > 10 && h > 10)
            {
                int x = (int)Math.Min(_drawable.StartPoint.X, _drawable.CurrentPoint.X);
                int y = (int)Math.Min(_drawable.StartPoint.Y, _drawable.CurrentPoint.Y);

                var croppedImage = _originalCapture.Clone(new System.Drawing.Rectangle(x, y, w, h), _originalCapture.PixelFormat);
                var mainWindow = Application.Current.Windows[0];
                var nativeMainWindow = mainWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
                IntPtr mainHwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeMainWindow);

                _isClosing = true;
                StopInputTimer();
                _originalCapture.Dispose();
                Application.Current.CloseWindow(this.Window);

                var savePicker = new global::Windows.Storage.Pickers.FileSavePicker();
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, mainHwnd);
                savePicker.FileTypeChoices.Add("PNG", new List<string> { ".png" });
                savePicker.SuggestedFileName = "SmartCapture_" + DateTime.Now.ToString("HHmmss");

                var file = await savePicker.PickSaveFileAsync();
                if (file != null) croppedImage.Save(file.Path, System.Drawing.Imaging.ImageFormat.Png);
                croppedImage.Dispose();
            }
            else
            {
                CancelSelection();
            }
        };

        _graphicsView.GestureRecognizers.Add(pointer);

        this.HandlerChanged += (s, e) => {
            var nativeWin = this.Window?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (nativeWin == null) return;

            nativeWin.Activated += (sender, args) => {
                if (_framelessApplied) return;
                _framelessApplied = true;
                ApplyFrameless(nativeWin);
                StartInputTimer();
            };
        };
    }

    private void ApplyFrameless(Microsoft.UI.Xaml.Window nativeWin)
    {
        var appWindow = nativeWin.AppWindow;

        var presenter = OverlappedPresenter.Create();
        presenter.IsResizable    = false;
        presenter.IsMaximizable  = false;
        presenter.IsMinimizable  = false;
        presenter.SetBorderAndTitleBar(hasBorder: false, hasTitleBar: false);
        appWindow.SetPresenter(presenter);

        var display = DeviceDisplay.Current.MainDisplayInfo;
        int screenW = (int)display.Width;
        int screenH = (int)display.Height;

        appWindow.MoveAndResize(new Windows.Graphics.RectInt32(0, 0, screenW, screenH));

        IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWin);
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, screenW, screenH, SWP_SHOWWINDOW);
    }

    private void StartInputTimer()
    {
        _inputTimer = new System.Timers.Timer(30);
        _inputTimer.Elapsed += (s, e) => {
            if (_isClosing) { StopInputTimer(); return; }
            if (GetAsyncKeyState(0x1B) < 0 || GetAsyncKeyState(0x02) < 0)
                MainThread.BeginInvokeOnMainThread(CancelSelection);
        };
        _inputTimer.AutoReset = true;
        _inputTimer.Start();
    }

    private void StopInputTimer()
    {
        _inputTimer?.Stop();
        _inputTimer?.Dispose();
        _inputTimer = null;
    }

    private void CancelSelection()
    {
        if (_isClosing) return;
        _isClosing = true;
        StopInputTimer();
        _originalCapture?.Dispose();
        MainThread.BeginInvokeOnMainThread(() => Application.Current.CloseWindow(this.Window));
    }
}
#endif
