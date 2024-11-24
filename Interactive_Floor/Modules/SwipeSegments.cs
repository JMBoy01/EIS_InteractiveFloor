using Microsoft.Kinect;

namespace KinectSimpleGesture
{
    public interface IGestureSegment
    {
        GesturePartResult Update(Skeleton skeleton);
    }
    public class SwipeRightSegment1 : IGestureSegment
    {
        public GesturePartResult Update(Skeleton skeleton)
        {
            // Hand above elbow
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
            {
                // Hand left of shoulder
                if (skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ShoulderRight].Position.X)
                {
                    return GesturePartResult.Succeeded;
                }
            }

            // Hand dropped or not in position
            return GesturePartResult.Failed;
        }
    }
    public class SwipeRightSegment2 : IGestureSegment
    {
        public GesturePartResult Update(Skeleton skeleton)
        {
            // Hand above elbow
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
            {
                // Hand right of shoulder
                if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ShoulderRight].Position.X)
                {
                    return GesturePartResult.Succeeded;
                }
            }

            // Hand dropped or not in position
            return GesturePartResult.Failed;
        }
    }
}
