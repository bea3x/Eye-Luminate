using System.Windows;
using System.Windows.Controls;

namespace RecorderApp.Dialogs
{
    /// <summary>
    /// Interaction logic for HelpDialog
    /// </summary>
    public partial class HelpDialog : UserControl
    {
        public HelpDialog()
        {
            InitializeComponent();
            Loaded += HelpDialog_Loaded;
        }

        private void HelpDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((Window)Parent).ResizeMode = ResizeMode.NoResize;
        }
    }
}
