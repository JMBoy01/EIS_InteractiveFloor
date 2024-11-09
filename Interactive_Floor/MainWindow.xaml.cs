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
            InitializeComponent();
            this.SizeChanged += Window_SizeChanged; // Setup on window change event

            app = new MainApp(); // Initialiseer de Kinect en kalibratieklasse
            
            CameraWindow cameraWindow = new CameraWindow(app.m_kinectSensor);
            cameraWindow.Show(); // Het camera-venster wordt getoond

            PartialCalibrationClass.CalibrationPointsUpdated += OnCalibrationPointsUpdated;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Ratio behouden van 1:1 (of een andere verhouding die je wilt)
            double ratio = 9.0/16.0; // bijvoorbeeld 1:1 verhouding, dus breedte en hoogte gelijk
            double newWidth = e.NewSize.Width * 0.9; // Dit bepaalt de maximale breedte van de rechthoek
            double newHeight = newWidth * ratio; // Hoogte gebaseerd op de breedte met de ratio

            // Pas de nieuwe breedte en hoogte van de rechthoek aan
            PlayFieldRect.Width = newWidth;
            PlayFieldRect.Height = newHeight;
        }

        private void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            // Start kalibratie (optioneel)
            app.CollectCalibrationPoint();
        }

        private void OnCalibrationPointsUpdated(List<Point> points)
        {
            HorizontalAlignment horizontal = HorizontalAlignment.Left;
            VerticalAlignment vertical = VerticalAlignment.Top;

            if (points.Count == 1) {
                horizontal = HorizontalAlignment.Right;
            }
            else if (points.Count == 2) {
                horizontal = HorizontalAlignment.Right;
                vertical = VerticalAlignment.Bottom;
            }
            else if (points.Count == 3) {
                vertical = VerticalAlignment.Bottom;
            }

            CalibratePosCircle.HorizontalAlignment = horizontal;
            CalibratePosCircle.VerticalAlignment = vertical;
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
