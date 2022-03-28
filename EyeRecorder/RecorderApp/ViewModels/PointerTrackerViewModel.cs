using CsvHelper;
using EyeXFramework;
using EyeXFramework.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using RecorderApp.Models;
using RecorderApp.Utility;
using RecorderApp.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace RecorderApp.ViewModels
{
    public class PointerTrackerViewModel : BindableBase, IControlWindows
    {
        public string initialTime;
        public string outputFileDirectory;
        public string outputFileName;

        IEventAggregator _ea;
        public IView _view;
        public IView2 _view2;
        public IView3 _view3;


        bool fileCreated;
        public PointerTrackerViewModel(IEventAggregator ea, IView3 view3)
        {
            _view3 = view3;
            _ea = ea;
            IsTrackingGaze = true;

            fileCreated = false;

            _ea.GetEvent<SavePathEvent>().Subscribe(GetVidPath);
            _ea.GetEvent<RecStatusEvent>().Subscribe(GetTrackingStatus);
            _ea.GetEvent<TimeWatchEvent>().Subscribe(GetTimeElapsed);
            _ea.GetEvent<MediaWatchEvent>().Subscribe(GetMediaStatus);

            
            //ea.GetEvent<CalibrationEvent>().Subscribe(GetCalibrationSignal);

        }

        private void GetVidPath(string obj)
        {
            string vidpath = obj;

            setPath(vidpath);
        }

        private void GetTrackingStatus(bool started)
        {
            if (started && videoSource != null)
            {
                startTracking();
                fileCreated = true;
            }
        }

        #region Pointer Tracking

        private void startTracking()
        {
            try
            {
                if (!fileCreated)
                {

                    #region write to output file

                    // get path for output
                    string exeRuntimeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                    // filename with filename of clip and timestamp of recording
                    String timeStamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    //if (videoSource.ToString() != null)
                    string clipName = Path.GetFileName(videoSource.ToString());

                    int dotPos = clipName.IndexOf(".");
                    clipName = clipName.Substring(0, dotPos);
                    Console.WriteLine("video name: " + clipName);
                    outputFileName = clipName + "_" + timeStamp;
                    //string outputFolder = outputFileName;

                    // find output foldr/ create if it doesn't exit
                    outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output", "GazeTrackerOutput");
                    if (!System.IO.Directory.Exists(outputFileDirectory))
                    {
                        System.IO.Directory.CreateDirectory(outputFileDirectory);
                    }
                    Console.WriteLine(exeRuntimeDirectory);
                    Console.WriteLine(outputFileDirectory);


                    //outputFileName = "gazeTrackerOutput";
                    // write headers on csv file
                    System.IO.File.WriteAllText(outputFileDirectory + @"\" + outputFileName + ".csv", "X Gaze Data, Y Gaze Data, Time \r\n");
                    #endregion
                }
            }
            catch
            {
                Console.WriteLine("Unable to write file");
            }
        }

        private void dispose()
        {
            _ea.GetEvent<SavePathEvent>().Unsubscribe(GetVidPath);
            _ea.GetEvent<RecStatusEvent>().Unsubscribe(GetTrackingStatus);
            _ea.GetEvent<TimeWatchEvent>().Unsubscribe(GetTimeElapsed);
            _ea.GetEvent<MediaWatchEvent>().Unsubscribe(GetMediaStatus);
        }

        #endregion

        public DelegateCommand MediaEndedCommand { get; set; }

        private void GetMediaStatus(bool ended)
        {
            if (ended)
            {
                //writeFile(rawData).Await();
                dispose();
                ShowQuickResultsWindow();
            }
        }

        void ShowQuickResultsWindow()
        {
            Next?.Invoke();
            SendTrackingStatus(true);
            _view3.OpenQRes();
        }

        private void SendTrackingStatus(bool status)
        {
            _ea.GetEvent<RecStatusEvent>().Publish(status);
        }

        #region Set Video Source Methods/Properties 


        private Uri videoSource;
        /// <summary>
        ///  bind videoSource to media element source property
        /// </summary>
        public Uri VideoSource
        {
            get { return videoSource; }
            set
            {
                //videoSource = value;
                //RaisePropertyChanged("VideoSource");
                SetProperty(ref videoSource, value);
            }
        }

        /// <summary>
        /// set VideoSource variable using filepath value passed from open file dialog
        /// </summary>
        /// <param name="filePath"></param>
        public void setPath(string filePath)
        {
            if (filePath != null)
            {
                VideoSource = new Uri(filePath);
            }
        }

        private int timeSpan = 0;

        public int Timespan
        {
            get { return timeSpan; }
            set
            {
                timeSpan = value;
                RaisePropertyChanged("Timespan");
            }
        }

        public void GetTimeElapsed(Tuple<int, int, int> data)
        {
            ///*
            int x = data.Item1;
            int y = data.Item2;
            int timeMs = data.Item3;
            //*/
            Timespan = timeMs;
            if (timeMs != 0)
                updateGazeData(x, y, Timespan);
        }


        #endregion


        // instance of GazeData class
        public GazeData gazeData = new GazeData();
        #region GazeData model <-> GazeTracker view binding properties

        /// <summary>
        /// bind to view element in GazeTrackerView.xaml & gaze data model 
        /// </summary>
        /// 
        private int _gazeX;
        public int GazeX
        {
            get { return _gazeX; }
            set { SetProperty(ref _gazeX, value); }
        }

        private int _gazeY;
        public int GazeY
        {
            get { return _gazeY; }
            set { SetProperty(ref _gazeY, value); }
        }

        public int Time
        {
            get
            {
                return gazeData.Time;
            }
            set
            {
                gazeData.Time = value;
                RaisePropertyChanged("Time");
            }
        }

        #endregion

        List<RawGaze> rawData = new List<RawGaze>();
        /// <summary>
        /// displays data on view + calls writeToCsv function
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="time"></param>
        private void updateGazeData(int x, int y, int time)
        {

            // values from eyeXengine passed here and assigned to view elements
            GazeX = x;
            GazeY = y;
            Time = time;

            //rawData.Add(new RawGaze(x, y, time));
            string csvFormattedGazeData = x.ToString() + "," + y.ToString() + "," + time.ToString();
            // appends string to csv every function call
            writeDataToFile(csvFormattedGazeData);



            //writeFile(rawData, "gazeTrackerOutput");
        }

        #region Write to File Method
        /// <summary>
        /// writes to csv file
        /// </summary>
        /// <param name="text"></param>
        /// 

        private void writeDataToFile(string text)
        {
            using (StreamWriter sw = System.IO.File.AppendText(outputFileDirectory + @"\" + outputFileName + ".csv"))
            {
                sw.WriteLine(text);
            }
        }

        private async Task writeFile(List<RawGaze> data)
        {

            // get path for output
            string exeRuntimeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // filename with filename of clip and timestamp of recording
            String timeStamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            //if (videoSource.ToString() != null)
            string clipName = Path.GetFileName(videoSource.ToString());

            int dotPos = clipName.IndexOf(".");
            clipName = clipName.Substring(0, dotPos);
            Console.WriteLine("video name: " + clipName);
            outputFileName = clipName + "_" + timeStamp + ".csv";
            //string outputFolder = outputFileName;

            // find output foldr/ create if it doesn't exit
            outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output", "GazeTrackerOutput");
            if (!System.IO.Directory.Exists(outputFileDirectory))
            {
                System.IO.Directory.CreateDirectory(outputFileDirectory);
            }

            
            string fullOutput = outputFileDirectory + @"\" + outputFileName;
            using (var writer = new StreamWriter(fullOutput))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await Task.Run(()=>csv.WriteHeader<RawGaze>());
                await Task.Run(()=>csv.NextRecord());
                await Task.Run(()=>csv.WriteRecords(data));
            }
            Console.WriteLine("written: " + fullOutput);

            rawData.Clear();

        }

        #endregion

        #region app properties

        private static void RunOnMainThread(Action action)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(action);
            }
        }

        public void Dispose()
        {

        }

        #endregion


        #region Close Window 

        private DelegateCommand _closeCommand;
        public DelegateCommand CloseCommand =>
            _closeCommand ?? (_closeCommand = new DelegateCommand(CloseWindow));

        void CloseWindow()
        {
            Close?.Invoke();
        }

        public Action Close { get; set; }

        public Action Back { get; set; }
        public Action Next { get; set; }
        public bool IsTrackingGaze { get; private set; }

        #endregion

    }
}
