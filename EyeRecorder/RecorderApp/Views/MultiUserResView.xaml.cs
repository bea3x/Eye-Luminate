using RecorderApp.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;

namespace RecorderApp.Views
{
    /// <summary>
    /// Interaction logic for MultiUserResView.xaml
    /// </summary>
    public partial class MultiUserResView : Window, IView4
    {
        public MultiUserResView()
        {
            InitializeComponent();

            Loaded += MultiUserResView_Loaded;
        }

        private void MultiUserResView_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow mView = new MainWindow();
            if (DataContext is IControlWindows vm)
            {
                vm.Close += () =>
                {
                    this.Close();
                };

                vm.Back += () =>
                {
                    CancelEventArgs arg = new CancelEventArgs();
                    BackToMain(arg);
                    mView.ShowDialog();
                };

            }
        }

        protected void BackToMain(CancelEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;

            Console.WriteLine("Baxk To Main");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Application.Current.Shutdown();

        }

        public bool? Open()
        {
            return this.ShowDialog();
        }

        public bool? Open(string filePath)
        {
            return this.ShowDialog();
        }
    }

    public interface IView4
    {
        bool? Open();

        bool? Open(string filePath);

        //bool? Close();
    }
}
