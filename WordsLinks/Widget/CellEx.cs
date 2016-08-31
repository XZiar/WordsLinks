using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Xamarin.Forms;
using static Main.Util.BasicUtils;

namespace WordsLinks.Widget
{
    public class SelectCell : ViewCell
    {
        private static ImageSource checkImg = ImageSource.FromStream(
            () => AssembleResource("yes.png", typeof(SelectCell)));

        public Label CellText { get; set; }
        private Image CheckImage;

        public bool IsSelected
        {
            get { return (bool)CheckImage.GetValue(VisualElement.IsVisibleProperty); }
            set { CheckImage.SetValue(VisualElement.IsVisibleProperty, value); }
        }

        public SelectCell()
        {
            Height = 60;
            CellText = new Label()
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
            };
            CellText.SetBinding(Label.TextProperty, "Text");

            CheckImage = new Image()
            {
                HorizontalOptions = LayoutOptions.EndAndExpand,
                Margin = new Thickness(10),
                IsVisible = false,
            };

            CheckImage.SetBinding(VisualElement.IsVisibleProperty, "IsSelected");
            CheckImage.Source = checkImg;

            var layout = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 8,
                Padding = new Thickness(16, 4, 4, 4),
            };
            layout.Children.Add(CellText);
            layout.Children.Add(CheckImage);

            View = layout;
        }

        public SelectCell(string name, bool isSelect) : this()
        {
            CellText.SetValue(Label.TextProperty, name);
            IsSelected = isSelect;
        }
    }

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
                HeightRequest = 25,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.FromRgb(240, 240, 240),
                Padding = new Thickness(16, 0),
                Orientation = StackOrientation.Horizontal,
                Children = { title }
            };
        }
    }

    public class TextCellEx : TextCell
    {
        public enum RightIndicator { None, Check, Entry };

        public static readonly BindableProperty IsShowProperty = 
            BindableProperty.Create("IsShow", typeof(bool), typeof(TextCellEx), true, propertyChanged:OnIsShowChanged);
        public bool IsShow
        {
            get { return (bool)GetValue(IsShowProperty); }
            set { SetValue(IsShowProperty, value); }
        }
        public RightIndicator ShowIndicator { get; set; } = RightIndicator.None;

        private static void OnIsShowChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as TextCellEx).OnPropertyChanged("HasContextActions");
        }
    }

}
