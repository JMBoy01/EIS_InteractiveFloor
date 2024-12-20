using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using KinectSimpleGesture;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.ControlsBasics;

namespace GameClass
{
    public class Game
    {
        public static event Action<List<Point>,List<double>> PlayerPositionsUpdated;
        public static event Action<int, int> ScoresUpdated;
        public static event Action<Point> BallPositionUpdated;

        private KinectSensor m_kinectSensor = null;
        private Emgu.CV.Matrix<double> m_transform;
        private Matrix3D m_groundPlaneTransform;

        private Dictionary<int, Skeleton> trackedSkeletons = new Dictionary<int, Skeleton>();

        private double canvasWidth = 400;
        private double canvasHeight = 200;
        private Ball ball;
        private int player1Score = 0;
        private int player2Score = 0;

        private Point? player1Position = null;
        private Point? player2Position = null;
        List<double> feetAngles = new List<double>();

        private TPoseGesture tPoseGesture;
        private RaiseHandGesture raiseHandGesture;

        public Game(KinectSensor m_kinectSensor, Emgu.CV.Matrix<double> transform_matrix_cam2floor, Matrix3D m_groundPlaneTransform)
        {
            this.m_kinectSensor = m_kinectSensor;
            m_transform = transform_matrix_cam2floor;
            this.m_groundPlaneTransform = m_groundPlaneTransform;

            tPoseGesture = new TPoseGesture();
            raiseHandGesture = new RaiseHandGesture();

            tPoseGesture.GestureRecognized += OnTPoseGestureRecognized;
            raiseHandGesture.GestureRecognized += OnRaiseHandGestureRecognized;

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            canvasWidth = mainWindow.GetWindowDimensions()[0];
            canvasHeight = mainWindow.GetWindowDimensions()[1];

            InitializeKinect();
            InitializeBall();

            MainWindow.WindowSizeChanged += WindowSizeChanged;
        }

        private void InitializeKinect()
        {
            m_kinectSensor.SkeletonStream.Enable();
            m_kinectSensor.SkeletonFrameReady += KinectSkeletonFrameReady;
            m_kinectSensor.Start();
        }

        private void InitializeBall()
        {
            ball = new Ball(canvasWidth, canvasHeight,1,1);
            ball.BallPositionUpdated += OnBallPositionUpdated;
        }

        private void OnBallPositionUpdated(Point position)
        {
            BallPositionUpdated?.Invoke(position);
        }

        // Deze word elke keer gecalled bij een nieuwe skeletonFrame.
         public void GameLoop()
        {
            double deltaTime = 0.016; // Assuming 60 FPS
            ball.UpdatePosition(deltaTime);
            CheckBallCollisions();
        }


        private SkeletonPoint GetMidpointBetweenFeet(Skeleton skeleton)
        {
            Joint leftFoot = skeleton.Joints[JointType.FootLeft];
            Joint rightFoot = skeleton.Joints[JointType.FootRight];

            if (leftFoot.TrackingState == JointTrackingState.Tracked && rightFoot.TrackingState == JointTrackingState.Tracked)
            {
                SkeletonPoint midpoint = new SkeletonPoint
                {
                    X = (leftFoot.Position.X + rightFoot.Position.X) / 2,
                    Y = (leftFoot.Position.Y + rightFoot.Position.Y) / 2,
                    Z = (leftFoot.Position.Z + rightFoot.Position.Z) / 2
                };
                //Console.WriteLine("Voeten zijn WEL gedetecteerd.");
                return midpoint;
            }
            else
            {
                //Console.WriteLine("Voeten zijn niet gedetecteerd.");
                return new SkeletonPoint(); // Geeft een leeg SkeletonPoint terug
            }
        }

        private Point ConvertPointKinectToScreen(SkeletonPoint skeleton_point)
        {
            // TODO implementeren (voorbeeld stappenplan, kan misschien ook anders)
            // Stap 0: Map skeleton punt op diepte punt in kinect beeld
            Point3D depth_point = conertSkeletonPointToDepthPoint(skeleton_point);

            // Stap 1: Converteer diepte punt naar homogeen punt -> WEET NIET ALS HET NODIG IS
            // Point4D hom_depth_point = new Point4D(depth_point.X, depth_point.Y, depth_point.Z, 1);

            // Stap 2: Transformeer het punt naar het grondvlak met m_groundPlaneTransform
            Point3D plane_point = m_groundPlaneTransform.Transform(depth_point);

            // Stap 3: Transformeer punt op grondvlak naar punt op scherm met m_transform
            System.Drawing.PointF[] src_point = { new System.Drawing.PointF((float)plane_point.X, (float)plane_point.Y) };
            System.Drawing.PointF dest_point = Emgu.CV.CvInvoke.PerspectiveTransform(src_point, m_transform)[0];

            // Stap 4: Haal X en Y uit het getransformeerd punt en return
            return new Point(dest_point.X, dest_point.Y);
        }

        private Point3D conertSkeletonPointToDepthPoint(SkeletonPoint skeletonPoint)
        {
            DepthImagePoint imgPt = m_kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);

            return new Point3D(imgPt.X, imgPt.Y, imgPt.Depth);
        }

        private void KinectSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    Skeleton[] skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);

                    List<SkeletonPoint> midPoints = new List<SkeletonPoint>();
                    List<Point> pointsOnPlane = new List<Point>();
                    feetAngles.Clear();

                    // Clear the trackedSkeletons dictionary
                    trackedSkeletons.Clear();

                    foreach (var skeleton in skeletons)
                    {
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            trackedSkeletons[skeleton.TrackingId] = skeleton;

                            // Get midpoint between feet
                            SkeletonPoint midPoint = GetMidpointBetweenFeet(skeleton);
                            Point pointOnPlane = ConvertPointKinectToScreen(midPoint);

                            // Calculate the torso angle based on shoulder positions
                            var leftFoot = skeleton.Joints[JointType.FootLeft].Position;
                            var rightFoot = skeleton.Joints[JointType.FootRight].Position;

                            double feetAngle = Math.Atan2(rightFoot.Y - leftFoot.Y, rightFoot.X - leftFoot.X);

                            // Add calculated torso angle and position to the lists
                            midPoints.Add(midPoint);
                            pointsOnPlane.Add(pointOnPlane);
                            feetAngles.Add(feetAngle); // Store torso angle

                            tPoseGesture.Update(skeleton);
                            raiseHandGesture.Update(skeleton);
                        }

                        if (trackedSkeletons.Count >= 2)
                            break;
                    }

                    if (trackedSkeletons.Count > 0)
                    {
                        player1Position = pointsOnPlane[0];
                        if(trackedSkeletons.Count == 2)
                        {
                            player2Position = pointsOnPlane[1];
                        }
                        
                        // Pass midpoints, points on screen, and torso angles to the event
                        PlayerPositionsUpdated?.Invoke(pointsOnPlane, feetAngles);
                    }

                    GameLoop();
                }
            }
        }

        private void CheckBallCollisions()
        {

            // Assuming ballRect represents the ball's bounding box and player positions are defined.
            Rect ballRect = ball.GetBoundingBox();

            // Check for player1 collision with the line
            if (player1Position.HasValue && feetAngles.Count > 0)
            {
                double player1Angle = feetAngles[0];
                Point player1Midpoint = player1Position.Value;

                // Define line direction (40 units each side from the midpoint)
                double lineLength = 40;
                Point leftLineEnd = new Point(
                    player1Midpoint.X - Math.Cos(player1Angle) * lineLength,
                    player1Midpoint.Y - Math.Sin(player1Angle) * lineLength
                );
                Point rightLineEnd = new Point(
                    player1Midpoint.X + Math.Cos(player1Angle) * lineLength,
                    player1Midpoint.Y + Math.Sin(player1Angle) * lineLength
                );

                // Check for collision with left or right line
                if (IsBallIntersectingWithLine(ballRect, leftLineEnd, rightLineEnd))
                {
                    ball.BallSpeedX = -ball.BallSpeedX;
                }
            }

            // Check for player2 collision with the line
            if (player2Position.HasValue && feetAngles.Count > 1)
            {
                double player2Angle = feetAngles[1];
                Point player2Midpoint = player2Position.Value;

                // Define line direction (40 units each side from the midpoint)
                double lineLength = 40;
                Point leftLineEnd = new Point(
                    player2Midpoint.X - Math.Cos(player2Angle) * lineLength,
                    player2Midpoint.Y - Math.Sin(player2Angle) * lineLength
                );
                Point rightLineEnd = new Point(
                    player2Midpoint.X + Math.Cos(player2Angle) * lineLength,
                    player2Midpoint.Y + Math.Sin(player2Angle) * lineLength
                );

                // Check for collision with left or right line
                if (IsBallIntersectingWithLine(ballRect, leftLineEnd, rightLineEnd))
                {
                    ball.BallSpeedX = -ball.BallSpeedX;
                }
            }

            // Check if ball passed the top or bottom
            if (ballRect.Left  <= 0)
            {
                player2Score++;
                ScoresUpdated?.Invoke(player1Score, player2Score);
                ball.ResetBall();
            }
            else if (ballRect.Right >= canvasWidth)
            {
                player1Score++;
                ScoresUpdated?.Invoke(player1Score, player2Score);
                ball.ResetBall();
            }
        }

        private bool IsBallIntersectingWithLine(Rect ballRect, Point lineStart, Point lineEnd)
        {
            LineGeometry lineGeometry = new LineGeometry(lineStart, lineEnd);
            EllipseGeometry ballGeometry = new EllipseGeometry(new Point(ballRect.Left + ballRect.Width / 2, ballRect.Top + ballRect.Height / 2), ballRect.Width / 2, ballRect.Height / 2);
            return ballGeometry.Bounds.IntersectsWith(lineGeometry.Bounds);
        }

        private void OnTPoseGestureRecognized(object sender, EventArgs e)
        {
            Console.WriteLine("TPose recognised");
            ResetScores();
        }


        private void OnRaiseHandGestureRecognized(object sender, EventArgs e)
        {
            Console.WriteLine("Raise Hand Gesture Recognized");
            StopGame();
        }

        public void StopGame()
        {
            m_kinectSensor.SkeletonFrameReady -= KinectSkeletonFrameReady;
            m_kinectSensor.SkeletonStream.Disable();
        }

        public void ResetScores(){
            player1Score = 0;
            player2Score = 0;
        }

        private void WindowSizeChanged(List<double> newDimensions)
        {
            Console.WriteLine("window size changed");
            canvasWidth = newDimensions[0];
            canvasHeight = newDimensions[1];
        }
    }
}