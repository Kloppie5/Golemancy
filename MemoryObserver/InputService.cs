using System;
using System.Runtime.InteropServices;
using System.Threading;

public class InputService
{
    public void Click(int x, int y, string button = "left", int delayMs = 10)
    {
        SetCursorPos(x, y);
        if (button == "left")
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(Math.Max(1, delayMs));
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        else
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
            Thread.Sleep(Math.Max(1, delayMs));
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }
    }

    public void Drag(int fromX, int fromY, int toX, int toY, int durationMs = 200)
    {
        SetCursorPos(fromX, fromY);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        int steps = Math.Max(4, durationMs / 10);
        for (int i = 1; i <= steps; i++)
        {
            int nx = fromX + (toX - fromX) * i / steps;
            int ny = fromY + (toY - fromY) * i / steps;
            SetCursorPos(nx, ny);
            Thread.Sleep(durationMs / steps);
        }
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }

    #region PInvoke
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll", SetLastError = true)]
    static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    const uint MOUSEEVENTF_MOVE = 0x0001;
    const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    const uint MOUSEEVENTF_LEFTUP = 0x0004;
    const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    #endregion
}
