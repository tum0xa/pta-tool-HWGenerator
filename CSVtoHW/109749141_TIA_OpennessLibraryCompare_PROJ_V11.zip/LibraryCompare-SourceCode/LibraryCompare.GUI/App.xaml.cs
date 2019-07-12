using System;
using System.Windows;
using System.Windows.Media.Imaging;
using LibraryCompare.Core;
using LibraryCompare.Core.Interfaces;
using LibraryCompare.GUI.Views;
using Microsoft.Practices.Unity;

namespace LibraryCompare.GUI
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly IUnityContainer _container;

        /// <summary>
        ///     Construct the application
        /// </summary>
        public App()
        {
            Resolver.AssemblyResolve();
            _container = CreateContainer();
        }

        /// <summary>
        ///     Startup event
        /// </summary>
        /// <param name="e">Event data</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = _container.Resolve<ShellView>();

            // Show the main window
            MainWindow.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Icons/Openness.ico",
                UriKind.Absolute));
            MainWindow.Show();
        }

        /// <summary>
        ///     Exit event
        /// </summary>
        /// <param name="e">Event data</param>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _container.Dispose();
        }

        private static IUnityContainer CreateContainer()
        {
            UnityContainer testContainer = null;
            UnityContainer container;

            try
            {
                testContainer = new UnityContainer();

                // Register the implementations that have to be used for the matching interfaces
                testContainer.RegisterType(typeof(IOpenness), typeof(Openness.Openness), new ContainerControlledLifetimeManager());

                container = testContainer;
                testContainer = null;
            }
            finally
            {
                if (testContainer != null)
                    testContainer.Dispose();
            }

            return container;
        }
    }
}