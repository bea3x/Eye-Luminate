using CsvHelper;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RecorderApp.Models;
using RecorderApp.Utility;
using RecorderApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace RecorderApp.ViewModels
{
    public class ResultsViewModel : BindableBase, IControlWindows
    {
        string exeRuntimeDirectory;
        string outputFileDirectory;
        IDialogService _dialogService;

        string pythonPath;
        IEventAggregator _ea;
        int runCount;
        public ResultsViewModel(IDialogService dialogService, IEventAggregator ea)
        {
            runCount = 0;
            _dialogService = dialogService;
            _ea = ea;
            ea.GetEvent<SavePythonPathEvent>().Subscribe(SetPythonPath);

            // initialize bindings to buttons from view
            this.OpenCommand = new RelayCommand(this.OpenFile);
            this.HelpCommand = new RelayCommand(this.ShowHelp);

            this.CleanDataCommand = new RelayCommand(this.CleanData);
            this.IVTCommand = new RelayCommand(this.PerformIVT);
            this.GroupFixations = new RelayCommand(this.groupFixations);

            this.OpenVidCommand = new RelayCommand(this.OpenVid);
            this.ExtractFrames = new RelayCommand(this.getFrames);

            this.SubmitRateCommand = new RelayCommand(this.SaveRating);
            this.SaveHeatmapCommand = new RelayCommand(this.SaveHeatmap);

            this.ChooseDestPath = new RelayCommand(this.ChooseFolder);
            // get path for output
            exeRuntimeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output");
            selectedPath = Path.Combine(outputFileDirectory, "Clips");
        }


        //get python path
        private void SetPythonPath(string path)
        {
            pythonPath = path;
        }

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

        public void setPath(string filePath)
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
            fd.Filter = "(.csv) |*.csv";

            //outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output");
            fd.InitialDirectory = outputFileDirectory;

            fd.OpenCommand.Execute(null);

            if (fd.FileName == null)
            {
                SelectedFile = "blank";
            }

            this.SelectedFile = fd.FileName;
        }


        #endregion


        #region Open Video File

        private string selectedVid;

        public string SelectedVid
        {
            get { return selectedVid; }
            set
            {
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


                }
            });
            inProcess(false);
            Output = "";
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
            inProcess(false);
            Output = "";
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

        #region Clean Data

        public ICommand CleanDataCommand { get; set; }

        private void CleanData()
        {
            try
            {
                saveName = SaveCsv();
                Console.WriteLine(saveName);
                inProcess(true);
                _output = "Cleaning Data...";
                Console.WriteLine(outputFileDirectory);
                cleanDataAsync().Await();
                Console.WriteLine(outputFileDirectory);
                inProcess(false);

                var msg = "Successfully written: " + saveName;
                ShowNDialog(msg, outputFileDirectory);
            }
            catch
            {

            }
        }
        private List<RawGaze> getCleanData()
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

        private async Task cleanDataAsync()
        {
            //clean data and write validGazeData csv
            List<RawGaze> validGazeData = new List<RawGaze>();
            Output = "Cleaning Data...";
            Console.WriteLine(outputFileDirectory);
            validGazeData = await Task.Run(() => getCleanData());

            Console.WriteLine("CleanDataAsync: " + saveName);
            if (saveName != null || saveName != "")
                SelectedFile = writeFile(validGazeData, saveName);
            else
                SelectedFile = writeFile(validGazeData, "validGazeData.csv");

        }

        #endregion

        #region Perform IVT
        /// <summary>
        /// see IVTViewModel for function definitions
        /// </summary>
        public ICommand IVTCommand { get; set; }
        void PerformIVT()
        {
            try
            {
                saveName = SaveCsv();
                Console.WriteLine(saveName);
                inProcess(true);
                _output = "Applying IVT...";
                Console.WriteLine(outputFileDirectory);
                doIVTAsync().Await();
                Console.WriteLine(outputFileDirectory);
                inProcess(false);

                var msg = "Successfully written: " + saveName;
                ShowNDialog(msg, outputFileDirectory);
            }
            catch
            {

            }
        }

        List<GazeData> getIVT()
        {
            IVTViewModel vm = new IVTViewModel();

            List<RawGaze> preIVT = new List<RawGaze>();

            preIVT = readFile<RawGaze>(SelectedFile);
            List<GazeData> finalGazeData = new List<GazeData>();

            finalGazeData = vm.runIVT(preIVT);
            finalGazeData = vm.fixationGroup(finalGazeData);
            //getClipDimensions(SelectedVid).Await();

            //TODO: Change width, height
            finalGazeData = vm.normalizeCoords(finalGazeData, 1920, 1080);
            return finalGazeData;

            //Console.WriteLine(finalGazeData.Count);
        }

        private async Task doIVTAsync()
        {
            List<GazeData> finalGazeData = new List<GazeData>();
            Output = "Performing IVT...";
            finalGazeData = await Task.Run(() => getIVT());

            if (saveName != null || saveName != "")
                SelectedFile = writeFile(finalGazeData, saveName);
            else
                SelectedFile = writeFile(finalGazeData, "finalGazeData.csv");
        }

        #endregion

        #region Execute Python Scripts for Grouping Fixations

        public ICommand GroupFixations { get; set; }
        void groupFixations()
        {
            string scriptPath = "../../../../Scripts/groupFixations.py";

            //get file name from selected file path
            string fileName = Path.GetFileName(SelectedFile);

            runScript(scriptPath, fileName);
        }

        #endregion

        #region Extract Frames

        /// <summary>
        /// number of scenes to get based on highest fixation duration (?)
        /// </summary>
        private string numScenes;

        public string NumScenes
        {
            get { return numScenes; }
            set
            {
                numScenes = value;
                RaisePropertyChanged("NumScenes");
            }
        }

        public ICommand ExtractFrames { get; set; }

        private async Task extractFrames()
        {
            //string scriptPath = "../../../../Scripts/extractFrames.py";

            string currentDir = Directory.GetCurrentDirectory();
            var gparent = Directory.GetParent(currentDir).Parent.Parent;
            //string parentFolder = Path.Combine(currentDir, @"..\..\..");
            Console.WriteLine("parent folder: " + gparent.FullName);
            //string scriptPath = Path.Combine(gparent.FullName, @"Scripts\extractFrames.py");
            string scriptPath = Path.Combine(gparent.FullName, @"Scripts\extractScenes.py");

            changeOutputFileDir(outputFileDirectory);

            //TODO: change ui so separate csv file for scene selection and ivt+data processing
            string csvFile = Path.GetFileName(selectedFile);
            Console.WriteLine("extractframes: " + csvFile);
            string sceneCount = numScenes;
            string vidPath = selectedVid;

            string args = csvFile + " " + sceneCount + " " + '"' + vidPath + '"';
            Console.WriteLine(args);
            await Task.Run(() => runScript(scriptPath, args, SelectedPath));
        }

        private async Task LoadClips()
        {
            //TODO: modify
            try
            {
                string fn = Path.GetFileNameWithoutExtension(selectedFile);
                fn = fn.Replace("_finalGazeData", "");
                string filename = fn + "_selectedClipInfo.csv";
                string infoDir = Path.Combine(outputFileDirectory, filename);
                Console.WriteLine(infoDir);
                if (File.Exists(infoDir))
                {
                    Console.WriteLine("file exists");
                    await Task.Run(() => Load(infoDir));
                }
                else
                {
                    ShowDialog("There was an error loading the clips", true);
                }
            }
            catch
            {

            }
        }

        private async void getFrames()
        {
            if (checkFields(true))
            {
                if (runCount != 0)
                {
                    reload();
                }
                runCount++;
                inProcess(true);
                Output = "Extracting Scenes...";
                await extractFrames();
                await LoadClips();

                inProcess(false);

                var msg = "Done Loading Clips";
                //ShowDialog(msg, false);

                ShowNDialog("Scenes successfully saved", selectedPath);
                Output = msg;
            }

        }


        #endregion

        #region Folder with filename
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

                    cdFilename = Path.GetFileNameWithoutExtension(SelectedFile);
                    cdFilename = cdFilename.Replace("_finalGazeData", "");
                    Console.WriteLine("cdfilename: " + cdFilename);
                    selectedPath = Path.Combine(outputFileDirectory, "Clips", cdFilename);
                }

            }
            catch
            {

            }
        }
        #endregion

        #region run cmd
        void runScript(string pythonScript, string args)
        {

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(exeRuntimeDirectory + @"\..\pyenv\python.exe", pythonScript + " " + args)
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

        void runScript(string pythonScript, string args, string destFolder)
        {
            destFolder = '"' + destFolder + '"';
            Console.WriteLine("argument: " + @"C:\Python37\python.exe" + " " + pythonScript + " " + args + " " + destFolder);
            Process p = new Process();
            //p.StartInfo = new ProcessStartInfo(@"C:\Python37\python.exe", pythonScript + " " + args + " " + destFolder)
            p.StartInfo = new ProcessStartInfo(pythonPath, pythonScript + " " + args + " " + destFolder)
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
                outputFileDirectory = sfd.FileObj.DirectoryName;
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
                        writeFile(UserClipData, fn);
                        Console.WriteLine(fn + " created!");
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

        public void reload()
        {
            clipsDoneLoading(false);
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                clipData.Clear();
            });

        }

        #endregion

        #region Event Aggregators
        /// <summary>
        /// change Vid Path based on the selected item in listbox (receive change trigger)
        /// </summary>
        /// <param name="obj"></param>
        private void ChangeVidPath(string obj)
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

        /// <summary>
        /// Receives the saved path of selected video from experiment
        /// </summary>
        /// <param name="obj"></param>
        private void GetVidPath(string obj)
        {
            SelectedVid = obj;
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

        private string saveName;
        public string SaveName
        {
            get { return saveName; }
            set { SetProperty(ref saveName, value); }
        }

        public ICommand ChooseDestPath { get; set; }
        private void ChooseFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SelectedPath = fbd.SelectedPath;
                if (SelectedFile != null)
                {
                    string fn = Path.GetFileNameWithoutExtension(SelectedPath);
                    fn.Replace("_finalGazeData", "");
                    selectedPath = Path.Combine(outputFileDirectory, "Clips", fn);
                }
            }
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
                //Console.WriteLine(sfd.FileObj.Directory);
                outputFileDirectory = sfd.FileObj.Directory.ToString();
                Console.WriteLine("savecsv func:" + outputFileDirectory);
                if (sfd.FileObj.Name != null)
                {
                    return sfd.FileObj.Name;
                }
                else return "validGazes.csv";
            }
            else
                return null;


        }

        #endregion

        #region new additions from quick mode

        #endregion


        #region Save Heatmaps

        public ICommand SaveHeatmapCommand { get; set; }

        private async void SaveHeatmap()
        {
            try
            {
                if (checkFields(false))
                {
                    string fn = SaveHeatmapDialog();
                    inProcess(true);
                    Output = "Generating clip with fixation map...";

                    //string dPath = Path.GetDirectoryName(fn);
                    //Console.WriteLine("dest: " + dPath);
                    Console.WriteLine("savename: " + fn);
                    if (fn != null && SelectedVid != null)
                    {
                        await saveHeatmap(fn);
                    }
                    else
                    {
                        ShowDialog("An error occured", true);
                    }

                    string fullOutput = outputFileDirectory + @"\" + fn;

                    if (File.Exists(fullOutput))
                    {
                        inProcess(false);

                        // notification dialog => done task
                        var msg = "Generating clip with fixation map...Done";
                        ShowNDialog(msg, outputFileDirectory);
                    }
                    else
                    {
                        ShowDialog("An error occured2", true);
                    }

                }
            }
            catch
            {
                ShowDialog("An error occured.", true);
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
                outputFileDirectory = sfd.FileObj.Directory.ToString();

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
            string fn = Path.GetFileNameWithoutExtension(selectedFile);
            Console.WriteLine("csv fn: " + fn);



            Console.WriteLine(fn + " exists");

            //string csvFile = Path.GetFileName(fn);
            string csvFile = selectedFile;
            string vidPath = selectedVid;

            string args = csvFile + " " + '"' + vidPath + '"' + " " + vidFileName;
            //runScript(scriptPath, args, destPath);
            Console.WriteLine("chosen args: " + args);
            await Task.Run(() => runScript(scriptPath, args, outputFileDirectory));


        }

        #endregion

        #region clips
        private FileInfo selectedCSV;

        public FileInfo SelectedCSV
        {
            get { return selectedCSV; }
            set
            {
                //selectedCSV = value;
                //RaisePropertyChanged("SelectedCSV");
                SetProperty(ref selectedCSV, value);
            }
        }
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
            List<VideoClip> dataList = readFile<VideoClip>(csvPath);
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
                        obj.timeStamp = obj.GetTimestamp();
                        clipData.Add(obj);
                    }
                });
            }

            //announce loading of clips successfully
            //_ea.GetEvent<LoadedClipsEvent>().Publish(true);

            clipsDoneLoading(true);
            inProcess(false);
        }

        #endregion

        
        private bool checkFields(bool All)
        {
            if (numScenes != null && All)
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
                if (All)
                {
                    ShowDialog("Number of scenes cannot be empty.", true);
                    return false;
                }
                    
            }

            if (selectedFile == null)
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

        #region loading circle bind
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

        #region Show Help Dialog
        public ICommand HelpCommand { get; set; }

        private void ShowHelp()
        {
            var p = new DialogParameters();
            p.Add("type", "resview");

            _dialogService.ShowDialog("HelpDialog", p, result =>
            {
                if (result.Result == ButtonResult.OK)
                {

                }
            });
        }

        #endregion
    }


}
