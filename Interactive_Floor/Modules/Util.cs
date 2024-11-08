using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    public static class Util
    {

        public static Matrix3D make_align_axis_matrix(Vector3D initialAxis, Vector3D alignWithAxis)
        {
            initialAxis.Normalize();
            alignWithAxis.Normalize();

            double axisDot = Vector3D.DotProduct(initialAxis, alignWithAxis);
            Vector3D axisCross = Vector3D.CrossProduct(initialAxis, alignWithAxis);

            if (sqrlenv3(axisCross) > float_epsilon())
            {
                axisCross.Normalize();
                double fAngle = Math.Acos(axisDot);
                return make_axis_angle_matrix(axisCross, fAngle);
            }
            else if (axisDot < 0)
            {
                Vector3D perp1 = new Vector3D();
                Vector3D perp2 = new Vector3D();
                make_perp_vectors(initialAxis, ref perp1, ref perp2);
                return make_axis_angle_matrix(perp1, 3.14159265);
            }
            else
            {
                return Matrix3D.Identity;
            }
        }

        public static double sqrlenv3(Vector3D a)
        {
            //compute squared length of 3-vector
            return a.X * a.X + a.Y * a.Y + a.Z * a.Z;
        }
        public static double float_epsilon()
        {
            //smallest difference between numbers for 32-bit floats
            return 0.000000119;
        }

        public static void make_perp_vectors(Vector3D a, ref Vector3D out1, ref Vector3D out2)
        {
            //construct two perpendicular vectors to input vector, return as a pair of 3-tuples
            if (Math.Abs(a.X) > Math.Abs(a.Y) && Math.Abs(a.X) > Math.Abs(a.Z))
                out1 = new Vector3D(-a.Y, a.X, 0.0);
            else
                out1 = new Vector3D(0.0, a.Z, -a.Y);

            out1.Normalize();
            out2 = Vector3D.CrossProduct(a, out1);
        }

        public static Matrix3D make_axis_angle_matrix(Vector3D axis, double angle)
        {
            //construct a matrix that rotates around axis by angle (in radians)
            double fCos = Math.Cos(angle);
            double fSin = Math.Sin(angle);
            double fX2 = axis.X * axis.X;
            double fY2 = axis.Y * axis.Y;
            double fZ2 = axis.Z * axis.Z;
            double fXYM = axis.X * axis.Y * (1 - fCos);
            double fXZM = axis.X * axis.Z * (1 - fCos);
            double fYZM = axis.Y * axis.Z * (1 - fCos);
            double fXSin = axis.X * fSin;
            double fYSin = axis.Y * fSin;
            double fZSin = axis.Z * fSin;

            Matrix3D result = new Matrix3D();
            result.M11 = fX2 * (1 - fCos) + fCos;
            result.M12 = fXYM - fZSin;
            result.M13 = fXZM + fYSin;
            result.M21 = fXYM + fZSin;
            result.M22 = fY2 * (1 - fCos) + fCos;
            result.M23 = fYZM - fXSin;
            result.M31 = fXZM - fYSin;
            result.M32 = fYZM + fXSin;
            result.M33 = fZ2 * (1 - fCos) + fCos;
            return result;
        }
    }
}
