using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace WordsLinks.Widget
{
    public class HeaderCell : ViewCell
    {
        public HeaderCell()
        {
            Height = 25;
            var title = new Label
            {
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center
            };
            title.SetBinding(Label.TextProperty, "Prefix");

            View = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromRgb(220, 220, 220),
                Padding = new Thickness(16,0),
                Orientation = StackOrientation.Horizontal,
                Children = { title }
            };
        }
    }
}
