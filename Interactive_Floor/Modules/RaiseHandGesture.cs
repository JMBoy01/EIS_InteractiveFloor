using Microsoft.Kinect;
using System;

namespace KinectSimpleGesture
{
    public class RaiseHandGesture
    {
        readonly int WINDOW_SIZE = 50;

        IGestureSegment[] _segments;

        int _currentSegment = 0;
        int _frameCount = 0;

        public event EventHandler GestureRecognized;

        public RaiseHandGesture()
        {
            RaiseHandSegment1 raiseHandSegment1 = new RaiseHandSegment1();
            RaiseHandSegment2 raiseHandSegment2 = new RaiseHandSegment2();

            _segments = new IGestureSegment[]
            {
                raiseHandSegment1,
                raiseHandSegment2
            };
        }

        public void Update(Skeleton skeleton)
        {
            GesturePartResult result = _segments[_currentSegment].Update(skeleton);

            if (result == GesturePartResult.Succeeded)
            {
                if (_currentSegment + 1 < _segments.Length)
                {
                    _currentSegment++;
                    _frameCount = 0;
                }
                else
                {
                    GestureRecognized?.Invoke(this, new EventArgs());
                    Reset();
                }
            }
            else if (result == GesturePartResult.Failed || _frameCount == WINDOW_SIZE)
            {
                Reset();
            }
            else
            {
                _frameCount++;
            }
        }

        public void Reset()
        {
            _currentSegment = 0;
            _frameCount = 0;
        }
    }
}
