using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RecorderApp.Dialogs
{
    public class NotifDialogViewModel : BindableBase, IDialogAware
    {
        public NotifDialogViewModel()
        {
            this.CloseDialogCommand = new DelegateCommand(CloseDialog);
            OpenPathCommand = new DelegateCommand(OpenPath);
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

        private bool _pathExists;
        public bool PathExists
        {
            get { return _pathExists; }
            set { SetProperty(ref _pathExists, value); }
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

            if (Directory == "")
            {
                PathExists = false;
            }
            else
            {
                PathExists = true;
            }
        }

    }
}
