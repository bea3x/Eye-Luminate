using System.Windows;
using System.Windows.Controls;

namespace RecorderApp.Dialogs
{
    /// <summary>
    /// Interaction logic for RateChartView
    /// </summary>
    public partial class RateChartView : UserControl
    {
        public RateChartView()
        {
            InitializeComponent();
            Loaded += RateChartView_Loaded;
        }

        private void RateChartView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((Window)Parent).ResizeMode = ResizeMode.NoResize;
        }
    }
}
