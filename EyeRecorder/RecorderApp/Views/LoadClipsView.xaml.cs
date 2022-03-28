using Prism.Events;
using Prism.Services.Dialogs;
using RecorderApp.Utility;
using RecorderApp.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RecorderApp.Views
{
    /// <summary>
    /// Interaction logic for LoadClipsView.xaml
    /// </summary>
    public partial class LoadClipsView : Window, IView5
    {
        IEventAggregator _ea;
        IDialogService _dialogService;
        public LoadClipsView(IEventAggregator ea, IDialogService dialogService)
        {
            InitializeComponent();

            Loaded += LoadClipsView_Loaded;
            lbFileList.SelectionChanged += LbFileList_SelectionChanged;
            MediaElement clipEl = new MediaElement();
            _ea = ea;
            _dialogService = dialogService;
        }

        private void LbFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            try
            {
                Console.WriteLine("Index:" + lbFileList.SelectedIndex);
                if (lbFileList.SelectedIndex != -1)
                {
                    _ea.GetEvent<ListboxWatchEvent>().Publish(lbFileList.SelectedItem.ToString());

                }
            }
            catch (Exception ex)
            {
                var msg = "Something went wrong. Error: " + ex.Message;
                ShowDialog(msg, true);
            }
            */
        }

        private void LoadClipsView_Loaded(object sender, RoutedEventArgs e)
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

        private void ShowDialog(string dialogMessage, bool error)
        {
            var p = new DialogParameters();
            p.Add("message", dialogMessage);
            p.Add("error", error);

            _dialogService.ShowDialog("MessageDialog", p, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    Console.WriteLine("Naclose mo ata");

                }
            });
        }

        #region navigation
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
        #endregion


        #region Mouse behavior
        private void clipEl_MouseEnter(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("enteresd");
            MediaElement clip = sender as MediaElement;
            clip.Play();
            //clipEl.Play();
        }

        private void clipEl_MouseLeave(object sender, MouseEventArgs e)
        {
            MediaElement clip = sender as MediaElement;
            clip.Pause();
            //clipEl.Stop();
        }

        private void clipEl_Loaded(object sender, RoutedEventArgs e)
        {
            MediaElement clip = sender as MediaElement;
            clip.Pause();
            clip.Position = TimeSpan.FromMilliseconds(10);
            //clipEl.Pause();
        }

        private void PreviewClip_Loaded(object sender, RoutedEventArgs e)
        {
            MediaElement clip = sender as MediaElement;
            clip.Pause();
            clip.Position = TimeSpan.FromMilliseconds(10);
            //clipEl.Pause();
        }

        private void PreviewClip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MediaElement clip = sender as MediaElement;
            clip.Play();
            //PreviewClip.Play();
        }

        private void PreviewClip_MouseEnter(object sender, MouseEventArgs e)
        {
            MediaElement clip = sender as MediaElement;
            clip.Play();
            //PreviewClip.Play();
        }

        private void clipEl_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement clip = sender as MediaElement;
            clip.Pause();
            clip.Position = TimeSpan.FromMilliseconds(1);
        }

        private void PreviewClip_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement clip = sender as MediaElement;
            clip.Pause();
            clip.Position = TimeSpan.FromMilliseconds(1);
        }

        private void PreviewClip_MouseLeave(object sender, MouseEventArgs e)
        {
            MediaElement clip = sender as MediaElement;
            clip.Pause();
        }

        #endregion

        #region interface implementations
        public bool? Open()
        {
            return this.ShowDialog();
        }

        public bool? Open(string filePath)
        {
            return this.ShowDialog();
        }
        #endregion
    }

    public interface IView5
    {
        bool? Open();

        bool? Open(string filePath);

        //bool? Close();
    }
}
