using Microsoft.Kinect;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CameraViewWindow
{
    public partial class CameraWindow : Window
    {
        private KinectSensor _kinectSensor;  // Kinect sensor
        private ColorImageStream _colorStream;    // Kleurstream

        public CameraWindow(KinectSensor kinectSensor)
        {
            InitializeComponent(); // Laad de UI xaml file in
            _kinectSensor = kinectSensor;
            InitializeKinect(); // Zorg ervoor dat de Kinect goed wordt ingesteld
        }

        private void InitializeKinect()
        {
            if (_kinectSensor == null)
            {
                MessageBox.Show("Kinect-sensor niet gevonden.");
                return;
            }

            // Zorg ervoor dat de kleurstream is geconfigureerd en ingeschakeld
            _colorStream = _kinectSensor.ColorStream;
            _colorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);  // Configureer kleurstream voor 640x480, 30 FPS

            // Abonneer je op het ColorFrameReady-event
            _kinectSensor.ColorFrameReady += KinectSensor_ColorFrameReady;

            // Start de Kinect sensor
            _kinectSensor.Start();
        }

        private void KinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            // Verkrijg het kleurframe
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Verkrijg de pixels van het kleurframe
                    byte[] pixelData = new byte[colorFrame.PixelDataLength];
                    colorFrame.CopyPixelDataTo(pixelData);

                    // Zet de pixeldata om naar een WriteableBitmap
                    WriteableBitmap writeableBitmap = new WriteableBitmap(
                        colorFrame.Width, 
                        colorFrame.Height, 
                        96.0, 
                        96.0, 
                        System.Windows.Media.PixelFormats.Bgr32, 
                        null);

                    writeableBitmap.WritePixels(
                        new System.Windows.Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                        pixelData,
                        colorFrame.Width * 4,  // Omdat elke pixel 4 bytes (BGRA) is
                        0);

                    KinectImage.Source = writeableBitmap; // Image object in xaml waarde naartoe schrijven
                }
            }
        }

        // Stop de Kinect-sensor en de event-handler wanneer het venster wordt gesloten
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Stop de Kinect-sensor en de event-handler
            if (_kinectSensor != null && _kinectSensor.IsRunning)
            {
                _kinectSensor.ColorFrameReady -= KinectSensor_ColorFrameReady;  // Verwijder de event-handler
                _colorStream.Disable();  // Zet de kleurstream uit
                _kinectSensor.Stop();  // Stop de Kinect-sensor
            }
        }
    }
}
