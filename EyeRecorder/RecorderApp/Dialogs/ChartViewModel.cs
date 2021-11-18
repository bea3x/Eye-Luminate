using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RecorderApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RecorderApp.Dialogs
{
    public class ChartViewModel : BindableBase, IDialogAware
    {
        public ChartViewModel()
        {
            this.CloseDialogCommand = new DelegateCommand(CloseDialog);
            OpenPathCommand = new DelegateCommand(OpenPath);

            this.SummaryModel = new PlotModel 
            { 
                Title = "Fixation Summary"
            };

        }

        public PlotModel SummaryModel { get; private set; }

        public string Title => "Eye Luminate";

        public event Action<IDialogResult> RequestClose;

        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private string _directory;
        public string Directory
        {
            get { return _directory; }
            set { SetProperty(ref _directory, value); }
        }

        private List<Variability> _fixList;
        public List<Variability> FixList
        {
            get { return _fixList; }
            set { SetProperty(ref _fixList, value); }
        }

        public DelegateCommand CloseDialogCommand { get; }

        public bool CanCloseDialog()
        {
            return true;
        }

        private void CloseDialog()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        public void OnDialogClosed()
        {
            //throw new NotImplementedException();
        }

        public DelegateCommand OpenPathCommand { get; }

        private void OpenPath()
        {
            CloseDialog();
            Process.Start(_directory);
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Message = parameters.GetValue<string>("message");
            Directory = parameters.GetValue<string>("path");
            FixList = parameters.GetValue<List<Variability>>("fixList");

            createPlot();
            exportPng();

        }

        private void createPlot()
        {
            List<double> durationLen = new List<double>();
            List<double> fixCount = new List<double>();
            List<double> stdev = new List<double>();
            List<double> pct = new List<double>();

            int sceneCount = FixList.Count;
            foreach (Variability obj in FixList)
            {
                durationLen.Add(obj.durationLen);
                fixCount.Add(obj.fixationCount);
                stdev.Add(obj.standardDev);
            }

            var seriesA = new LineSeries()
            {
                Title = "Duration Length",
                LineLegendPosition = LineLegendPosition.Start,
                Color = OxyColors.Blue,
                StrokeThickness = 1,
                XAxisKey = "Value",
                YAxisKey = "Category"

            };

            var seriesB = new LineSeries()
            {
                Title = "Fixation Count",
                LineLegendPosition = LineLegendPosition.Start,
                Color = OxyColors.Yellow,
                StrokeThickness = 1,
                XAxisKey = "Value",
                YAxisKey = "Category"
            };

            var seriesC = new LineSeries()
            {
                Title = "Standard Deviation",
                LineLegendPosition = LineLegendPosition.Start,
                Color = OxyColors.Violet,
                StrokeThickness = 1,
                XAxisKey = "Value",
                YAxisKey = "Category"
            };


            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "Category",
                Title = "Scene #"
            };


            for (int i = 0; i < sceneCount; i++)
            {
                seriesA.Points.Add(new DataPoint (durationLen[i], i + 1));
                seriesB.Points.Add(new DataPoint (fixCount[i], i + 1));
                seriesC.Points.Add(new DataPoint (stdev[i], i + 1));
                categoryAxis.Labels.Add((i + 1).ToString());
            }


            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Key = "Value",
                Title = "Frequency"
            };

            SummaryModel.Axes.Add(categoryAxis);
            SummaryModel.Axes.Add(valueAxis);

            //SummaryModel.Axes.Add(new OxyPlot.Axes.CategoryAxis());
            SummaryModel.Series.Add(seriesA);
            SummaryModel.Series.Add(seriesB);
            SummaryModel.Series.Add(seriesC);

            SummaryModel.InvalidatePlot(true);

        }

        private void exportPng()
        {
            var pngExporter = new PngExporter { Width = 600, Height = 400 };
            string fn = Message.Replace(".csv", ".png");
            string path = Directory + @"\" + fn;
            Console.WriteLine(path);
            pngExporter.ExportToFile(SummaryModel, path);
        }
    }
}
