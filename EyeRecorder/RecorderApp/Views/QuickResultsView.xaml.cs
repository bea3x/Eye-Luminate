using Prism.Events;
using RecorderApp.Utility;
using RecorderApp.ViewModels;
using System;
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
using System.Windows.Shapes;

namespace RecorderApp.Views
{
    /// <summary>
    /// Interaction logic for QuickResultsView.xaml
    /// </summary>
    public partial class QuickResultsView : Window, IView3
    {
        IEventAggregator _ea;


        public QuickResultsView(IEventAggregator ea)
        {
            InitializeComponent();
            _ea = ea;
            Loaded += QuickResultsView_Loaded;
            lbFiles.SelectionChanged += LbFiles_SelectionChanged;
            MediaElement clipEl = new MediaElement();
        }

        private void LbFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ea.GetEvent<ListboxWatchEvent>().Publish(lbFiles.SelectedItem.ToString());
        }

        private void QuickResultsView_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow mView = new MainWindow();
            if (DataContext is IControlWindows vm)
            {
                vm.Close += () =>
                {
                    this.Close();
                    mView.Show();
                };

            }
        }

        #region Show New Window Methods
        public bool? OpenQRes()
        {
            return this.ShowDialog();
        }


        public bool? Open(string filePath)
        {
            // choose file to open (send filepath string to ResultsViewModel)
            //qResVm.setPath(filePath);
            return this.ShowDialog();
        }

        #endregion

        public bool? Back()
        {
            this.Close();
            MainWindow menuVw = new MainWindow();
            return menuVw.ShowDialog();
        }

        private void VidPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            //VidPlayer.SetCurrentValue(VidPlayer.Source, new Uri(@"D:\tobii\thess\EyeGazeTracker\EyeRecorder\RecorderApp\Assets\blank.jpg"));
            //VidPlayer.Source = new Uri(@"D:\tobii\thess\EyeGazeTracker\EyeRecorder\RecorderApp\Assets\blank.jpg");
        }

        private void VidPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            //VidPlayer.Source = new Uri(@"D:\tobii\thess\EyeGazeTracker\EyeRecorder\RecorderApp\Assets\blank.jpg");
            
        }

        private void VidPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            //VidPlayer.Visibility = Visibility.Visible;
        }

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

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("oo, selection changed bobo. now fix it!!");
        }
    }


    public interface IView3
    {
        bool? OpenQRes();
        //bool? Back();
        bool? Open(string filePath);

        //bool? Close();
    }
}
