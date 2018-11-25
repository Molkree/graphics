﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace lab6
{
    public enum Axis { AXIS_X, AXIS_Y, AXIS_Z, OTHER };
    public enum Projection { PERSPECTIVE = 0, ISOMETRIC, ORTHOGR_X, ORTHOGR_Y, ORTHOGR_Z };
    public delegate float Function(float x, float y);

    public class Point3d
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Point3d(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3d(Point3d p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
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
            //int points_cnt = int.Parse(arr[0], CultureInfo.InvariantCulture);
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

        public void find_normal(Point3d p_center, /*Do we need it?*/Edge camera)
        {
            Point3d Q = Points[1], R = Points[2], S = Points[0];
            List<float> QR = new List<float> { R.X - Q.X, R.Y - Q.Y, R.Z - Q.Z };
            List<float> QS = new List<float> { S.X - Q.X, S.Y - Q.Y, S.Z - Q.Z };


            Normal = new List<float> { QR[1] * QS[2] - QR[2] * QS[1],
                                       -(QR[0] * QS[2] - QR[2] * QS[0]),
                                       QR[0] * QS[1] - QR[1] * QS[0] }; // cross product

            List<float> CQ = new List<float> { Q.X - p_center.X, Q.Y - p_center.Y, Q.Z - p_center.Z };
            if (Point3d.mul_matrix(Normal, 1, 3, CQ, 3, 1)[0] > 1E-6)
            {
                Normal[0] *= -1;
                Normal[1] *= -1;
                Normal[2] *= -1;
            }

            // we move scene, not camera, so our point of view is always in (0,0,0)
            Point3d E = camera.P1; // point of view
            List<float> CE = new List<float> { E.X - Center.X, E.Y - Center.Y, E.Z - Center.Z };
            // these two options are stored here for the history so that everyone can see how stupid I am
            //List<float> EC = new List<float> { camera.P1.X - Center.X, camera.P1.Y - Center.Y, camera.P1.Z - Center.Z };
            //List<float> EC = new List<float> { camera.P1.X - camera.P2.X, camera.P1.Y - camera.P2.Y, camera.P1.Z - camera.P2.Z };
            float dot_product = Point3d.mul_matrix(Normal, 1, 3, CE, 3, 1)[0];
            IsVisible = Math.Abs(dot_product) < 1E-6 || dot_product < 0 ;
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
        public List<PointF> make_perspective(float k = 1000, float z_camera = 1000)
        {
            List<PointF> res = new List<PointF>();

            foreach (Point3d p in Points)
            {
                // not to show object behind the camera  -  bad idea
            //    if (p.Z > k || p.Z > z_camera)
              //      continue;
                // 
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
                        pts = make_perspective(k, camera.P1.Z);
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

        // костыли
        public bool is_graph = false;
        public Function graph_function = null;
        public int graph_breaks = 0;

        //    public SortedDictionary<float, PointF> graph_function = null;
        private Dictionary<Point3d, List<int>> map = null;
        private Dictionary<Point3d, List<float>> point_to_normal = null;
        private Dictionary<Point3d, float> point_to_intensive = null;

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
            is_graph = polyhedron.is_graph;
            graph_function = polyhedron.graph_function;
            graph_breaks = polyhedron.graph_breaks;
        }

        public Polyhedron(string s, int mode = MODE_POL)
        {
            Faces = new List<Face>();
            switch (mode)
            {
                case MODE_POL:
                    var arr1 = s.Split('\n');
                    //int faces_cnt = int.Parse(arr1[0], CultureInfo.InvariantCulture);
                    for (int i = 1; i < arr1.Length; ++i)
                    {
                        if (string.IsNullOrEmpty(arr1[i]))
                            continue;
                        Face f = new Face(arr1[i]);
                        Faces.Add(f);
                    }
                    find_center();
                    break;
                case MODE_ROT:
                    var arr2 = s.Split('\n');
                    int cnt_breaks = int.Parse(arr2[0], CultureInfo.InvariantCulture);
                    Edge rot_line = new Edge(arr2[1]);
                    int cnt_points = int.Parse(arr2[2], CultureInfo.InvariantCulture);
                    var arr3 = arr2[3].Split(' ');
                    List<Point3d> pts = new List<Point3d>();
                    for (int i = 0; i < 3 * cnt_points; i += 3)
                        pts.Add(new Point3d(
                            float.Parse(arr3[i], CultureInfo.InvariantCulture),
                            float.Parse(arr3[i + 1], CultureInfo.InvariantCulture),
                            float.Parse(arr3[i + 2], CultureInfo.InvariantCulture)));
                    make_rotation_figure(cnt_breaks, rot_line, pts);
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
            //foreach (Face f in Faces)
            //{
            //    f.find_normal(Center);
            //}
        }

        private void create_map(Edge camera, Point3d light)
        {
            map = new Dictionary<Point3d, List<int>>(new Point3dEqComparer());
            point_to_normal = new Dictionary<Point3d, List<float>>(new Point3dEqComparer());
            point_to_intensive = new Dictionary<Point3d, float>(new Point3dEqComparer());
            for (int i = 0; i < Faces.Count; ++i)
            {
                Faces[i].find_normal(Center, camera);
                var n = Faces[i].Normal;
                foreach (var p in Faces[i].Points)
                {
                    if (!map.ContainsKey(p))
                        map[p] = new List<int>();
                    map[p].Add(i);
                    if (!point_to_normal.ContainsKey(p))
                        point_to_normal[p] = new List<float>() { 0, 0, 0 };
                    point_to_normal[p][0] += n[0];
                    point_to_normal[p][1] += n[1];
                    point_to_normal[p][2] += n[2];
                }
            }
            float max = 0;
            foreach (var el in map)
            {
                var p = el.Key;
                var lenght = (float)Math.Sqrt(point_to_normal[p][0] * point_to_normal[p][0] + point_to_normal[p][1] * point_to_normal[p][1] + point_to_normal[p][2] * point_to_normal[p][2]);
                point_to_normal[p][0] /= lenght;
                point_to_normal[p][1] /= lenght;
                point_to_normal[p][2] /= lenght;

                List<float> to_light = new List<float>() { light.X - p.X, light.Y - p.Y, light.Z - p.Z };
                lenght = (float)Math.Sqrt(to_light[0] * to_light[0] + to_light[1] * to_light[1] + to_light[2] * to_light[2]);
                to_light[0] /= lenght; to_light[1] /= lenght; to_light[2] /= lenght;

                //ka - свойство материала воспринимать фоновое освещение, ia - мощность фонового освещения
                float ka = 1; float ia = 0.7f;
                float Ia = ka * ia;
                //kd - свойство материала воспринимать рассеянное освещение, id - мощность рассеянного освещения
                float kd = 0.7f; float id = 1f;
                float Id = kd * id * (point_to_normal[p][0] * to_light[0] + point_to_normal[p][1] * to_light[1] + point_to_normal[p][2] * to_light[2]);
                point_to_intensive[p] = Ia + Id;
                if (point_to_intensive[p] > max)
                    max = point_to_intensive[p];
            }
            //может ли быть больше 1?
            if (max != 0)
                foreach (var el in point_to_normal)
                {
                    point_to_intensive[el.Key] /= max;
                    if (point_to_intensive[el.Key] < 0)
                        point_to_intensive[el.Key] = 0;
                }

        }

        public void show(Graphics g, Projection pr = 0, Pen pen = null)
        {
            foreach (Face f in Faces)
                //if (f.IsVisible)
                    f.show(g, pr, pen);
        }

        private int[] Interpolate(int i0, int d0, int i1, int d1)
        {
            if (i0 == i1)
            {
                return new int[] { d0 };
            }
            int[] values = new int[Math.Abs(i1 - i0 + 1)];
            float a = (float)(d1 - d0) / (i1 - i0);
            float d = d0;
            int ind = 0;
            for (int i = i0; i <= i1; ++i)
            {
                values[ind] = (int)(d+0.5);
                d = d + a;
                ++ind;
            }
            return values;
        }

        private void DrawFilledTriangle(Edge camera, Point3d P0, Point3d P1, Point3d P2, int[] buff, int width, int height, int[] colors, int color)
        {
            PointF p0 = P0.make_perspective();
            PointF p1 = P1.make_perspective();
            PointF p2 = P2.make_perspective();

            //y0 <= y1 <= y2
            int y0 = (int)p0.Y; int x0 = (int)p0.X; int z0 = (int)P0.Z;
            int y1 = (int)p1.Y; int x1 = (int)p1.X; int z1 = (int)P1.Z;
            int y2 = (int)p2.Y; int x2 = (int)p2.X; int z2 = (int)P2.Z;

            var x01 = Interpolate(y0, x0, y1, x1);
            var x12 = Interpolate(y1, x1, y2, x2);
            var x02 = Interpolate(y0, x0, y2, x2);

            var h01 = Interpolate(y0, z0, y1, z1);
            var h12 = Interpolate(y1, z1, y2, z2);
            var h02 = Interpolate(y0, z0, y2, z2);
            // Конкатенация коротких сторон
            int[] x012 = x01.Take(x01.Length - 1).Concat(x12).ToArray();
            int[] h012 = h01.Take(h01.Length - 1).Concat(h12).ToArray();

            //Определяем, какая из сторон левая и правая
            int m = x012.Length / 2;
            int[] x_left, x_right, h_left, h_right;
            if (x02[m] < x012[m])
            {
                x_left = x02;
                x_right = x012;

                h_left = h02;
                h_right = h012;
            }
            else
            {
                x_left = x012;
                x_right = x02;


                h_left = h012;
                h_right = h02;
            }
            
            Face f = new Face(new List<Point3d>() { P0, P1, P2});
            //Отрисовка горизонтальных отрезков
            for (int y = y0; y <= y2; ++y)
            {
                int x_l = x_left[y - y0];
                int x_r = x_right[y - y0];
                int[] h_segment;
                //interpolation
                if (x_l > x_r)
                {
                    continue;
                    h_segment = Interpolate(x_r, h_left[y - y0], x_l, h_right[y - y0]); // костыль
                }
                else
                    h_segment = Interpolate(x_l, h_left[y - y0], x_r, h_right[y - y0]);
                for (int x = x_l; x <= x_r; ++x)
                {
                    int z = h_segment[x - x_l];
                    //i, j, z - координаты в пространстве, в пикчербоксе x, y
                    //int xx = (x + width / 2) % width;
                    //int yy = (-y + height / 2) % height;
                    int xx = x + width / 2;
                    int yy = -y + height / 2;
                    if (xx < 0 || xx > width || yy < 0 || yy > height || (xx * height + yy) < 0 || (xx * height + yy) > (buff.Length - 1))
                        continue;
                    if (z > buff[xx * height + yy])
                    {
                        buff[xx * height + yy] = (int)(z + 0.5);
                        colors[xx * height + yy] = color;
                    }
                }
            }

        }


        private void magic(Edge camera, Point3d P0, Point3d P1, Point3d P2, int[] buff, int width, int height, int[] colors, int color)
        {
            //сортируем p0, p1, p2: y0 <= y1 <= y2
            PointF p0 = P0.make_perspective();
            PointF p1 = P1.make_perspective();
            PointF p2 = P2.make_perspective();

            if (p1.Y < p0.Y)
            {
                Point3d tmpp = new Point3d(P0);
                P0.X = P1.X; P0.Y = P1.Y; P0.Z = P1.Z;
                P1.X = tmpp.X; P1.Y = tmpp.Y; P1.Z = tmpp.Z;
                PointF tmppp = new PointF(p0.X, p0.Y);
                p0.X = p1.X; p0.Y = p1.Y;
                p1.X = tmppp.X; p1.Y = tmppp.Y;
            }
            if (p2.Y < p0.Y)
            {
                Point3d tmpp = new Point3d(P0);
                P0.X = P2.X; P0.Y = P2.Y; P0.Z = P2.Z;
                P2.X = tmpp.X; P2.Y = tmpp.Y; P2.Z = tmpp.Z;
                PointF tmppp = new PointF(p0.X, p0.Y);
                p0.X = p2.X; p0.Y = p2.Y;
                p2.X = tmppp.X; p2.Y = tmppp.Y;
            }
            if (p2.Y < p1.Y)
            {
                Point3d tmpp = new Point3d(P1);
                P1.X = P2.X; P1.Y = P2.Y; P1.Z = P2.Z;
                P2.X = tmpp.X; P2.Y = tmpp.Y; P2.Z = tmpp.Z;
                PointF tmppp = new PointF(p1.X, p1.Y);
                p1.X = p2.X; p1.Y = p2.Y;
                p2.X = tmppp.X; p2.Y = tmppp.Y;
            }

            DrawFilledTriangle(camera, P0, P1, P2, buff, width, height, colors, color);
        }

        public void calc_z_buff(Edge camera, int width, int height, out int[] buf, out int[] colors)
        {
            buf = new int[width * height];
            for (int i = 0; i < width * height; ++i)
                buf[i] = int.MinValue;
            colors = new int[width * height];
            for (int i = 0; i < width * height; ++i)
                colors[i] = 255;

            Random r = new Random();
            int color = 0;
            foreach (var f in Faces)
            {

                //color = r.Next(200);
                color = (color+ 30) % 255;
                //треугольник
                Point3d P0 = new Point3d(f.Points[0]);
                Point3d P1 = new Point3d(f.Points[1]);
                Point3d P2 = new Point3d(f.Points[2]);
                magic(camera, P0, P1, P2, buf, width, height, colors, color);
                //4
                if (f.Points.Count > 3)
                {
                    P0 = new Point3d(f.Points[2]);
                    P1 = new Point3d(f.Points[3]);
                    P2 = new Point3d(f.Points[0]);
                    magic(camera, P0, P1, P2, buf, width, height, colors, color);
                }
                //5  убейте додекаэдр,пожалуйста
                if (f.Points.Count > 4)
                {
                    P0 = new Point3d(f.Points[3]);
                    P1 = new Point3d(f.Points[4]);
                    P2 = new Point3d(f.Points[0]);
                    magic(camera, P0, P1, P2, buf, width, height, colors, color);
                }
            }

            int min_v = int.MaxValue;
            int max_v = 0;
            for (int i = 0; i<width*height; ++i)
            {
                if (buf[i] != int.MinValue && buf[i] < min_v)
                    min_v = buf[i];
                if (buf[i] > max_v)
                    max_v = buf[i];
            }
            if (min_v < 0)
            {
                min_v = -min_v;
                max_v += min_v;
                for (int i = 0; i < width * height; ++i)
                    if (buf[i] != int.MinValue)
                    buf[i] = (buf[i]+min_v)%int.MaxValue;
            }
            for (int i = 0; i < width * height; ++i)
                if (buf[i] == int.MinValue)
                    buf[i] = 255;
                else if (max_v != 0) buf[i] = buf[i] * 225 / max_v;
            
        }

        private void G_DrawFilledTriangle(Edge camera, Point3d P0, Point3d P1, Point3d P2, int[] buff, int width, int height, float[] colors, float c_P0, float c_P1, float c_P2)
        {
            PointF p0 = P0.make_perspective();
            PointF p1 = P1.make_perspective();
            PointF p2 = P2.make_perspective();

            //y0 <= y1 <= y2
            int y0 = (int)p0.Y; int x0 = (int)p0.X; int z0 = (int)P0.Z;
            int y1 = (int)p1.Y; int x1 = (int)p1.X; int z1 = (int)P1.Z;
            int y2 = (int)p2.Y; int x2 = (int)p2.X; int z2 = (int)P2.Z;

            var x01 = Interpolate(y0, x0, y1, x1);
            var x12 = Interpolate(y1, x1, y2, x2);
            var x02 = Interpolate(y0, x0, y2, x2);

            var h01 = Interpolate(y0, z0, y1, z1);
            var h12 = Interpolate(y1, z1, y2, z2);
            var h02 = Interpolate(y0, z0, y2, z2);

            var c01 = Interpolate(y0, (int)(c_P0*100), y1, (int)(c_P1 * 100));
            var c12 = Interpolate(y1, (int)(c_P1 * 100), y2, (int)(c_P2 * 100));
            var c02 = Interpolate(y0, (int)(c_P0 * 100), y2, (int)(c_P2 * 100));
            // Конкатенация коротких сторон
            int[] x012 = x01.Take(x01.Length - 1).Concat(x12).ToArray();
            int[] h012 = h01.Take(h01.Length - 1).Concat(h12).ToArray();
            int[] c012 = c01.Take(c01.Length - 1).Concat(c12).ToArray();

            //Определяем, какая из сторон левая и правая
            int m = x012.Length / 2;
            int[] x_left, x_right, h_left, h_right, c_left, c_right;
            if (x02[m] < x012[m])
            {
                x_left = x02;
                x_right = x012;

                h_left = h02;
                h_right = h012;

                c_left = c02;
                c_right = c012;
            }
            else
            {
                x_left = x012;
                x_right = x02;

                h_left = h012;
                h_right = h02;

                c_left = c012;
                c_right = c02;
            }
           
            //Отрисовка горизонтальных отрезков
            for (int y = y0; y <= y2; ++y)
            {
                int x_l = x_left[y - y0];
                int x_r = x_right[y - y0];
                int[] h_segment;
                int[] c_segment;
                //interpolation
                if (x_l > x_r)
                    continue;
                h_segment = Interpolate(x_l, h_left[y - y0], x_r, h_right[y - y0]);
                c_segment = Interpolate(x_l, c_left[y - y0], x_r, c_right[y - y0]);
                for (int x = x_l; x <= x_r; ++x)
                {
                    int z = h_segment[x - x_l];
                    float color = c_segment[x - x_l] / 100f;
                    //i, j, z - координаты в пространстве, в пикчербоксе x, y
                    //int xx = (x + width / 2) % width;
                    //int yy = (-y + height / 2) % height;
                    int xx = x + width / 2;
                    int yy = -y + height / 2;
                    if (xx < 0 || xx > width || yy < 0 || yy > height || (xx * height + yy) < 0 || (xx * height + yy) > (buff.Length - 1))
                        continue;
                    if (z > buff[xx * height + yy])
                    {
                        buff[xx * height + yy] = (int)(z + 0.5);
                        colors[xx * height + yy] = color;
                    }
                }
            }

        }


        private void G_magic(Edge camera, Point3d P0, Point3d P1, Point3d P2, int[] buff, int width, int height, float[] colors, float c_P0, float c_P1, float c_P2)
        {
            //сортируем p0, p1, p2: y0 <= y1 <= y2
            PointF p0 = P0.make_perspective();
            PointF p1 = P1.make_perspective();
            PointF p2 = P2.make_perspective();

            if (p1.Y < p0.Y)
            {
                Point3d tmpp = new Point3d(P0);
                P0.X = P1.X; P0.Y = P1.Y; P0.Z = P1.Z;
                P1.X = tmpp.X; P1.Y = tmpp.Y; P1.Z = tmpp.Z;
                PointF tmppp = new PointF(p0.X, p0.Y);
                p0.X = p1.X; p0.Y = p1.Y;
                p1.X = tmppp.X; p1.Y = tmppp.Y;
                var tmpc = c_P1;
                c_P1 = c_P0;
                c_P0 = tmpc;
            }
            if (p2.Y < p0.Y)
            {
                Point3d tmpp = new Point3d(P0);
                P0.X = P2.X; P0.Y = P2.Y; P0.Z = P2.Z;
                P2.X = tmpp.X; P2.Y = tmpp.Y; P2.Z = tmpp.Z;
                PointF tmppp = new PointF(p0.X, p0.Y);
                p0.X = p2.X; p0.Y = p2.Y;
                p2.X = tmppp.X; p2.Y = tmppp.Y;
                var tmpc = c_P2;
                c_P2 = c_P0;
                c_P0 = tmpc;
            }
            if (p2.Y < p1.Y)
            {
                Point3d tmpp = new Point3d(P1);
                P1.X = P2.X; P1.Y = P2.Y; P1.Z = P2.Z;
                P2.X = tmpp.X; P2.Y = tmpp.Y; P2.Z = tmpp.Z;
                PointF tmppp = new PointF(p1.X, p1.Y);
                p1.X = p2.X; p1.Y = p2.Y;
                p2.X = tmppp.X; p2.Y = tmppp.Y;
                var tmpc = c_P1;
                c_P1 = c_P2;
                c_P2 = tmpc;
            }

            G_DrawFilledTriangle(camera, P0, P1, P2, buff, width, height, colors, c_P0, c_P1, c_P2);
        }


        public void calc_gouraud(Edge camera, int width, int height, out float[] intensive, Point3d light)
        {
            int[] buf = new int[width * height];
            for (int i = 0; i < width * height; ++i)
                buf[i] = int.MinValue;
            intensive = new float[width * height];
            for (int i = 0; i < width * height; ++i)
                intensive[i] = 0;

            create_map(camera, light);
            foreach (var f in Faces)
            {
                //треугольник
                Point3d P0 = new Point3d(f.Points[0]);
                Point3d P1 = new Point3d(f.Points[1]);
                Point3d P2 = new Point3d(f.Points[2]);
                float i_p0 = point_to_intensive[P0], i_p1 = point_to_intensive[P1], i_p2 = point_to_intensive[P2];
                G_magic(camera, P0, P1, P2, buf, width, height, intensive, i_p0, i_p1, i_p2);
                //4
                if (f.Points.Count > 3)
                {
                    P0 = new Point3d(f.Points[2]);
                    P1 = new Point3d(f.Points[3]);
                    P2 = new Point3d(f.Points[0]);
                    i_p0 = point_to_intensive[P0]; i_p1 = point_to_intensive[P1]; i_p2 = point_to_intensive[P2];
                    G_magic(camera, P0, P1, P2, buf, width, height, intensive, i_p0, i_p1, i_p2);
                }
                //5
                if (f.Points.Count > 4)
                {
                    P0 = new Point3d(f.Points[3]);
                    P1 = new Point3d(f.Points[4]);
                    P2 = new Point3d(f.Points[0]);
                    i_p0 = point_to_intensive[P0]; i_p1 = point_to_intensive[P1]; i_p2 = point_to_intensive[P2];
                    G_magic(camera, P0, P1, P2, buf, width, height, intensive, i_p0, i_p1, i_p2);
                }
            }

            SortedSet<float> test = new SortedSet<float>();
            for (int i = 0; i < width * height; ++i)
                test.Add(intensive[i]);

            int max = 0;
        }


        public void show_camera(Graphics g, Camera camera, Pen pen = null)
        {
            if (is_graph)
            {
                //this.rotate(-90, Axis.AXIS_Y);
 
             //   floating_horizon(g, camera, pen);
                //show(g);

                //this.rotate(90, Axis.AXIS_Y);

            }
            else
                foreach (Face f in Faces)
                {
                    f.find_normal(Center, camera.view);
                    if (f.IsVisible)
                    {
                        //float k = (float)Math.Sqrt(
                        //    (camera.P1.X - Center.X) * (camera.P1.X - Center.X) + 
                        //    (camera.P1.Y - Center.Y) * (camera.P1.Y - Center.Y) +
                        //    (camera.P1.Z - Center.Z) * (camera.P1.Z - Center.Z));
                        //List<PointF> pts = f.make_perspective(k/*1000*/);
                        //g.DrawLines(pen, pts.ToArray());
                        //g.DrawLine(pen, pts[0], pts[pts.Count - 1]);
                        f.show(g, Projection.PERSPECTIVE, pen, camera.view); //, camera.P1.Z);
                    }
                }
        }

        /* Floating horizon stuff */
        
        // ПОПЫТКА НОМЕР N
        public void Floating_horizon(Bitmap bmp, Camera camera, Pen up_pen = null, Pen down_pen = null)
        {
            if (up_pen == null)
                up_pen = Pens.Black;
            if (down_pen == null)
                down_pen = Pens.LightGray;

            



        }

        private void DrawLine(Point p1, Point p2, ref Bitmap bmp, ref Dictionary<double, double> Ymax, ref Dictionary<double, double> Ymin, Pen up_pen = null, Pen down_pen = null)
        {
            Color up_color, down_color;
            if (up_pen == null)
                up_color = Color.Black;
            else up_color = up_pen.Color;
            if (down_pen == null)
                down_color = Color.LightGray;
            else down_color = down_pen.Color;



            int dx = Math.Abs(p2.X - p1.X);
            int dy = Math.Abs(p2.Y - p1.Y);

            int sx = p2.X >= p1.X ? 1 : -1;
            int sy = p2.Y >= p1.Y ? 1 : -1;

            int first_x = p2.X > p1.X ? p1.X : p2.X;
            int last_x = p2.X > p1.X ? p2.X : p1.X;

            for (int i = first_x; i <= 2 * last_x + dx; ++i)
                if (!Ymax.Keys.Contains(i))
                { Ymax[i] = Ymin[i] = Int32.MaxValue; }

            if (dy <= dx)
            {
                int d = -dx;
                int d1 = dy << 1;
                int d2 = (dy - dx) << 1;
                for (int x = p1.X, y = p1.Y, i = 0; i <= dx; i++, x += sx)
                {
                    if (!Ymin.Keys.Contains(x))
                    {
                        Ymin.Add(x, Int32.MaxValue);
                        Ymax.Add(x, Int32.MaxValue);
                    }

                    if (Ymin[x] == Int32.MaxValue) // YMin, YMax not inited
                    {
                        if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
                            bmp.SetPixel(x, y, up_color);
                        Ymin[x] = Ymax[x] = y;
                    }
                    else
                    if (y < Ymin[x])
                    {
                        if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
                            bmp.SetPixel(x, y, up_color);
                        Ymin[x] = y;
                    }
                    else
                    if (y > Ymax[x])
                    {
                        if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
                            bmp.SetPixel(x, y, down_color);
                        Ymax[x] = y;
                    }

                    if (d > 0)
                    {
                        d += d2;
                        y += sy;
                    }
                    else
                        d += d1;
                }
            }
            else
            {
                int d = -dy; // or just dy?
                int d1 = dx << 1;
                int d2 = (dx - dy) << 1;

                if (!Ymin.Keys.Contains(p1.X))
                {
                    Ymin.Add(p1.X, Int32.MaxValue);
                    Ymax.Add(p1.X, Int32.MaxValue);
                }

                double m1 = Ymin[p1.X];
                double m2 = Ymax[p1.X];

                for (int x = p1.X, y = p1.Y, i = 0; i <= dy; i++, y += sy)
                {
                    if (!Ymin.Keys.Contains(x))
                    {
                        Ymax.Add(x, Int32.MaxValue);
                        Ymin.Add(x, Int32.MaxValue);
                    }

                    if (Ymin[x] == Int32.MaxValue) // YMin, YMax not inited
                    {
                        if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
                            bmp.SetPixel(x, y, up_color);
                        Ymin[x] = Ymax[x] = y;
                    }
                    else
                    if (y < m1)
                    {
                        if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
                            bmp.SetPixel(x, y, up_color);
                        if (y < Ymin[x])
                            Ymin[x] = y;

                    }
                    else
                    if (y > m2)
                    {
                        if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
                            bmp.SetPixel(x, y, down_color);
                        if (y > Ymax[x])
                            Ymax[x] = y;
                    }

                    if (d > 0)
                    {
                        d += d2;
                        x += sx;

                        if (!Ymin.Keys.Contains(x))
                        {
                            Ymax.Add(x, Int32.MaxValue);
                            Ymin.Add(x, Int32.MaxValue);
                        }

                        m1 = Ymin[x];
                        m2 = Ymax[x];
                    }
                    else
                        d += d1;
                }
            }

        }

        public void PlotSurface(ref Bitmap bmp)
        {
            HashSet<Point3d> pts = new HashSet<Point3d>(new Point3dEqComparer());

            double x1, x2, y1, y2, fmin, fmax;
            int n1, n2;
            n1 = n2 = graph_breaks;

            x1 = y1 = fmin = Int32.MaxValue;
            x2 = y2 = fmax = Int32.MinValue;

            foreach (var f in Faces)
                foreach (var p in f.Points)
                {
                    if (p.X < x1)
                        x1 = p.X;
                    if (p.X > x2)
                        x2 = p.X;
                    if (p.Y < y1)
                        y1 = p.Y;
                    if (p.Y > y2)
                        y2 = p.Y;
                    if (p.Z < fmin)
                        fmin = p.Z;
                    if (p.Z > fmax)
                        fmax = p.Z;

                }



            Dictionary<double, double> Ymax = new Dictionary<double, double>();
            Dictionary<double, double> Ymin = new Dictionary<double, double>();

            Pen up_pen = null;
            Pen down_pen = null;

            //double phiz = Double.Parse(textBox6.Text) * 3.1415926 / 180;
            //double psiz = Double.Parse(textBox7.Text) * 3.1415926 / 180;

            //double phi = 30 * 3.1415926 / 180 + phiz;
            //double psi = 10 * 3.1415926 / 180 + psiz;

            double phi = 30 * Math.PI / 180;
            double psi = 10 * Math.PI / 180;

            double sphi = Math.Sin(phi);
            double cphi = Math.Cos(phi);
            double spsi = Math.Sin(psi);
            double cpsi = Math.Cos(psi);

            double[] e1 = { cphi, sphi, 0 };
            double[] e2 = { spsi * sphi, spsi * cphi, cpsi };

            double x, y;
            double hx = (x2 - x1) / n1;
            double hy = (y2 - y1) / n2;

            double xmin = (e1[0] >= 0 ? x1 : x2) * e1[0] + (e1[1] >= 0 ? y1 : y2) * e1[1];
            double xmax = (e1[0] >= 0 ? x2 : x1) * e1[0] + (e1[1] >= 0 ? y2 : y1) * e1[1];
            double ymin = (e2[0] >= 0 ? x1 : x2) * e2[0] + (e2[1] >= 0 ? y1 : y2) * e2[1];
            double ymax = (e2[0] >= 0 ? x2 : x1) * e2[0] + (e2[1] >= 0 ? y2 : y1) * e2[1];

            if (e2[2] >= 0)
            {
                ymin += fmin * e2[2];
                ymax += fmax * e2[2];
            }
            else
            {
                ymin += fmax * e2[2];
                ymax += fmin * e2[2];
            }

            double ax = 10 - bmp.Width * xmin / (xmax - xmin);
            double bx = bmp.Width / (xmax - xmin);
            double ay = 10 - bmp.Height * ymin / (ymax - ymin);
            double by = bmp.Height / (ymax - ymin);

            for (int i = 0; i < Math.Abs(x2 - x1); i++)
                Ymin[i] = Ymax[i] = Int32.MaxValue;

            for (int i = 0; i < Ymax.Count; ++i)
                Ymin[i] = Ymax[i] = Int32.MaxValue;

            Point[] CurLine = new Point[n1];
            Point[] NextLine = new Point[n1];

            for (int i = 0; i < n1; ++i)
            {
                x = x1 + i * hx;
                y = y1 + (n2 - 1) * hy;
                CurLine[i].X = (int)(ax + bx * (x * e1[0] + y * e1[1]));
                CurLine[i].Y = (int)(ay + by * (x * e2[0] + y * e2[1] + graph_function((float)x, (float)(y)) * e2[2]));
            }

            for (int i = n2 - 1; i > -1; --i)
            {
                for (int j = 0; j < n1 - 1; ++j)
                    DrawLine(CurLine[j], CurLine[j + 1], ref bmp, ref Ymax, ref Ymin, up_pen, down_pen);

                if (i > 0)
                    for (int j = 0; j < n1; ++j)
                    {
                        x = x1 + j * hx;
                        y = y1 * (i - 1) + hy;

                        NextLine[j].X = (int)(ax + bx * (x * e1[0] + y * e1[1]));
                        NextLine[j].Y = (int)(ay + by * (x * e2[0] + y * e2[1] + graph_function((float)x, (float)y) * e2[2]));

                        DrawLine(CurLine[j], NextLine[j], ref bmp, ref Ymax, ref Ymin, up_pen, down_pen);
                        
                    }
            }

        }

        /* End of floating horizon stuff */




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
            if (Faces != null )
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
                    new Point3d(-cube_half_size, cube_half_size, cube_half_size),
                    new Point3d(cube_half_size, cube_half_size, cube_half_size),
                    new Point3d(cube_half_size, -cube_half_size, cube_half_size),
                    new Point3d(-cube_half_size, -cube_half_size, cube_half_size)
                }
            );
            

            Faces = new List<Face>{ f }; // front face

            
            List<Point3d> l1 = new List<Point3d>();
            // back face
            foreach (var point in f.Points)
            {
                l1.Add(new Point3d(point.X, point.Y, point.Z - 2*cube_half_size));
            }
            Face f1 = new Face(
                    new List<Point3d>
                    {
                        new Point3d(-cube_half_size, cube_half_size, -cube_half_size),
                        new Point3d(-cube_half_size, -cube_half_size, -cube_half_size),
                        new Point3d(cube_half_size, -cube_half_size, -cube_half_size),
                        new Point3d(cube_half_size, cube_half_size, -cube_half_size)
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
            Face f5 = new Face(l5);
            Faces.Add(f5);

            Cube_size = 2*cube_half_size;
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

            Face f1 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[1].Points[3]),
                    new Point3d(cube.Faces[1].Points[1]),
                    new Point3d(cube.Faces[0].Points[2])
                }
            );

            Face f2 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[0].Points[2]),
                    new Point3d(cube.Faces[1].Points[1]),
                    new Point3d(cube.Faces[0].Points[0])
                }
            );

            Face f3 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[0].Points[2]),
                    new Point3d(cube.Faces[0].Points[0]),
                    new Point3d(cube.Faces[1].Points[3])
                }
            );

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

            Face f1 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[2].Center),
                    new Point3d(cube.Faces[1].Center),
                    new Point3d(cube.Faces[5].Center)
                }
            );

            Face f2 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[2].Center),
                    new Point3d(cube.Faces[5].Center),
                    new Point3d(cube.Faces[0].Center)
                }
            );

            Face f3 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[2].Center),
                    new Point3d(cube.Faces[0].Center),
                    new Point3d(cube.Faces[4].Center)
                }
            );

            // down
            Face f4 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[3].Center),
                    new Point3d(cube.Faces[1].Center),
                    new Point3d(cube.Faces[4].Center)
                }
            );

            Face f5 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[3].Center),
                    new Point3d(cube.Faces[1].Center),
                    new Point3d(cube.Faces[5].Center)
                }
            );

            Face f6 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[3].Center),
                    new Point3d(cube.Faces[5].Center),
                    new Point3d(cube.Faces[0].Center)
                }
            );

            Face f7 = new Face(
                new List<Point3d>
                {
                    new Point3d(cube.Faces[3].Center),
                    new Point3d(cube.Faces[0].Center),
                    new Point3d(cube.Faces[4].Center)
                }
            );

            Faces = new List<Face> { f0, f1, f2, f3, f4, f5, f6, f7 };
            find_center();
        }

        public void make_icosahedron()
        {
            Faces = new List<Face>();

            float size = 100;

            float r1 = size * (float)Math.Sqrt(3) / 4;   // половина высоты правильного треугольника - для высоты цилиндра
            float r = size * (3 + (float)Math.Sqrt(5)) / (4 * (float)Math.Sqrt(3)); // радиус вписанной сферы - для правильных пятиугольников

            Point3d up_center = new Point3d(0, -r1, 0);  // центр верхней окружности
            Point3d down_center = new Point3d(0, r1, 0); // центр нижней окружности

            // up
            double a = Math.PI / 2;
            List<Point3d> up_points = new List<Point3d>();
            for (int i = 0; i < 5; ++i)
            {
                up_points.Add( new Point3d(up_center.X + r * (float)Math.Cos(a), up_center.Y, up_center.Z - r * (float)Math.Sin(a)));
                a += 2 * Math.PI / 5;
            }

            // down
            a = Math.PI / 2 - Math.PI / 5;
            List<Point3d> down_points = new List<Point3d>();
            for (int i = 0; i < 5; ++i)
            {
                down_points.Add(new Point3d(down_center.X + r * (float)Math.Cos(a), down_center.Y, down_center.Z - r * (float)Math.Sin(a)));
                a += 2 * Math.PI / 5;
            }

            var R = Math.Sqrt(2*(5 + Math.Sqrt(5))) * size / 4; // радиус описанной сферы - для пирамидок над цилиндром

            Point3d p_up = new Point3d(up_center.X, (float)(-R), up_center.Z);
            Point3d p_down = new Point3d(down_center.X, (float)R, down_center.Z);

            // upper faces
            for (int i = 0; i < 5; ++i)
            {
                Faces.Add(
                    new Face(new List<Point3d>
                    {
                        new Point3d(p_up),
                        new Point3d(up_points[i]),
                        new Point3d(up_points[(i+1) % 5]),
                    })
                    );
            }

            // lower faces
            for (int i = 0; i < 5; ++i)
            {
                Faces.Add(
                    new Face(new List<Point3d>
                    {
                        new Point3d(p_down),
                        new Point3d(down_points[i]),
                        new Point3d(down_points[(i+1) % 5]),
                    })
                    );
            }
            
            // vertical
            for (int i = 0; i < 5; ++i)
            {
                // triangle \/
                Faces.Add(
                    new Face(new List<Point3d>
                    {
                        new Point3d(up_points[i]),
                        new Point3d(up_points[(i+1) % 5]),
                        new Point3d(down_points[(i+1) % 5])
                    })
                    );

                // triangle /\
                Faces.Add(
                    new Face(new List<Point3d>
                    {
                        new Point3d(up_points[i]),
                        new Point3d(down_points[i]),
                        new Point3d(down_points[(i+1) % 5])
                    })
                    );
            }
            
            find_center();
        }

        public void make_dodecahedron()
        {
            Faces = new List<Face>();
            Polyhedron ik = new Polyhedron();
            ik.make_icosahedron();

            List<Point3d> pts = new List<Point3d>();
            foreach (Face f in ik.Faces)
            {
                pts.Add(f.Center);
            }

            // up
            Faces.Add(new Face(new List<Point3d>
            {
                new Point3d(pts[0]),
                new Point3d(pts[1]),
                new Point3d(pts[2]),
                new Point3d(pts[3]),
                new Point3d(pts[4])
            }));

            // down
            Faces.Add(new Face(new List<Point3d>
            {
                new Point3d(pts[5]),
                new Point3d(pts[6]),
                new Point3d(pts[7]),
                new Point3d(pts[8]),
                new Point3d(pts[9])
            }));

            // side / up
            for (int i = 0; i < 5; ++i)
            {
                Faces.Add(new Face(new List<Point3d>
                {
                    new Point3d(pts[i]),
                    new Point3d(pts[(i + 1) % 5]),
                    new Point3d(pts[(i == 4) ? 10 : 2*i + 12]),
                    new Point3d(pts[(i == 4) ? 11 : 2*i + 13]),
                    new Point3d(pts[2*i + 10])
                }));
            }

           Faces.Add(new Face(new List<Point3d>
            {
                new Point3d(pts[5]),
                new Point3d(pts[6]),
                new Point3d(pts[13]),
                new Point3d(pts[10]),
                new Point3d(pts[11])
            }));
             Faces.Add(new Face(new List<Point3d>
            {
                new Point3d(pts[6]),
                new Point3d(pts[7]),
                new Point3d(pts[15]),
                new Point3d(pts[12]),
                new Point3d(pts[13])
            }));
            Faces.Add(new Face(new List<Point3d>
            {
                new Point3d(pts[7]),
                new Point3d(pts[8]),
                new Point3d(pts[17]),
                new Point3d(pts[14]),
                new Point3d(pts[15])
            }));
            Faces.Add(new Face(new List<Point3d>
            {
                new Point3d(pts[8]),
                new Point3d(pts[9]),
                new Point3d(pts[19]),
                new Point3d(pts[16]),
                new Point3d(pts[17])
            }));
            Faces.Add(new Face(new List<Point3d>
            {
                new Point3d(pts[9]),
                new Point3d(pts[5]),
                new Point3d(pts[11]),
                new Point3d(pts[18]),
                new Point3d(pts[19])
            }));

            find_center();
        }

        private void make_rotation_figure(int cnt_breaks, Edge rot_line, List<Point3d> pts)
        {
            double angle = 360.0 / cnt_breaks;
            float Ax = rot_line.P1.X, Ay = rot_line.P1.Y, Az = rot_line.P1.Z;
            foreach(var p in pts)
                p.translate(-Ax, -Ay, -Az);

            List<Point3d> new_pts = new List<Point3d>();
            foreach (var p in pts)
                new_pts.Add(new Point3d(p.X, p.Y, p.Z));


            for (int i = 0; i < cnt_breaks; ++i)
            {
                foreach (var np in new_pts)
                    np.rotate(angle, Axis.OTHER, rot_line);
                for (int j = 1; j < pts.Count; ++j)
                {
                    Face f = new Face(new List<Point3d>(){ new Point3d(pts[j - 1]), new Point3d(new_pts[j - 1]),
                        new Point3d(new_pts[j]), new Point3d(pts[j])});
                    Faces.Add(f);
                }
                foreach (var p in pts)
                    p.rotate(angle, Axis.OTHER, rot_line);
            }


            foreach (var f in Faces)
                f.translate(Ax, Ay, Az);
            find_center();
        }
    }

    public class Camera
    {
        public Edge view = new Edge(new Point3d(0, 0, 300), new Point3d(0, 0, 250));
        Polyhedron small_cube = new Polyhedron();
        public Edge rot_line { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Camera(int w, int h)
        {
            int camera_half_size = 5;
            small_cube.make_hexahedron(camera_half_size);
            small_cube.translate(view.P1.X, view.P1.Y, view.P1.Z);
            set_rot_line();
            Width = w;
            Height = h;
        }

        public void set_rot_line(Axis a = Axis.AXIS_X)
        {
            Point3d p1, p2;
            p1 = new Point3d(view.P1);
            switch (a)
            {
                case Axis.AXIS_Y:
                    p2 = new Point3d(p1.X, p1.Y + 10, p1.Z);
                    break;
                case Axis.AXIS_Z:
                    p2 = new Point3d(p1.X, p1.Y, p1.Z + 10);
                    break;
                default:
                    p2 = new Point3d(p1.X + 10, p1.Y, p1.Z);
                    break;
            }
            rot_line = new Edge(p1, p2);
        }

        public void show(Graphics g, Projection pr = 0, int x = 0, int y = 0, int z = 0, Pen pen = null)
        {
            pen = Pens.Red;
            //view.translate(x, y, z);
            //view.show(g, pr, pen);
            //view.translate(-x, -y, -z);

            //small_cube.translate(x, y, z);
            //small_cube.show(g, pr, pen);
            //small_cube.translate(-x, -y, -z);
        }
        

        public void translate(float x, float y, float z)
        {
            view.translate(x, y, z);
            small_cube.translate(x, y, z);
            rot_line.translate(x, y, z);
        }

        public void rotate(double angle, Axis a, Edge line = null)
        {
            view.rotate(angle, a, line);
            small_cube.rotate(angle, a, line);
            rot_line.rotate(angle, a, line);
        }
    }
    
    public sealed class Point3dEqComparer : IEqualityComparer<Point3d>
    {
        public bool Equals(Point3d x, Point3d y)
        {
            return x.X.Equals(y.X) && x.Y.Equals(y.Y) && x.Z.Equals(y.Z);
        }

        public int GetHashCode(Point3d obj)
        {
            return obj.X.GetHashCode() + obj.Y.GetHashCode() + obj.Z.GetHashCode();
        }
    }

    public sealed class Point3dComparer : IComparer<Point3d>
    {
        public int Compare(Point3d p1, Point3d p2)
        {
            if (p1.X.Equals(p2.X))
            {
                if (p1.Y.Equals(p2.Y))
                    return p1.Z.CompareTo(p2.Z);
                else return p1.Y.CompareTo(p2.Y);
            }
            else return p1.X.CompareTo(p2.X);
        }
    }

    public sealed class PointFComparer : IComparer<PointF>
    {
        public int Compare(PointF p1, PointF p2)
        {
            if (p1.X.Equals(p2.X))
                return p1.Y.CompareTo(p2.Y);
            else return p1.X.CompareTo(p2.X);
        }
    }

    public sealed class PointComparer : IComparer<Point>
    {
        public int Compare(Point p1, Point p2)
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

    public class PointFEqComparer : IEqualityComparer<PointF>
    {
        public int GetHashCode(PointF obj)
        {
            return (obj.X.GetHashCode() ^ obj.Y.GetHashCode());
        }
        public bool Equals(PointF obj1, PointF obj2)
        {
            return (obj1.X.Equals(obj2.X) && obj1.Y.Equals(obj2.Y));
        }
    }
}
