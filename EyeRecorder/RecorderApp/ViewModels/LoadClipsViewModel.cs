using CsvHelper;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RecorderApp.Models;
using RecorderApp.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RecorderApp.ViewModels
{
    public class LoadClipsViewModel : BindableBase, IControlWindows
    {
        IDialogService _dialogService;


        string exeRuntimeDirectory;
        string outputFileDirectory;
        FileInfo[] Files;
        DirectoryInfo dir;
        public LoadClipsViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            
            // get path for output
            exeRuntimeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output");

            // gaze output directory
            string gazeOutputPath = Path.Combine(exeRuntimeDirectory, "Output", "GazeTrackerOutput");

            dir = new DirectoryInfo(outputFileDirectory);

            this.LoadClipsCommand = new RelayCommand(LoadClips);

            getFileList();
        }

        #region Listbox

        private ObservableCollection<Object> _fileList = new ObservableCollection<Object>();

        public ObservableCollection<Object> VidFileList
        {
            get { return _fileList; }
            set
            {
                _fileList = value;
                RaisePropertyChanged("FileList");
            }
        }

        public void getFileList()
        {
            //FileInfo[] Files = dir.GetFiles("*.csv"); //Getting CSV files
            Files = dir.GetFiles("*_selectedClipInfo.csv");
            // sort by last write time
            Array.Sort(Files, (f1, f2) => f1.LastWriteTime.CompareTo(f2.LastWriteTime));
            // reverse array to sort by descending order
            Array.Reverse(Files);

            foreach (FileInfo file in Files)
            {
                Console.WriteLine(file.Name);
                VidFileList.Add(file);
            }

            //SelectedCSV = (FileInfo)FileList.FirstOrDefault();

        }

        private string _selectedCSVFile;
        public string SelectedCSVFile
        {
            get { return _selectedCSVFile; }
            set { SetProperty(ref _selectedCSVFile, value); }
        }

        #endregion

        #region clips

        private ObservableCollection<VideoClip> clipData = new ObservableCollection<VideoClip>();

        public ObservableCollection<VideoClip> ClipData
        {
            get { return clipData; }
            set
            {

                clipData = value;
                RaisePropertyChanged("ClipData");
            }
        }


        private VideoClip selectedItem;

        public VideoClip SelectedItem
        {
            get { return selectedItem; }
            set
            {

                selectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }

        private VideoClip _mergedClip;

        public VideoClip MergedClip
        {
            get { return _mergedClip; }
            set
            {
                SetProperty(ref _mergedClip, value);
            }
        }


        public void Load(string csvPath)
        {
            try
            {
                List<VideoClip> dataList = readFile<VideoClip>(csvPath);
                if (dataList.Count < 1)
                    throw new Exception("No clips to be loaded...");
                else
                {

                    dataList = dataList.OrderBy(o => o.rank).ToList();
                    foreach (VideoClip obj in dataList)
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            // separate merged clip and add the rest to observable collection
                            if (obj.rank == 0)
                            {
                                MergedClip = obj;
                            }
                            else
                            {
                                //obj.timeStamp = GetTimestamp(obj.timeStart, obj.timeEnd);
                                obj.timeStamp = obj.GetTimestamp();
                                clipData.Add(obj);
                                Console.WriteLine("ts: " + obj.timeStamp);
                            }
                        });
                    }


                    clipsDoneLoading(true);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ShowDialog(ex.Message, true);
            }
        }

        #endregion

        #region LoadClipsCommand

        public ICommand LoadClipsCommand { get; set; }

        private async void LoadClips()
        {
            try
            {
                string filename = SelectedCSVFile;
                string infoDir = Path.Combine(outputFileDirectory, filename);
                Console.WriteLine(infoDir);
                if (File.Exists(infoDir))
                {
                    Console.WriteLine("file exists");
                    await Task.Run(() => Load(infoDir));


                    inProcess(false);

                    var msg = "Done Loading Clips";
                    //ShowDialog(msg, false);

                    Output = msg;
                }
                else
                {
                    ShowDialog("Scenes failed to load", true);
                }
            }
            catch
            {

            }
        }


        #endregion

        #region check if clips are loaded to set visibility

        private bool _areClipsLoaded;

        public bool AreClipsLoaded
        {
            get { return _areClipsLoaded; }
            set
            {
                SetProperty(ref _areClipsLoaded, value);
            }
        }

        private void clipsDoneLoading(bool status)
        {
            AreClipsLoaded = status;
        }


        /// <summary>
        /// refresh listbox
        /// </summary>
        public void reload()
        {
            clipsDoneLoading(false);
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                clipData.Clear();
            });

        }

        private string _output;
        public string Output
        {
            get { return _output; }
            set
            {
                SetProperty(ref _output, value);
            }
        }

        #endregion

        #region loading circle bidn

        private bool _isProcessing;

        public bool IsProcessing
        {
            get { return _isProcessing; }
            set
            {
                //_isProcessing = value; 
                SetProperty(ref _isProcessing, value);
            }
        }

        private void inProcess(bool status)
        {
            IsProcessing = status;
        }

        #endregion

        #region Read/Write CSV
        public List<T> readFile<T>(string outputPath)
        {

            List<T> gazeList = new List<T>();

            // find output foldr/ create if it doesn't exit
            //outputFileDirectory = @"" + outputPath;

            //reads csv file as a list
            using (var reader = new StreamReader(@"" + outputPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                gazeList = csv.GetRecords<T>().ToList();
            }

            return gazeList;
        }
        #endregion

        #region Error Dialog + Notif Dialog

        private void ShowDialog(string dialogMessage, bool error)
        {
            var p = new DialogParameters();
            p.Add("message", dialogMessage);
            p.Add("error", error);

            _dialogService.ShowDialog("MessageDialog", p, result =>
            {
                if (result.Result == ButtonResult.OK)
                {

                }
            });
        }

        private void ShowNDialog(string dialogMessage, string path)
        {
            var p = new DialogParameters();
            p.Add("message", dialogMessage);
            p.Add("path", path);

            _dialogService.ShowDialog("NotifDialog", p, result =>
            {
                if (result.Result == ButtonResult.OK)
                {

                }
            });
        }


        #endregion

        #region Back to MainWindow 

        private DelegateCommand _backCommand;
        public DelegateCommand BackCommand =>
            _backCommand ?? (_backCommand = new DelegateCommand(GoBack));

        void CloseWindow()
        {
            Close?.Invoke();
        }

        void GoBack()
        {
            Back?.Invoke();
        }

        public Action Close { get; set; }

        public Action Back { get; set; }

        public Action Next { get; set; }
        #endregion
    }
}
