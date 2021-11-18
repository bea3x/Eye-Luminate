using EyeXFramework.Wpf;
using Prism.Events;
using Prism.Mvvm;
using RecorderApp.Models;
using RecorderApp.Utility;
using RecorderApp.ViewModels;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace RecorderApp.Views
{
    /// <summary>
    /// Interaction logic for GazeTrackerView.xaml
    /// </summary>
    public partial class GazeTrackerView : Window, IView
    {

        IEventAggregator _ea;
        public GazeTrackerView(IEventAggregator ea)
        {
            InitializeComponent();
            //this.DataContext = gazeVm;
            _ea = ea;
            // start timer in milliseconds
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            //Loaded += GazeTrackerView_Loaded;
            Loaded += GazeTrackerViewModel_Loaded;

        }

        private void GazeTrackerViewModel_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IControlWindows vm)
            {
                vm.Next += () =>
                {
                    this.Close();
                };

                vm.Close += () =>
                {
                    this.Close();
                };

            }
        }

        #region Show New Window Methods
        public bool? Open()
        {
            return this.ShowDialog();
        }

        public bool? Open(string filePath)
        {
            //videoWindow.Source = new Uri(filePath);
            // send string path to GazeTrackerViewModel
            //gazeVm.setPath(filePath);
            
            //gazeVm.startTracking();

            return this.ShowDialog();
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            //this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
            Application.Current.Shutdown();
            //base.OnClosed(e);
        }
        #endregion

        #region MediaHandler Methods
        private void videoWindow_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show(e.ErrorException.ToString());
        }

        private void videoWindow_MediaEnded(object sender, RoutedEventArgs e)
        {
            SendMediaStatus(true);
        }

        private void SendMediaStatus(bool ended)
        {
            _ea.GetEvent<MediaWatchEvent>().Publish(ended);
        }

        #endregion

        #region timer/position

        //private DispatcherTimer timerVideoTime;

        /// <summary>
        /// send time from timer to viewmodel
        /// </summary>
        /// <param name="timeElapsed"></param>
        private void SendTimeElapsed(int timeElapsed)
        {
            _ea.GetEvent<TimeWatchEvent>().Publish(timeElapsed);
            Console.WriteLine("Recording triggered");
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if (videoWindow.Source != null)
            {

                int timeElapsed = Convert.ToInt32(videoWindow.Position.TotalMilliseconds);

                SendTimeElapsed(timeElapsed);
                //gazeVm.setTimeElapsed(timeElapsed);
                if (videoWindow.NaturalDuration.HasTimeSpan)
                {
                    //txtTime.Text = String.Format("{0}", videoWindow.Position.ToString(@"ss.fff"));
                    //lblTime.Content = timeElapsed;


                }
            }

        }

        #endregion

    }
    public interface IView
    {
        bool? Open();

        bool? Open(string filePath);

        //bool? Close();
    }

}
