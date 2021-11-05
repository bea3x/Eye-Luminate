using Microsoft.Win32;
using RecorderApp.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;

namespace RecorderApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, MainView
    {
        
        public MainWindow()
        {
            InitializeComponent(); 

            Loaded += MainWindow_Loaded; 
        }

        

        /// <summary>
        /// closes window when nextcommand is invoked
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IControlWindows vm)
            {
                vm.Next += () =>
                {
                    this.Close();
                };

            }

        }


        public bool? Open()
        {
            return this.ShowDialog();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }

    }

    public interface MainView
    {

        bool? Open();
    }




}
