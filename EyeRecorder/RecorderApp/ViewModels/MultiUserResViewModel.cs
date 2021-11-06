using CsvHelper;
using NReco.VideoInfo;
using Prism.Commands;
using Prism.Mvvm;
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
    public class MultiUserResViewModel : BindableBase, IControlWindows
    {
        string exeRuntimeDirectory;
        string outputFileDirectory;
        public MultiUserResViewModel()
        {
            this.AddFileCommand = new RelayCommand(this.OpenFiles);
            this.RemoveFileCommand = new RelayCommand(this.RemoveFile);
            this.ClearListCommand = new RelayCommand(this.ClearList);

            this.OpenVidCommand = new RelayCommand(this.OpenVid);

            this.GetResultsCommand = new RelayCommand(this.GetResults);

            exeRuntimeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); 
            
            outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output");
            if (!System.IO.Directory.Exists(outputFileDirectory))
            {
                System.IO.Directory.CreateDirectory(outputFileDirectory);
            }

            //testRun();
        }

        #region
        private int getClipDuration(string videoPath)
        {
            var ffProbe = new FFProbe();

            //var videoInfo = ffProbe.GetMediaInfo(@"D:\tobii\thess\EyeGazeTracker\videos\Nike.mp4");
            var videoInfo = ffProbe.GetMediaInfo(videoPath);

            int duration = Convert.ToInt32(videoInfo.Duration.TotalSeconds);
            return duration;
        }

        private RatingSummary rateList;

        public RatingSummary RateList
        {
            get { return rateList; }
            set { SetProperty(ref rateList, value); }
        }


        /// <summary>
        /// initialize list of summary to be edited
        /// </summary>
        private List<RatingSummary> initList(double duration, int skip=5)
        {

            List<RatingSummary> initialLst = new List<RatingSummary>();

            // interval times
            int int_start = 0;
            int int_end = int_start + skip;
            int scene_count = 0;
            int dur = Convert.ToInt32(duration);
            while(int_start < duration)
            {
                scene_count++;

                if (int_end > duration)
                    int_end = dur;

                RatingSummary obj = new RatingSummary(scene_count,int_start,int_end);
                initialLst.Add(obj);

                int_start = int_end + 1;
                int_end += skip;

            }

            return initialLst;
        }

        private List<RatingSummary> countData(List<RatingSummary> rateLst, List<VideoClip> dataLst)
        {
            List<RatingSummary> ratedList = new List<RatingSummary>();

            foreach (RatingSummary row in rateLst)
            {
                int extra = (row.intervalEnd - row.intervalStart) / 2;

                foreach (VideoClip userData in dataLst)
                {
                    int time_start = userData.timeStart/1000;
                    int time_end = userData.timeEnd / 1000;
                    if (time_start >= row.intervalStart && time_end <= row.intervalEnd + extra)
                    {
                        if (userData.rank == 1)
                            row.top1Count++;

                        if (userData.rateValue == "Positive")
                            row.positiveCount++;
                        else if (userData.rateValue == "Negative")
                            row.negativeCount++;
                        else if (userData.rateValue == "Neutral")
                            row.neutralCount++;
                    }
                }

            }

            return rateLst;
        }


        public ICommand GetResultsCommand { get; set; }

        private async void GetResults()
        {
            string fn = SaveCsv();
            if (fn != null) 
                await generateResults(fn);

        }

        private async Task generateResults(string filename)
        {
            int d = getClipDuration(SelectedVid);

            List<RatingSummary> rateLst = new List<RatingSummary>();
            rateLst = initList(d);

            List<string> csvList = new List<string>();

            csvList = readFilesFromList();

            foreach (string csvF in csvList)
            {
                List<VideoClip> csvData = readFile<VideoClip>(csvF);
                rateLst = await Task.Run(() => countData(rateLst, csvData));
            }

            Console.WriteLine(writeFile(rateLst, filename) + " created!");
        }


        private List<string> readFilesFromList()
        {
            List<string> lst = new List<string>();
            if (UserFileList.Count >= 1)
            {
                foreach (FileInfo file in UserFileList)
                {
                    lst.Add(file.FullName);
                }
            }

            return lst;
        }

#endregion

        #region save csv file

        private string SaveCsv()
        {
            FileDialogViewModel sfd = new FileDialogViewModel();
            sfd.Extension = "*.csv";
            sfd.Filter = "CSV Files(.csv)|*.csv | All(*.*)|*";

            sfd.InitialDirectory = outputFileDirectory;
            sfd.SaveFileCommand.Execute(null);
            if (sfd.FileObj != null)
            {
                Console.WriteLine(sfd.FileObj.Directory);
                outputFileDirectory = sfd.FileObj.DirectoryName;
                if (sfd.FileObj.Name != null)
                {
                    return sfd.FileObj.Name;
                }
                else
                {
                    return "Summary";
                }
            }
            else
                return null;

            
        }

        #endregion


        #region bind to listbox from view 

        private ObservableCollection<Object> _userFileList = new ObservableCollection<Object>();

        public ObservableCollection<Object> UserFileList
        {
            get { return _userFileList; }
            set
            {
                SetProperty(ref _userFileList, value);
            }
        }

        private FileInfo _selectedCSVFile;

        public FileInfo SelectedCSVFile
        {
            get { return _selectedCSVFile; }
            set 
            {
                SetProperty(ref _selectedCSVFile, value);
            }
        }


        #endregion

        #region add/remove files listbox
        public ICommand AddFileCommand { get; set; }
        /// <summary>
        /// open csv file
        /// </summary>
        private void OpenFile()
        {
            FileDialogViewModel fd = new FileDialogViewModel();
            fd.Extension = "*.csv";
            fd.Filter = "(.csv)|*.csv";

            fd.InitialDirectory = outputFileDirectory;


            fd.OpenCommand.Execute(null);
            if (fd.FileName != null)
            {
                var file = new FileInfo(fd.FileName);
                if (!DuplicateExists(file))
                    UserFileList.Add(file);
            }
            //Console.WriteLine("Open file");
        }

        private void OpenFiles()
        {
            FileDialogViewModel fd = new FileDialogViewModel();
            fd.Extension = "*.csv";
            fd.Filter = "(.csv)|*.csv";

            fd.InitialDirectory = outputFileDirectory;
            

            fd.OpenMultipleFiles.Execute(null);
            if (fd.FileNames != null)
            {
                foreach (string fn in fd.FileNames)
                {
                    var file = new FileInfo(fn);
                    if (!DuplicateExists(file))
                        UserFileList.Add(file);
                }
                
            }
            //Console.WriteLine("Open file");
        }

        public ICommand RemoveFileCommand { get; set; }
        private void RemoveFile()
        {
            Console.WriteLine(SelectedCSVFile.Name);
            if (SelectedCSVFile != null)
            {
                UserFileList.Remove(SelectedCSVFile);
            }
        }

        public ICommand ClearListCommand { get; set; }

        private void ClearList()
        {
            UserFileList.Clear();
        }

        private bool DuplicateExists(FileInfo file)
        {
            foreach(FileInfo obj in UserFileList)
            {
                if (file.Name == obj.Name)
                    return true;
            }

            return false;
        }

        #endregion

        #region Read/Write CSV
        public List<T> readFile<T>(string outputPath)
        {

            List<T> dataList = new List<T>();

            // find output foldr/ create if it doesn't exit
            //outputFileDirectory = @"" + outputPath;

            //reads csv file as a list
            using (var reader = new StreamReader(@"" + outputPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                dataList = csv.GetRecords<T>().ToList();
            }

            return dataList;
        }

        public string writeFile<T>(List<T> data, string outputFileName)
        {

            //writes list to csv file
            string fullOutput = outputFileDirectory + @"\" + outputFileName;
            using (var writer = new StreamWriter(fullOutput))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }

            return fullOutput;
        }
        #endregion

        #region Open Video File

        private string selectedVid;

        public string SelectedVid
        {
            get { return selectedVid; }
            set
            {
                SetProperty(ref selectedVid, value);
            }
        }

        /// <summary>
        /// get path of videos folder
        /// </summary>
        /// <returns></returns>
        string getParent()
        {
            string currentPath = Directory.GetCurrentDirectory();
            string vidPath = Path.GetFullPath(Path.Combine(currentPath, @"..\..\..\..\..\videos"));
            Console.WriteLine(vidPath);
            return vidPath;
        }


        public ICommand OpenVidCommand { get; set; }
        private void OpenVid()
        {
            FileDialogViewModel fd = new FileDialogViewModel();
            fd.Extension = "*.mp4";
            fd.Filter = "(.mp4) |*.mp4";

            fd.InitialDirectory = getParent();
            fd.OpenCommand.Execute(null);

            if (fd.FileName == null)
            {
                SelectedVid = "blank";
            }

            this.SelectedVid = fd.FileName;
        }


        #endregion

        #region Back to MainWindow 

        private DelegateCommand _backCommand;
        public DelegateCommand BackCommand =>
            _backCommand ?? (_backCommand = new DelegateCommand(CloseWindow));

        void CloseWindow()
        {
            Close?.Invoke();
        }

        public Action Close { get; set; }

        public Action Next { get; set; }
        #endregion

    }
}
