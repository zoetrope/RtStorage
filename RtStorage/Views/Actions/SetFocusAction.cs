using System.Windows;
using System.Windows.Interactivity;

namespace RtStorage.Views.Actions
{
    /// <summary>
    /// フォーカスを設定するアクション
    /// Uxeenを参考: http://sourceforge.jp/projects/uxeen/
    /// </summary>
    public class SetFocusAction : TriggerAction<UIElement>
    {
        public static readonly DependencyProperty FocusItemProperty =
            DependencyProperty.Register("FocusItem", typeof (UIElement), typeof (SetFocusAction),
            new UIPropertyMetadata(null));

        public UIElement FocusItem
        {
            get
            {
                return (UIElement)GetValue(FocusItemProperty);
            }
            set
            {
                SetValue(FocusItemProperty, value);
            }
        }
        
        protected override void Invoke(object parameter)
        {
            if (FocusItem != null)
            {
                FocusItem.Focus();
            }
        }
    }

}
