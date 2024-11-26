using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Samples.Kinect.ControlsBasics;

namespace GameClass
{
    public class Ball
    {
        public event Action<Point> BallPositionUpdated;

        public Ellipse BallShape { get; private set; }
        private double ballRadius = 10;
        public double BallSpeedX { get; set; }
        public double BallSpeedY { get; set; }
        private double canvasWidth;
        private double canvasHeight;
        private double ballX;
        private double ballY;

        public Ball(double playFieldWidth, double playFieldHeight, double speedX, double speedY)
        {
            canvasWidth = playFieldWidth;
            canvasHeight = playFieldHeight;
            BallSpeedX = speedX;
            BallSpeedY = speedY;

            BallShape = new Ellipse
            {
                Width = ballRadius * 2,
                Height = ballRadius * 2,
                Fill = Brushes.Red // Example: set ball color to red
            };

            ResetBall();

            MainWindow.WindowSizeChanged += WindowSizeChanged;
        }

        public void ResetBall()
        {
            ballX = canvasWidth / 2 - ballRadius;
            ballY = canvasHeight / 2 - ballRadius;
            UpdatePosition(ballX, ballY);

            Random rand = new Random();
            // Minimum speed of 100 pixels per second, randomized in both X and Y directions
            BallSpeedX = rand.Next(150, 200) * (rand.Next(2) == 0 ? 1 : -1);  // Random direction
            BallSpeedY = rand.Next(150, 200) * (rand.Next(2) == 0 ? 1 : -1);  // Random direction
        }

        public void UpdatePosition(double deltaTime)
        {
            // Update ball position based on its speed and delta time
            ballX += BallSpeedX * deltaTime;
            ballY += BallSpeedY * deltaTime;

            // Check for wall collisions
            if (ballX <= 0 || ballX >= canvasWidth - ballRadius * 2)
            {
                BallSpeedX = -BallSpeedX;
                ballX = Clamp(ballX, 0, canvasWidth - ballRadius * 2);
            }

            if (ballY <= 0 || ballY >= canvasHeight - ballRadius * 2)
            {
                BallSpeedY = -BallSpeedY;
                ballY = Clamp(ballY, 0, canvasHeight - ballRadius * 2);
            }

            // Trigger position update event
            UpdatePosition(ballX, ballY);
        }

        private void UpdatePosition(double x, double y)
        {
            // Position updates are now sent via the event
            BallPositionUpdated?.Invoke(new Point(x, y));
        }

        public Rect GetBoundingBox()
        {
            return new Rect(ballX, ballY, BallShape.Width, BallShape.Height);
        }

        private double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        private void WindowSizeChanged(List<double> newDimensions)
        {
            canvasWidth = newDimensions[0];
            canvasHeight = newDimensions[1];
        }

    }
}
