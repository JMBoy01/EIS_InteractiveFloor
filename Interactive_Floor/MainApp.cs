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
                    
                    if (calibrationPhase) calibration.ProcessSkeletonFrame(trackedSkeleton);
                    else game.ProcessSkeletonFrame(trackedSkeleton);
                }
            }
        }

        public void CollectCalibrationPoint()
        {
            calibration.collectCalibrationPoint();
        }
    }
}
