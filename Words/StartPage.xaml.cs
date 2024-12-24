using Microsoft.Maui.Controls;
using Plugin.Maui.Audio;

namespace Words
{
    public partial class StartPage : ContentPage
    {
        private IAudioPlayer audioPlayer;
        public StartPage()
        {
            InitializeComponent();
        }

        private async void OnStartGameClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text?.Trim();

            if (string.IsNullOrEmpty(username))
            {

                await DisplayAlert("Invalid Username", "Please enter a valid username.", "OK");
                return;
            }

            await Navigation.PushAsync(new MainPage(username));
        }
    }
}
