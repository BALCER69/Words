﻿using Microsoft.Maui.Controls;

namespace Words
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new StartPage());
        }
    }
}
