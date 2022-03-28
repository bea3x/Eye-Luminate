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
using System.Threading.Tasks;
using System.Windows.Input;

namespace RecorderApp.ViewModels
{
    public class MultiUserResViewModel : BindableBase, IControlWindows
    {
        string exeRuntimeDirectory;
        string outputFileDirectory;

        string hmOutputPath;
        string pythonPath;
        IEventAggregator _ea;

        IDialogService _dialogService;
        public MultiUserResViewModel(IDialogService dialogService, IEventAggregator ea)
        {
            _dialogService = dialogService;
            _ea = ea;
            ea.GetEvent<SavePythonPathEvent>().Subscribe(SetPythonPath);

            this.AddFileCommand = new RelayCommand(this.OpenFiles);
            this.RemoveFileCommand = new RelayCommand(this.RemoveFile);
            this.ClearListCommand = new RelayCommand(this.ClearList);
            this.HelpCommand = new RelayCommand(this.ShowHelp);

            this.OpenVidCommand = new RelayCommand(this.OpenVid);

            this.GetResultsCommand = new RelayCommand(this.GetResults);
            this.GetFixationsResults = new RelayCommand(this.GetFixResults);
            this.GetGrpHeatmap = new RelayCommand(this.GetHeatmap);

            exeRuntimeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); 
            
            outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output");
            if (!System.IO.Directory.Exists(outputFileDirectory))
            {
                System.IO.Directory.CreateDirectory(outputFileDirectory);
            }

            //testRun();
        }

        //get python path
        private void SetPythonPath(string path) {
            pythonPath = path;
        }

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
        private void RChart(string dialogMessage, string path, List<RatingSummary> data)
        {
            var p = new DialogParameters();
            p.Add("message", dialogMessage);
            p.Add("path", path);
            p.Add("rateList", data);

            _dialogService.ShowDialog("RateChartView", p, result =>
            {
                if (result.Result == ButtonResult.OK)
                {

                }
            });
        }

        private void FChart(string dialogMessage, string path, List<Variability> data)
        {
            var p = new DialogParameters();
            p.Add("message", dialogMessage);
            p.Add("path", path);
            p.Add("fixList", data);

            _dialogService.ShowDialog("ChartView", p, result =>
            {
                if (result.Result == ButtonResult.OK)
                {

                }
            });
        }

        #endregion

        #region pre Get Results
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

        #region Get Results

        public ICommand GetResultsCommand { get; set; }
        /// <summary>
        /// main void for getting ratings summary
        /// </summary>
        private async void GetResults()
        {
            if (SelectedVid == null)
            {
                ShowDialog("Please select a video file to continue.", false);
            }
            else if (UserFileList.Count == 0)
            {
                ShowDialog("No selected CSV files to process.", true);
            }
            else
            {
                List<RatingSummary> rateLst = new List<RatingSummary>();
                string fn = SaveCsv();
                if (fn != null && SelectedVid != null)
                {
                    try
                    {

                        inProcess(true);
                        Output = "Generating ratings summary...";
                        rateLst = await generateResults(); 
                        rateLst = await Task.Run(() => convertIntervals(rateLst));
                        rateLst = await Task.Run(() => getAccuracy(rateLst));

                        inProcess(false);
                        Console.WriteLine(writeFile(rateLst, fn) + " created!");
                        Console.WriteLine("csv dir: " + Path.GetDirectoryName(fn));
                        var msg = fn + " successfully created.";
                        //ShowDialog(msg, false);
                        //ShowNDialog(msg, outputFileDirectory);
                        RChart(fn, outputFileDirectory, rateLst);
                    }
                    catch (CsvHelper.TypeConversion.TypeConverterException ex)
                    {
                        
                        Console.WriteLine(ex.Message);
                        ShowDialog("Error reading CSV files. Please recheck.", true);
                    }
                    catch (System.IO.IOException ex2)
                    {
                        Console.WriteLine(ex2.Message);
                        ShowDialog("Incorrect CSV files. Please recheck.", true);
                    }
                    catch (CsvHelper.MissingFieldException ex3)
                    {
                        Console.WriteLine(ex3.Message);
                        ShowDialog("Error reading CSV files. Please recheck.", true);
                    }
                    finally
                    {
                        clearProc();
                    }

                }
            }
            

            //Console.WriteLine(writeFile(rateLst, filename) + " created!");

        }

        private async Task<List<RatingSummary>> generateResults()
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
            return rateLst;
        }

        private List<RatingSummary> convertIntervals(List<RatingSummary> data)
        {
            List<RatingSummary> convData = new List<RatingSummary>();
            RatingSummary obj;
            foreach (RatingSummary row in data)
            {
                string timestamp = secondsToTimestamp(row.intervalStart, row.intervalEnd);
                obj = new RatingSummary(row.sceneNumber, timestamp, row.top1Count, row.positiveCount, row.negativeCount, row.neutralCount);

                convData.Add(obj);
            }

            return convData;
        }

        private string secondsToTimestamp(int start, int end)
        {
            TimeSpan s = TimeSpan.FromSeconds(start);
            TimeSpan e = TimeSpan.FromSeconds(end);
            string ts = string.Format("{1:D2}:{2:D2}",
                        s.Hours,
                        s.Minutes,
                        s.Seconds,
                        s.Milliseconds);

            string te = string.Format("{1:D2}:{2:D2}",
                        e.Hours,
                        e.Minutes,
                        e.Seconds,
                        e.Milliseconds);

            return ts + " - " + te;
        }

        private List<RatingSummary> getAccuracy(List<RatingSummary> data)
        {
            RatingSummary obj;
            int count = 0;
            double avgPct = 0, sum = 0;
            foreach (RatingSummary row in data)
            {
                double pct = calculateAccuracy(row.positiveCount, row.negativeCount, row.neutralCount);
                row.accuracyPct = pct;
                sum += pct;
                count++;
            }

            avgPct = sum / count;
            obj = new RatingSummary(0, "Average" ,0,0,0,0,avgPct);
            data.Add(obj);
            return data;
        }

        private double calculateAccuracy(int posCount, int negCount, int neutCount)
        {
            double total = posCount + negCount + neutCount;
            if (total == 0)
                return 0;
            else
            {
                double pct = (posCount + negCount) / total;
                pct *= 100;

                return pct;
            }
        }



        #endregion

        #region pre Fix Results

        private List<Variability> initVList(double duration, int skip = 5)
        {

            List<Variability> initialLst = new List<Variability>();

            // interval times
            int int_start = 0;
            int int_end = int_start + skip;
            int scene_count = 0;
            int dur = Convert.ToInt32(duration);
            while (int_start < duration)
            {
                scene_count++;

                if (int_end > duration)
                    int_end = dur;

                Variability obj = new Variability(scene_count, int_start, int_end);
                initialLst.Add(obj);

                int_start = int_end + 1;
                int_end += skip;

            }

            return initialLst;
        }

        private List<Variability> countData(List<Variability> fixLst, List<Fixation> dataLst)
        {
            //List<RatingSummary> ratedList = new List<RatingSummary>();

            foreach (Variability row in fixLst)
            {
                int extra = (row.intervalEnd - row.intervalStart) / 2;

                foreach (Fixation userData in dataLst)
                {
                    int time_start = userData.timeStart / 1000;
                    int time_end = userData.timeEnd / 1000;
                    double duration = Convert.ToDouble(userData.duration) / 1000.0;
                    if (time_start >= row.intervalStart && time_end <= row.intervalEnd + extra)
                    {
                        int Width = 1920;
                        int Height = 1080;
                        //Console.WriteLine("here " + userData.centroidX);
                        row.centroidXList.Add(userData.centroidX*Width);
                        row.centroidYList.Add(userData.centroidY*Height);
                        row.durationList.Add(duration);

                        //row.addToXList(userData.centroidX);
                        //row.addToYList(userData.centroidY);
                        //row.addToDurationList(userData.duration);
                        row.fixationCount++;
                    }
                }

                Console.WriteLine("centroidX: " + row.centroidXList.Count());

            }

            return fixLst;
        }

        #endregion

        #region calculation methods

        private double getZ(double mean, List<double> centroidList)
        {
            double sum = 0;
            foreach(double point in centroidList)
            {
                double Zi = Math.Pow((point - mean), 2);
                sum += Zi;
            }

            return sum;
        }

        private double getMean(List<double> centroidList)
        {
            double sum = 0;
            foreach (double point in centroidList)
            {
                sum += point;
            }

            int n = centroidList.Count();
            return sum/n;
        }

        private double getSum(List<double> durList)
        {
            double sum = 0;
            foreach (double val in durList)
            {
                Console.WriteLine(val);
                sum += val;
            }

            return sum;
        }

        private List<Variability> calculateSD(List<Variability> fixLst)
        {
            foreach (Variability row in fixLst)
            {
                double meanX = getMean(row.centroidXList);
                double meanY = getMean(row.centroidYList);

                int countX = row.centroidXList.Count();
                int countY = row.centroidYList.Count();

                double Zx = getZ(meanX, row.centroidXList) / countX;
                double Zy = getZ(meanY, row.centroidYList) / countY;

                row.standardDev = Math.Round(Math.Sqrt(Zx + Zy), 2);
                row.durationLen = Math.Round(getSum(row.durationList),2);
                //row.durationMean = Math.Round((getSum(row.durationList) / row.durationList.Count), 2);
                //row.durationLen = getSum(row.durationList);

            }
            return fixLst;
        }

        #endregion

        #region Get Fix Results

        public ICommand GetFixationsResults { get; set; }
        private async void GetFixResults()
        {
            if (SelectedVid == null)
            {
                ShowDialog("Please select a video file to continue.", false);
            }
            else if (UserFileList.Count == 0)
            {
                ShowDialog("No selected CSV files to process.", true);
            }
            else
            {
                
                List<Variability> fixLst = new List<Variability>();
                string fn = SaveCsv();
                if (fn != null)
                {
                    try
                    {

                        inProcess(true);
                        Output = "Generating fixation summary...";
                        fixLst = await generateFixResults();
                        fixLst = await Task.Run(() => calculateSD(fixLst));
                        fixLst = await Task.Run(() => convertIntervals(fixLst));

                        inProcess(false);
                        Console.WriteLine(writeFile(fixLst, fn) + " created!");
                        var msg = fn + " successfully created.";
                        //ShowDialog(msg, false);
                        //ShowNDialog(msg, outputFileDirectory);
                        FChart(fn, outputFileDirectory, fixLst);

                    }
                    catch (CsvHelper.TypeConversion.TypeConverterException ex)
                    {
                        Console.WriteLine(ex.Message);
                        ShowDialog("Error reading CSV files. Please recheck.", true);
                    }
                    catch (System.IO.IOException ex2)
                    {
                        Console.WriteLine(ex2.Message);
                        ShowDialog("Incorrect CSV files. Please recheck.", true);
                    }
                    catch (CsvHelper.MissingFieldException ex3)
                    {
                        Console.WriteLine(ex3.Message);
                        ShowDialog("Error reading CSV files. Please recheck.", true);
                    }
                    finally
                    {
                        clearProc();
                    }
                }
            }

            //Console.WriteLine(writeFile(rateLst, filename) + " created!");

        }

        private async Task<List<Variability>> generateFixResults()
        {
            int d = getClipDuration(SelectedVid);

            List<Variability> fixLst = new List<Variability>();
            fixLst = initVList(d);

            List<string> csvList = new List<string>();

            csvList = readFilesFromList();

            foreach (string csvF in csvList)
            {
                List<Fixation> csvData = readFile<Fixation>(csvF);
                fixLst = await Task.Run(() => countData(fixLst, csvData));
            }
            return fixLst;
        }

        private List<Variability> convertIntervals(List<Variability> data)
        {
            List<Variability> convData = new List<Variability>();
            Variability obj;
            foreach (Variability row in data)
            {
                //Console.WriteLine("intervals: " + row.durationLen);
                string timestamp = secondsToTimestamp(row.intervalStart, row.intervalEnd);
                obj = new Variability(row.sceneNumber, timestamp, row.centroidX, row.centroidY, row.durationLen, row.fixationCount, row.standardDev);

                convData.Add(obj);
            }

            return convData;
        }

        #endregion

        #region Group Heatmap
        public ICommand GetGrpHeatmap { get; set; }

        private async void GetHeatmap()
        {
            if (SelectedVid == null)
            {
                ShowDialog("Please select a video file to continue.", false);
            }
            else if (UserFileList.Count == 0)
            {
                ShowDialog("No selected CSV files to process.", true);
            }
            else
            {


                try
                {
                    // 1. write to text file
                    string txtFile = await writeTxt();
                    // 2. start loading
                    // 3. start python script
                    inProcess(true);
                    Output = "Generating clip with fixation map...";

                    string fn = SaveHeatmapDialog();
                    //string dPath = Path.GetDirectoryName(fn);
                    //Console.WriteLine("dest: " + dPath);
                    if (fn != null && SelectedVid != null)
                    {
                        await saveHeatmap(txtFile,fn);
                        inProcess(false);

                        // notification dialog => done task
                        var msg = "Generating clip with fixation map...Done";
                        ShowNDialog(msg, hmOutputPath);
                    }
                    // 4. end loading

                }
                catch (CsvHelper.TypeConversion.TypeConverterException ex)
                {
                    Console.WriteLine(ex.Message);
                    ShowDialog("Error reading CSV files. Please recheck.", true);
                }
                catch (System.IO.IOException ex2)
                {
                    Console.WriteLine(ex2.Message);
                    ShowDialog("Incorrect CSV files. Please recheck.", true);
                }
                catch (CsvHelper.MissingFieldException ex3)
                {
                    Console.WriteLine(ex3.Message);
                    ShowDialog("Error reading CSV files. Please recheck.", true);
                }
                finally
                {
                    clearProc();
                }
            }
        }

        private async Task<string> writeTxt()
        {
            List<string> csvList = new List<string>();
            csvList = readFilesFromList();

            foreach (string csvF in csvList)
            {
                //check csv file
                List<Fixation> csvData = readFile<Fixation>(csvF);
            }

            string outputFileName = "temp.txt";
            string fullOutput = outputFileDirectory + @"\" + outputFileName;

            await Task.Run(()=>File.WriteAllLines(fullOutput, csvList));

            return fullOutput;
        }

        #endregion

        #region Save Heatmap

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
                outputFileDirectory = sfd.FileObj.DirectoryName;
                hmOutputPath = Path.Combine(outputFileDirectory, "Fixation-maps");
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


        private async Task saveHeatmap(string textFile, string vidFileName = "")
        {
            //string scriptPath = "../../../../Scripts/getHeatmap.py";
            string currentDir = Directory.GetCurrentDirectory();
            var gparent = Directory.GetParent(currentDir).Parent.Parent;
            string scriptPath = Path.Combine(gparent.FullName, @"Scripts\groupHeatmap.py");


            string vidPath = selectedVid;

            string args = textFile + " " + '"' + vidPath + '"' + " " + vidFileName;
            //runScript(scriptPath, args, destPath);
            Console.WriteLine("chosen args: " + args);
            await Task.Run(() => runScript(scriptPath, args, hmOutputPath));


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

        #region run cmd
        void runScript(string pythonScript, string args, string destFolder)
        {
            destFolder = '"' + destFolder + '"';
            Console.WriteLine(pythonPath, pythonScript + " " + args + " " + destFolder);
            Process p = new Process();
            //p.StartInfo = new ProcessStartInfo(exeRuntimeDirectory + @"\..\pyenv\python.exe", pythonScript + " " + args + " " + destFolder)
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
            if (SelectedCSVFile != null)
            {
                UserFileList.Remove(SelectedCSVFile);
            } 
            else
            {
                ShowDialog("No selected file to remove.", true);
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

        #region Show Help Dialog
        public ICommand HelpCommand { get; set; }

        private void ShowHelp()
        {
            var p = new DialogParameters();
            p.Add("type", "multiview");

            _dialogService.ShowDialog("HelpDialog", p, result =>
            {
                if (result.Result == ButtonResult.OK)
                {

                }
            });
        }

        #endregion

        private void clearProc()
        {
            inProcess(false);
            Output = "";
        }

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
            //Console.WriteLine(vidPath);
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
