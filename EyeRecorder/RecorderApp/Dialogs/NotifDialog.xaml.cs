using System.Windows;
using System.Windows.Controls;

namespace RecorderApp.Dialogs
{
    /// <summary>
    /// Interaction logic for NotifDialog
    /// </summary>
    public partial class NotifDialog : UserControl
    {
        public NotifDialog()
        {
            InitializeComponent();
            Loaded += NotifDialog_Loaded;
        }

        private void NotifDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((Window)Parent).ResizeMode = ResizeMode.NoResize;
        }
    }
}
