﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;

namespace Lab9_task2
{
    public enum Axis { AXIS_X, AXIS_Y, AXIS_Z, OTHER };
    public enum Projection { PERSPECTIVE = 0, ISOMETRIC, ORTHOGR_X, ORTHOGR_Y, ORTHOGR_Z };

    public class Point3d
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public PointF TextureCoordinates { get; set; } = new PointF();

        public Point3d(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3d(float x, float y, float z, PointF textureCoordinates)
        {
            X = x;
            Y = y;
            Z = z;
            TextureCoordinates = textureCoordinates;
        }

        public Point3d(Point3d p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
            TextureCoordinates = p.TextureCoordinates;
        }

        public string to_string()
        {
            return X.ToString(CultureInfo.InvariantCulture) + " " +
                Y.ToString(CultureInfo.InvariantCulture) + " " +
                Z.ToString(CultureInfo.InvariantCulture);
        }

        public void reflectX()
        {
            X = -X;
        }
        public void reflectY()
        {
            Y = -Y;
        }
        public void reflectZ()
        {
            Z = -Z;
        }
        /* ------ Projections ------ */

        // get point for central (perspective) projection
        public PointF make_perspective(float k = 1000)
        {
            // for safety - in order not to get infinity
            if (Math.Abs(Z - k) < 1e-10)
                k += 1;

            List<float> P = new List<float> { 1, 0, 0, 0,
                                              0, 1, 0, 0,
                                              0, 0, 0, -1/k,
                                              0, 0, 0, 1 };

            List<float> xyz = new List<float> { X, Y, Z, 1 };
            List<float> c = mul_matrix(xyz, 1, 4, P, 4, 4);

            return new PointF(c[0] / c[3], c[1] / c[3]);
        }

        // get point for isometric projection
        public PointF make_isometric()
        {
            double r_phi = Math.Asin(Math.Tan(Math.PI * 30 / 180));
            double r_psi = Math.PI * 45 / 180;
            float cos_phi = (float)Math.Cos(r_phi);
            float sin_phi = (float)Math.Sin(r_phi);
            float cos_psi = (float)Math.Cos(r_psi);
            float sin_psi = (float)Math.Sin(r_psi);

            List<float> M = new List<float> { cos_psi,  sin_phi * sin_psi,   0,  0,
                                                 0,          cos_phi,        0,  0,
                                              sin_psi,  -sin_phi * cos_psi,  0,  0,
                                                 0,              0,          0,  1 };

            List<float> xyz = new List<float> { X, Y, Z, 1 };
            List<float> c = mul_matrix(xyz, 1, 4, M, 4, 4);

            return new PointF(c[0], c[1]);
        }

        // get point for orthographic projection
        public PointF make_orthographic(Axis a)
        {
            List<float> P = new List<float>();
            for (int i = 0; i < 16; ++i)
            {
                if (i % 5 == 0) // main diag
                    P.Add(1);
                else
                    P.Add(0);
            }

            // x
            if (a == Axis.AXIS_X)
                P[0] = 0;
            // y
            else if (a == Axis.AXIS_Y)
                P[5] = 0;
            // z
            else
                P[10] = 0;

            List<float> xyz = new List<float> { X, Y, Z, 1 };
            List<float> c = mul_matrix(xyz, 1, 4, P, 4, 4);

            // x
            if (a == Axis.AXIS_X)
                return new PointF(c[1], c[2]); // (y, z)
            // y
            else if (a == Axis.AXIS_Y)
                return new PointF(c[0], c[2]); // (x, z)
            // z
            else
                return new PointF(c[0], c[1]); // (x, y)
        }

        public void show(Graphics g, Projection pr = 0, Pen pen = null)
        {
            if (pen == null)
                pen = Pens.Black;

            PointF p;
            switch (pr)
            {
                case Projection.ISOMETRIC:
                    p = make_isometric();
                    break;
                case Projection.ORTHOGR_X:
                    p = make_orthographic(Axis.AXIS_X);
                    break;
                case Projection.ORTHOGR_Y:
                    p = make_orthographic(Axis.AXIS_Y);
                    break;
                case Projection.ORTHOGR_Z:
                    p = make_orthographic(Axis.AXIS_Z);
                    break;
                default:
                    p = make_perspective();
                    break;
            }
            g.DrawRectangle(pen, p.X, p.Y, 2, 2);
        }

        /* ------ Affine transformations ------ */

        static public List<float> mul_matrix(List<float> matr1, int m1, int n1, List<float> matr2, int m2, int n2)
        {
            if (n1 != m2)
                return new List<float>();
            int l = m1;
            int m = n1;
            int n = n2;

            List<float> c = new List<float>();
            for (int i = 0; i < l * n; ++i)
                c.Add(0f);

            for (int i = 0; i < l; ++i)
                for (int j = 0; j < n; ++j)
                {
                    for (int r = 0; r < m; ++r)
                        c[i * l + j] += matr1[i * m1 + r] * matr2[r * n2 + j];
                }
            return c;
        }

        public void translate(float x, float y, float z)
        {
            List<float> T = new List<float> { 1, 0, 0, 0,
                                              0, 1, 0, 0,
                                              0, 0, 1, 0,
                                              x, y, z, 1 };
            List<float> xyz = new List<float> { X, Y, Z, 1 };
            List<float> c = mul_matrix(xyz, 1, 4, T, 4, 4);

            X = c[0];
            Y = c[1];
            Z = c[2];
        }

        public void rotate(double angle, Axis a, Edge line = null)
        {
            double rangle = Math.PI * angle / 180; // угол в радианах

            List<float> R = null;

            float sin = (float)Math.Sin(rangle);
            float cos = (float)Math.Cos(rangle);
            switch (a)
            {
                case Axis.AXIS_X:
                    R = new List<float> { 1,   0,     0,   0,
                                          0,  cos,   sin,  0,
                                          0,  -sin,  cos,  0,
                                          0,   0,     0,   1 };
                    break;
                case Axis.AXIS_Y:
                    R = new List<float> { cos,  0,  -sin,  0,
                                           0,   1,   0,    0,
                                          sin,  0,  cos,   0,
                                           0,   0,   0,    1 };
                    break;
                case Axis.AXIS_Z:
                    R = new List<float> { cos,   sin,  0,  0,
                                          -sin,  cos,  0,  0,
                                           0,     0,   1,  0,
                                           0,     0,   0,  1 };
                    break;
                case Axis.OTHER:
                    float l = Math.Sign(line.P2.X - line.P1.X);
                    float m = Math.Sign(line.P2.Y - line.P1.Y);
                    float n = Math.Sign(line.P2.Z - line.P1.Z);
                    float length = (float)Math.Sqrt(l * l + m * m + n * n);
                    l /= length; m /= length; n /= length;

                    R = new List<float> {  l * l + cos * (1 - l * l),   l * (1 - cos) * m + n * sin,   l * (1 - cos) * n - m * sin,  0,
                                          l * (1 - cos) * m - n * sin,   m * m + cos * (1 - m * m),    m * (1 - cos) * n + l * sin,  0,
                                          l * (1 - cos) * n + m * sin,  m * (1 - cos) * n - l * sin,    n * n + cos * (1 - n * n),   0,
                                                       0,                            0,                             0,               1 };

                    break;
            }
            List<float> xyz = new List<float> { X, Y, Z, 1 };
            List<float> c = mul_matrix(xyz, 1, 4, R, 4, 4);

            X = c[0];
            Y = c[1];
            Z = c[2];
        }

        public void scale(float kx, float ky, float kz)
        {
            List<float> D = new List<float> { kx, 0,  0,  0,
                                              0,  ky, 0,  0,
                                              0,  0,  kz, 0,
                                              0,  0,  0,  1 };
            List<float> xyz = new List<float> { X, Y, Z, 1 };
            List<float> c = mul_matrix(xyz, 1, 4, D, 4, 4);

            X = c[0];
            Y = c[1];
            Z = c[2];
        }
    }

    public class Edge
    {
        public Point3d P1 { get; set; }
        public Point3d P2 { get; set; }

        public Edge(Point3d pt1, Point3d pt2)
        {
            P1 = new Point3d(pt1);
            P2 = new Point3d(pt2);
        }

        public Edge(string s)
        {
            var arr = s.Split(' ');
            P1 = new Point3d(float.Parse(arr[0], CultureInfo.InvariantCulture),
                float.Parse(arr[1], CultureInfo.InvariantCulture),
                float.Parse(arr[2], CultureInfo.InvariantCulture));
            P2 = new Point3d(float.Parse(arr[3], CultureInfo.InvariantCulture),
                float.Parse(arr[4], CultureInfo.InvariantCulture),
                float.Parse(arr[5], CultureInfo.InvariantCulture));
        }

        // get points for central (perspective) projection
        private List<PointF> make_perspective(int k = 1000)
        {
            List<PointF> res = new List<PointF>
            {
                P1.make_perspective(k),
                P2.make_perspective(k)
            };

            return res;
        }

        private List<PointF> make_orthographic(Axis a)
        {
            List<PointF> res = new List<PointF>
            {
                P1.make_orthographic(a),
                P2.make_orthographic(a)
            };
            return res;
        }

        private List<PointF> make_isometric()
        {
            List<PointF> res = new List<PointF>
            {
                P1.make_isometric(),
                P2.make_isometric()
            };
            return res;
        }


        private void show_perspective(Graphics g, Pen pen)
        {
            var pts = make_perspective();
            g.DrawLine(pen, pts[0], pts[1]);
        }

        public void show(Graphics g, Projection pr = 0, Pen pen = null)
        {
            if (pen == null)
                pen = Pens.Black;

            List<PointF> pts;
            switch (pr)
            {
                case Projection.ISOMETRIC:
                    pts = make_isometric();
                    break;
                case Projection.ORTHOGR_X:
                    pts = make_orthographic(Axis.AXIS_X);
                    break;
                case Projection.ORTHOGR_Y:
                    pts = make_orthographic(Axis.AXIS_Y);
                    break;
                case Projection.ORTHOGR_Z:
                    pts = make_orthographic(Axis.AXIS_Z);
                    break;
                default:
                    pts = make_perspective();
                    break;
            }

            g.DrawLine(pen, pts[0], pts[pts.Count - 1]);
        }

        public void translate(float x, float y, float z)
        {
            P1.translate(x, y, z);
            P2.translate(x, y, z);
        }

        public void rotate(double angle, Axis a, Edge line = null)
        {
            P1.rotate(angle, a, line);
            P2.rotate(angle, a, line);
        }

        public void scale(float kx, float ky, float kz)
        {
            P1.scale(kx, ky, kz);
            P2.scale(kx, ky, kz);
        }
    }

    // многоугольник (грань)
    public class Face
    {
        public List<Point3d> Points { get; }
        public Point3d Center { get; set; } = new Point3d(0, 0, 0);
        public List<float> Normal { get; set; }
        public bool IsVisible { get; set; }

        public Face(Face face)
        {
            Points = face.Points.Select(pt => new Point3d(pt.X, pt.Y, pt.Z)).ToList();
            Center = new Point3d(face.Center);
            if (Normal != null)
                Normal = new List<float>(face.Normal);
            IsVisible = face.IsVisible;
        }

        public Face(List<Point3d> pts = null)
        {
            if (pts != null)
            {
                Points = new List<Point3d>(pts);
                find_center();
            }
        }

        public Face(string s)
        {
            Points = new List<Point3d>();

            var arr = s.Split(' ');
            for (int i = 1; i < arr.Length; i += 3)
            {
                if (string.IsNullOrEmpty(arr[i]))
                    continue;
                float x = float.Parse(arr[i], CultureInfo.InvariantCulture);
                float y = float.Parse(arr[i + 1], CultureInfo.InvariantCulture);
                float z = float.Parse(arr[i + 2], CultureInfo.InvariantCulture);
                Point3d p = new Point3d(x, y, z);
                Points.Add(p);
            }
            find_center();
        }

        public string to_string()
        {
            string res = "";
            res += Points.Count.ToString(CultureInfo.InvariantCulture) + " ";
            foreach (var f in Points)
            {
                res += f.to_string() + " ";
            }

            return res;
        }

        private void find_center()
        {
            Center.X = 0;
            Center.Y = 0;
            Center.Z = 0;
            foreach (Point3d p in Points)
            {
                Center.X += p.X;
                Center.Y += p.Y;
                Center.Z += p.Z;
            }
            Center.X /= Points.Count;
            Center.Y /= Points.Count;
            Center.Z /= Points.Count;
        }

        public void find_normal(Point3d p_center)
        {
            Point3d Q = Points[1], R = Points[2], S = Points[0];
            List<float> QR = new List<float> { R.X - Q.X, R.Y - Q.Y, R.Z - Q.Z };
            List<float> QS = new List<float> { S.X - Q.X, S.Y - Q.Y, S.Z - Q.Z };


            Normal = new List<float> { QR[1] * QS[2] - QR[2] * QS[1],
                                       -(QR[0] * QS[2] - QR[2] * QS[0]),
                                       QR[0] * QS[1] - QR[1] * QS[0] }; // cross product

            List<float> CQ = new List<float> { Q.X - p_center.X, Q.Y - p_center.Y, Q.Z - p_center.Z };
            if (Point3d.mul_matrix(Normal, 1, 3, CQ, 3, 1)[0] > 0)
            {
                Normal[0] *= -1;
                Normal[1] *= -1;
                Normal[2] *= -1;
            }

            Point3d E = new Point3d(0, 0, 1000); // point of view
            List<float> CE = new List<float> { E.X - Center.X, E.Y - Center.Y, E.Z - Center.Z };
            // these two options are stored here for the history so that everyone can see how stupid I am
            //List<float> EC = new List<float> { camera.P1.X - Center.X, camera.P1.Y - Center.Y, camera.P1.Z - Center.Z };
            //List<float> EC = new List<float> { camera.P1.X - camera.P2.X, camera.P1.Y - camera.P2.Y, camera.P1.Z - camera.P2.Z };
            float dot_product = Point3d.mul_matrix(Normal, 1, 3, CE, 3, 1)[0];
            IsVisible = dot_product < 0;
        }

        public void reflectX()
        {
            Center.X = -Center.X;
            if (Points != null)
                foreach (var p in Points)
                    p.reflectX();
        }
        public void reflectY()
        {
            Center.Y = -Center.Y;
            if (Points != null)
                foreach (var p in Points)
                    p.reflectY();
        }
        public void reflectZ()
        {
            Center.Z = -Center.Z;
            if (Points != null)
                foreach (var p in Points)
                    p.reflectZ();
        }

        /* ------ Projections ------ */

        // get points for central (perspective) projection
        public List<PointF> make_perspective(float k = 1000)
        {
            List<PointF> res = new List<PointF>();

            foreach (Point3d p in Points)
            {
                res.Add(p.make_perspective(k));
            }
            return res;
        }

        // get point for isometric projection
        public List<PointF> make_isometric()
        {
            List<PointF> res = new List<PointF>();

            foreach (Point3d p in Points)
                res.Add(p.make_isometric());

            return res;
        }

        // get point for orthographic projection
        public List<PointF> make_orthographic(Axis a)
        {
            List<PointF> res = new List<PointF>();

            foreach (Point3d p in Points)
                res.Add(p.make_orthographic(a));

            return res;
        }

        public void show(Graphics g, Projection pr = 0, Pen pen = null, Edge camera = null, float k = 1000)
        {
            if (pen == null)
                pen = Pens.Black;

            List<PointF> pts;

            switch (pr)
            {
                case Projection.ISOMETRIC:
                    pts = make_isometric();
                    break;
                case Projection.ORTHOGR_X:
                    pts = make_orthographic(Axis.AXIS_X);
                    break;
                case Projection.ORTHOGR_Y:
                    pts = make_orthographic(Axis.AXIS_Y);
                    break;
                case Projection.ORTHOGR_Z:
                    pts = make_orthographic(Axis.AXIS_Z);
                    break;
                default:
                    if (camera != null)
                        pts = make_perspective(k);
                    else pts = make_perspective(k);
                    break;
            }

            if (pts.Count > 1)
            {
                g.DrawLines(pen, pts.ToArray());
                g.DrawLine(pen, pts[0], pts[pts.Count - 1]);
            }
            else if (pts.Count == 1)
                g.DrawRectangle(pen, pts[0].X, pts[0].Y, 1, 1);
        }

        /* ------ Affine transformations ------ */

        public void translate(float x, float y, float z)
        {
            foreach (Point3d p in Points)
                p.translate(x, y, z);
            find_center();
        }

        public void rotate(double angle, Axis a, Edge line = null)
        {
            foreach (Point3d p in Points)
                p.rotate(angle, a, line);
            find_center();
        }

        public void scale(float kx, float ky, float kz)
        {
            foreach (Point3d p in Points)
                p.scale(kx, ky, kz);
            find_center();
        }
    }

    // многогранник
    public class Polyhedron
    {
        public const int MODE_POL = 0;
        public const int MODE_ROT = 1;
        public List<Face> Faces { get; set; } = null;
        public Point3d Center { get; set; } = new Point3d(0, 0, 0);
        public float Cube_size { get; set; }

        public Polyhedron(List<Face> fs = null)
        {
            if (fs != null)
            {
                Faces = fs.Select(face => new Face(face)).ToList();
                find_center();
            }
        }

        public Polyhedron(Polyhedron polyhedron)
        {
            Faces = polyhedron.Faces.Select(face => new Face(face)).ToList();
            Center = new Point3d(polyhedron.Center);
            Cube_size = polyhedron.Cube_size;
        }

        public Polyhedron(string s, int mode = MODE_POL)
        {
            Faces = new List<Face>();
            switch (mode)
            {
                case MODE_POL:
                    var arr1 = s.Split('\n');
                    for (int i = 1; i < arr1.Length; ++i)
                    {
                        if (string.IsNullOrEmpty(arr1[i]))
                            continue;
                        Face f = new Face(arr1[i]);
                        Faces.Add(f);
                    }
                    find_center();
                    break;
                default: break;
            }
        }

        public string to_string()
        {
            string res = "";
            res += Faces.Count.ToString(CultureInfo.InvariantCulture) + "\n";
            foreach (var f in Faces)
            {
                res += f.to_string() + "\n";
            }

            return res;
        }

        /// <summary>
        /// Linear interpolation from i0 to i1
        /// </summary>
        /// <param name="i0">First independent variable</param>
        /// <param name="d0">First dependent variable</param>
        /// <param name="i1">Last independent variable</param>
        /// <param name="d1">Last dependent variable</param>
        private static double[] Interpolate(int i0, double d0, int i1, double d1)
        {
            if (i0 == i1)
                return new double[] { d0 };
            double[] values = new double[i1 - i0 + 1];
            double a = (d1 - d0) / (i1 - i0);
            double d = d0;

            int ind = 0;
            for (int i = i0; i <= i1; ++i)
            {
                values[ind++] = d;
                d += a;
            }

            return values;
        }

        /// <summary>
        /// Sort triangle vertices by Y from min to max
        /// </summary>
        private static Point3d[] SortTriangleVertices(Point3d P0, Point3d P1, Point3d P2)
        {
            Point3d p0 = new Point3d(P0)
            {
                X = P0.make_perspective().X,
                Y = P0.make_perspective().Y
            },
            p1 = new Point3d(P1)
            {
                X = P1.make_perspective().X,
                Y = P1.make_perspective().Y
            },
            p2 = new Point3d(P2)
            {
                X = P2.make_perspective().X,
                Y = P2.make_perspective().Y
            };
            Point3d[] points = new Point3d[3] { p0, p1, p2 };

            if (points[1].Y < points[0].Y)
            {
                points[0] = p1;
                points[1] = p0;
            }
            if (points[2].Y < points[0].Y)
            {
                points[2] = points[0];
                points[0] = p2;
            }
            if (points[2].Y < points[1].Y)
            {
                Point3d temp = points[1];
                points[1] = points[2];
                points[2] = temp;
            }

            return points;
        }

        private static void DrawTexture(Point3d P0, Point3d P1, Point3d P2, Bitmap bmp, BitmapData bmpData, byte[] rgbValues, Bitmap texture, BitmapData bmpDataTexture, byte[] rgbValuesTexture)
        {
            // Sort the points so that y0 <= y1 <= y2
            var points = SortTriangleVertices(P0, P1, P2);
            Point3d SortedP0 = points[0], SortedP1 = points[1], SortedP2 = points[2];

            // Compute the x coordinates and u, v texture coordinates of the triangle edges
            var x01 = Interpolate((int)SortedP0.Y, SortedP0.X, (int)SortedP1.Y, SortedP1.X);
            var u01 = Interpolate((int)SortedP0.Y, SortedP0.TextureCoordinates.X, (int)SortedP1.Y, SortedP1.TextureCoordinates.X);
            var v01 = Interpolate((int)SortedP0.Y, SortedP0.TextureCoordinates.Y, (int)SortedP1.Y, SortedP1.TextureCoordinates.Y);
            var x12 = Interpolate((int)SortedP1.Y, SortedP1.X, (int)SortedP2.Y, SortedP2.X);
            var u12 = Interpolate((int)SortedP1.Y, SortedP1.TextureCoordinates.X, (int)SortedP2.Y, SortedP2.TextureCoordinates.X);
            var v12 = Interpolate((int)SortedP1.Y, SortedP1.TextureCoordinates.Y, (int)SortedP2.Y, SortedP2.TextureCoordinates.Y);
            var x02 = Interpolate((int)SortedP0.Y, SortedP0.X, (int)SortedP2.Y, SortedP2.X);
            var u02 = Interpolate((int)SortedP0.Y, SortedP0.TextureCoordinates.X, (int)SortedP2.Y, SortedP2.TextureCoordinates.X);
            var v02 = Interpolate((int)SortedP0.Y, SortedP0.TextureCoordinates.Y, (int)SortedP2.Y, SortedP2.TextureCoordinates.Y);

            // Concatenate the short sides
            x01 = x01.Take(x01.Length - 1).ToArray(); // remove last element, it's the first in x12
            var x012 = x01.Concat(x12).ToArray();
            u01 = u01.Take(u01.Length - 1).ToArray(); // remove last element, it's the first in u12
            var u012 = u01.Concat(u12).ToArray();
            v01 = v01.Take(v01.Length - 1).ToArray(); // remove last element, it's the first in v12
            var v012 = v01.Concat(v12).ToArray();

            // Determine which is left and which is right
            int m = x012.Length / 2;
            double[] x_left, x_right, u_left, u_right, v_left, v_right;
            if (x02[m] < x012[m])
            {
                x_left = x02;
                x_right = x012;
                u_left = u02;
                u_right = u012;
                v_left = v02;
                v_right = v012;
            }
            else
            {
                x_left = x012;
                x_right = x02;
                u_left = u012;
                u_right = u02;
                v_left = v012;
                v_right = v02;
            }

            // Draw the horizontal segments
            for (int y = (int)SortedP0.Y; y < (int)SortedP2.Y; ++y)
            {
                int screen_y = -y + bmp.Height / 2;
                if (screen_y < 0)
                    break;
                if (bmp.Height <= screen_y)
                    continue;

                var x_l = x_left[y - (int)SortedP0.Y];
                var x_r = x_right[y - (int)SortedP0.Y];

                var u_segment = Interpolate((int)x_l, u_left[y - (int)SortedP0.Y], (int)x_r, u_right[y - (int)SortedP0.Y]);
                var v_segment = Interpolate((int)x_l, v_left[y - (int)SortedP0.Y], (int)x_r, v_right[y - (int)SortedP0.Y]);
                for (int x = (int)x_l; x < (int)x_r; ++x)
                {
                    int screen_x = x + bmp.Width / 2;
                    if (screen_x < 0)
                        continue;
                    if (bmp.Width <= screen_x)
                        break;

                    int texture_u = (int)(u_segment[x - (int)x_l] * (texture.Width - 1));
                    int texture_v = (int)(v_segment[x - (int)x_l] * (texture.Height - 1));

                    rgbValues[screen_x * 3 + screen_y * bmpData.Stride] = rgbValuesTexture[texture_u * 3 + texture_v * bmpDataTexture.Stride];
                    rgbValues[screen_x * 3 + 1 + screen_y * bmpData.Stride] = rgbValuesTexture[texture_u * 3 + 1 + texture_v * bmpDataTexture.Stride];
                    rgbValues[screen_x * 3 + 2 + screen_y * bmpData.Stride] = rgbValuesTexture[texture_u * 3 + 2 + texture_v * bmpDataTexture.Stride];
                }
            }
        }

        public void ApplyTexture(Bitmap bmp, BitmapData bmpData, byte[] rgbValues, Bitmap texture, BitmapData bmpDataTexture, byte[] rgbValuesTexture)
        {
            foreach (var f in Faces)
            {
                f.find_normal(Center);
                if (!f.IsVisible)
                    continue;

                // 3 vertices
                Point3d P0 = new Point3d(f.Points[0]);
                Point3d P1 = new Point3d(f.Points[1]);
                Point3d P2 = new Point3d(f.Points[2]);
                DrawTexture(P0, P1, P2, bmp, bmpData, rgbValues, texture, bmpDataTexture, rgbValuesTexture);

                // 4 vertices
                if (f.Points.Count == 4)
                {
                    P0 = new Point3d(f.Points[2]);
                    P1 = new Point3d(f.Points[3]);
                    P2 = new Point3d(f.Points[0]);
                    DrawTexture(P0, P1, P2, bmp, bmpData, rgbValues, texture, bmpDataTexture, rgbValuesTexture);
                }
            }
        }

        private void find_center()
        {
            Center.X = 0;
            Center.Y = 0;
            Center.Z = 0;
            foreach (Face f in Faces)
            {
                Center.X += f.Center.X;
                Center.Y += f.Center.Y;
                Center.Z += f.Center.Z;
            }
            Center.X /= Faces.Count;
            Center.Y /= Faces.Count;
            Center.Z /= Faces.Count;
        }

        public void show(Graphics g, Projection pr = 0, Pen pen = null)
        {
            foreach (Face f in Faces)
            {
                f.find_normal(Center);
                if (f.IsVisible)
                    f.show(g, pr, pen);
            }
        }

        /* ------ Affine transformation ------ */

        public void translate(float x, float y, float z)
        {
            foreach (Face f in Faces)
                f.translate(x, y, z);
            find_center();
        }

        public void rotate(double angle, Axis a, Edge line = null)
        {
            foreach (Face f in Faces)
                f.rotate(angle, a, line);
            find_center();
        }

        public void scale(float kx, float ky, float kz)
        {
            foreach (Face f in Faces)
                f.scale(kx, ky, kz);
            find_center();
        }

        public void reflectX()
        {
            if (Faces != null)
                foreach (var f in Faces)
                    f.reflectX();
            find_center();
        }

        public void reflectY()
        {
            if (Faces != null)
                foreach (var f in Faces)
                    f.reflectY();
            find_center();
        }

        public void reflectZ()
        {
            if (Faces != null)
                foreach (var f in Faces)
                    f.reflectZ();
            find_center();
        }

        /* ------ Figures ------- */

        public void make_hexahedron(float cube_half_size = 50)
        {
            Face f = new Face(
                new List<Point3d>
                {
                    new Point3d(-cube_half_size, cube_half_size, cube_half_size, new PointF(0, 0)),
                    new Point3d(cube_half_size, cube_half_size, cube_half_size, new PointF(1, 0)),
                    new Point3d(cube_half_size, -cube_half_size, cube_half_size, new PointF(1, 1)),
                    new Point3d(-cube_half_size, -cube_half_size, cube_half_size, new PointF(0, 1))
                }
            );

            Faces = new List<Face> { f }; // front face

            // back face
            Face f1 = new Face(
                    new List<Point3d>
                    {
                        new Point3d(-cube_half_size, cube_half_size, -cube_half_size, new PointF(1, 0)),
                        new Point3d(-cube_half_size, -cube_half_size, -cube_half_size, new PointF(1, 1)),
                        new Point3d(cube_half_size, -cube_half_size, -cube_half_size, new PointF(0, 1)),
                        new Point3d(cube_half_size, cube_half_size, -cube_half_size, new PointF(0, 0))
                    });

            Faces.Add(f1);

            // down face
            List<Point3d> l2 = new List<Point3d>
            {
                new Point3d(f.Points[2]),
                new Point3d(f1.Points[2]),
                new Point3d(f1.Points[1]),
                new Point3d(f.Points[3]),
            };
            l2[0].TextureCoordinates = new PointF(1, 0);
            l2[1].TextureCoordinates = new PointF(1, 1);
            l2[2].TextureCoordinates = new PointF(0, 1);
            l2[3].TextureCoordinates = new PointF(0, 0);
            Face f2 = new Face(l2);
            Faces.Add(f2);

            // up face
            List<Point3d> l3 = new List<Point3d>
            {
                new Point3d(f1.Points[0]),
                new Point3d(f1.Points[3]),
                new Point3d(f.Points[1]),
                new Point3d(f.Points[0]),
            };
            l3[0].TextureCoordinates = new PointF(0, 0);
            l3[1].TextureCoordinates = new PointF(1, 0);
            l3[2].TextureCoordinates = new PointF(1, 1);
            l3[3].TextureCoordinates = new PointF(0, 1);
            Face f3 = new Face(l3);
            Faces.Add(f3);

            // left face
            List<Point3d> l4 = new List<Point3d>
            {
                new Point3d(f1.Points[0]),
                new Point3d(f.Points[0]),
                new Point3d(f.Points[3]),
                new Point3d(f1.Points[1])
            };
            l4[0].TextureCoordinates = new PointF(0, 0);
            l4[1].TextureCoordinates = new PointF(1, 0);
            l4[2].TextureCoordinates = new PointF(1, 1);
            l4[3].TextureCoordinates = new PointF(0, 1);
            Face f4 = new Face(l4);
            Faces.Add(f4);

            // right face
            List<Point3d> l5 = new List<Point3d>
            {
                new Point3d(f1.Points[3]),
                new Point3d(f1.Points[2]),
                new Point3d(f.Points[2]),
                new Point3d(f.Points[1])
            };
            l5[0].TextureCoordinates = new PointF(1, 0);
            l5[1].TextureCoordinates = new PointF(1, 1);
            l5[2].TextureCoordinates = new PointF(0, 1);
            l5[3].TextureCoordinates = new PointF(0, 0);
            Face f5 = new Face(l5);
            Faces.Add(f5);

            Cube_size = 2 * cube_half_size;
            find_center();
        }

        public void make_tetrahedron(Polyhedron cube = null)
        {
            if (cube == null)
            {
                cube = new Polyhedron();
                cube.make_hexahedron();
            }
            Face f0 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[0].Points[0]),
                    new Point3d(cube.Faces[1].Points[1]),
                    new Point3d(cube.Faces[1].Points[3])
                }
            );
            f0.Points[0].TextureCoordinates = new PointF(0.5f, 0);
            f0.Points[1].TextureCoordinates = new PointF(0, 1);
            f0.Points[2].TextureCoordinates = new PointF(1, 1);

            Face f1 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[1].Points[3]),
                    new Point3d(cube.Faces[1].Points[1]),
                    new Point3d(cube.Faces[0].Points[2])
                }
            );
            f1.Points[0].TextureCoordinates = new PointF(0.5f, 0);
            f1.Points[1].TextureCoordinates = new PointF(0, 1);
            f1.Points[2].TextureCoordinates = new PointF(1, 1);

            Face f2 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[0].Points[2]),
                    new Point3d(cube.Faces[1].Points[1]),
                    new Point3d(cube.Faces[0].Points[0])
                }
            );
            f2.Points[0].TextureCoordinates = new PointF(0.5f, 0);
            f2.Points[1].TextureCoordinates = new PointF(0, 1);
            f2.Points[2].TextureCoordinates = new PointF(1, 1);

            Face f3 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[0].Points[2]),
                    new Point3d(cube.Faces[0].Points[0]),
                    new Point3d(cube.Faces[1].Points[3])
                }
            );
            f3.Points[0].TextureCoordinates = new PointF(0.5f, 0);
            f3.Points[1].TextureCoordinates = new PointF(0, 1);
            f3.Points[2].TextureCoordinates = new PointF(1, 1);

            Faces = new List<Face> { f0, f1, f2, f3 };
            find_center();
        }

        public void make_octahedron(Polyhedron cube = null)
        {
            if (cube == null)
            {
                cube = new Polyhedron();
                cube.make_hexahedron();
            }

            // up
            Face f0 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[2].Center),
                    new Point3d(cube.Faces[1].Center),
                    new Point3d(cube.Faces[4].Center)
                }
            );
            f0.Points[0].TextureCoordinates = new PointF(0.5f, 0);
            f0.Points[1].TextureCoordinates = new PointF(0, 1);
            f0.Points[2].TextureCoordinates = new PointF(1, 1);

            Face f1 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[2].Center),
                    new Point3d(cube.Faces[1].Center),
                    new Point3d(cube.Faces[5].Center)
                }
            );
            f1.Points[0].TextureCoordinates = new PointF(0.5f, 0);
            f1.Points[1].TextureCoordinates = new PointF(0, 1);
            f1.Points[2].TextureCoordinates = new PointF(1, 1);

            Face f2 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[2].Center),
                    new Point3d(cube.Faces[5].Center),
                    new Point3d(cube.Faces[0].Center)
                }
            );
            f2.Points[0].TextureCoordinates = new PointF(0.5f, 0);
            f2.Points[1].TextureCoordinates = new PointF(0, 1);
            f2.Points[2].TextureCoordinates = new PointF(1, 1);

            Face f3 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[2].Center),
                    new Point3d(cube.Faces[0].Center),
                    new Point3d(cube.Faces[4].Center)
                }
            );
            f3.Points[0].TextureCoordinates = new PointF(0.5f, 0);
            f3.Points[1].TextureCoordinates = new PointF(0, 1);
            f3.Points[2].TextureCoordinates = new PointF(1, 1);

            // down
            Face f4 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[3].Center),
                    new Point3d(cube.Faces[1].Center),
                    new Point3d(cube.Faces[4].Center)
                }
            );
            f4.Points[0].TextureCoordinates = new PointF(0.5f, 0);
            f4.Points[1].TextureCoordinates = new PointF(0, 1);
            f4.Points[2].TextureCoordinates = new PointF(1, 1);

            Face f5 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[3].Center),
                    new Point3d(cube.Faces[1].Center),
                    new Point3d(cube.Faces[5].Center)
                }
            );
            f5.Points[0].TextureCoordinates = new PointF(0.5f, 0);
            f5.Points[1].TextureCoordinates = new PointF(0, 1);
            f5.Points[2].TextureCoordinates = new PointF(1, 1);

            Face f6 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[3].Center),
                    new Point3d(cube.Faces[5].Center),
                    new Point3d(cube.Faces[0].Center)
                }
            );
            f6.Points[0].TextureCoordinates = new PointF(0.5f, 0);
            f6.Points[1].TextureCoordinates = new PointF(0, 1);
            f6.Points[2].TextureCoordinates = new PointF(1, 1);

            Face f7 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[3].Center),
                    new Point3d(cube.Faces[0].Center),
                    new Point3d(cube.Faces[4].Center)
                }
            );
            f7.Points[0].TextureCoordinates = new PointF(0.5f, 0);
            f7.Points[1].TextureCoordinates = new PointF(0, 1);
            f7.Points[2].TextureCoordinates = new PointF(1, 1);

            Faces = new List<Face> { f0, f1, f2, f3, f4, f5, f6, f7 };
            find_center();
        }
    }

    public sealed class PointComparer : IComparer<PointF>
    {
        public int Compare(PointF p1, PointF p2)
        {
            if (p1.X.Equals(p2.X))
                return p1.Y.CompareTo(p2.Y);
            else return p1.X.CompareTo(p2.X);
        }
    }

    public sealed class ReverseFloatComparer : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            return y.CompareTo(x);
        }
    }
}
