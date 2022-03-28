using System.Windows;
using System.Windows.Controls;

namespace RecorderApp.Dialogs
{
    /// <summary>
    /// Interaction logic for ChartView
    /// </summary>
    public partial class ChartView : UserControl
    {
        public ChartView()
        {
            InitializeComponent();
            Loaded += ChartView_Loaded;
        }

        private void ChartView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((Window)Parent).ResizeMode = ResizeMode.NoResize;
        }
    }
}
