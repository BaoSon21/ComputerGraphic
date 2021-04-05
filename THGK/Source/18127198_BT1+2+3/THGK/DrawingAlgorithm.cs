using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THGK
{ 
    class DrawingAlgorithm
    {     
        // draw a line with start point and end point
        public static void Line(Shape a, Point pStart, Point pEnd)
        {
            //start form smaller x
            if (pStart.X > pEnd.X)
                (pStart, pEnd) = (pEnd, pStart);

            //Translate pStart coincides with coor O
            Point move = new Point(pStart.X, pStart.Y);
            (pStart.X, pStart.Y) = (0, 0);
            (pEnd.X, pEnd.Y) = (pEnd.X - move.X, pEnd.Y - move.Y);

            //If dx = 0, vertical line up
            if (pEnd.X == 0)
            {
                for (int i = Math.Min(0, pEnd.Y); i <= Math.Max(0, pEnd.Y); i++)
                    a.listPoints.Add(new Point(move.X, i + move.Y));
                return;
            }

            //Bresenham method
            int dy2 = 2 * pEnd.Y, dx2 = 2 * pEnd.X;
            float m = (float)dy2 / dx2;
            bool negativeM = false, largeM = false;

            //If m < 0, symmetry through x=0
            if (m < 0)
            {
                pEnd.Y = -pEnd.Y;
                dy2 = -dy2;
                m = -m;
                negativeM = true;
            }
            //If m > 1, symmetry through y=x
            if (m > 1)
            {
                (pEnd.X, pEnd.Y) = (pEnd.Y, pEnd.X);
                (dy2, dx2) = (dx2, dy2);
                largeM = true;
            }

            //calculate p0
            int p = dy2 - pEnd.X;
            List<Point> points = new List<Point>();

            int x = 0, y = 0;
            points.Add(new Point(x, y));
            while (x < pEnd.X)
            {
                if (p >= 0)
                {
                    x++;
                    y++;
                    p += dy2 - dx2;
                }
                else
                {
                    x++;
                    p += dy2;
                }
                points.Add(new Point(x, y));
            }

            //symmetry through y=x (if necessary)
            if (largeM == true)
                for (int i = 0; i < points.Count; i++)
                    points[i] = new Point(points[i].Y, points[i].X);

            //symmetry through y = 0 (if necessary)
            if (negativeM == true)
                for (int i = 0; i < points.Count; i++)
                    points[i] = new Point(points[i].X, -points[i].Y);

            //add all to list drawpoint
            for (int i = 0; i < points.Count; i++)
                a.listPoints.Add(new Point(points[i].X + move.X, points[i].Y + move.Y));

            //Create center point
            double centerPointX = 0, centerPointY = 0;
            for (int i = 0; i < a.controlPoints.Count; i++)
            {
                centerPointX += a.controlPoints[i].X;
                centerPointY += a.controlPoints[i].Y;
            }
            centerPointX /= a.controlPoints.Count;
            centerPointY /= a.controlPoints.Count;
            a.centerPoint = new Tuple<double, double>(centerPointX, centerPointY);

            //Create rotate and scale point
            double dx = a.controlPoints[1].X - centerPointX, dy = a.controlPoints[1].Y - centerPointY;
            double len = Math.Sqrt(dx * dx + dy * dy);
            AffineTransformation transformer = new AffineTransformation();
            transformer.Translate(30 * dx / len, 30 * dy / len);
            a.extraPoint = transformer.Transform(a.controlPoints[1]);

            points.Clear();
        }

        // draw a line circle start point and end point
        public static void Circle(Shape a, Point pStart, Point pEnd)
        {
            //get set of points through counter-clockwise
            //calculate p0 and r
            double r = Math.Sqrt(Math.Pow(pStart.X - pEnd.X, 2) + Math.Pow(pStart.Y - pEnd.Y, 2)) / 2;
            double decision = 5 / 4 - r;

            //fisrt point (0, r)
            int x = 0;
            int y = (int)r; // get interger value

            //Center of circle is also vector translate
            Point pcenterPoint = new Point((pStart.X + pEnd.X) / 2, (pStart.Y + pEnd.Y) / 2);

            //Listpoint in corner 8 and symmetry it through y=x
            List<Point> partsOfCircle = new List<Point>();

            //add first point to list points.
            partsOfCircle.Add(new Point(x, y));
            a.listPoints.Add(new Point(x + pcenterPoint.X, y + pcenterPoint.Y));

            int x2 = x * 2, y2 = y * 2;

            //Midpoint algorithm
            while (y > x)
            {
                if (decision < 0)
                {
                    decision += x2 + 3;
                    x++;
                    x2 += 2;
                }
                else
                {
                    decision += x2 - y2 + 5;
                    x++;
                    y--;
                    x2 += 2;
                    y2 -= 2;
                }

                //translate each point arrcoding center,add to list point
                partsOfCircle.Add(new Point(x, y));
                a.listPoints.Add(new Point(x + pcenterPoint.X, y + pcenterPoint.Y));
            }
            //symmetry through y=x and translate through center
            Point p;
            int i, size = partsOfCircle.Count();
            for (i = size - 1; i >= 0; i--)
            {
                p = partsOfCircle[i];
                partsOfCircle.Add(new Point(p.Y, p.X));
                a.listPoints.Add(new Point(p.Y + pcenterPoint.X, p.X + pcenterPoint.Y));
            }

            //symmetry 1/4 through y=0 and translate through center
            size = partsOfCircle.Count();
            for (i = size - 1; i >= 0; i--)
            {
                p = partsOfCircle[i];
                partsOfCircle.Add(new Point(p.X, -p.Y));
                a.listPoints.Add(new Point(p.X + pcenterPoint.X, -p.Y + pcenterPoint.Y));
            }

            //symmetry 1/2 through x=0 and translate through center
            size = partsOfCircle.Count();
            for (i = size - 1; i >= 0; i--)
            {
                p = partsOfCircle[i];
                a.listPoints.Add(new Point(-p.X + pcenterPoint.X, p.Y + pcenterPoint.Y));
            }

            //create center
            double centerPointX = 0, centerPointY = 0;
            for (i = 0; i < a.controlPoints.Count; i++)
            {
                centerPointX += a.controlPoints[i].X;
                centerPointY += a.controlPoints[i].Y;
            }
            centerPointX /= a.controlPoints.Count;
            centerPointY /= a.controlPoints.Count;
            a.centerPoint = new Tuple<double, double>(centerPointX, centerPointY);

            //Create extraPoint (rotate/scale)
            double dx = pEnd.X - centerPointX, dy = pEnd.Y - centerPointY;
            double len = Math.Sqrt(dx * dx + dy * dy);
            AffineTransformation transformer = new AffineTransformation();
            transformer.Translate(30 * dx / len, 30 * dy / len);
            a.extraPoint = transformer.Transform(pEnd);

            partsOfCircle.Clear();
        }

        //draw rectangle
        public static void Rectangle(Shape a, Point pStart, Point pEnd)
        {
            //calculate 2 other points
            Point p1 = new Point(pEnd.X, pStart.Y);
            Point p2 = new Point(pStart.X, pEnd.Y);

            //get 4 point of rectangle
            a.controlPoints.Clear();
            a.controlPoints.Add(pStart);
            a.controlPoints.Add(p1);
            a.controlPoints.Add(pEnd);
            a.controlPoints.Add(p2);
            //draw rectangle by Pylogon function
            Polygon(a);
        }

        public static void Ellipse(Shape a, Point pStart, Point pEnd)
        {
            //calculate center point
            Point pcenterPoint = new Point((pStart.X + pEnd.X) / 2, (pStart.Y + pEnd.Y) / 2);
            //calculate rX,rY
            double rX = Math.Sqrt(Math.Pow(pStart.X - pEnd.X, 2) + Math.Pow(pStart.Y - pStart.Y, 2)) / 2;
            double rY = Math.Sqrt(Math.Pow(pStart.X - pStart.X, 2) + Math.Pow(pStart.Y - pEnd.Y, 2)) / 2;

            //first point (0, rY)
            int x = 0;
            int y = (int)rY;

            //list point at 1/4
            List<Point> oneFourth = new List<Point>();
            // add first point to list
            oneFourth.Add(new Point(x, y));
            a.listPoints.Add(new Point(x + pcenterPoint.X, y + pcenterPoint.Y));

            //Caculate 
            double rX2 = rX * rX, rY2 = rY * rY;
            double rX2y = 2 * rX2 * y;
            double rY2x = 2 * rY2 * x;
            double decision = rY2 - rX2 * rY + rX2 / 4;

            while (rY2x < rX2y)
            {
                if (decision < 0)
                {
                    x++;
                    rY2x += 2 * rY2;
                    decision += rY2x + rY2;
                }
                else
                {
                    x++;
                    y--;
                    rY2x += 2 * rY2;
                    rX2y -= 2 * rX2;
                    decision += rY2x - rX2y + rY2;
                }
                //translate each point according to center and add to list point
                oneFourth.Add(new Point(x, y));
                a.listPoints.Add(new Point(x + pcenterPoint.X, y + pcenterPoint.Y));
            }

            //xLast, yLast
            rX2y = 2 * rX2 * y;
            rY2x = 2 * rY2 * x;
            decision = rY2 * Math.Pow((x + (1 / 2)), 2) + rX2 * Math.Pow((y - 1), 2) - rX2 * rY2;

            while (y >= 0)
            {
                if (decision > 0)
                {
                    y--;
                    rX2y -= 2 * rX2;
                    decision -= rX2y + rX2;
                }
                else
                {
                    x++;
                    y--;
                    rY2x += 2 * rY2;
                    rX2y -= 2 * rX2;
                    decision += rY2x - rX2y + rX2;
                }
                //translate each point according center and add to list points
                oneFourth.Add(new Point(x, y));
                a.listPoints.Add(new Point(x + pcenterPoint.X, y + pcenterPoint.Y));
            }
            //symmetry 1/4 through x = 0 and add to list points
            int size = oneFourth.Count();
            for (int i = size - 1; i >= 0; i--)
            {
                Point p = oneFourth[i];
                oneFourth.Add(new Point(p.X, -p.Y));
                a.listPoints.Add(new Point(p.X + pcenterPoint.X, -p.Y + pcenterPoint.Y));
            }

            //symmetry 1/2 through y = 0 and add to list points
            size = oneFourth.Count();
            for (int i = size - 1; i >= 0; i--)
            {
                Point p = oneFourth[i];
                oneFourth.Add(new Point(-p.X, p.Y));
                a.listPoints.Add(new Point(-p.X + pcenterPoint.X, p.Y + pcenterPoint.Y));
            }

            //create centerpoint
            double centerPointX = 0, centerPointY = 0;
            for (int i = 0; i < a.controlPoints.Count; i++)
            {
                centerPointX += a.controlPoints[i].X;
                centerPointY += a.controlPoints[i].Y;
            }
            centerPointX /= a.controlPoints.Count;
            centerPointY /= a.controlPoints.Count;
            a.centerPoint = new Tuple<double, double>(centerPointX, centerPointY);

            //create extrapoint (rotate/scale)
            double dx = pEnd.X - centerPointX, dy = pEnd.Y - centerPointY;
            double len = Math.Sqrt(dx * dx + dy * dy);
            AffineTransformation transformer = new AffineTransformation();
            transformer.Translate(25 * dx / len, 25 * dy / len);
            a.extraPoint = transformer.Transform(pEnd);
        }

        public static void Triangle(Shape a, Point pStart, Point pEnd)
        {
            // rotate pEnd angle 60 to get new controlpoint
            AffineTransformation transformer = new AffineTransformation();
            transformer.Translate(-pStart.X, -pStart.Y);
            transformer.Rotate(60 * Math.PI / 180);
            transformer.Translate(pStart.X, pStart.Y);
            Point p = transformer.Transform(pEnd);

            //list 3 points of triangel
            a.controlPoints.Clear();
            a.controlPoints.Add(pStart);
            a.controlPoints.Add(pEnd);
            a.controlPoints.Add(p);
            //use polygon function to draw triangle
            Polygon(a);
        }

        public static void Pentagon(Shape a, Point pStart, Point pEnd)
        {
            // rotate pStart angle 72 to get new controlpoint
            AffineTransformation transformer = new AffineTransformation();
            transformer.Translate(-pStart.X, -pStart.Y);
            transformer.Rotate(72 * Math.PI / 180);
            transformer.Translate(pStart.X, pStart.Y);

            Point p1 = transformer.Transform(pEnd);
            Point p2 = transformer.Transform(p1);
            Point p3 = transformer.Transform(p2);
            Point p4 = transformer.Transform(p3);

            //list 5 point of pentagon
            a.controlPoints.Clear();
            a.controlPoints.Add(pEnd);
            a.controlPoints.Add(p1);
            a.controlPoints.Add(p2);
            a.controlPoints.Add(p3);
            a.controlPoints.Add(p4);
            //use polygon function to draw pentagon
            Polygon(a);
        }

        public static void Hexagon(Shape a, Point pStart, Point pEnd)
        {

            // find rotate matrix angle 60 with center is start point
            AffineTransformation transformer = new AffineTransformation();
            transformer.Translate(-pStart.X, -pStart.Y);
            transformer.Rotate(60 * Math.PI / 180);
            transformer.Translate(pStart.X, pStart.Y);

            Point p1 = transformer.Transform(pEnd);
            Point p2 = transformer.Transform(p1);
            Point p3 = transformer.Transform(p2);
            Point p4 = transformer.Transform(p3);
            Point p5 = transformer.Transform(p4);

            //list 6 points of hexagon
            a.controlPoints.Clear();
            a.controlPoints.Add(pEnd);
            a.controlPoints.Add(p1);
            a.controlPoints.Add(p2);
            a.controlPoints.Add(p3);
            a.controlPoints.Add(p4);
            a.controlPoints.Add(p5);
            //use function polygon to draw
            Polygon(a);
        }

        public static void Polygon(Shape a)
        {
            //Connect adjacent points
            for (int i = 0; i < a.controlPoints.Count - 1; i++)
                Line(a, a.controlPoints[i], a.controlPoints[i + 1]);
            //Connect the end point with startpoint
            Line(a, a.controlPoints.Last(), a.controlPoints[0]);

            //create center
            double centerPointX = 0, centerPointY = 0;
            for (int i = 0; i < a.controlPoints.Count; i++)
            {
                centerPointX += a.controlPoints[i].X;
                centerPointY += a.controlPoints[i].Y;
            }
            centerPointX /= a.controlPoints.Count;
            centerPointY /= a.controlPoints.Count;
            a.centerPoint = new Tuple<double, double>(centerPointX, centerPointY);

            //create extrapoint(rotate/scale)
            double dx = a.controlPoints[1].X - centerPointX, dy = a.controlPoints[1].Y - centerPointY;
            double len = Math.Sqrt(dx * dx + dy * dy);
            AffineTransformation transformer = new AffineTransformation();
            transformer.Translate(25 * dx / len, 25 * dy / len);
            a.extraPoint = transformer.Transform(a.controlPoints[1]);
        }
    }
}
