using System.Drawing;
using System.Runtime.InteropServices;

namespace SmartCapture;

#if WINDOWS
public static class ScreenCapturer
{
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    public static Bitmap CaptureEntireScreen()
    {
        int width = GetSystemMetrics(0);
        int height = GetSystemMetrics(1);

        Bitmap bmp = new Bitmap(width, height);
        
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
        }
        
        return bmp;
    }
}
#endif