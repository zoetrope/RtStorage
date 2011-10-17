using System.Windows.Controls;
using System.Windows.Interactivity;

namespace RtStorage.Views.Behaviors
{
    /// <summary>
    /// フォーカスが移ったときにテキストを全選択状態にするビヘイビア
    /// http://stackoverflow.com/questions/660554/how-to-automatically-select-all-text-on-focus-in-wpf-textbox
    /// </summary>
    public class SelectAllTextOnFocusBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotKeyboardFocus += AssociatedObjectGotKeyboardFocus;
            AssociatedObject.GotMouseCapture += AssociatedObjectGotMouseCapture;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotKeyboardFocus -= AssociatedObjectGotKeyboardFocus;
            AssociatedObject.GotMouseCapture -= AssociatedObjectGotMouseCapture;
        }

        private void AssociatedObjectGotKeyboardFocus(object sender,
            System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            AssociatedObject.SelectAll();
        }

        private void AssociatedObjectGotMouseCapture(object sender,
            System.Windows.Input.MouseEventArgs e)
        {
            AssociatedObject.SelectAll();
        }
    }
}
