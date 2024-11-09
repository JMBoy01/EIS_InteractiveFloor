using Microsoft.Kinect;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using Microsoft.Samples.Kinect.ControlsBasics;
using Emgu.CV.Cuda;

namespace CameraViewWindow
{
    public partial class CameraWindow : Window
    {
        private KinectSensor _kinectSensor;  // Kinect sensor
        private ColorImageStream _colorStream;    // Kleurstream

        private List<System.Windows.Point> _points = new List<System.Windows.Point>();

        public CameraWindow(KinectSensor kinectSensor, PartialCalibrationClass calibration)
        {
            InitializeComponent(); // Laad de UI xaml file in
            _kinectSensor = kinectSensor;
            InitializeKinect(); // Zorg ervoor dat de Kinect goed wordt ingesteld

            calibration.CalibrationPointsUpdated += OnCalibrationPointsUpdated;
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

                    // TODO get points
                    DrawPointsOnImage(writeableBitmap, _points);

                    KinectImage.Source = writeableBitmap; // Image object in xaml waarde naartoe schrijven
                }
            }
        }

        private void DrawPointsOnImage(WriteableBitmap bitmap, List<System.Windows.Point> points)
        {
            // Maak een visuele laag om te tekenen
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawImage(bitmap, new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                for (int i = 0; i < points.Count; i++)
                {
                    // Teken een cirkel voor elke punt
                    dc.DrawEllipse(System.Windows.Media.Brushes.Red, null, points[i], 5, 5);  // 5 is de straal

                    var point0 = points[i];
                    var point1 = points[0];
                    if (i != points.Count - 1 && points.Count > 1) {
                        point1 = points[i+1];
                    }

                    var pen = new System.Windows.Media.Pen();
                    pen.Brush = System.Windows.Media.Brushes.Red;
                    pen.Thickness = 2;

                    dc.DrawLine(pen, point0, point1);
                }
            }

            // Voeg de visuele laag toe aan de bitmap
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                bitmap.PixelWidth,
                bitmap.PixelHeight,
                bitmap.DpiX,
                bitmap.DpiY,
                PixelFormats.Pbgra32);

            renderBitmap.Render(visual);

            // Kopieer de pixels van de RenderTargetBitmap naar de WriteableBitmap
            bitmap.Lock();
            renderBitmap.CopyPixels(
                new Int32Rect(0, 0, renderBitmap.PixelWidth, renderBitmap.PixelHeight),
                bitmap.BackBuffer,
                bitmap.BackBufferStride * renderBitmap.PixelHeight,
                bitmap.BackBufferStride);
            bitmap.Unlock();
        }

        public static WriteableBitmap ConvertToWriteableBitmap(Image<Bgr, byte> image)
        {
            // Zorg dat de afmetingen en het format kloppen
            WriteableBitmap writeableBitmap = new WriteableBitmap(
                image.Width,
                image.Height,
                96, // DPI-X
                96, // DPI-Y
                PixelFormats.Bgr24, // Zorg voor hetzelfde format als Bgr
                null);

            // Converteer de afbeelding van Emgu CV naar een array van bytes
            byte[] pixelData = image.Bytes;

            // Schrijf de pixeldata naar het WriteableBitmap
            writeableBitmap.WritePixels(
                new Int32Rect(0, 0, image.Width, image.Height),
                pixelData,
                image.Width * 3, // Omdat Bgr24 3 bytes per pixel gebruikt
                0);

            return writeableBitmap;
        }

        private void OnCalibrationPointsUpdated(List<System.Windows.Point> calibrationPoints)
        {
            _points = calibrationPoints;
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
