using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace WordsLinks.View
{
    public partial class DebugPage : ContentPage
    {
        private string txt;
        public DebugPage(string title, string content)
        {
            InitializeComponent();
            Title = title;
            txt = content;
            Task.Run(() => Debug.WriteLine(content));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            maintext.Text = txt;
        }
    }
}
