using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace WordsLinks.UWP.Util
{
    public class NavTrueWidthConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty ObjSplitViewProperty =
        DependencyProperty.Register("ObjSplitView",
                                    typeof(SplitView),
                                    typeof(NavTrueWidthConverter),
                                    new PropertyMetadata(null));

        public SplitView ObjSplitView
        {
            private get { return (SplitView)GetValue(ObjSplitViewProperty); }
            set { SetValue(ObjSplitViewProperty, value); }
        }
        
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ObjSplitView == null ? 128 : ((bool)value ? ObjSplitView.OpenPaneLength : ObjSplitView.CompactPaneLength);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException("not suitable for two-way binding");
        }
    }
}
