using Microsoft.Kinect;
using System;
using System.Linq;
using System.Windows;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    public class MainApp
    {
        public KinectSensor m_kinectSensor = null;
        private PartialCalibrationClass calibration;

        public MainApp()
        {
            InitializeKinect();
            calibration = new PartialCalibrationClass(m_kinectSensor);
            // LoadCalibrationPoints("calibrationPoints.xml"); DOET NOG NIKS
        }

        private void InitializeKinect()
        {
            m_kinectSensor = KinectSensor.KinectSensors.FirstOrDefault(sensor => sensor.Status == KinectStatus.Connected);

            if (m_kinectSensor != null)
            {
                m_kinectSensor.SkeletonStream.Enable();
                m_kinectSensor.Start();
                m_kinectSensor.SkeletonFrameReady += KinectSkeletonFrameReady;
            }
            else
            {
                MessageBox.Show("Geen Kinect-sensor aangesloten.");
            }
        }

        private void KinectSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    Skeleton[] skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    Skeleton trackedSkeleton = skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);

                    calibration.ProcessSkeletonFrame(trackedSkeleton);
                }
            }
        }

        public void CollectCalibrationPoint()
        {
            // Start de kalibratieprocedure in de PartialCalibrationClass
            calibration.collectCalibrationPoint();
        }

        private void LoadCalibrationPoints(string fileName)
        {
            // Logica om kalibratiepunten vanuit XML te laden
        }

        public PartialCalibrationClass GetCalibrationInstance()
        {
            return calibration;
        }
    }
}
