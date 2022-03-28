using System.Windows;
using System.Windows.Controls;

namespace RecorderApp.Dialogs
{
    /// <summary>
    /// Interaction logic for ConfigureDialog
    /// </summary>
    public partial class ConfigureDialog : UserControl
    {
        public ConfigureDialog()
        {
            InitializeComponent();
            Loaded += ConfigureDialog_Loaded;
        }

        private void ConfigureDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((Window)Parent).ResizeMode = ResizeMode.NoResize;
        }
    }
}
