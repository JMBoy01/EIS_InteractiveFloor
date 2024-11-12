using Microsoft.Kinect;
using System;
using System.Linq;
using System.Windows;
using GameClass;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    public class MainApp
    {
        public KinectSensor m_kinectSensor = null;
        private PartialCalibrationClass calibration;
        private Game game;

        private bool calibrationPhase = true;

        public MainApp()
        {
            InitializeKinect();
            calibration = new PartialCalibrationClass(m_kinectSensor);
        }

        private void InitializeKinect()
        {
            m_kinectSensor = KinectSensor.KinectSensors.FirstOrDefault(sensor => sensor.Status == KinectStatus.Connected);

            if (m_kinectSensor != null)
            {
                m_kinectSensor.Start();
            }
            else
            {
                MessageBox.Show("Geen Kinect-sensor aangesloten.");
            }
        }

        public void CollectCalibrationPoint(Point point_2D)
        {
            calibration.collectCalibrationPoint(point_2D);

            // Check als de calibratie voltooid is.
            var transform = calibration.GetTransform();
            if (transform != null) {
                // calibrationPhase = false;

                var groundPlaneTransform = calibration.GetGroundPlaneTransform();
                game = new Game(m_kinectSensor, transform, groundPlaneTransform);
            }
        }

        public bool GetCalibrationPhase()
        {
            return calibrationPhase;
        }

        public void SetCalibrationPhase(bool calibrationPhase)
        {
            this.calibrationPhase = calibrationPhase;
        }
    }
}
