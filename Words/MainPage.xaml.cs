using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Storage;

namespace Words
{
    public partial class MainPage : ContentPage
    {
        private string targetWord;
        private int currentRow = 0;
        private Entry[,] entryGrid = new Entry[6, 5];
        private List<string> validWords = new List<string>();
        private List<GuessHistory> guessHistory = new List<GuessHistory>();

        private DateTime startTime;
        private DateTime endTime;

        private string username;

        public MainPage(string username)
        {
            InitializeComponent();
            this.username = username;
            CreateGrid();
            LoadWordListAsync();
            LoadHistory();
        }

        private void OnPageAppearing(object sender, EventArgs e)
        {
            Title = $"Welcome, {username}!";
        }

        private void CreateGrid()
        {
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            for (int row = 0; row < 6; row++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            }

            for (int col = 0; col < 5; col++)
            {
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            }

            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    var entry = new Entry
                    {
                        MaxLength = 1,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Keyboard = Keyboard.Text,
                        BackgroundColor = Colors.LightGray,
                        TextColor = Colors.Black,
                        HeightRequest = 60,
                        WidthRequest = 60,
                        FontSize = 24
                    };

                    entry.TextChanged += OnLetterTextChanged;
                    entryGrid[row, col] = entry;

                    var frame = new Frame
                    {
                        Content = entry,
                        BackgroundColor = Colors.Transparent,
                        BorderColor = Colors.Black,
                        CornerRadius = 5,
                        Padding = 0,
                        Margin = new Thickness(5)
                    };

                    Grid.SetRow(frame, row);
                    Grid.SetColumn(frame, col);

                    GameGrid.Children.Add(frame);
                }
            }
        }

        private void OnLetterTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;

            if (!string.IsNullOrEmpty(entry?.Text) && entry.Text.Length > 1)
            {
                entry.Text = entry.Text.Substring(0, 1);
            }

            if (!string.IsNullOrEmpty(entry?.Text) && !char.IsLetter(entry.Text[0]))
            {
                entry.Text = string.Empty;
                DisplayAlert("Invalid Input", "Please enter a valid letter.", "OK");
            }
        }

        private async void OnSubmitGuess(object sender, EventArgs e)
        {
            if (currentRow >= 6)
            {
                DisplayAlert("Game Over", $"You've run out of guesses. The correct word was: {targetWord}", "OK");
                return;
            }

            string guess = GetCurrentGuess(currentRow);

            if (guess.Length != 5)
            {
                DisplayAlert("Invalid Guess", "Please enter exactly 5 letters.", "OK");
                return;
            }

            if (!validWords.Contains(guess.ToUpper()))
            {
                DisplayAlert("Invalid Word", "The word you entered is not a valid word. Please try again.", "OK");
                return;
            }

            if (guess.ToUpper() == targetWord)
            {
                endTime = DateTime.Now;
                DisplayAlert("Congratulations!", "You've guessed the correct word!", "OK");
                guessHistory.Add(new GuessHistory
                {
                    Guess = guess,
                    Feedback = GetFeedback(guess),
                    DateTime = DateTime.Now,
                    IsAnswer = true,
                    TotalTime = endTime - startTime
                });
                SaveHistory();
                return;
            }

            ProvideFeedback(guess, currentRow);
            guessHistory.Add(new GuessHistory
            {
                Guess = guess,
                Feedback = GetFeedback(guess),
                DateTime = DateTime.Now,
                IsAnswer = false,
                TotalTime = DateTime.Now - startTime
            });
            currentRow++;

            if (currentRow == 6 && guess.ToUpper() != targetWord)
            {
                guessHistory.Add(new GuessHistory
                {
                    Guess = targetWord,
                    Feedback = GetFeedback(targetWord),
                    DateTime = DateTime.Now,
                    IsAnswer = true,
                    TotalTime = DateTime.Now - startTime
                });
                DisplayAlert("Game Over", $"You've run out of guesses. The correct word was: {targetWord}", "OK");
                SaveHistory();
            }
        }

        private string GetCurrentGuess(int row)
        {
            string guess = string.Empty;
            for (int col = 0; col < 5; col++)
            {
                var entry = entryGrid[row, col];
                guess += entry.Text?.ToUpper() ?? string.Empty;
            }
            return guess;
        }

        private void ProvideFeedback(string guess, int row)
        {
            for (int col = 0; col < 5; col++)
            {
                var entry = entryGrid[row, col];
                char guessedLetter = guess[col];
                char targetLetter = targetWord[col];

                Device.BeginInvokeOnMainThread(() =>
                {
                    if (guessedLetter == targetLetter)
                    {
                        entry.BackgroundColor = Colors.Green;
                    }
                    else if (targetWord.Contains(guessedLetter))
                    {
                        entry.BackgroundColor = Colors.Yellow;
                    }
                    else
                    {
                        entry.BackgroundColor = Colors.Gray;
                    }
                });
            }
        }

        private void OnNewGame(object sender, EventArgs e)
        {
            LoadWordListAsync();
            currentRow = 0;
            startTime = DateTime.Now;

            foreach (var entry in entryGrid)
            {
                entry.Text = string.Empty;
                entry.BackgroundColor = Colors.LightGray;
            }
            guessHistory.Clear();
            SaveHistory();
        }

        private void OnViewHistory(object sender, EventArgs e)
        {
            var historyContent = new StackLayout { Padding = 10 };

            foreach (var history in guessHistory)
            {
                var historyStack = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 5,
                    Margin = new Thickness(0, 5)
                };

                var dateLabel = new Label
                {
                    Text = $"{history.DateTime:yyyy-MM-dd HH:mm:ss}",
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Start
                };
                historyStack.Children.Add(dateLabel);

                if (history.IsAnswer)
                {
                    var answerLabel = new Label
                    {
                        Text = $"Answer: {history.Guess}",
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Start
                    };
                    historyStack.Children.Add(answerLabel);
                }
                else
                {
                    var guessLabel = new Label
                    {
                        Text = $"Guess: {history.Guess}",
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Start
                    };
                    historyStack.Children.Add(guessLabel);

                    foreach (var color in history.Feedback)
                    {
                        var colorBox = new BoxView
                        {
                            Color = color,
                            WidthRequest = 20,
                            HeightRequest = 20,
                            HorizontalOptions = LayoutOptions.Center
                        };
                        historyStack.Children.Add(colorBox);
                    }
                }

                historyContent.Children.Add(historyStack);
            }

            var returnButton = new Button
            {
                Text = "Return to Game",
                BackgroundColor = Colors.Gray,
                TextColor = Colors.White,
                FontSize = 18,
                HeightRequest = 50,
                Padding = new Thickness(10, 0),
                BorderColor = Colors.LightGray,
                BorderWidth = 2
            };
            returnButton.Clicked += (s, args) => Navigation.PopAsync();

            historyContent.Children.Add(returnButton);

            var historyPage = new ContentPage
            {
                Title = "Game History",
                BackgroundColor = Colors.LightGray,
                Content = historyContent
            };

            Navigation.PushAsync(historyPage);
        }

        private void OnClearHistory(object sender, EventArgs e)
        {
            guessHistory.Clear();
            SaveHistory();
            DisplayAlert("History Cleared", "The game history has been erased.", "OK");
        }

        private List<Color> GetFeedback(string guess)
        {
            var feedback = new List<Color>();

            for (int i = 0; i < 5; i++)
            {
                if (guess[i] == targetWord[i])
                {
                    feedback.Add(Colors.Green);
                }
                else if (targetWord.Contains(guess[i]))
                {
                    feedback.Add(Colors.Yellow);
                }
                else
                {
                    feedback.Add(Colors.Gray);
                }
            }

            return feedback;
        }

        private async void LoadWordListAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string wordsText = await client.GetStringAsync("https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt");

                    validWords = new List<string>(wordsText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                    validWords = validWords.FindAll(word => word.Length == 5).ConvertAll(word => word.Trim().ToUpper());

                    Random random = new Random();
                    targetWord = validWords[random.Next(validWords.Count)];

                    Console.WriteLine($"Selected target word: {targetWord}");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Failed to load the word list. Please try again.", "OK");
                Console.WriteLine($"Error loading words: {ex.Message}");
            }
        }

        private void SaveHistory()
        {
            var historyJson = System.Text.Json.JsonSerializer.Serialize(guessHistory);
            Preferences.Set($"guessHistory_{username}", historyJson);
        }

        private void LoadHistory()
        {
            var historyJson = Preferences.Get($"guessHistory_{username}", string.Empty);
            if (!string.IsNullOrEmpty(historyJson))
            {
                guessHistory = System.Text.Json.JsonSerializer.Deserialize<List<GuessHistory>>(historyJson);
            }
        }
    }

    public class GuessHistory
    {
        public string Guess { get; set; }
        public List<Color> Feedback { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsAnswer { get; set; }
        public TimeSpan TotalTime { get; set; }
    }
}
