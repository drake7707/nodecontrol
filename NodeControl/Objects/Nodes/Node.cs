using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NodeControl.Nodes
{
    /// <summary>
    /// The base class for all node objects
    /// </summary>
    public abstract class Node : INodeObject
    {
        /// <summary>
        /// The parent diagram
        /// </summary>
        protected NodeDiagram parent;
        public Node(NodeDiagram parent)
        {
            this.parent = parent;
            ParentNodes = new ParentNodeCollection();
        }

        /// <summary>
        /// The node position
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// The text of the node
        /// </summary>
        public virtual string Text { get; set; }

        /// <summary>
        /// An object associated to this node
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// The standard node size of a node is the one defined in the diagram
        /// </summary>
        public virtual Size NodeSize
        {
            get { return parent.NodeSize; }
        }

        /// <summary>
        /// Returns the nodes where this nodes links to
        /// </summary>
        /// <returns>A list of nodes that this node links to</returns>
        public abstract IEnumerable<Node> GetLinkedNodes();

        /// <summary>
        /// A collection of the parent nodes (which are the nodes that link to this node)
        /// </summary>
        public ParentNodeCollection ParentNodes { get; set; }

        /// <summary>
        /// The container it resides in, if the node isn't in a container, this will be null
        /// </summary>
        internal ContainerNode Container { get; set; }

        /// <summary>
        /// The possible directions of a node
        /// </summary>
        public enum DirectionEnum
        {
            Horizontal = 0,
            Vertical = 1
        }
        /// <summary>
        /// The current direction of the node (influences which sides the links attach to)
        /// </summary>
        public DirectionEnum Direction { get; set; }

        /// <summary>
        /// Draws the node
        /// </summary>
        /// <param name="g"></param>
        /// <param name="f"></param>
        /// <param name="viewportRect"></param>
        /// <param name="isSelected"></param>
        protected internal virtual void Draw(Graphics g, Font f, Rectangle viewportRect, bool isSelected)
        {
            Rectangle area = Area;

            // if it is visible in the current viewing area
            if (viewportRect.IntersectsWith(area))
            {
                // draw the gradient
                using (LinearGradientBrush br = new LinearGradientBrush(area, Color.White, Color.FromArgb(229, 229, 229), LinearGradientMode.Vertical))
                    g.FillRectangle(br, area);

                // draw the border (red if selected)
                if (isSelected)
                    g.DrawRectangle(Pens.Red, area);
                else
                    g.DrawRectangle(Pens.Black, area);

                // draw the text of the node centered
                using (var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString((Text + ""), f, Brushes.Black, new RectangleF(area.Left, area.Top, area.Width, area.Height), sf);
            }
        }

        /// <summary>
        /// The bounds of the node, based on the position and node size
        /// </summary>
        public Rectangle Area
        {
            get
            {
                Rectangle area = new Rectangle(Position, NodeSize);
                return area;
            }
        }

        /// <summary>
        /// Returns a collection of all the line segments (within certain % range) to all the nodes the current node links to
        /// </summary>
        /// <param name="skipPercent">The skip percentage, all segments before the given % will be ignored</param>
        /// <param name="takePercent">The amount of segments to take</param>
        /// <returns></returns>
        internal protected virtual IEnumerable<KeyValuePair<Point, Point>> GetLineSegmentsOfLinks(float skipPercent, float takePercent)
        {
            Rectangle area = Area;

            List<KeyValuePair<Point, Point>> segments = new List<KeyValuePair<Point, Point>>();

            var nodes = GetLinkedNodes().ToArray();
            int index = 0;
            foreach (var subn in nodes)
            {
                if (subn != null)
                {
                    // get the start & end positions of the line segment
                    Point source;
                    Point dest;
                    GetSourceAndDestPointsForLink(area, nodes.Length, index, subn, out source, out dest);
                    PointF[] points = GetPointsForLink(subn, source, dest);

                    var segmentsOfLink = new List<KeyValuePair<Point, Point>>();

                    // generate line segments based on the line type
                    if (parent.LineType == LineTypeEnum.Bezier)
                    {
                        LinkManager.DoBezier(points, 100, (p, p2) =>
                        {
                            segmentsOfLink.Add(new KeyValuePair<Point, Point>(new Point((int)p.X, (int)p.Y), new Point((int)p2.X, (int)p2.Y)));
                        });
                    }
                    else if (parent.LineType == LineTypeEnum.FourWay)
                    {
                        LinkManager.Do4WayLines(points, (p, p2) =>
                        {
                            segmentsOfLink.Add(new KeyValuePair<Point, Point>(new Point((int)p.X, (int)p.Y), new Point((int)p2.X, (int)p2.Y)));
                        });
                    }
                    else if (parent.LineType == LineTypeEnum.Straight)
                    {
                        LinkManager.DoStraight(points, (p, p2) =>
                        {
                            segmentsOfLink.Add(new KeyValuePair<Point, Point>(new Point((int)p.X, (int)p.Y), new Point((int)p2.X, (int)p2.Y)));
                        });
                    }
                    // add the segments to the collection
                    segments.AddRange(segmentsOfLink.Skip((int)(skipPercent * segmentsOfLink.Count)).Take((int)(takePercent * segmentsOfLink.Count)));
                }
                index++;
            }

            return segments;
        }


        /// <summary>
        /// Draw all the links of the current node to the child nodes
        /// </summary>
        /// <param name="g">The graphics to draw with</param>
        /// <param name="fnt">The font to use</param>
        /// <param name="viewportRect">The viewport bounds</param>
        /// <param name="selectedLinks">The links that are selected in the diagram</param>
        internal virtual void DrawLinks(Graphics g, Font fnt, Rectangle viewportRect, IEnumerable<Link> selectedLinks)
        {
            Rectangle area = Area;
            // make a hashset for the selected links to look up
            var selectedLnks = new HashSet<Link>(selectedLinks);

            // get all linked nodes
            var nodes = GetLinkedNodes().ToArray();
            int index = 0;
            foreach (var subn in nodes)
            {
                if (subn != null)
                {
                    // get the source & destination point between the current node and child node
                    Point source;
                    Point dest;
                    GetSourceAndDestPointsForLink(area, nodes.Length, index, subn, out source, out dest);

                    Link curLink = new Link(this, subn, index); // struct, so it's by contents not by reference!
                    bool isSelected = selectedLnks.Contains(curLink);
                    // draw the link
                    DrawLinkTo(subn, g, fnt, viewportRect, source, dest, isSelected);
                }
                index++;
            }
        }


        /// <summary>
        /// Draws a link to a node
        /// </summary>
        /// <param name="subn">The node to draw the link to</param>
        /// <param name="g">The graphics to draw with</param>
        /// <param name="f">The font used (not used now, but could be used for labels)</param>
        /// <param name="viewportRect">The viewport bounds</param>
        /// <param name="source">The source point</param>
        /// <param name="dest">The destination point</param>
        /// <param name="isSelected">Selected or not</param>
        protected internal virtual void DrawLinkTo(Node subn, Graphics g, Font f, Rectangle viewportRect, Point source, Point dest, bool isSelected)
        {
            PointF[] points = GetPointsForLink(subn, source, dest);
            var startPoint = points[0];

            Pen pen;
            if (isSelected)
                pen = Pens.Red;
            else
                pen = Pens.Black;
            g.DrawEllipse(pen, RectangleF.FromLTRB(startPoint.X - 2, startPoint.Y - 2, startPoint.X + 2, startPoint.Y + 2));
            //foreach (var p in points)
            //    g.FillRectangle(Brushes.Black, RectangleF.FromLTRB(p.X - 2, p.Y - 2, p.X + 2, p.Y + 2));

            //using (Pen p = new Pen(pen.Color))
            //{
            //    p.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            //    g.DrawLines(p, points);
            //}
            if (parent.LineType == LineTypeEnum.Bezier)
                DrawBezier(g, viewportRect, pen.Color, points);
            else if (parent.LineType == LineTypeEnum.FourWay)
                Draw4WayLine(g, viewportRect, pen.Color, points);
            else if (parent.LineType == LineTypeEnum.Straight)
                DrawStraight(g, viewportRect, pen.Color, points);
        }

        /// <summary>
        /// Returns the control points for the link. A link uses control points to
        /// snake around objects (so it's not a direct line towards the destination)
        /// </summary>
        /// <param name="subn">The node where the link goes to</param>
        /// <param name="source">The source point</param>
        /// <param name="dest">The destination point</param>
        /// <returns></returns>
        protected virtual PointF[] GetPointsForLink(Node subn, Point source, Point dest)
        {
            Rectangle area = Area;
            Rectangle subnArea = subn.Area;

            PointF[] points;
            if (Direction == DirectionEnum.Horizontal)
            {
                if (Math.Abs(subn.Position.X - Position.X) < area.Size.Width || subn.Position.X <= this.Position.X)
                {

                    //                            --  (right, sourceY)
                    // (left, destY)   --          |
                    //                |            |
                    //                 ------------ 
                    int longHorY;
                    if (subn.Position.Y > Position.Y)
                    {
                        if (Math.Abs(subn.Position.Y - Position.Y) < Math.Abs(subn.Position.X - Position.X) / area.Size.Width * area.Size.Height)
                            longHorY = source.Y - area.Size.Height;
                        else
                            longHorY = source.Y + area.Size.Height;
                    }
                    else if (subn.Position.Y == Position.Y)
                    {
                        longHorY = source.Y + area.Size.Height;
                    }
                    else
                    {
                        if (Math.Abs(subn.Position.Y - Position.Y) < Math.Abs(subn.Position.X - Position.X) / area.Size.Width * area.Size.Height)
                            longHorY = source.Y + area.Size.Height;
                        else
                            longHorY = source.Y - area.Size.Height;
                    }


                    points = new PointF[] { new PointF(source.X, source.Y),
                                                     new PointF(source.X + area.Width /2, source.Y), 
                                                     new PointF(source.X + area.Width /2, longHorY),

                                                     new PointF(dest.X - subnArea.Width /2  + 2 * (source.X + area.Width /2 - (dest.X- subnArea.Width /2 )) / 3, longHorY),
                                                     new PointF(dest.X - subnArea.Width /2  + 1 * (source.X + area.Width /2 - (dest.X - subnArea.Width /2 )) / 3, longHorY),

                                                     new PointF(dest.X - subnArea.Width /2, longHorY),
                                                     new PointF(dest.X - subnArea.Width /2,  dest.Y),
                                                     new PointF(dest.X, dest.Y)
                    };
                }
                else
                {
                    points = new PointF[] { new PointF(source.X, source.Y),
                                                     new PointF(source.X + (dest.X  - source.X) / 2, source.Y ),
                                                     new PointF(source.X + (dest.X  - source.X) / 2,  dest.Y),
                                                     new PointF(dest.X,  dest.Y)
                    };
                }
                return points;
            }
            else
            {
                // similar to horizontal but swap X with Y and width with height
                if (Math.Abs(subn.Position.Y - Position.Y) < area.Size.Height || subn.Position.Y <= this.Position.Y)
                {
                    int longVerX;
                    if (subn.Position.X > Position.X)
                    {
                        if (Math.Abs(subn.Position.X - Position.X) < Math.Abs(subn.Position.Y - Position.Y) / area.Size.Height * area.Size.Width)
                            longVerX = source.X - area.Size.Width;
                        else
                            longVerX = source.X + area.Size.Width;
                    }
                    else if (subn.Position.X == Position.X)
                    {
                        longVerX = source.X + area.Size.Width;
                    }
                    else
                    {
                        if (Math.Abs(subn.Position.X - Position.X) < Math.Abs(subn.Position.Y - Position.Y) / area.Size.Height * area.Size.Width)
                            longVerX = source.X + area.Size.Width;
                        else
                            longVerX = source.X - area.Size.Width;
                    }


                    points = new PointF[] { new PointF(source.X, source.Y),
                                                     new PointF(source.X, source.Y + area.Height /2), 
                                                     new PointF(longVerX, source.Y + area.Height /2),

                                                     new PointF(longVerX, dest.Y - subnArea.Height /2  + 2 * (source.Y + area.Height /2 - (dest.Y- subnArea.Height /2 )) / 3),
                                                     new PointF(longVerX, dest.Y - subnArea.Height /2  + 1 * (source.Y + area.Height /2 - (dest.Y - subnArea.Height /2 )) / 3),

                                                     new PointF(longVerX, dest.Y - subnArea.Height /2),
                                                     new PointF(dest.X, dest.Y - subnArea.Height / 2),
                                                     new PointF(dest.X, dest.Y)
                    };
                }
                else
                {
                    points = new PointF[] { new PointF(source.X, source.Y),
                                                     new PointF(source.X, source.Y + (dest.Y  - source.Y) / 2),
                                                     new PointF(dest.X,  source.Y + (dest.Y  - source.Y) / 2),
                                                     new PointF(dest.X,  dest.Y)
                    };
                }
                return points;
            }
        }

        /// <summary>
        /// Draw the link with the bezier algorithm to get the line segments
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="viewportRect">The viewport bounds</param>
        /// <param name="color">The color of the line segements</param>
        /// <param name="points">The points to do bezier with</param>
        private void DrawBezier(Graphics g, Rectangle viewportRect, Color color, params PointF[] points)
        {
            LinkManager.DoBezier(points, 100, (pt1, pt2, progress) =>
            {
                Pen p = new Pen(Color.FromArgb(255, color), 1f);
                if (progress == 1)
                {
                    // at the end, draw an arrow
                    p.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                }

                // if the line falls inside the viewport, draw it
                if (viewportRect.IntersectsWithLine(pt1.X, pt1.Y, pt2.X, pt2.Y))
                {
                    g.DrawLine(p, pt1, pt2);
                }

                p.Dispose();
            });
        }

        /// <summary>
        /// Draw the link with the 4 way lines algorithm
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="viewportRect">The viewport bounds</param>
        /// <param name="color">The color of the line segements</param>
        /// <param name="points">The points to generate the line segments from</param>
        private void Draw4WayLine(Graphics g, Rectangle viewportRect, Color color, params PointF[] points)
        {
            LinkManager.Do4WayLines(points, (pt1, pt2, progress) =>
            {
                Pen p = new Pen(color);
                if (progress == 1)
                {
                    // at the end, draw an arrow
                    p.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                }

                // if the line falls inside the viewport, draw it
                if (viewportRect.IntersectsWithLine(pt1.X, pt1.Y, pt2.X, pt2.Y))
                    g.DrawLine(p, pt1, pt2);

                p.Dispose();

            });
        }

        private void DrawStraight(Graphics g, Rectangle viewportRect, Color color, PointF[] points)
        {
            LinkManager.DoStraight(points, (pt1, pt2, progress) =>
            {
                Pen p = new Pen(color);
                if (progress == 1)
                {
                    // at the end, draw an arrow
                    p.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                }

                // if the line falls inside the viewport, draw it
                if (viewportRect.IntersectsWithLine(pt1.X, pt1.Y, pt2.X, pt2.Y))
                    g.DrawLine(p, pt1, pt2);

                p.Dispose();
            });
        }


        /// <summary>
        /// Gets the source and destination points for links between the current node and a child node
        /// </summary>
        /// <param name="area">The bounds of the current node</param>
        /// <param name="nrOfNodes">The total nr of child nodes that this node links to</param>
        /// <param name="index">The current link index</param>
        /// <param name="subn">The child node</param>
        /// <param name="source">The resulting source point</param>
        /// <param name="dest">The resulting destination point</param>
        protected virtual void GetSourceAndDestPointsForLink(Rectangle area, int nrOfNodes, int index, Node subn, out Point source, out Point dest)
        {
            Rectangle subnArea = subn.Area;
            var parentNodesOfTargetNode = new HashSet<Node>(subn.ParentNodes).ToList();

            if (Direction == DirectionEnum.Horizontal)
            {
                int yOffset = (int)((index / (float)nrOfNodes) * area.Height) + (area.Height / nrOfNodes) / 2;

                source = new Point(area.Right, area.Top + yOffset);
                int destYOffset = (int)((parentNodesOfTargetNode.IndexOf(this) / (float)parentNodesOfTargetNode.Count) * subnArea.Height) + (subnArea.Height / parentNodesOfTargetNode.Count) / 2;
                dest = new Point(subnArea.Left, subnArea.Top + destYOffset);
            }
            else
            {
                int xOffset = (int)((index / (float)nrOfNodes) * area.Width) + (area.Width / nrOfNodes) / 2;

                source = new Point(area.Left + xOffset, area.Bottom);
                int destXOffset = (int)((parentNodesOfTargetNode.IndexOf(this) / (float)parentNodesOfTargetNode.Count) * subnArea.Width) + (subnArea.Width / parentNodesOfTargetNode.Count) / 2;
                dest = new Point(subnArea.Left + destXOffset, subnArea.Top);
            }
        }

        /// <summary>
        /// Tries to get a link at the given x,y coordinates
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <param name="l">The link if there is one at the given coordinates</param>
        /// <returns>True if a link is found</returns>
        internal bool TryGetLink(int x, int y, out Link l)
        {
            Rectangle area = Area;
            Point clickPoint = new Point(x, y);

            var nodes = GetLinkedNodes().ToArray();
            int index = 0;

            Link link = default(Link);
            bool foundLink = false;

            // check all links to child nodes
            foreach (var subn in nodes)
            {
                if (subn != null)
                {
                    // get the source & dest point
                    Point source;
                    Point dest;
                    GetSourceAndDestPointsForLink(area, nodes.Length, index, subn, out source, out dest);

                    // get all the control points
                    PointF[] points = GetPointsForLink(subn, source, dest);

                    // define the line segment action
                    Action<PointF, PointF> lineSegmentAction = (p, p2) =>
                    {
                        // check if the distance between the line segment and the point clicked is smaller than 3
                        float distToLine = (float)GetPointToLineDistance(p, p2, clickPoint);
                        if (distToLine < 3)
                        {
                            // determine if the closest point to the line segment actually falls inside the segment
                            float normalLength = (float)Math.Sqrt((p2.X - p.X) * (p2.X - p.X) + (p2.Y - p.Y) * (p2.Y - p.Y));
                            float u = (clickPoint.X - p.X) * (p2.X - p.X) + (clickPoint.Y - p.Y) * (p2.Y - p.Y);
                            u /= normalLength;

                            //PointF lineVector = new PointF((p2.X - p.X) / normalLength, (p2.Y - p.Y) / normalLength);
                            if (u > 0 && u < normalLength)
                            {
                                // link found
                                link = new Link(this, subn, index);
                                foundLink = true;
                            }
                        }

                    };

                    // do the appropriate algorithm
                    if (parent.LineType == LineTypeEnum.Bezier)
                        LinkManager.DoBezier(points, 100, lineSegmentAction);
                    else if (parent.LineType == LineTypeEnum.FourWay)
                        LinkManager.Do4WayLines(points, lineSegmentAction);
                    else if (parent.LineType == LineTypeEnum.Straight)
                        LinkManager.DoStraight(points, lineSegmentAction);
                }
                index++;
            }

            l = link;
            return foundLink;
        }

        /// <summary>
        /// Returns the distance of a point to a line
        /// </summary>
        /// <param name="lineStart">The start point</param>
        /// <param name="lineEnd">The end point</param>
        /// <param name="p">The point to get the distance between</param>
        /// <returns>The distance between the point and a line</returns>
        private double GetPointToLineDistance(PointF lineStart, PointF lineEnd, PointF p)
        {
            double normalLength = Math.Sqrt((lineEnd.X - lineStart.X) * (lineEnd.X - lineStart.X) + (lineEnd.Y - lineStart.Y) * (lineEnd.Y - lineStart.Y));
            return Math.Abs((p.X - lineStart.X) * (lineEnd.Y - lineStart.Y) - (p.Y - lineStart.Y) * (lineEnd.X - lineStart.X)) / normalLength;
        }

        /// <summary>
        /// Removes the link to a child node
        /// </summary>
        /// <param name="n">The child node</param>
        internal protected abstract void RemoveLinkTo(Node n);

        /// <summary>
        /// Returns the required data when a mouse down on the node is triggered
        /// </summary>
        /// <param name="e">The mouse event args</param>
        /// <returns>The information required about this node</returns>
        internal virtual MouseDownInfo GetMouseDownInfo(MouseEventArgs e)
        {
            return new MouseDownInfo() { StartNode = this };
        }

        /// <summary>
        /// Adds a link to the given target node, along with the data built when the mouse was pressed 
        /// </summary>
        /// <param name="mouseDownInfo">The additional mousedown info when the mouse was pressed</param>
        /// <param name="targetNode">The target node to link to</param>
        internal protected abstract void AddLink(MouseDownInfo mouseDownInfo, Node targetNode);

        /// <summary>
        /// Opens the editor of the node
        /// </summary>
        /// <returns>True if the node was changed, otherwise false</returns>
        internal protected abstract bool OpenEditor();

        /// <summary>
        /// Can the node be linked to child nodes
        /// </summary>
        public virtual bool CanLink
        {
            get { return true; }
        }

        /// <summary>
        /// Can the node be linked to by a parent node
        /// </summary>
        public virtual bool CanBeLinkedTo
        {
            get { return true; }
        }

        /// <summary>
        /// Determines if the node is selected
        /// </summary>
        public virtual bool IsSelected
        {
            get
            {
                return parent.SelectedObjects.Contains(this);
            }
        }

        /// <summary>
        /// Occurs when another node is drag & dropped onto this node
        /// </summary>
        /// <param name="otherNode">The other node that was drag & dropped</param>
        internal virtual void OnNodeDragDrop(IEnumerable<Node> otherNode)
        {

        }

        /// <summary>
        /// Occurs when the mouse is clicked on the node
        /// </summary>
        /// <param name="ev"></param>
        internal virtual void OnMouseClick(MouseEventArgs ev)
        {
            if (ev.Button == MouseButtons.Middle)
            {
                // toggle the direction
                Direction = Direction == DirectionEnum.Vertical ? DirectionEnum.Horizontal : DirectionEnum.Vertical;
                parent.Redraw();
            }
        }
    }

    /// <summary>
    /// The parent node collection of a node
    /// </summary>
    public class ParentNodeCollection : IEnumerable<Node>
    {
        /// <summary>
        /// A list of parents for a node
        /// </summary>
        private List<Node> nodes;
        internal ParentNodeCollection()
        {
            nodes = new List<Node>();
        }

        /// <summary>
        /// Adds the node to the parent list without having the parent child node collection changed
        /// </summary>
        /// <param name="itm">The node to add to the parent collection</param>
        internal void AddWithoutRefBack(Node itm)
        {
            nodes.Add(itm);
        }
        /// <summary>
        /// Removes the node from the parent list without changing the parent's child node collection
        /// </summary>
        /// <param name="itm"></param>
        internal void RemoveWithoutRefBack(Node itm)
        {
            nodes.Remove(itm);
        }

        /// <summary>
        /// Gets an enumator for all the parent nodes
        /// </summary>
        /// <returns>An enumerator for all the parent nodes</returns>
        public IEnumerator<Node> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        /// <summary>
        /// Gets an enumator for all the parent nodes
        /// </summary>
        /// <returns>An enumerator for all the parent nodes</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        /// <summary>
        /// Determines the index of a node in the collection
        /// </summary>
        /// <param name="node">The node to look for</param>
        /// <returns>The index of the node</returns>
        internal int IndexOf(Node node)
        {
            return nodes.IndexOf(node);
        }
        /// <summary>
        /// The number of nodes in the parent collection
        /// </summary>
        public int Count { get { return nodes.Count; } }
    }

    /// <summary>
    /// A node collection
    /// </summary>
    //public class NodeCollection : IList<Node>
    //{
    //    private Node owner;
    //    private List<Node> lst;
    //    internal NodeCollection(Node owner)
    //    {
    //        this.lst = new List<Node>();
    //        this.owner = owner;
    //    }

    //    internal void AddWithoutRefBack(Node itm)
    //    {
    //        lst.Add(itm);
    //    }
    //    internal void RemoveWithoutRefBack(Node itm)
    //    {
    //        lst.Remove(itm);
    //    }

    //    public int IndexOf(Node item)
    //    {
    //        return lst.IndexOf(item);
    //    }

    //    public void Insert(int index, Node item)
    //    {
    //        lst.Insert(index, item);
    //        item.ParentNodes.AddWithoutRefBack(owner);
    //    }

    //    public void RemoveAt(int index)
    //    {
    //        Node n = lst[index];
    //        lst.RemoveAt(index);
    //        n.ParentNodes.RemoveWithoutRefBack(owner);
    //    }

    //    public Node this[int index]
    //    {
    //        get
    //        {
    //            return lst[index];
    //        }
    //        set
    //        {
    //            var n = lst[index];
    //            if (value != n)
    //            {
    //                n.ParentNodes.RemoveWithoutRefBack(owner);
    //                lst[index] = value;
    //                value.ParentNodes.AddWithoutRefBack(owner);
    //            }
    //        }
    //    }

    //    public void Add(Node item)
    //    {
    //        lst.Add(item);
    //        item.ParentNodes.AddWithoutRefBack(owner);
    //    }

    //    public void Clear()
    //    {
    //        foreach (var item in lst)
    //            item.ParentNodes.RemoveWithoutRefBack(owner);
    //        lst.Clear();
    //    }

    //    public bool Contains(Node item)
    //    {
    //        return lst.Contains(item);
    //    }

    //    public void CopyTo(Node[] array, int arrayIndex)
    //    {
    //        lst.CopyTo(array, arrayIndex);
    //    }

    //    public int Count
    //    {
    //        get { return lst.Count; }
    //    }

    //    public virtual bool IsReadOnly
    //    {
    //        get { return false; }
    //    }

    //    public bool Remove(Node item)
    //    {
    //        item.ParentNodes.RemoveWithoutRefBack(owner);
    //        return lst.Remove(item);
    //    }

    //    public IEnumerator<Node> GetEnumerator()
    //    {
    //        return lst.GetEnumerator();
    //    }

    //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    //    {
    //        return lst.GetEnumerator();
    //    }
    //}

    /// <summary>
    /// Information stored when the mouse was pressed
    /// </summary>
    public class MouseDownInfo
    {
        /// <summary>
        /// The node the mouse was pressed on
        /// </summary>
        public Node StartNode { get; set; }

    }

}
