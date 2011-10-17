using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Interop;

namespace RtStorage.Views.Behaviors
{
    /// <summary>
    /// Window位置とサイズを保存・復元するビヘイビア
    /// </summary>
    /// <remarks>
    /// ウィンドウの配置状態の保存のサンプル
    /// http://msdn.microsoft.com/ja-jp/library/aa972163%28v=vs.90%29.aspx
    /// .NETのアプリケーション設定でユーザ定義型を扱う方法
    /// http://d.hatena.ne.jp/Dryad/20100109/1263070081
    /// </remarks>
    public class RestoreWindowPlacementBehavior : Behavior<Window>
    {
        /// <summary>
        /// アプリケーション設定の名前を指定する
        /// </summary>
        public string SettingName
        {
            get { return (string)GetValue(SettingNameProperty); }
            set { SetValue(SettingNameProperty, value); }
        }

        public static readonly DependencyProperty SettingNameProperty =
            DependencyProperty.Register("SettingName", typeof(string), typeof(RestoreWindowPlacementBehavior), new UIPropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SourceInitialized += LoadWindowPlacement;
            AssociatedObject.Closing += SaveWindowPlacement;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SourceInitialized -= LoadWindowPlacement;
            AssociatedObject.Closing -= SaveWindowPlacement;
        }

        void LoadWindowPlacement(object sender, EventArgs e)
        {
            var window = sender as Window;

            // Windowのサイズと位置をファイルから読み込んで設定する
            try
            {
                var wp = (WINDOWPLACEMENT)Properties.Settings.Default[SettingName];

                if (wp.normalPosition.Top == 0 &&
                    wp.normalPosition.Bottom == 0 &&
                    wp.normalPosition.Left == 0 &&
                    wp.normalPosition.Right == 0)
                {
                    // 初めてアプリケーションを起動したときは、サイズがすべて0になっている。
                    return;
                }

                wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                wp.flags = 0;
                wp.showCmd = (wp.showCmd == NativeWindowPlacementHelper.SW_SHOWMINIMIZED ? NativeWindowPlacementHelper.SW_SHOWNORMAL : wp.showCmd);
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                NativeWindowPlacementHelper.SetWindowPlacement(hwnd, ref wp);
            }
            catch { }
        }

        void SaveWindowPlacement(object sender, CancelEventArgs e)
        {
            var window = sender as Window;

            // Windowのサイズと位置をファイルに保存する
            WINDOWPLACEMENT wp;
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            NativeWindowPlacementHelper.GetWindowPlacement(hwnd, out wp);
            Properties.Settings.Default[SettingName] = wp;
            Properties.Settings.Default.Save();
        }

    }

    // RECT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    // POINT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    // WINDOWPLACEMENT stores the position, size, and state of a window
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;
    }

    internal static class NativeWindowPlacementHelper
    {
        [DllImport("user32.dll")]
        public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
    }
}
