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

        private Skeleton current_skeleton;
        public Game(KinectSensor kinect_sensor, Emgu.CV.Matrix<double> transform_matrix_cam2floor)
        {
            m_kinectSensor = kinect_sensor;
            m_transform = transform_matrix_cam2floor;
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
                return midpoint;
            }
            else
            {
                Console.WriteLine("Voeten zijn niet gedetecteerd.");
                return new SkeletonPoint(); // Geeft een leeg SkeletonPoint terug
            }
        }

        private Point KinectToPlane(SkeletonPoint point)
        {
            Console.WriteLine("TODO");
            // TODO implementeren (voorbeeld stappenplan, kan misschien ook anders)
            // Stap 1: Converteer skeleton punt naar homogeen punt
            // Stap 2: Bereken getransformeerd punt met homogeen punt en m_transform (member variabele)
            // Stap 3: Haal X en Y uit het getransformeerd punt en return
            return new Point();
        }

        public void ProcessSkeletonFrame(Skeleton tracked_skeleton)
        {
            current_skeleton = tracked_skeleton;
        }
    }
}