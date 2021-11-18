using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecorderApp.Dialogs
{
    public class MessageDialogViewModel : BindableBase, IDialogAware
    {
        public MessageDialogViewModel()
        {
            CloseDialogCommand = new DelegateCommand(CloseDialog);
        }

        public string Title => "Eyeluminate";

        public event Action<IDialogResult> RequestClose;

        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private bool _errorIcon;
        public bool ErrorIcon
        {
            get { return _errorIcon; }
            set { SetProperty(ref _errorIcon, value); }
        }

        private bool _notifIcon;
        public bool NotifIcon
        {
            get { return _notifIcon; }
            set { SetProperty(ref _notifIcon, value); }
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

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Message = parameters.GetValue<string>("message");
            bool isError = parameters.GetValue<bool>("error");
            changeIcon(isError);
            
        }

        private void changeIcon(bool isError)
        {
            if (isError)
            {
                ErrorIcon = true;
                NotifIcon = false;
            }
            else
            {
                NotifIcon = true;
                ErrorIcon = false;
            }
        }
    }
}
