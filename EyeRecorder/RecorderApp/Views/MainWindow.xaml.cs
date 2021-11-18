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
            Console.WriteLine("MainWindow Loaded");
            if (DataContext is IControlWindows vm)
            {
                vm.Next += () =>
                {

                    //this.Close();
                    this.Visibility = Visibility.Collapsed;
                    Console.WriteLine("vm.Next +=");
                };

                vm.Close += () =>
                {
                    Console.WriteLine("vm.Close +=");
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

            Application.Current.Shutdown();
            Console.WriteLine("OnClosing");
        }

        
    }
    
    public interface MainView
    {

        bool? Open();
    }

    


}
