using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace NodeControl
{
    /// <summary>
    /// A manager that can create line segments for various algorithms
    /// </summary>
    class LinkManager
    {
        /// <summary>
        /// Create line segments between the given points
        /// </summary>
        /// <param name="points">The first and last points are the source & destination, while the rest are control points inbetween</param>
        /// <param name="fragments">The number of fragments that should be generated</param>
        /// <param name="lineFragemntAction">The action that should be done with specified line segment and the overall progress value</param>
        public static void DoBezier(PointF[] points, int fragments, Action<PointF, PointF, float> lineFragemntAction)
        {
            if (points.Length >= 2)
            {
                PointF lastPoint = points[0];
                for (int i = 0; i < fragments; i++)
                {
                    PointF curPoint = LinkManager.DoBezier(points, lastPoint, i / (float)(fragments - 1), lineFragemntAction);
                    lastPoint = curPoint;
                }
            }
        }

        /// <summary>
        /// Create line segments between the given points
        /// </summary>
        /// <param name="points">The first and last points are the source & destination, while the rest are control points inbetween</param>
        /// <param name="fragments">The number of fragments that should be generated</param>
        /// <param name="lineFragemntAction">The action that should be done with specified line segment</param>
        public static void DoBezier(PointF[] points, int fragments, Action<PointF, PointF> lineFragemntAction)
        {
            DoBezier(points, fragments, new Action<PointF, PointF, float>((p, p2, progress) => lineFragemntAction(p, p2)));
        }

        /// <summary>
        /// The bezier algorithm
        /// </summary>
        /// <param name="points"></param>
        /// <param name="lastPoint"></param>
        /// <param name="progress"></param>
        /// <param name="lineFragmentAction"></param>
        /// <returns></returns>
        private static PointF DoBezier(PointF[] points, PointF lastPoint, float progress, Action<PointF, PointF, float> lineFragmentAction)
        {
            if (points.Length == 2)
            {
                PointF curPoint = GetPointOfProgress(points[0], points[1], progress);

                lineFragmentAction(lastPoint, curPoint, progress);

                return curPoint;
            }
            else
            {
                // diminish points
                List<PointF> newpoints = new List<PointF>();
                for (int j = 0; j < points.Length - 1; j++)
                    newpoints.Add(GetPointOfProgress(points[j], points[j + 1], progress));

                return DoBezier(newpoints.ToArray(), lastPoint, progress, lineFragmentAction);
            }
        }

        /// <summary>
        /// Linearly interpolate between 2 points and gets the point at the progress (alpha) value
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        private static PointF GetPointOfProgress(PointF p1, PointF p2, float progress)
        {
            return new PointF((p1.X + (p2.X - p1.X) * progress), (p1.Y + (p2.Y - p1.Y) * progress));
        }


        /// <summary>
        /// Creates line segments between the given points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="lineFragmentAction">The action that should be done with the line segments</param>
        public static void Do4WayLines(PointF[] points, Action<PointF, PointF, float> lineFragmentAction)
        {
            if (points.Length >= 2)
            {
                for (int i = 0; i < points.Length - 1; i++)
                {
                    lineFragmentAction(points[i], points[i + 1], (i + 1) / (float)(points.Length - 1));
                }
            }
        }

        /// <summary>
        /// Creates line segments between the given points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="lineFragmentAction">The action that should be done with the line segments</param>
        public static void Do4WayLines(PointF[] points, Action<PointF, PointF> lineFragmentAction)
        {
            if (points.Length >= 2)
            {
                for (int i = 0; i < points.Length - 1; i++)
                    lineFragmentAction(points[i], points[i + 1]);
            }
        }

        /// <summary>
        /// Creates line segments between the given points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="lineFragmentAction">The action that should be done with the line segments</param>
        public static void DoStraight(PointF[] points, Action<PointF, PointF> lineFragmentAction)
        {
            if (points.Length >= 2)
            {
                lineFragmentAction(points.First(), points.Last());
            }
        }

        /// <summary>
        /// Creates line segments between the given points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="lineFragmentAction">The action that should be done with the line segments</param>
        public static void DoStraight(PointF[] points, Action<PointF, PointF, float> lineFragmentAction)
        {
            if (points.Length >= 2)
            {
                lineFragmentAction(points.First(), points.Last(), 1f);
            }
        }
    }
}
