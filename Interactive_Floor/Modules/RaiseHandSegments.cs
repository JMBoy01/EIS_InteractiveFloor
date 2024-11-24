using Microsoft.Kinect;

namespace KinectSimpleGesture
{
    public class RaiseHandSegment1 : IGestureSegment
    {
        public GesturePartResult Update(Skeleton skeleton)
        {
            // Hand above head
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.Head].Position.Y)
            {
                return GesturePartResult.Succeeded;
            }

            // Hand not raised
            return GesturePartResult.Failed;
        }
    }
    public class RaiseHandSegment2 : IGestureSegment
    {
        public GesturePartResult Update(Skeleton skeleton)
        {
            // Hand above head
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.Head].Position.Y)
            {
                return GesturePartResult.Succeeded;
            }

            // Hand not raised
            return GesturePartResult.Failed;
        }
    }
}
