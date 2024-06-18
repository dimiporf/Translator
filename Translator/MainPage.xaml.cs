namespace Translator
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            // Show the "Hello World" label when the button is clicked
            HelloLabel.IsVisible = true;
        }
    }
}
