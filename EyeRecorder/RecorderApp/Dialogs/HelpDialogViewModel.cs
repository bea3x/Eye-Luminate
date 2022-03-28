using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace RecorderApp.Dialogs
{
    public class HelpDialogViewModel : BindableBase, IDialogAware
    {
        public HelpDialogViewModel()
        {
            //SetMessages();

            this.CloseDialogCommand = new DelegateCommand(CloseDialog);
            
        }

        public string Title => "Help";
        private string _ratingMsg;
        public string RatingsMsg
        {
            get { return _ratingMsg; }
            set { SetProperty(ref _ratingMsg, value); }
        }

        private string _fixationMsg;
        public string FixationMsg
        {
            get { return _fixationMsg; }
            set { SetProperty(ref _fixationMsg, value); }
        }

        private string _heatmapMsg;
        public string HeatmapMsg
        {
            get { return _heatmapMsg; }
            set { SetProperty(ref _heatmapMsg, value); }
        }

        private string type { get; set; }

        private void SetMessages()
        {
            RatingsMsg = "Generate Ratings Summary: Add 'rating.csv' type of files";
            FixationMsg = "Generate Fixations Summary: Add 'fixGrouped.csv' files";
            HeatmapMsg = "Generate Heatmap: Add 'fixGrouped.csv' files";
        }
        private void SetMessages2()
        {
            RatingsMsg = "Clean Data: Select GazeTrackingOutput file\n" + "Perform IVT: Select output from Clean Data";
            FixationMsg = "Extract Frames: Select '_finalGazeData.csv' file (output from IVT)";
            HeatmapMsg = "Fixation Map/Heatmap: Select a '_fixations.csv' file and video";
        }
        public event Action<IDialogResult> RequestClose;

        public DelegateCommand CloseDialogCommand { get; }

        public bool CanCloseDialog()
        {
            return true;
        }

        private void CloseDialog()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            type = parameters.GetValue<string>("type");

            if (type == "multiview")
            {
                Console.WriteLine("multi view");
                SetMessages();
            }
            else if (type == "resview")
            {
                SetMessages2();
            }
            //throw new NotImplementedException();
        }

        public void OnDialogClosed()
        {
            //throw new NotImplementedException();
        }
    }
}
