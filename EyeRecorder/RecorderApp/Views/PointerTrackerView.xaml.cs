using System.Windows;

using Prism.Events;
using Prism.Mvvm;
using RecorderApp.Models;
using RecorderApp.Utility;
using RecorderApp.ViewModels;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;
using System.Linq;

namespace RecorderApp.Views
{
    /// <summary>
    /// Interaction logic for PointerTracker.xaml
    /// </summary>
    public partial class PointerTrackerView : Window, IView1
    {
        int x = 0;
        int y = 0;
        int _timeElapsed = 0;
        IEventAggregator _ea;
        // start timer in milliseconds
        DispatcherTimer timer = new DispatcherTimer();
        public PointerTrackerView(IEventAggregator ea)
        {
            InitializeComponent();
            _ea = ea;
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            Loaded += PointerTrackerView_Loaded;

            
            Cursor = Cursors.None;
        }


        private void PointerTrackerView_Loaded(object sender, RoutedEventArgs e)
        {

            if (DataContext is IControlWindows vm)
            {
                vm.Next += () =>
                {
                    //this.Close();
                    resetTimer();
                    timer.Tick -= timer_Tick;
                    this.Visibility = Visibility.Collapsed;
                    Console.WriteLine("vm.Next +=");
                    EndTask("GazePointer");
                };

                vm.Close += () =>
                {
                    //this.Close();
                    Console.WriteLine("vm.Close +=");
                };

            }
        }

        private void resetTimer()
        {

            timer.Interval = TimeSpan.FromMilliseconds(1);
        }

        public void EndTask(string taskname)
        {
            string processName = taskname.Replace(".exe", "");

            foreach (Process process in Process.GetProcessesByName(processName))
            {
                process.Kill();
            }
        }

        #region Show New Window Methods
        public bool? Open()
        {

            return this.ShowDialog();
        }

        public bool? Open(string filePath)
        {

            return this.ShowDialog();
        }


        protected override void OnClosing(CancelEventArgs e)
        {

            Application.Current.Shutdown();
            Console.WriteLine("OnClosing");
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
            Console.WriteLine(timeElapsed);
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if (videoWindow.Source != null)
            {

                _timeElapsed = Convert.ToInt32(videoWindow.Position.TotalMilliseconds);
                lblTime.Text = _timeElapsed.ToString();
                SendTimeElapsed(x, y, _timeElapsed);
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

        private void videoWindow_MediaOpened(object sender, RoutedEventArgs e)
        {
            //videoWindow.Position = new TimeSpan(0, 0, 0, 0, 0);

        }
    }

    public interface IView1
    {
        bool? Open();

        bool? Open(string filePath);

        //bool? Close();
    }
}
