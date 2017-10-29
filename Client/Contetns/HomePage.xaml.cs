﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AniFile3.Contetns
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void NewSubscription_Click(object sender, RoutedEventArgs e)
        {
            // 표시 이름\
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow.NewSubscriptionFlyout.IsOpen = true;
        }
    }
}
