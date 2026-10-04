using System;
using System.Windows;
using System.Windows.Interop;
using MyAutoSuperFastClicker.Interop;

namespace MyAutoSuperFastClicker.Hotkeys
{
    public class GlobalHotkeyManager : IDisposable
    {
        private const int HOTKEY_ID_TOGGLE = 9001;
        private const int HOTKEY_ID_PICK = 9002;

        private IntPtr _hWnd;
        private HwndSource? _hwndSource;

        public event Action? ToggleRequested;
        public event Action? PickLocationRequested;

        public bool IsRegistered { get; private set; }

        public void Initialize(Window window)
        {
            var helper = new WindowInteropHelper(window);
            _hWnd = helper.Handle;

            _hwndSource = HwndSource.FromHwnd(_hWnd);
            _hwndSource?.AddHook(HwndHook);

            Register();
        }

        public void Register()
        {
            if (_hWnd == IntPtr.Zero) return;

            // F6 for start/stop toggle
            NativeMethods.RegisterHotKey(_hWnd, HOTKEY_ID_TOGGLE, NativeMethods.MOD_NOREPEAT, NativeMethods.VK_F6);

            // F7 for pick coordinate
            NativeMethods.RegisterHotKey(_hWnd, HOTKEY_ID_PICK, NativeMethods.MOD_NOREPEAT, NativeMethods.VK_F7);

            IsRegistered = true;
        }

        public void Unregister()
        {
            if (_hWnd != IntPtr.Zero)
            {
                NativeMethods.UnregisterHotKey(_hWnd, HOTKEY_ID_TOGGLE);
                NativeMethods.UnregisterHotKey(_hWnd, HOTKEY_ID_PICK);
            }
            IsRegistered = false;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (id == HOTKEY_ID_TOGGLE)
                {
                    ToggleRequested?.Invoke();
                    handled = true;
                }
                else if (id == HOTKEY_ID_PICK)
                {
                    PickLocationRequested?.Invoke();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Unregister();
            _hwndSource?.RemoveHook(HwndHook);
            _hwndSource = null;
        }
    }
}
