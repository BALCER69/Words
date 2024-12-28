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
            // Get the username input
            string username = UsernameEntry.Text?.Trim();

           
            if (string.IsNullOrEmpty(username))
            {
                // If username is invalid show a warning message
                await DisplayAlert("Invalid Username", "Please enter a valid username.", "OK");
                return; 
            }

            // Proceed main game page passing the username to it
            await Navigation.PushAsync(new MainPage(username));
        }
    }
}
