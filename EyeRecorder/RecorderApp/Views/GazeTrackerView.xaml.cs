using EyeXFramework.Wpf;
using Prism.Events;
using Prism.Mvvm;
using RecorderApp.Models;
using RecorderApp.Utility;
using RecorderApp.ViewModels;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace RecorderApp.Views
{
    /// <summary>
    /// Interaction logic for GazeTrackerView.xaml
    /// </summary>
    public partial class GazeTrackerView : Window, IView
    {

        int x = 0;
        int y = 0;

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

            Cursor = Cursors.None;
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
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
            //Application.Current.Shutdown();
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
            Cursor = Cursors.Arrow;
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
        private void SendTimeElapsed(int x, int y, int timeElapsed)
        {
            var data = Tuple.Create(x, y, timeElapsed);
            _ea.GetEvent<TimeWatchEvent>().Publish(data);
            Console.WriteLine("Recording triggered");
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if (videoWindow.Source != null)
            {

                int timeElapsed = Convert.ToInt32(videoWindow.Position.TotalMilliseconds);

                SendTimeElapsed(x, y, timeElapsed);
                //SendTimeElapsed(timeElapsed);
                //gazeVm.setTimeElapsed(timeElapsed);
                if (videoWindow.NaturalDuration.HasTimeSpan)
                {
                    //txtTime.Text = String.Format("{0}", videoWindow.Position.ToString(@"ss.fff"));
                    //lblTime.Content = timeElapsed;


                }
            }

        }

        #endregion

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = Mouse.GetPosition(this);
            Point ptS = PointToScreen(position);

            x = Convert.ToInt32(ptS.X);
            y = Convert.ToInt32(ptS.Y);
            txtGazeX.Text = ptS.X.ToString();
            txtGazeY.Text = ptS.Y.ToString();

            string Text = "X:" + ptS.X + " Y:" + ptS.Y;
            Console.WriteLine(Text);
        }


    }
    public interface IView
    {
        bool? Open();

        bool? Open(string filePath);

        //bool? Close();
    }

}
