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
    public class RateChartViewModel : BindableBase, IDialogAware
    {


        //List<int> sceneNum = new List<int>();

        public RateChartViewModel()
        {
            this.CloseDialogCommand = new DelegateCommand(CloseDialog);
            OpenPathCommand = new DelegateCommand(OpenPath);


            this.SummaryModel = new PlotModel
            {
                Title = "Ratings Summary",
                Background = OxyColor.FromArgb(16, 255, 34, 38)
            };
            //tryRun();
        }


        private PlotModel summaryModel;
        public PlotModel SummaryModel
        {
            get { return summaryModel; }
            set { SetProperty(ref summaryModel, value); }
        }

        private PlotModel model;
        public PlotModel Model
        {
            get { return model; }
            set { SetProperty(ref model, value); }
        }

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

        private List<RatingSummary> _rateList;
        public List<RatingSummary> RateList
        {
            get { return _rateList; }
            set { SetProperty(ref _rateList, value); }
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
            RateList = parameters.GetValue<List<RatingSummary>>("rateList");

            createPlot();
            exportPng();
            //tryRun();

        }

        private void createPlot()
        {
            List<int> posCount = new List<int>();
            List<int> negCount = new List<int>();
            List<int> neutCount = new List<int>();
            List<double> pct = new List<double>();
            Console.WriteLine("umabot dito");
            int sceneCount = RateList.Count;
            foreach (RatingSummary obj in RateList)
            {
                posCount.Add(obj.positiveCount);
                negCount.Add(obj.negativeCount);
                neutCount.Add(obj.neutralCount);
                pct.Add(obj.accuracyPct);
            }

            var seriesA = new BarSeries()
            {
                Title = "Positive Count",
                LabelPlacement = LabelPlacement.Outside,
                StrokeColor = OxyColors.Black,
                FillColor = OxyColors.LightGreen,
                StrokeThickness = 1,
                XAxisKey = "Value",
                YAxisKey = "Category"
          
            };

            var seriesB = new BarSeries()
            {
                Title = "Negative Count",
                LabelPlacement = LabelPlacement.Outside,
                StrokeColor = OxyColors.Black,
                FillColor = OxyColors.Red,
                StrokeThickness = 1,
                XAxisKey = "Value",
                YAxisKey = "Category"
            };

            var seriesC = new BarSeries()
            {
                Title = "Neutral Count",
                LabelPlacement = LabelPlacement.Outside,
                StrokeColor = OxyColors.Black,
                FillColor = OxyColors.Yellow,
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


            for (int i = 0; i < sceneCount-1; i++)
            {
                seriesA.Items.Add(new BarItem { Value = posCount[i] });
                seriesB.Items.Add(new BarItem { Value = negCount[i] });
                seriesC.Items.Add(new BarItem { Value = neutCount[i] });
                categoryAxis.Labels.Add((i+1).ToString());
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

        private void tryRun()
        {
            Model = new PlotModel
            {
                Title = "BarSeries",
            };


            var rand = new Random();
            double[] cakePopularity = new double[5];
            for (int i = 0; i < 5; ++i)
            {
                cakePopularity[i] = rand.NextDouble();
            }
            var sum = cakePopularity.Sum();

            var s2 = new BarSeries { Title = "Series 1", StrokeColor = OxyColors.Black, StrokeThickness = 1 };
            s2.Items.Add(new BarItem { Value = (cakePopularity[0] / sum * 100) });
            s2.Items.Add(new BarItem { Value = (cakePopularity[1] / sum * 100) });
            s2.Items.Add(new BarItem { Value = (cakePopularity[2] / sum * 100) });
            s2.Items.Add(new BarItem { Value = (cakePopularity[3] / sum * 100) });
            s2.Items.Add(new BarItem { Value = (cakePopularity[4] / sum * 100) });

            var categoryAxis = new CategoryAxis { Position = AxisPosition.Left };
            categoryAxis.Labels.Add("Apple cake");
            categoryAxis.Labels.Add("Baumkuchen");
            categoryAxis.Labels.Add("Bundt Cake");
            categoryAxis.Labels.Add("Chocolate cake");
            categoryAxis.Labels.Add("Carrot cake");
            var valueAxis = new LinearAxis { Position = AxisPosition.Bottom, MinimumPadding = 0, MaximumPadding = 0.06, AbsoluteMinimum = 0 };
            Model.Series.Add(s2);
            //model.Series.Add(s2);
            Model.Axes.Add(categoryAxis);
            Model.Axes.Add(valueAxis);

        }
    }
}
