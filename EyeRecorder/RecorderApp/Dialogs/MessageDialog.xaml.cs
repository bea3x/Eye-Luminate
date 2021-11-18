using System.Windows;
using System.Windows.Controls;

namespace RecorderApp.Dialogs
{
    /// <summary>
    /// Interaction logic for MessageDialog
    /// </summary>
    public partial class MessageDialog : UserControl
    {
        public MessageDialog()
        {
            InitializeComponent();
            Loaded += MessageDialog_Loaded;
        }

        private void MessageDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((Window)Parent).ResizeMode = ResizeMode.NoResize;
        }
    }
}
