﻿using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace WordsLinks.View
{
    public partial class MainPage : TabbedPage
    {
        public NavigationPage theSettingPage { get { return settingPage; } }
        public MainPage()
        {
            InitializeComponent();

            if (Device.OS == TargetPlatform.iOS)
            {
                // Fix : status bar position
                Debug.WriteLine("Fix status bar position for iOS");
                foreach (var ch in Children)
                {
                    ch.Padding = new Thickness(0, 20, 0, 0);
                    ch.BackgroundColor = Color.FromHex("FFFCF8");
                }
            }
        }
    }
}