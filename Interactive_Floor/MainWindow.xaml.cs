using CameraViewWindow;
using Microsoft.Kinect;
using System;
using System.Windows;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    public partial class MainWindow : Window
    {
        private MainApp app;

        public MainWindow()
        {
            // InitializeComponent();
            app = new MainApp(); // Initialiseer de Kinect en kalibratieklasse
            
            CameraWindow cameraWindow = new CameraWindow(app.m_kinectSensor, app.GetCalibrationInstance());
            cameraWindow.Show(); // Het camera-venster wordt getoond
        }

        private void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            // Start kalibratie (optioneel)
            app.CollectCalibrationPoint();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (app != null && app.m_kinectSensor != null)
            {
                app.m_kinectSensor.Stop(); // Stop de Kinect-sensor bij sluiten van venster
            }
        }
    }
}
