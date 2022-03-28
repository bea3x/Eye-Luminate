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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Tobii.EyeX.Framework;

namespace RecorderApp.ViewModels
{
    public class GazeTrackerViewModel : BindableBase, IControlWindows
    {
        public WpfEyeXHost _eyeXHost;
        //public GazeData showGazeData = new GazeData(); // From GazeData class from Models
        //public bool userPresent;
        public string initialTime;
        public string outputFileDirectory;
        public string outputFileName;
        //GazeDataService gazeDataService;


        IEventAggregator _ea;
        public IView _view;
        public IView2 _view2;
        public IView3 _view3;

        public GazeTrackerViewModel(IEventAggregator ea, IView3 view3)
        {
            IsTrackingGaze = false;
            IsTrackingGazeSupported = false;
            IsUserPresent = false;

            #region eyeXHost

            // Start WpfEyeXHost to connect to the EyeX engine.
            // Start receiving events and values from engine states
            _eyeXHost = new WpfEyeXHost();

            // Register event listener for gaze tracking status
            _eyeXHost.GazeTrackingChanged += EyeXHost_GazeTrackingChanged;



            // start eyeX host
            _eyeXHost.Start();

            if (_eyeXHost.WaitUntilConnected(TimeSpan.FromSeconds(5)))
            {
                // check EyeX Engine version
                var engineVersion = _eyeXHost.GetEngineVersion().Result;
                if (engineVersion.Major != 1 || engineVersion.Major == 1 && engineVersion.Minor < 4)
                {
                    IsTrackingGazeSupported = false;
                }
            }

            #endregion

            ea.GetEvent<SavePathEvent>().Subscribe(GetVidPath);
            ea.GetEvent<RecStatusEvent>().Subscribe(GetTrackingStatus);
            ea.GetEvent<TimeWatchEvent>().Subscribe(GetTimeElapsed);
            ea.GetEvent<MediaWatchEvent>().Subscribe(GetMediaStatus);
            ea.GetEvent<CalibrationEvent>().Subscribe(GetCalibrationSignal);

            _view3 = view3;
            _ea = ea;
            //MediaEndedCommand = new DelegateCommand(OnMediaEnd);
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
            }
        }

        #region Eyetracking control methods

        public void startTracking()
        {

            // uncomment to initiate calibration
            //_eyeXHost.LaunchGuestCalibration();

            var stream = _eyeXHost.CreateGazePointDataStream(Tobii.EyeX.Framework.GazePointDataMode.LightlyFiltered);

            /*
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
            */

            stream.Next += (s, e) => updateGazeData((int)e.X, (int)e.Y, Timespan);
        }

        public void startCalibration()
        {
            //_eyeXHost.LaunchGuestCalibration();
            //_eyeXHost.LaunchCalibrationTesting();
            _eyeXHost.LaunchRecalibration();

        }


        private void GetCalibrationSignal(bool start)
        {
            startCalibration();
        }
        #endregion

        public DelegateCommand MediaEndedCommand { get; set; }

        private void GetMediaStatus(bool ended)
        {
            if (ended)
            {
                writeFile(rawData);
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

        private int timeSpan;

        public int Timespan
        {
            get { return timeSpan; }
            set
            {
                timeSpan = value;
                RaisePropertyChanged("Timespan");
            }
        }

        public void GetTimeElapsed(Tuple<int,int,int> data)
        {
            ///*
            int x = data.Item1;
            int y = data.Item2;
            int timeMs = data.Item3;
            //*/
            Timespan = timeMs;

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

            rawData.Add(new RawGaze(x, y, time));
            //string csvFormattedGazeData = x.ToString() + "," + y.ToString() + "," + time.ToString();
            // appends string to csv every function call
            //writeDataToFile(csvFormattedGazeData);



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

        private void writeFile(List<RawGaze> data)
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
                csv.WriteHeader<RawGaze>();
                csv.NextRecord();
                csv.WriteRecords(data);
            }
            Console.WriteLine("written: " + fullOutput);

        }

        #endregion


        #region IsTrackingGaze + IsUserPresent


        /// <summary>
        /// check if user is present
        /// </summary>
        private bool _isUserPresent;

        public bool IsUserPresent
        {
            get
            {
                return _isUserPresent;
            }
            set
            {
                _isUserPresent = value;
                RaisePropertyChanged("IsUserPresent");
            }
        }


        /// <summary>
        /// Checks gaze tracking status
        /// </summary>
        private bool _isTrackingGaze;

        public bool IsTrackingGaze
        {
            get
            {
                return _isTrackingGaze;
            }
            set
            {
                _isTrackingGaze = value;
                //SetProperty(ref _isTrackingGaze, value);
                RaisePropertyChanged("IsTrackingGaze");
            }
        }

        private bool _isTrackingGazeSupported;

        public bool IsTrackingGazeSupported
        {
            get
            {
                return _isTrackingGazeSupported;
            }
            set
            {
                _isTrackingGazeSupported = value;
                RaisePropertyChanged("IsTrackingGazeSupported");
                RaisePropertyChanged("IsTrackingGazeNotSupported");
            }
        }

        public bool IsTrackingGazeNotSupported
        {
            get
            {
                return !IsTrackingGazeSupported;
            }
        }

        #endregion


        #region EyeXHost Engine State Values

        private void EyeXHost_UserPresenceChanged(object sender, EngineStateValue<UserPresence> value)
        {
            RunOnMainThread(() =>
            {
                IsUserPresent = value.IsValid && value.Value == UserPresence.Present;
            });
        }

        private void EyeXHost_GazeTrackingChanged(object sender, EngineStateValue<GazeTracking> value)
        {
            RunOnMainThread(() =>
            {
                IsTrackingGaze = value.IsValid && value.Value == GazeTracking.GazeTracked;
            });
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
            _eyeXHost.UserPresenceChanged -= EyeXHost_UserPresenceChanged;
            _eyeXHost.GazeTrackingChanged -= EyeXHost_GazeTrackingChanged;
            _eyeXHost.Dispose();
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

        #endregion
    }
}
