using RecorderApp.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System;
using RecorderApp.Dialogs;

namespace RecorderApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        private const int MINIMUM_SPLASH_TIME = 1500; // Miliseconds 
        private const int SPLASH_FADE_TIME = 500;     // Miliseconds 
        protected override Window CreateShell()
        {
            SplashScreen splash = new SplashScreen("/Assets/logo3.png");
            splash.Show(false, true);

            // Step 2 - Start a stop watch 
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // Step 3 - Load your windows but don't show it yet 
            //base.OnStartup(e);

            // Step 4 - Make sure that the splash screen lasts at least two seconds 
            timer.Stop();
            int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
            if (remainingTimeToShowSplash > 0)
                Thread.Sleep(remainingTimeToShowSplash);

            // Step 5 - show the page 
            splash.Close(TimeSpan.FromMilliseconds(SPLASH_FADE_TIME));
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // goes to GazeTrackerView for next command
            containerRegistry.Register<IView,GazeTrackerView>();
            // goes to ResultsView for next command
            containerRegistry.Register<IView2, ResultsView>();            
            // goes to QuickResultsView for next command
            containerRegistry.Register<IView3, QuickResultsView>();

            containerRegistry.Register<MainView, MainWindow>();

            // message dialog
            containerRegistry.RegisterDialog<MessageDialog, MessageDialogViewModel>();

        }
        
    }
}
