using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net.Sockets;
using System.Net;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace AlarmDeactivate
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }

	        try
	        {
		        var vcd = await Package.Current.InstalledLocation.GetFileAsync("VoiceCommands.xml");
		        await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcd);
	        }
	        catch (Exception ex)
	        {
		        Debug.WriteLine(ex.Message);
	        }
		}

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

	    protected override void OnActivated(IActivatedEventArgs args)
	    {
		    base.OnActivated(args);
		    if (args.Kind != ActivationKind.VoiceCommand)
		    {
			    return;
		    }
		    Frame frame = Window.Current.Content as Frame;

		    // When a Voice Command activates the app, this method is going to 
		    // be called and OnLaunched is not. Because of that we need similar
		    // code to the code we have in OnLaunched
		    Frame rootFrame = Window.Current.Content as Frame;

		    if (rootFrame == null)
		    {
			    rootFrame = new Frame();
			    rootFrame.CacheSize = 1;
			    Window.Current.Content = rootFrame;
		    }

		    if (rootFrame.Content == null)
		    {

			    // When the navigation stack isn't restored, navigate to the  
			    // first page, configuring the new page by passing required  
			    // information as a navigation parameter 

			    rootFrame.Navigate(typeof(MainPage), null);
		    }

		    Window.Current.Activate();

		    var voiceArgs = args as VoiceCommandActivatedEventArgs;
		    SpeechRecognitionResult speechRecognitionResult = voiceArgs.Result;

		    string commandName = speechRecognitionResult.RulePath[0];
		    string spokenText = speechRecognitionResult.Text;
		    MainPage page = rootFrame.Content as MainPage;

		    switch (commandName)
		    {
			    case "alarmOff":
				    page?.StopAlarm();
				    break;
		    }

	    }
	}
}
