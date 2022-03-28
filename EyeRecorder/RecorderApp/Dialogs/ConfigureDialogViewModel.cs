using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RecorderApp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecorderApp.Dialogs
{
    public class ConfigureDialogViewModel : BindableBase, IDialogAware
    {
        IEventAggregator _ea;
        public ConfigureDialogViewModel(IEventAggregator ea)
        {
            _ea = ea;
            CloseDialogCommand = new DelegateCommand(CloseDialog);

            if (Properties.Settings.Default.ShowCursor_set == "show")
                CursorIsChecked = true;
            else
                CursorIsChecked = false;

            if (Properties.Settings.Default.GazeApp_Path != "")
            {
                PathToApp = Properties.Settings.Default.GazeApp_Path;
            }
            else
            {
                PathToApp = Properties.Settings.Default.GazeApp_default;
            }

            if (Properties.Settings.Default.Python_Path != "")
            {
                PathToPython = Properties.Settings.Default.Python_Path;
            }


        }


        public event Action<IDialogResult> RequestClose;

        private string _pathToApp;
        public string PathToApp
        {
            get { return _pathToApp; }
            set { SetProperty(ref _pathToApp, value); }
        }

        private bool cursorIsChecked;
        public bool CursorIsChecked
        {
            get { return cursorIsChecked; }
            set { SetProperty(ref cursorIsChecked, value); }
        }

        private void KeepAppPath()
        {

            _ea.GetEvent<SaveAppPathEvent>().Publish(PathToApp);
            _ea.GetEvent<SavePythonPathEvent>().Publish(PathToPython);
            //Console.WriteLine(PathToApp);
        }

        //python path
        private string _pathToPython;
        public string PathToPython
        {
            get { return _pathToPython; }
            set { SetProperty(ref _pathToPython, value); }
        }


        public string Title => "Eye Luminate";

        public DelegateCommand CloseDialogCommand { get; }
        public bool CanCloseDialog()
        {
            //throw new NotImplementedException();
            return true; 
        }
        private void CloseDialog()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        public void OnDialogClosed()
        {
            if (PathToApp != "")
            {
                Properties.Settings.Default.GazeApp_Path = PathToApp;
                Properties.Settings.Default.Python_Path = PathToPython;

                if (CursorIsChecked)
                {
                    Properties.Settings.Default.ShowCursor_set = "show";
                }
                else
                {
                    Properties.Settings.Default.ShowCursor_set = "hide";
                }
                Console.WriteLine(CursorIsChecked);

                Properties.Settings.Default.Save();
                Console.WriteLine(Properties.Settings.Default.GazeApp_default);
                Console.WriteLine(Properties.Settings.Default.GazeApp_Path);
                KeepAppPath();
            }
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            //throw new NotImplementedException();
        }
    }
}
