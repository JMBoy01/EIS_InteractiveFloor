using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;

namespace GameClass
{
    public class Game
    {
        public static event Action<List<Point>> PlayerPositionsUpdated;

        private KinectSensor m_kinectSensor = null;
        private Emgu.CV.Matrix<double> m_transform;
        private Matrix3D m_groundPlaneTransform;

        public Game(KinectSensor m_kinectSensor, Emgu.CV.Matrix<double> transform_matrix_cam2floor, Matrix3D m_groundPlaneTransform)
        {
            this.m_kinectSensor = m_kinectSensor;
            m_transform = transform_matrix_cam2floor;
            this.m_groundPlaneTransform = m_groundPlaneTransform;

            InitializeKinect();
        }

        private void InitializeKinect()
        {
            m_kinectSensor.SkeletonStream.Enable();
            m_kinectSensor.SkeletonFrameReady += KinectSkeletonFrameReady;
            m_kinectSensor.Start();
        }

        // Deze word elke keer gecalled bij een nieuwe skeletonFrame.
        public void GameLoop()
        {
            Console.WriteLine("TODO: make game loop");
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
                Console.WriteLine("Voeten zijn WEL gedetecteerd.");
                return midpoint;
            }
            else
            {
                Console.WriteLine("Voeten zijn niet gedetecteerd.");
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
                    // DIT IS OOK ONZE GAME LOOP
                    Skeleton[] skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    // current_skeletons = skeletons;

                    List<SkeletonPoint> midPoints = new List<SkeletonPoint>{};
                    List<Point> pointsOnPlane = new List<Point>{};
                    foreach (var skeleton in skeletons) {
                        SkeletonPoint midPoint = GetMidpointBetweenFeet(skeleton);
                        Point pointOnPlane = ConvertPointKinectToScreen(midPoint);

                        midPoints.Add(midPoint);
                        pointsOnPlane.Add(pointOnPlane);
                    }

                    PlayerPositionsUpdated?.Invoke(pointsOnPlane);

                    GameLoop();
                }
            }
        }

        public void StopGame()
        {
            m_kinectSensor.SkeletonFrameReady -= KinectSkeletonFrameReady;
            m_kinectSensor.SkeletonStream.Disable();
        }
    }
}