using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;

//shared with students

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    public class PartialCalibrationClass
    {
        public static event Action<List<Point>> CalibrationPointsUpdated;

        private KinectSensor m_kinectSensor = null;

        private List<Point> m_calibPoints = new List<Point>(); //2d calibration points
        private List<SkeletonPoint> m_skeletonCalibPoints = new List<SkeletonPoint>(); //3d skeleton points

        private Matrix3D m_groundPlaneTransform; //step 2 transform
        private Emgu.CV.Matrix<double> m_transform; //step 3 transform

        private Skeleton current_skeleton;

        public PartialCalibrationClass(KinectSensor m_kinectSensor)
        {
            this.m_kinectSensor = m_kinectSensor;
        }

        private void calibrate()
        {
            if (m_skeletonCalibPoints.Count == m_calibPoints.Count)
            {
                Console.WriteLine("Calibrating...");

                //seketon 3D positions --> 3d positions in depth camera
                Point3D p0 = conertSkeletonPointToDepthPoint(m_skeletonCalibPoints[0]);
                Point3D p1 = conertSkeletonPointToDepthPoint(m_skeletonCalibPoints[1]);
                Point3D p2 = conertSkeletonPointToDepthPoint(m_skeletonCalibPoints[2]);
                Point3D p3 = conertSkeletonPointToDepthPoint(m_skeletonCalibPoints[3]);

                //3d positions depth camera --> positions on a 2D plane
                Vector3D v1 = p1 - p0;
                v1.Normalize();

                Vector3D v2 = p2 - p0;
                v2.Normalize();

                Vector3D planeNormalVec = Vector3D.CrossProduct(v1, v2);
                planeNormalVec.Normalize();

                Vector3D resultingPlaneNormal = new Vector3D(0, 0, 1);
                m_groundPlaneTransform = Util.make_align_axis_matrix(resultingPlaneNormal, planeNormalVec);

                Point3D p0OnPlane = m_groundPlaneTransform.Transform(p0);
                Point3D p1OnPlane = m_groundPlaneTransform.Transform(p1);
                Point3D p2OnPlane = m_groundPlaneTransform.Transform(p2);
                Point3D p3OnPlane = m_groundPlaneTransform.Transform(p3);

                //2d plane positions --> exact 2d square on screen (using perspective transform)
                System.Drawing.PointF[] src = new System.Drawing.PointF[4];
                src[0] = new System.Drawing.PointF((float)p0OnPlane.X, (float)p0OnPlane.Y);
                src[1] = new System.Drawing.PointF((float)p1OnPlane.X, (float)p1OnPlane.Y);
                src[2] = new System.Drawing.PointF((float)p2OnPlane.X, (float)p2OnPlane.Y);
                src[3] = new System.Drawing.PointF((float)p3OnPlane.X, (float)p3OnPlane.Y);

                System.Drawing.PointF[] dest = new System.Drawing.PointF[4];
                dest[0] = new System.Drawing.PointF((float)m_calibPoints[0].X, (float)m_calibPoints[0].Y);
                dest[1] = new System.Drawing.PointF((float)m_calibPoints[1].X, (float)m_calibPoints[1].Y);
                dest[2] = new System.Drawing.PointF((float)m_calibPoints[2].X, (float)m_calibPoints[2].Y);
                dest[3] = new System.Drawing.PointF((float)m_calibPoints[3].X, (float)m_calibPoints[3].Y);

                Emgu.CV.Mat transform = Emgu.CV.CvInvoke.GetPerspectiveTransform(src, dest);

                m_transform = new Emgu.CV.Matrix<double>(transform.Rows, transform.Cols, transform.NumberOfChannels);
                transform.CopyTo(m_transform);

                //test to see if resulting perspective transform is correct
                //tResultx should be same as points in m_calibPoints
                Point tResult0 = kinectToProjectionPoint(m_skeletonCalibPoints[0]);
                Point tResult1 = kinectToProjectionPoint(m_skeletonCalibPoints[1]);
                Point tResult2 = kinectToProjectionPoint(m_skeletonCalibPoints[2]);
                Point tResult3 = kinectToProjectionPoint(m_skeletonCalibPoints[3]);

                CalibrationPointsUpdated?.Invoke(new List<Point>{tResult0, tResult1, tResult2, tResult3});

                Console.WriteLine("Calibration succesfull? (Hopefully...)");
            }
        }

        private Point3D conertSkeletonPointToDepthPoint(SkeletonPoint skeletonPoint)
        {
            DepthImagePoint imgPt = m_kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);

            return new Point3D(imgPt.X, imgPt.Y, imgPt.Depth);
        }

        private Point kinectToProjectionPoint(SkeletonPoint point)
        {
            DepthImagePoint depthP = m_kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(point, DepthImageFormat.Resolution640x480Fps30);
            Point3D p = new Point3D(depthP.X, depthP.Y, depthP.Depth);

            Point3D pOnGroundPlane = m_groundPlaneTransform.Transform(p);

            System.Drawing.PointF[] testPoint = new System.Drawing.PointF[1];
            testPoint[0] = new System.Drawing.PointF((float)pOnGroundPlane.X, (float)pOnGroundPlane.Y);

            System.Drawing.PointF[] resultPoint = Emgu.CV.CvInvoke.PerspectiveTransform(testPoint, m_transform);

            return new Point(resultPoint[0].X, resultPoint[0].Y);
        }

        public void collectCalibrationPoint()
        {
            if (current_skeleton == null) {
                MessageBox.Show("Geen skeleton gevonden in het frame...");
                return;
            }

            SkeletonPoint midpoint3D = GetMidpointBetweenFeet(current_skeleton);

            if (midpoint3D.X == 0 && midpoint3D.Y == 0 && midpoint3D.Z == 0) {
                MessageBox.Show("Geen correct punt kunnen vinden voor kalibratie...");
                return;
            }

            Point imagePoint2D = GetImagePointFromSkeletonPoint(midpoint3D);

            m_skeletonCalibPoints.Add(midpoint3D);
            m_calibPoints.Add(imagePoint2D);

            CalibrationPointsUpdated?.Invoke(m_calibPoints);

            Console.WriteLine("Calibratiepunt toegevoegd: 3D -> {0}, 2D -> {1}", midpoint3D, imagePoint2D);

            if (m_skeletonCalibPoints.Count >= 4 && m_calibPoints.Count >= 4) calibrate();
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

        private Point GetImagePointFromSkeletonPoint(SkeletonPoint skeletonPoint)
        {
            DepthImagePoint depthPoint = m_kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                skeletonPoint, DepthImageFormat.Resolution640x480Fps30);

            return new Point(depthPoint.X, depthPoint.Y);
        }

        public void ProcessSkeletonFrame(Skeleton trackedSkeleton)
        {
            current_skeleton = trackedSkeleton;
        }
    }
}
