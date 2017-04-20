using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace NodeControl
{

    internal static class NodeExtensions
    {
        public static Point Center(this Rectangle r)
        {
            return new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
        }

        public static bool IntersectsWithLine(this Rectangle rect,
                              double a_p1x,
                              double a_p1y,
                              double a_p2x,
                              double a_p2y)
        {
            // Find min and max X for the segment


            double a_rectangleMinX = rect.Left;
            double a_rectangleMinY = rect.Top;
            double a_rectangleMaxX = rect.Right;
            double a_rectangleMaxY = rect.Bottom;

            double minX = a_p1x;
            double maxX = a_p2x;

            if (a_p1x > a_p2x)
            {
                minX = a_p2x;
                maxX = a_p1x;
            }

            // Find the intersection of the segment's and rectangle's x-projections

            if (maxX > a_rectangleMaxX)
            {
                maxX = a_rectangleMaxX;
            }

            if (minX < a_rectangleMinX)
            {
                minX = a_rectangleMinX;
            }

            if (minX > maxX) // If their projections do not intersect return false
            {
                return false;
            }

            // Find corresponding min and max Y for min and max X we found before

            double minY = a_p1y;
            double maxY = a_p2y;

            double dx = a_p2x - a_p1x;

            if (Math.Abs(dx) > 0.0000001)
            {
                double a = (a_p2y - a_p1y) / dx;
                double b = a_p1y - a * a_p1x;
                minY = a * minX + b;
                maxY = a * maxX + b;
            }

            if (minY > maxY)
            {
                double tmp = maxY;
                maxY = minY;
                minY = tmp;
            }

            // Find the intersection of the segment's and rectangle's y-projections

            if (maxY > a_rectangleMaxY)
            {
                maxY = a_rectangleMaxY;
            }

            if (minY < a_rectangleMinY)
            {
                minY = a_rectangleMinY;
            }

            if (minY > maxY) // If Y-projections do not intersect return false
            {
                return false;
            }

            return true;
        }



        public static Rectangle GetBoundingBox(IEnumerable<Rectangle> rectangles)
        {
            int left = int.MaxValue;
            int top = int.MaxValue;
            int right = int.MinValue;
            int bottom = int.MinValue;

            foreach (var area in rectangles)
            {
                if (left > area.Left)
                    left = area.Left;

                if (right < area.Right)
                    right = area.Right;

                if (top > area.Top)
                    top = area.Top;

                if (bottom < area.Bottom)
                    bottom = area.Bottom;
            }
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        public static Point RoundTo(this Point pt, int width, int height)
        {
            if (width == 0) width = 1;
            if (height == 0) height = 1;
            return new Point((int)Math.Round(pt.X / (float)width) * width, (int)Math.Round(pt.Y / (float)height) * height);
        }

        public static Size RoundTo(this Size pt, int width, int height)
        {
            if (width == 0) width = 1;
            if (height == 0) height = 1;
            return new Size((int)Math.Round(pt.Width / (float)width) * width, (int)Math.Round(pt.Height / (float)height) * height);
        }

        public static bool AreEqual<T>(this IList<T> lst1, IList<T> lst2)
        {
            if (lst1.Count != lst2.Count)
                return false;
            for (int i = 0; i < lst1.Count; i++)
            {
                if (!object.Equals(lst1[i], lst2[i]))
                    return false;
            }

            return true;
        }

        public static Rectangle RoundTo(this Rectangle pt, int width, int height)
        {
            return new Rectangle(pt.Location.RoundTo(width, height), pt.Size.RoundTo(width, height));
        }

        public static Rectangle Zoom(this Rectangle pt, float zoom)
        {
            return Rectangle.FromLTRB((int)(pt.Left * zoom), (int)(pt.Top * zoom), (int)(pt.Right * zoom), (int)(pt.Bottom * zoom));
        }


    }
}
