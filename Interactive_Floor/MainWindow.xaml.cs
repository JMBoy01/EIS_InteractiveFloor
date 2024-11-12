using CameraViewWindow;
using GameClass;
using Microsoft.Kinect;
using System;
using System.Windows;
using System.Windows.Controls;

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

            PartialCalibrationClass.VisualizationPointsUpdated += OnVisualizationPointsUpdated;
            Game.PlayerPositionsUpdated += OnPlayerPositionsUpdated;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Ratio behouden van 1:1 (of een andere verhouding die je wilt)
            double ratio = 9.0/16.0; // bijvoorbeeld 1:1 verhouding, dus breedte en hoogte gelijk
            double newWidth = e.NewSize.Width * 0.9; // Dit bepaalt de maximale breedte van de rechthoek
            double newHeight = newWidth * ratio; // Hoogte gebaseerd op de breedte met de ratio

            // Pas de nieuwe breedte en hoogte van de rechthoek aan
            CalibFieldRect.Width = newWidth;
            CalibFieldRect.Height = newHeight;

            PlayFieldRect.Width = newWidth;
            PlayFieldRect.Height = newHeight;
        }

        private void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            // Verkrijg de absolute positionering ten opzichte van het gehele window
            double circle_center_offset_x = CalibratePosCircle.Width / 2 + CalibratePosCircle.Margin.Left;
            double circle_center_offset_y = CalibratePosCircle.Height / 2 + CalibratePosCircle.Margin.Top;
            Point top_left_circle_window_pos = CalibratePosCircle.TransformToAncestor(this).Transform(new Point(0, 0));
            Point circle_window_pos = new Point(top_left_circle_window_pos.X + circle_center_offset_x, top_left_circle_window_pos.Y + circle_center_offset_y);
            app.CollectCalibrationPoint(circle_window_pos);
        }

        private void OnVisualizationPointsUpdated(List<Point> points)
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

        private void OnPlayerPositionsUpdated(List<Point> player_positions)
        {
            if(app.GetCalibrationPhase()) {
                app.SetCalibrationPhase(false);

                // Enable and disable elements to switch from calibration to game UI 
                SwitchUICalibrateTOGame();
            }

            double rect_offset_x = this.ActualWidth / 2 - PlayFieldRect.Width / 2;
            double rect_offset_y = this.ActualHeight / 2 - PlayFieldRect.Height / 2;

            // TODO set position of circles
            Canvas.SetLeft(PlayerPos1, (int)(player_positions[0].X - rect_offset_x));
            Canvas.SetTop(PlayerPos1, (int)(player_positions[0].Y - rect_offset_y));

            Canvas.SetLeft(PlayerPos2, (int)(player_positions[1].X - rect_offset_x));
            Canvas.SetTop(PlayerPos2, (int)(player_positions[1].Y - rect_offset_y));
        }

        private void SwitchUICalibrateTOGame()
        {   
            // Make calibration grid invisable
            CalibFieldRect.Visibility = Visibility.Collapsed;

            // Make canvas of position circles visable
            PlayFieldRect.Visibility = Visibility.Visible;
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
