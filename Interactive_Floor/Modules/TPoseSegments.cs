using Microsoft.Kinect;

namespace KinectSimpleGesture
{
    public interface IGestureSegment
    {
        GesturePartResult Update(Skeleton skeleton);
    }
    public class TPoseSegment1 : IGestureSegment
    {
        public GesturePartResult Update(Skeleton skeleton)
        {
            // Both arms extended horizontally at shoulder level
            if (IsArmExtended(skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft])
                && IsArmExtended(skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight]))
            {
                return GesturePartResult.Succeeded;
            }

            return GesturePartResult.Failed;
        }

        private bool IsArmExtended(Joint shoulder, Joint elbow, Joint wrist)
        {
            return Math.Abs(elbow.Position.Y - shoulder.Position.Y) < 0.3 && Math.Abs(wrist.Position.Y - shoulder.Position.Y) < 0.3;
        }
    }

    public class TPoseSegment2 : IGestureSegment
    {
        public GesturePartResult Update(Skeleton skeleton)
        {
            if (IsArmExtended(skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft])
                && IsArmExtended(skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight]))
            {
                return GesturePartResult.Succeeded;
            }

            return GesturePartResult.Failed;
        }

        private bool IsArmExtended(Joint shoulder, Joint elbow, Joint wrist)
        {
            
            return Math.Abs(elbow.Position.Y - shoulder.Position.Y) < 0.3 && Math.Abs(wrist.Position.Y - shoulder.Position.Y) < 0.3;
        }
    }
}