using CsvHelper;
using NReco.VideoInfo;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RecorderApp.Models;
using RecorderApp.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace RecorderApp.ViewModels
{
    public class QuickResultsViewModel : BindableBase, IControlWindows
    {
        string exeRuntimeDirectory;
        string outputFileDirectory;

        string hmOutputPath;

        DirectoryInfo dir;

        int runCount;
        IEventAggregator _ea;
        public bool getAll { get; set; }
        FileInfo[] Files;

        IDialogService _dialogService;
        public QuickResultsViewModel(IEventAggregator ea, IDialogService dialogService)
        {
            runCount = 0;
            _ea = ea;
            _dialogService = dialogService;

            _ea.GetEvent<SavePathEvent>().Subscribe(GetVidPath);
            _ea.GetEvent<RecStatusEvent>().Subscribe(GetTrackingStatus);
            _ea.GetEvent<ListboxWatchEvent>().Subscribe(ChangeVidPath);
            _ea.GetEvent<SaveFileName>().Subscribe(GetFN);

            //this.OpenCommand = new RelayCommand(this.OpenFile);
            this.OpenVidCommand = new RelayCommand(this.OpenVid);

            this.ChooseDestPath = new RelayCommand(this.ChooseFolder);

            this.SelectScenesCommand = new RelayCommand(this.SaveScenes);
            this.SubmitRateCommand = new RelayCommand(this.SaveRating);
            this.SaveHeatmapCommand = new RelayCommand(this.SaveHeatmap);

            // get path for output
            exeRuntimeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output");
            if (!System.IO.Directory.Exists(outputFileDirectory))
            {
                System.IO.Directory.CreateDirectory(outputFileDirectory);
            }

            // gaze output directory
            string gazeOutputPath = Path.Combine(exeRuntimeDirectory, "Output", "GazeTrackerOutput");
            if (!System.IO.Directory.Exists(gazeOutputPath))
            {
                System.IO.Directory.CreateDirectory(gazeOutputPath);
            }

            dir = new DirectoryInfo(gazeOutputPath);

            
            //InitLoad();
            Console.WriteLine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            inProcess(false);
        }

        private void GetFN(string fn)
        {
            Console.WriteLine("file name received: " + fn);
            if (fn != null)
            {
                cdFilename = fn;
            }
        }
        string cdFilename; // creation date filename from gazetrackervm
        private void changeOutputFileDir(string newPath)
        {
            try
            {

                outputFileDirectory = newPath;
                if (cdFilename != null)
                    selectedPath = Path.Combine(outputFileDirectory, "Clips", cdFilename);
                else
                {

                    cdFilename = Path.GetFileNameWithoutExtension(SelectedCSV.FullName);
                    selectedPath = Path.Combine(outputFileDirectory, "Clips", cdFilename);
                }

                hmOutputPath = Path.Combine(outputFileDirectory, "Fixation-maps");
                
            }
            catch
            {

            }
        }

        #region Binding for Rating

        public ICommand SubmitRateCommand { get; set; }
        private string SaveCSVDialog()
        {
            FileDialogViewModel sfd = new FileDialogViewModel();
            sfd.Extension = "*.csv";
            sfd.Filter = "CSV File(.csv)|*.csv | All(*.*)|*";

            sfd.InitialDirectory = outputFileDirectory;
            sfd.SaveFileCommand.Execute(null);
            if (sfd.FileObj != null)
            {
                changeOutputFileDir(sfd.FileObj.DirectoryName);
                if (sfd.FileObj.Name != null)
                {
                    return sfd.FileObj.Name;
                }
                else
                {
                    return "userClipData";
                }
            }
            else
                return null;

        }

        async void SaveRating()
        {

            string fn = SaveCSVDialog();

            if (fn != null && SelectedVid != null)
            {
                List<VideoClip> UserClipData = await submitRating();

                if (UserClipData != null)
                {
                    if (!uncheckedExists())
                    {
                        string newFile = writeFile(UserClipData, fn);
                        Console.WriteLine(newFile + " created!");
                        var msg = fn + " created!";
                        ShowNDialog(msg, outputFileDirectory);
                    }
                    else
                    {
                        var msg = "Please rate all scenes.";
                        ShowDialog(msg, true);
                    }
                }
            }
            
        }

        private bool uncheckedExists()
        {
            foreach (VideoClip clip in ClipData)
            {
                Console.WriteLine(clip.fileName + " " + clip.rating + " " + getRateValue(clip.rating));
                if (clip.rating == -1)
                    return true;
            }

            return false;
        }

        private async Task<List<VideoClip>> submitRating()
        {
            List<VideoClip> UserClipData = new List<VideoClip>();

            foreach (VideoClip clip in ClipData)
            {

                Console.WriteLine(clip.fileName + " " + clip.rating + " " + getRateValue(clip.rating));
                clip.rateValue = getRateValue(clip.rating);
                await Task.Run(() => UserClipData.Add(clip));

            }
            return UserClipData;
        }

        string getRateValue(int rateIndex)
        {
            switch (rateIndex)
            {
                case 1:
                    return "Positive";
                case 2:
                    return "Neutral";
                case 3:
                    return "Negative";
                default:
                    return "";
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

        #region Event Aggregators
        /// <summary>
        /// change Vid Path based on the selected item in listbox (receive change trigger)
        /// </summary>
        /// <param name="obj"></param>
        private void ChangeVidPath(string obj)
        {
            try
            {
                // 1. get the filename from SelectedCSV ,/
                string fn = obj;

                // 2. substring the name ,/
                int br = fn.IndexOf('_');
                string name = fn.Substring(0, br);

                // 3. find name from directory of clips if it exists
                string default_vidfolder = getParent();
                DirectoryInfo defaultDir = new DirectoryInfo(default_vidfolder);
                FileInfo[] matched = defaultDir.GetFiles(name + "*");

                // 4. assign path to SelectedVid
                if (matched.Any())
                {

                    Console.WriteLine(matched.First().FullName);
                    SelectedVid = matched.First().FullName;

                }
            }
            catch
            {
                SelectedVid = "";
            }

        }


        /// <summary>
        /// Receives the saved path of selected video from experiment
        /// </summary>
        /// <param name="obj"></param>
        private void GetVidPath(string obj)
        {
            SelectedVid = obj;
            Console.WriteLine("nagwork ba huhu?" + SelectedVid);
        }

        /// <summary>
        /// Checks if view is triggered before/after a gazetracking session
        /// if before, list of all files in directory is shown
        /// if after, the latest recorded session is shown
        /// </summary>
        /// <param name="done"></param>
        private void GetTrackingStatus(bool done)
        {
            Console.WriteLine("un nasend: " + done);
            if (done)
            {
                getLast();
                _ea.GetEvent<RecStatusEvent>().Unsubscribe(GetTrackingStatus);
            }
            else
            {
                getFileList();
            }
        }
        #endregion

        #region Listbox
        public void getFileList()
        {
            //FileInfo[] Files = dir.GetFiles("*.csv"); //Getting CSV files
            Files = dir.GetFiles("*.csv");
            // sort by last write time
            Array.Sort(Files, (f1, f2) => f1.LastWriteTime.CompareTo(f2.LastWriteTime));
            // reverse array to sort by descending order
            Array.Reverse(Files);

            foreach (FileInfo file in Files)
            {
                Console.WriteLine(file.Name);
                FileList.Add(file);
            }

            //SelectedCSV = (FileInfo)FileList.FirstOrDefault();

        }

        public void getLast()
        {
            try
            {

                Files = dir.GetFiles("*.csv");
                foreach (FileInfo File in Files) {
                    Console.WriteLine(File.Name);
                }
                // sort by last write time
                Array.Sort(Files, (f1, f2) => f1.LastWriteTime.CompareTo(f2.LastWriteTime));
                // reverse array to sort by descending order
                Array.Reverse(Files);
                FileList.Clear();
                FileList.Add(Files[0]);
                Console.WriteLine(Files[0]);
            }
            catch
            {
                Console.WriteLine("something wrong");
            }
        }

        private ObservableCollection<Object> _fileList = new ObservableCollection<Object>();

        public ObservableCollection<Object> FileList
        {
            get { return _fileList; }
            set
            {
                _fileList = value;
                RaisePropertyChanged("FileList");
            }
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

                    //announce loading of clips successfully
                    //_ea.GetEvent<LoadedClipsEvent>().Publish(true);

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

        #region
        #endregion


        #region UI bindings




        private string numScenes;
        public string NumScenes
        {
            get { return numScenes; }
            set { SetProperty(ref numScenes, value); }
        }


        private FileInfo selectedCSV;

        public FileInfo SelectedCSV
        {
            get { return selectedCSV; }
            set
            {
                selectedCSV = value;
                RaisePropertyChanged("SelectedCSV");
                //SetProperty(ref selectedCSV, value);
            }
        }




        #endregion
        

        #region Choose Destination Folder for output

        private string selectedPath;

        public string SelectedPath
        {
            get { return selectedPath; }
            set
            {
                SetProperty(ref selectedPath, value);
            }
        }

        private string _fbdTitle;

        public string FBDTitle
        {
            get { return _fbdTitle; }
            set { SetProperty(ref _fbdTitle, value); }
        }

        public ICommand ChooseDestPath { get; set; }
        private void ChooseFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (_fbdTitle != null)
            {
                fbd.Description = _fbdTitle;
            }
            fbd.SelectedPath = outputFileDirectory;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //SelectedPath = fbd.SelectedPath;
                //outputFileDirectory = fbd.SelectedPath;
                //selectedPath = Path.Combine(outputFileDirectory, "Clips");
                //hmOutputPath = Path.Combine(outputFileDirectory, "Heatmaps");
                changeOutputFileDir(fbd.SelectedPath);
                outputChosen = true;
            }
            else
                outputChosen = false;

        }

        public bool outputChosen;

        #endregion

        #region Open CSV File
        private string selectedFile;

        public string SelectedFile
        {
            get { return selectedFile; }
            set
            {
                selectedFile = value;
                RaisePropertyChanged("SelectedFile");
            }
        }

        public void setDestPath(string filePath)
        {
            SelectedFile = filePath;
        }


        public ICommand OpenCommand { get; set; }
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

            if (fd.FileName == null)
            {
                SelectedFile = "blank";
            }

            this.SelectedFile = fd.FileName;
            Console.WriteLine("Open file");
        }
        #endregion

        #region save csv file

        private void SaveFile()
        {
            FileDialogViewModel sfd = new FileDialogViewModel();
            sfd.Extension = "*.csv";
            sfd.Filter = "CSV Files(.csv)|*.csv | All(*.*)|*";

            sfd.InitialDirectory = outputFileDirectory;

            sfd.SaveFileCommand.Execute(null);
            if (sfd.FileObj.Directory != null)
            {
                Console.WriteLine(sfd.FileObj.Directory);
            }
        }

        #endregion

        #region Open Video File

        private string selectedVid;

        public string SelectedVid
        {
            get { return selectedVid; }
            set
            {
                if (SelectedCSV != null)
                {
                    Console.WriteLine("something is selected right now");

                    Console.WriteLine("selected filename: " + Path.GetFileNameWithoutExtension(selectedCSV.FullName));
                }
                selectedVid = value;
                RaisePropertyChanged("SelectedVid");
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

        #region Clean Data

        private List<RawGaze> CleanData()
        {

            /// <summary>
            /// Lists for gaze Data!!!
            /// </summary>
            List<RawGaze> pass1 = new List<RawGaze>(); //remove blanks
            List<RawGaze> pass2 = new List<RawGaze>(); //remove time duplicates
            List<RawGaze> pass3 = new List<RawGaze>(); //remove excess time
            List<RawGaze> validGazes = new List<RawGaze>(); //write file

            CleanDataViewModel cleanVm = new CleanDataViewModel();

            // functions defined in CleanDataViewModel
            pass1 = readFile<RawGaze>(SelectedFile);
            pass2 = cleanVm.removeSuspicious(pass1);
            pass3 = cleanVm.removeBlanks(pass2);
            validGazes = cleanVm.removeDuplicates(pass3);

            return validGazes;
        }

        #endregion

        #region Perform IVT
        /// <summary>
        /// see IVTViewModel for function definitions
        /// </summary>
        List<GazeData> PerformIVT()
        {
            IVTViewModel vm = new IVTViewModel();

            List<RawGaze> preIVT = new List<RawGaze>();

            preIVT = readFile<RawGaze>(SelectedFile);
            List<GazeData> finalGazeData = new List<GazeData>();

            finalGazeData = vm.runIVT(preIVT);
            finalGazeData = vm.fixationGroup(finalGazeData);
            //getClipDimensions(SelectedVid).Await();

            finalGazeData = vm.normalizeCoords(finalGazeData, Width, Height);
            return finalGazeData;

            //Console.WriteLine(finalGazeData.Count);
        }

        #endregion

        #region Get Video Dimensions

        public int Width { get; set; }
        public int Height { get; set; }

        private async Task getClipDimensions(string videoPath)
        {

            var ffProbe = new FFProbe();

            //var videoInfo = ffProbe.GetMediaInfo(@"D:\tobii\thess\EyeGazeTracker\videos\Nike.mp4");
            var videoInfo = ffProbe.GetMediaInfo(videoPath);
            //var rawXml = videoInfo.Result.CreateNavigator().OuterXml;

            //MediaInfo mediaInfo = new MediaInfo(videoInfo.Result);
            //Task getWidth = Task.Run(()=>videoInfo.Streams.First().Width);
            Width = await Task.Run(() => videoInfo.Streams.FirstOrDefault().Width);

            Height = await Task.Run(() => videoInfo.Streams.FirstOrDefault().Height);
            Console.WriteLine("Width: " + Width + " Height: " + Height);
        }

        #endregion

        #region Extract Scenes
        void extractScenes()
        {
            //string scriptPath = "../../../Scripts/extractScenes.py";
            Console.WriteLine("current dir: " + Directory.GetCurrentDirectory());
            string currentDir = Directory.GetCurrentDirectory();
            var gparent = Directory.GetParent(currentDir).Parent.Parent;
            //string parentFolder = Path.Combine(currentDir, @"..\..\..");
            Console.WriteLine("parent folder: " + gparent.FullName);
            string scriptPath = Path.Combine(gparent.FullName, @"Scripts\extractScenes.py");

            //TODO: change ui so separate csv file for scene selection and ivt+data processing
            string csvFile = Path.GetFileName(SelectedFile);
            string sceneCount = numScenes;
            string vidPath = selectedVid;

            string args = csvFile + " " + sceneCount + " " + '"' + vidPath + '"';
            Console.WriteLine(args);
            Console.WriteLine("chosen path: " + SelectedPath);
            runScript(scriptPath, args, selectedPath);
        }

        #endregion

        #region Save Heatmaps

        public ICommand SaveHeatmapCommand { get; set; }

        private async void SaveHeatmap()
        {
            try
            {

                inProcess(true);
                Output = "Generating clip with fixation map...";

                string fn = SaveHeatmapDialog();
                //string dPath = Path.GetDirectoryName(fn);
                //Console.WriteLine("dest: " + dPath);
                if (fn != null && SelectedVid != null)
                {
                    await saveHeatmap(fn);
                    inProcess(false);

                    // notification dialog => done task
                    var msg = "Generating clip with fixation map...Done";
                    ShowNDialog(msg, hmOutputPath);
                }
            }
            catch
            {

            }
            
        }
        private string SaveHeatmapDialog()
        {
            FileDialogViewModel sfd = new FileDialogViewModel();
            sfd.Extension = "*.mp4";
            sfd.Filter = "MP4 File(.mp4)|*.mp4 | All(*.*)|*";

            sfd.InitialDirectory = outputFileDirectory;
            sfd.SaveFileCommand.Execute(null);
            if (sfd.FileObj != null)
            {
                //Console.WriteLine(sfd.FileObj.Directory);
                //outputFileDirectory = sfd.FileObj.DirectoryName;
                changeOutputFileDir(sfd.FileObj.DirectoryName);
                if (!System.IO.Directory.Exists(hmOutputPath))
                {
                    System.IO.Directory.CreateDirectory(hmOutputPath);
                }
                if (sfd.FileObj.Name != null)
                {
                    return sfd.FileObj.Name;
                }
                else
                {
                    return "Heatmap";
                }
            }
            else
                return null;

        }


        private async Task saveHeatmap(string vidFileName = "")
        {
            //string scriptPath = "../../../../Scripts/getHeatmap.py";
            string currentDir = Directory.GetCurrentDirectory();
            var gparent = Directory.GetParent(currentDir).Parent.Parent;
            string scriptPath = Path.Combine(gparent.FullName, @"Scripts\getHeatmap.py");

            string selectedFn = SelectedFile;
            //selectedFn = selectedFn.Replace("_selectedInfo.csv", "_fixations.csv");
            string fn = Path.GetFileNameWithoutExtension(selectedFn);
            Console.WriteLine("csv fn: " + fn);


            //outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output");
            /*
            if (!System.IO.Directory.Exists(hmOutputPath))
            {
                System.IO.Directory.CreateDirectory(hmOutputPath);
            }*/

            //string destPath = Path.Combine(outputFileDirectory, "Heatmaps");
            Console.WriteLine("output file directory: " + hmOutputPath);
            //string infDir = Path.Combine(outputFileDirectory, fn);
            string infDir = selectedFn.Replace("_finalGazeData.csv","_fixations.csv");
            Console.WriteLine("info directory: " + infDir);
            if (File.Exists(infDir))
            {
                Console.WriteLine(fn + " exists");

                //string csvFile = Path.GetFileName(fn);
                string csvFile = infDir;
                string vidPath = selectedVid;

                string args = csvFile + " " + '"' + vidPath + '"' + " " + vidFileName;
                //runScript(scriptPath, args, destPath);
                Console.WriteLine("chosen args: " + args);
                await Task.Run(() => runScript(scriptPath, args, hmOutputPath));
            }

        }

        #endregion

        #region run cmd
        void runScript(string pythonScript, string args, string destFolder)
        {
            destFolder = '"' + destFolder + '"';
            Console.WriteLine("argument: " + exeRuntimeDirectory+@"\..\pyenv\python.exe" + " " + pythonScript + " " + args + " " + destFolder);
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(exeRuntimeDirectory + @"\..\pyenv\python.exe", pythonScript + " " + args + " " + destFolder)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = outputFileDirectory
            };

            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            Console.WriteLine("Running Python Script...");
            Console.WriteLine(output);
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

        public string writeFile<T>(List<T> data, string outputFileName)
        {
            /*
            if (outputFileDirectory == null || outputFileDirectory == "")
                outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output");
            */
            Console.WriteLine(exeRuntimeDirectory);
            Console.WriteLine("outputfiledirectory @writefile: " + outputFileDirectory);

            //writes list to csv file
            string fullOutput = outputFileDirectory + @"\" + outputFileName;
            using (var writer = new StreamWriter(fullOutput))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }
            Console.WriteLine("written: " + fullOutput);
            return fullOutput;
        }

        #endregion

        #region Scene Selection

        public ICommand SelectScenesCommand { get; set; }

        private async Task<bool> cleanDataAsync()
        {
            //clean data and write validGazeData csv
            List<RawGaze> validGazeData = new List<RawGaze>();
            Output = "Cleaning Data...";
            validGazeData = await Task.Run(() => CleanData());

            string filename = Path.GetFileNameWithoutExtension(selectedCSV.FullName) + "_validGazeData.csv";
            Console.WriteLine("CleanDataAsync: " + filename);
            SelectedFile = writeFile(validGazeData, filename);

            if (validGazeData.Count >= 1)
                return true;

            return false;
        }

        private async Task doIVTAsync()
        {
            List<GazeData> finalGazeData = new List<GazeData>();
            Output = "Performing IVT...";
            finalGazeData = await Task.Run(() => PerformIVT());

            string filename = Path.GetFileNameWithoutExtension(selectedCSV.FullName) + "_finalGazeData.csv";
            SelectedFile = writeFile(finalGazeData, filename);
        }

        private async Task getScenes()
        {
            Output = "Extracting Scenes...";
            await Task.Run(() => extractScenes());
        }

        private bool checkFields()
        {
            if (numScenes != null)
            {
                var isNumeric = int.TryParse(numScenes, out int n);

                if (n <= 0 || n > 20)
                {
                    ShowDialog("Invalid number of scenes entered. Please try again", true);
                    return false;
                }
           
            }
            else
            {
                ShowDialog("Number of scenes cannot be empty.", true);
                return false;
            }

            if (selectedCSV == null)
            {
                ShowDialog("No selected csv file", true);
                return false;
            }
            else if (selectedVid == null || selectedVid == "")
            {
                ShowDialog("No selected video file", true);
                return false;
            }

            return true;
        }

        #endregion

        #region Error Dialog

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

        #region Select Save Directory
        // select save directory 

        private void SaveScenes()
        {
            //SaveFile();
            _fbdTitle = "Select Folder to Save CSV Files";
            ChooseFolder();
            if (outputChosen)
                SelectScenes();
        }

        private async void SelectScenes()
        {
            if (checkFields())
            {
                if (runCount != 0)
                {
                    reload();
                }
                runCount++;

                inProcess(true);
                await getClipDimensions(SelectedVid);
                Console.WriteLine("Width: " + Width + " Height: " + Height);
                Thread.Sleep(100);
                SelectedFile = SelectedCSV.FullName;
                Thread.Sleep(100);
                bool valid = await cleanDataAsync();

                if (valid)
                {

                    //perform IVT algo
                    Thread.Sleep(100);
                    await doIVTAsync();


                    Thread.Sleep(100);
                    //group fixations and extract scenes
                    await getScenes();

                    //Output = "Loaded Clips...";
                    //TODO: modify and make not brute-force
                    string filename = Path.GetFileNameWithoutExtension(selectedCSV.FullName) + "_selectedClipInfo.csv";
                    string infoDir = Path.Combine(outputFileDirectory, filename);
                    Console.WriteLine(infoDir);
                    if (File.Exists(infoDir))
                    {
                        Console.WriteLine("file exists");
                        Load(infoDir);


                        inProcess(false);

                        var msg = "Done Loading Clips";
                        //ShowDialog(msg, false);

                        ShowNDialog("Scenes loaded and saved", selectedPath);
                        Output = msg;
                    }
                    else
                    {
                        ShowDialog("Scenes failed to load", true);
                    }

                }
                else
                {
                    ShowDialog("Something went wrong with the CSV file", true);
                    inProcess(false);
                    Output = "";
                }
            }
            
        }

        #endregion


        #region Back to MainWindow 

        private DelegateCommand _backCommand;
        public DelegateCommand BackCommand =>
            _backCommand ?? (_backCommand = new DelegateCommand(GoBack));

        void CloseWindow()
        {
            dispose();
            Close?.Invoke();
        }

        void GoBack()
        {
            dispose();
            Back?.Invoke();
        }

        public Action Close { get; set; }

        public Action Back { get; set; }

        public Action Next { get; set; }
        #endregion

        private void dispose()
        {
            _ea.GetEvent<SavePathEvent>().Unsubscribe(GetVidPath);
            _ea.GetEvent<RecStatusEvent>().Unsubscribe(GetTrackingStatus);
            _ea.GetEvent<ListboxWatchEvent>().Unsubscribe(ChangeVidPath);
            _ea.GetEvent<SaveFileName>().Unsubscribe(GetFN);
        }
    }
    public static class TaskExtensions
    {
        public async static void Await(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
