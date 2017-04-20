using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using NodeControl.Factories;
using NodeControl.Tools;
using NodeControl.Nodes;

namespace NodeControl
{
    /// <summary>
    /// Diagram control where nodes can be placed and linked with each other
    /// </summary>
    public class NodeDiagram : UserControl
    {
        public NodeDiagram()
        {
            InitializeComponent();

            // create default factory set
            Factories = new FactoryCollection(this);
            Factories.Add(new TextNodeFactory());
            Factories.Add(new ConditionNodeFactory());
            Factories.Add(new ContainerNodeFactory());
            Factories.Add(new StartNodeFactory());
            Factories.Add(new EndNodeFactory());

            // default nothing selected 
            SelectedObjects = new HashSet<INodeObject>();

            Nodes = new HashSet<Node>();

            LineType = LineTypeEnum.Bezier;
            Zoom = 1;

            GridSize = new System.Drawing.Size(8, 8);
            NodeSize = new Size(100, 50).RoundTo(GridSize.Width, GridSize.Height);
            DoubleBuffered = true;

            AutoScroll = true;
            SetStyle(ControlStyles.Selectable, true);
        }

        /// <summary>
        /// The list of nodes the diagram contains
        /// </summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public HashSet<Node> Nodes { get; private set; }

        /// <summary>
        /// The default size of a node
        /// </summary>
        public Size NodeSize { get; set; }

        /// <summary>
        /// The size of the grid to snap to
        /// </summary>
        public Size GridSize { get; set; }

        /// <summary>
        /// Shows the grid to snap to
        /// </summary>
        public bool ShowGrid { get; set; }

        /// <summary>
        /// Layouts the diagram automatically by following each link and placing them from left to right in increased depth
        /// This still doesn't work well (//TODO use a decent orthogonal layout algorithm)
        /// </summary>
        public void AutoLayout(bool horizontal)
        {
            Dictionary<Node, int> lanesOfNode = new Dictionary<Node, int>();

            Dictionary<int, List<Node>> lanes = new Dictionary<int, List<Node>>();

            int currentDepth = 0;

            var node = Nodes.FirstOrDefault();

            // determine of all the nodes on which lane (or column) they will be
            foreach (var n in Nodes)
            {
                if (!lanesOfNode.ContainsKey(n))
                    FillLane(n, lanesOfNode, lanes, currentDepth);
            }

            if (lanes.Count == 0)
                return;

            int totalWidth;
            int totalHeight;
            // calculate the total width & height of all the nodes
            if (horizontal)
            {
                totalWidth = (int)(lanes.Count * 1.5f * NodeSize.Width);
                totalHeight = (int)(lanes.Max(l => l.Value.Sum(n => n.NodeSize.Height + NodeSize.Height * 0.5f)));
            }
            else
            {
                totalWidth = (int)(lanes.Max(l => l.Value.Sum(n => n.NodeSize.Width + NodeSize.Width * 0.5f)));
                totalHeight = (int)(lanes.Count * 1.5f * NodeSize.Height);
            }

            int offsetLeft = NodeSize.Width;
            int offsetTop = NodeSize.Height;

            if (horizontal)
            {
                int left = offsetLeft;

                // do a first pass by placing all the nodes in their respective lanes, evenly placed
                for (int i = 0; i < lanes.Count; i++)
                {
                    List<Node> nodes = lanes[i];
                    if (nodes != null && nodes.Count > 0)
                    {
                        int top = 0 + offsetTop + (totalHeight / nodes.Count) / 2;

                        foreach (var n in nodes)
                        {

                            n.Position = new Point(left, top).RoundTo(GridSize.Width, GridSize.Height);
                            top += (int)(n.NodeSize.Height + n.NodeSize.Height * 0.5f);
                        }

                        int maxWidthOfPair = nodes.Max(n => n.NodeSize.Width);
                        left += (int)(maxWidthOfPair + NodeSize.Width * 0.5f);
                    }
                }


                // do a second pass to determine if links from previous nodes would intersect with the current lane, and if so
                // move the node either up or down, depending on its index on the lane it's on.
                left = offsetLeft;
                List<KeyValuePair<Point, Point>> lineSegments = new List<KeyValuePair<Point, Point>>();

                for (int i = 0; i < lanes.Count; i++)
                {
                    List<Node> nodes = lanes[i];
                    if (nodes != null && nodes.Count > 0)
                    {
                        int top = 0 + offsetTop + (totalHeight / nodes.Count) / 2;

                        // add all line segments between the current lane and the next one

                        // move the node up or down if any of the line segments intersect with the node
                        foreach (var n in nodes)
                        {
                            int retry = 0;

                            var linesOfNode = n.GetLineSegmentsOfLinks(0.25f, 0.5f).ToArray();

                            // attempt to dodge any overlap
                            while (lineSegments.Concat(linesOfNode).Where(l => n.Area.IntersectsWithLine(l.Key.X, l.Key.Y, l.Value.X, l.Value.Y)).Any() && retry < 5)
                            {
                                if (nodes.IndexOf(n) < nodes.Count / 2)
                                    top -= (int)(NodeSize.Height);
                                else
                                    top += (int)(NodeSize.Height);

                                n.Position = new Point(left, top).RoundTo(GridSize.Width, GridSize.Height);
                            }
                            top += (int)(n.NodeSize.Height + n.NodeSize.Height * 0.5f);
                            retry++;
                        }

                        foreach (var n in nodes)
                        {
                            // take all the line segments between 25% and 75% of the trajectory
                            // and add them to the line segment pool to avoid
                            var lines = n.GetLineSegmentsOfLinks(0.25f, 0.5f).ToArray();
                            lineSegments.AddRange(lines);
                        }


                        int maxWidthOfPair = nodes.Max(n => n.NodeSize.Width);
                        left += (int)(maxWidthOfPair + NodeSize.Width * 0.5f);
                    }
                }
            }
            else
            {
                int top = offsetTop;

                // do a first pass by placing all the nodes in their respective lanes, evenly placed
                for (int i = 0; i < lanes.Count; i++)
                {
                    List<Node> nodes = lanes[i];
                    if (nodes != null && nodes.Count > 0)
                    {
                        int left = 0 + offsetLeft + (totalWidth / nodes.Count) / 2;

                        foreach (var n in nodes)
                        {
                            left += this.NodeSize.Width / 2 - n.NodeSize.Width / 2;
                            n.Position = new Point(left, top);
                            left += (int)(n.NodeSize.Width + n.NodeSize.Width * 0.5f);

                        }

                        int maxHeightOfPair = nodes.Max(n => n.NodeSize.Height);
                        top += (int)(maxHeightOfPair + NodeSize.Height * 0.5f);
                    }
                }


                // do a second pass to determine if links from previous nodes would intersect with the current lane, and if so
                // move the node either left or right, depending on its index on the lane it's on.
                //top = offsetTop;
                //List<KeyValuePair<Point, Point>> lineSegments = new List<KeyValuePair<Point, Point>>();

                //for (int i = 0; i < lanes.Count; i++)
                //{
                //    List<Node> nodes = lanes[i];
                //    if (nodes != null && nodes.Count > 0)
                //    {
                //        int left = 0 + offsetLeft + (totalHeight / nodes.Count) / 2;

                //        // move the node up or down if any of the line segments intersect with the node
                //        foreach (var n in nodes)
                //        {


                //            var linesOfNode = n.GetLineSegmentsOfLinks(0.25f, 0.5f).ToArray();


                //            int retry = 0;

                //            while (lineSegments.Concat(linesOfNode).Where(l => n.Area.IntersectsWithLine(l.Key.X, l.Key.Y, l.Value.X, l.Value.Y)).Any() && retry < 5)
                //            {
                //                var blockingLinesegment = lineSegments.Concat(linesOfNode).Where(l => n.Area.IntersectsWithLine(l.Key.X, l.Key.Y, l.Value.X, l.Value.Y)).FirstOrDefault();
                //                if (nodes.IndexOf(n) < nodes.Count / 2)
                //                    left -= (int)(NodeSize.Width);
                //                else
                //                    left += (int)(NodeSize.Width);

                //                n.Position = new Point(left, top);
                //            }
                //            left += (int)(n.NodeSize.Width + n.NodeSize.Width * 0.5f);
                //            retry++;
                //        }

                //        foreach (var n in nodes)
                //        {
                //            var lines = n.GetLineSegmentsOfLinks(0.25f, 0.5f).ToArray();
                //            lineSegments.AddRange(lines);
                //        }


                //        int maxHeightOfPair = nodes.Max(n => n.NodeSize.Height);
                //        top += (int)(maxHeightOfPair + NodeSize.Height * 0.5f);
                //    }
                //}
            }

            // ensure that all container nodes are updated to fit their content that was moved
            foreach (var cn in Nodes.OfType<ContainerNode>())
                cn.UpdateBounds();

            Redraw();
        }

        /// <summary>
        /// Fills the lanes by traversing each linked nove recursively
        /// </summary>
        /// <param name="n">The node to traverse</param>
        /// <param name="lanesOfNode">The current node-lane lookup</param>
        /// <param name="lanes">The node list by lane</param>
        /// <param name="currentDepth">The current depth traversing</param>
        private void FillLane(Node n, Dictionary<Node, int> lanesOfNode, Dictionary<int, List<Node>> lanes, int currentDepth)
        {
            if (n != null)
            {
                List<Node> lane;
                if (!lanes.TryGetValue(currentDepth, out lane))
                    lanes[currentDepth] = lane = new List<Node>();


                if (!lanesOfNode.ContainsKey(n))
                {
                    lane.Add(n);
                    lanesOfNode.Add(n, currentDepth);

                    foreach (var subn in n.GetLinkedNodes())
                        FillLane(subn, lanesOfNode, lanes, currentDepth + 1);
                }
            }
        }

        /// <summary>
        /// The available node factories for the diagram
        /// </summary>
        public FactoryCollection Factories { get; private set; }


        private float zoom;
        /// <summary>
        /// The zoom factor of the control
        /// </summary>
        public float Zoom
        {
            get { return zoom; }
            set
            {
                if (value <= 0.1f)
                    value = 0.1f;
                if (value != zoom)
                {
                    // when the zoom has changed, update the bounding box & redraw
                    zoom = value;
                    UpdateBoundingBox();
                    Redraw();
                }
            }
        }

        /// <summary>
        /// Keep track of the autoscroll position since the last draw
        /// </summary>
        private Point lastAutoScrollPosition;
        /// <summary>
        /// The image buffer for the nodes & links. Only refreshes when explicitly asked for a redraw, not when invalidating
        /// for other annotations like selection
        /// </summary>
        private Image buffer;
        /// <summary>
        /// Flags if a redraw of the buffer is required
        /// </summary>
        private bool redrawRequired;
        /// <summary>
        /// Paints the diagram
        /// </summary>
        /// <param name="e">Paint arguments</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // create a buffer when it doesn't exist or the diagram is resized
            if (buffer == null || (buffer.Width != this.Width || buffer.Height != this.Height))
            {
                buffer = new Bitmap(this.Width, this.Height);
                redrawRequired = true;
            }

            // redraw if required or the scroll pos has changed
            if (redrawRequired || lastAutoScrollPosition != AutoScrollPosition)
            {

                using (Graphics g = Graphics.FromImage(buffer))
                {

                    // determine viewport for clipping 
                    var viewportRect = new Rectangle((int)Math.Ceiling((e.ClipRectangle.X - AutoScrollPosition.X) / Zoom), (int)Math.Ceiling((e.ClipRectangle.Y - AutoScrollPosition.Y) / Zoom), (int)((e.ClipRectangle.Width + 1) / Zoom), (int)((e.ClipRectangle.Height + 1) / Zoom));


                    // clear
                    g.FillRectangle(Brushes.White, e.ClipRectangle);

                    // draw grid if necessary
                    if (ShowGrid)
                        DrawGrid(g, viewportRect, AutoScrollPosition, Zoom);

                    // draw the contents
                    DrawContents(g, viewportRect, AutoScrollPosition, Zoom);
                }

                lastAutoScrollPosition = AutoScrollPosition;
                redrawRequired = false;
            }

            // draw the buffer onto the control
            e.Graphics.DrawImage(buffer, new Point(0, 0));

            // draw annotations by the active tool
            try
            {
                if (activeTool != null)
                    activeTool.OnDraw(e.Graphics);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Draws the grid where the nodes snap to
        /// </summary>
        /// <param name="g"></param>
        /// <param name="viewportRect"></param>
        /// <param name="offset"></param>
        /// <param name="zoom"></param>
        private void DrawGrid(Graphics g, Rectangle viewportRect, Point offset, float zoom)
        {
            // increase the viewport a bit to ensure all the grid lines are visible
            viewportRect.Inflate(GridSize.Width, GridSize.Height);
            viewportRect.Offset((int)(AutoScrollPosition.X / Zoom), (int)(AutoScrollPosition.Y / Zoom));

            var clip = viewportRect.RoundTo((int)(GridSize.Width * Zoom), (int)(GridSize.Height * Zoom));

            // draw the vertical grid lines
            for (float i = clip.Left; i <= clip.Right * Zoom; i += (GridSize.Width * Zoom))
                g.DrawLine(Pens.LightGray, new Point((int)i, clip.Top), new Point((int)i, (int)(clip.Bottom * Zoom)));

            // draw the horizontal grid lines
            for (float i = clip.Top; i <= clip.Bottom * Zoom; i += (GridSize.Height * Zoom))
                g.DrawLine(Pens.LightGray, new Point(clip.Left, (int)i), new Point((int)(clip.Right * Zoom), (int)i));
        }

        /// <summary>
        /// Draw the contents (nodes & associations)
        /// </summary>
        /// <param name="g"></param>
        /// <param name="viewportRect"></param>
        /// <param name="offset"></param>
        /// <param name="zoom"></param>
        private void DrawContents(Graphics g, Rectangle viewportRect, Point offset, float zoom)
        {
            // apply scroll offset & zoom scaling
            g.TranslateTransform(offset.X, offset.Y);
            g.ScaleTransform(zoom, zoom);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // draw all links
            foreach (var n in Nodes.ToArray().Where(n => !(n is ContainerNode)))
                DrawLinksOfNode(n, g, viewportRect);

            // draw all the container nodes, in correct Z-order
            foreach (var n in Nodes.ToArray().OfType<ContainerNode>().OrderBy(cn => cn.Depth))
                DrawNode(n, g, viewportRect);

            // draw all the other nodes
            foreach (var n in Nodes.ToArray().Where(n => !(n is ContainerNode)))
                DrawNode(n, g, viewportRect);

            

            
        }

        /// <summary>
        /// Redraws all nodes & links
        /// </summary>
        public void Redraw()
        {
            redrawRequired = true;
            Invalidate();
        }

        /// <summary>
        /// Draws a node by the given graphics
        /// </summary>
        /// <param name="n">The node to draw</param>
        /// <param name="g">The graphics to use to draw the node</param>
        /// <param name="viewportRect">The clipping rectangle</param>
        private void DrawNode(Node n, Graphics g, Rectangle viewportRect)
        {
            if (n != null)
            {
                bool isNodeSelected = SelectedObjects.Contains(n);

                // draw the node
                n.Draw(g, Font, viewportRect, isNodeSelected);
                
            }
        }

        /// <summary>
        /// Draw all the links of the given node
        /// </summary>
        /// <param name="n"></param>
        /// <param name="g"></param>
        /// <param name="viewportRect"></param>
        private void DrawLinksOfNode(Node n, Graphics g, Rectangle viewportRect)
        {
            // draw all attached links
            n.DrawLinks(g, Font, viewportRect, SelectedObjects.OfType<Link>());
        }

        /// <summary>
        /// Determines the node at a given position
        /// </summary>
        /// <param name="x">The x value of the position</param>
        /// <param name="y">The y value of the position</param>
        /// <returns>A node if there is one at the given position, otherwise null</returns>
        public Node NodeAt(int x, int y)
        {
            return NodesAt(x, y).FirstOrDefault();
        }

        /// <summary>
        /// Returns all the nodes at the given x,y coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public IEnumerable<Node> NodesAt(int x, int y)
        {
            int xZoom = (int)(x);
            int yZoom = (int)(y);
            // reverse because the last node painted will be in front
            foreach (var n in Nodes.Where(n => !(n is ContainerNode)).Reverse())
            {
                // check if the point falls inside
                if (n.Area.Contains(xZoom, yZoom))
                    yield return n;
            }
            // check the container nodes, also in reverse order of depth
            foreach (var n in Nodes.OfType<ContainerNode>().OrderBy(cn => cn.Depth).Reverse())
            {
                if (n.Area.Contains(xZoom, yZoom))
                    yield return n;
            }

        }
        /// <summary>
        /// Determines the link at a given position
        /// </summary>
        /// <param name="x">The x value of the position</param>
        /// <param name="y">The y value of the position</param>
        /// <returns>A link if there is one at the given position, otherwise the default value (where both From and To are null)</returns>
        public Link LinkAt(int x, int y)
        {
            HashSet<Node> nodeTracker = new HashSet<Node>();
            foreach (var n in Nodes)
            {
                Link l;
                if (n.TryGetLink(x, y, out l))
                    return l;
            }
            return default(Link);
        }

        //public INodeObject SelectedObject { get; private set; }

        /// <summary>
        /// The current selected objects of the diagram
        /// </summary>
        public HashSet<INodeObject> SelectedObjects { get; internal set; }

        /// <summary>
        /// The active tool used
        /// </summary>
        private ITool activeTool;

        /// <summary>
        /// The point where the mouse was last clicked
        /// </summary>
        private Point mouseClickPoint;

        /// <summary>
        /// Occurs when mouse button is down
        /// Sets an active tool if appropriate and forward the mousedown event to that tool
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            // convert the event args to local coordinates with scroll & zoom applied
            MouseEventArgs ev = new MouseEventArgs(e.Button, e.Clicks, (int)((e.X - AutoScrollPosition.X) / Zoom), (int)((e.Y - AutoScrollPosition.Y) / Zoom), e.Delta);

            mouseClickPoint = new Point(ev.X, ev.Y);
            base.OnMouseDown(e);

            // determine the tool used
            activeTool = null;
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // if control is pressed, multi select
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    activeTool = new MultiSelectTool(this);
                else
                {
                    // otherwise set the selected node and start dragging
                    SetSelection(ev);
                    activeTool = new DragTool(this);
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // if the right mouse button is used, start the create link tool
                activeTool = new CreateLinkTool(this);
            }

            // forward the mousedown to the active tool
            bool toolRetVal = false;
            if (activeTool != null)
                toolRetVal = activeTool.OnMouseDown(ev);

            // if the tool has not done something or there is no active tool, show the new items menu
            if (!toolRetVal)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    mnuItems.Show(this, new Point(e.X, e.Y));
            }
        }
        /// <summary>
        /// Occurs when double clicked
        /// Opens the editor of the node if there was a node at the position
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
            // if there is a node double clicked, open the editor
            var node = NodeAt(mouseClickPoint.X, mouseClickPoint.Y);
            if (node != null)
            {
                bool refresh = node.OpenEditor();
                // if things have been changed, redraw
                if (refresh)
                    Redraw();
            }
        }

        /// <summary>
        /// Selects the node or link at the given coordinates (if there is one)
        /// </summary>
        /// <param name="e"></param>
        private void SetSelection(MouseEventArgs e)
        {
            var selection = new HashSet<INodeObject>();
            var node = NodeAt(e.X, e.Y);

            bool isDifferent;

            if (node != null)
            {
                isDifferent = !SelectedObjects.Contains(node);
                selection.Add(node);
            }
            else
            {
                // there is no node selected, check if there is a link
                var link = LinkAt(e.X, e.Y);
                if (link.From != null && link.To != null)
                {
                    isDifferent = !SelectedObjects.Contains(link);
                    selection.Add(link);
                }
                else
                    isDifferent = SelectedObjects.Count != 0;
            }

            // if the selection has changed, redraw
            if (isDifferent)
            {
                Redraw();
                SelectedObjects = selection;
            }
        }
        /// <summary>
        /// Occurs when the mouse is moved
        /// If there is an active tool, forward the mousemove
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // convert the event args to local coordiantes (based on scroll pos & zoom)
            MouseEventArgs ev = new MouseEventArgs(e.Button, e.Clicks, (int)((e.X - AutoScrollPosition.X) / Zoom), (int)((e.Y - AutoScrollPosition.Y) / Zoom), e.Delta);
            base.OnMouseMove(e);
            // if there is an active tool, forward the mousemove
            if (activeTool != null)
                activeTool.OnMouseMove(ev);
        }
        /// <summary>
        /// Occurs when the mouse button is released
        /// If there is an active tool ,forward the mouseup
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            // convert the event args to local coordiantes (based on scroll pos & zoom)
            MouseEventArgs ev = new MouseEventArgs(e.Button, e.Clicks, (int)((e.X - AutoScrollPosition.X) / Zoom), (int)((e.Y - AutoScrollPosition.Y) / Zoom), e.Delta);
            base.OnMouseUp(e);
            // If there is an active tool ,forward the mouseup
            if (activeTool != null)
                activeTool.OnMouseUp(ev);

            UpdateBoundingBox();
        }

        /// <summary>
        /// Occurs when the mouse clicked once
        /// If there is a node, forward the mouse click to the node (because fuck past me's logic and not forward that to the active tool FFFFFFFFFFFF)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            // convert the event args to local coordiantes (based on scroll pos & zoom)
            MouseEventArgs ev = new MouseEventArgs(e.Button, e.Clicks, (int)((e.X - AutoScrollPosition.X) / Zoom), (int)((e.Y - AutoScrollPosition.Y) / Zoom), e.Delta);
            base.OnMouseClick(e);
            var node = NodeAt(ev.X, ev.Y);
            if (node != null)
                node.OnMouseClick(e);
        }


        /// <summary>
        /// Occurs when the mouse wheel is scrolled
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // if control is held
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {

                // get the mouse event args based on the old zoom factor
                MouseEventArgs ev = new MouseEventArgs(e.Button, e.Clicks, (int)((e.X - AutoScrollPosition.X) / Zoom), (int)((e.Y - AutoScrollPosition.Y) / Zoom), e.Delta);

                // change zoom based on the direction the mouse wheel was scrolled in   
                float zoom = Zoom;
                if (e.Delta < 0)
                    zoom -= 0.1f;
                else
                    zoom += 0.1f;

                Zoom = zoom;

                // get the mouse event args based on the new zoom factor
                MouseEventArgs newEv = new MouseEventArgs(e.Button, e.Clicks, (int)((e.X - AutoScrollPosition.X) / Zoom), (int)((e.Y - AutoScrollPosition.Y) / Zoom), e.Delta);

                // gets the difference between old and new
                var xDiff = new Point(ev.X - newEv.X, ev.Y - newEv.Y).RoundTo(GridSize.Width, GridSize.Height);

                // move all the nodes accordingly so the viewport remains the same
                foreach (var n in Nodes)
                    n.Position = new Point(n.Position.X - xDiff.X, n.Position.Y - xDiff.Y);

                UpdateBoundingBox();
                Redraw();
            }
            else
                base.OnMouseWheel(e);
        }

        /// <summary>
        /// Determines the bounding box of all the objects and update
        /// the scroll min position. If there are any nodes falling outside
        /// translate everything until they are visible
        /// </summary>
        public void UpdateBoundingBox()
        {

            var bbox = NodeExtensions.GetBoundingBox(Nodes.Select(n => n.Area));

            int left = bbox.Left;
            int top = bbox.Top;
            int right = bbox.Right;
            int bottom = bbox.Bottom;
            
            // if nodes are outside the boundaries move all the nodes so they become visible
            if (left < 0)
            {
                right += Math.Abs(left);
                foreach (var n in Nodes)
                    n.Position = new Point(n.Position.X + Math.Abs(left), n.Position.Y);
            }
            if (top < 0)
            {
                bottom += Math.Abs(top);
                foreach (var n in Nodes)
                    n.Position = new Point(n.Position.X, n.Position.Y + Math.Abs(top));
            }

            AutoScrollMinSize = new System.Drawing.Size((int)((right + 1) * Zoom), (int)((bottom + 1) * Zoom));
        }


        /// <summary>
        /// Occurs when a key is pressed
        /// Either delete the selection or 
        /// determine if the key combination is part of a shortcut defined in the menu items
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedObjects();
            }
            else
            {
                // accumulate keystrokes if they are between A and Z and the key combo is incomplete 
                if (e.Control && e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z && curKeyStrokes.Count < 2)
                    curKeyStrokes.Add(e.KeyCode);
                else
                    curKeyStrokes.Clear();

                // the key combo is complete, check if there are any factories that have the key combo defined
                if (curKeyStrokes.Count == 2)
                {
                    var factory = Factories.Where(f => f.GetShortcutKeys() != null && curKeyStrokes.AreEqual(f.GetShortcutKeys())).FirstOrDefault();
                    // if there is a factory with this keyboard shortcut, create a node
                    if (factory != null)
                    {
                        var n = factory.CreateNode(this);
                        // if a node was created, add it
                        if (n != null)
                            this.AddNewNode(n);

                    }
                    curKeyStrokes.Clear();
                }
            }
        }

        /// <summary>
        /// Removes the current selection
        /// </summary>
        private void DeleteSelectedObjects()
        {
            bool needToRedraw = false;
            foreach (var obj in SelectedObjects)
            {
                if (obj is Node)
                {
                    var n = ((Node)obj);

                    // also remove all the links that are attached to this node

                    // remove links going to other nodes
                    foreach (var subn in n.GetLinkedNodes())
                    {
                        if (subn != null)
                            n.RemoveLinkTo(subn);
                    }
                    // remove links coming to this node
                    foreach (var parentn in n.ParentNodes.ToArray())
                        parentn.RemoveLinkTo(n);

                    // remove node 
                    Nodes.Remove(n);

                    needToRedraw = true;
                }
                else if (obj is Link)
                {
                    var link = (Link)obj;
                    link.From.RemoveLinkTo(link.To);

                    needToRedraw = true;
                }
            }
            if (needToRedraw)
                Redraw();
        }
        /// <summary>
        /// Keeps track of the current keystrokes of a key combo
        /// </summary>
        private List<Keys> curKeyStrokes = new List<Keys>();

        /// <summary>
        /// Adds a new node to the diagram
        /// </summary>
        /// <param name="n"></param>
        internal void AddNewNode(Node n)
        {
            // add it on the last clicked position
            n.Position = new Point(mouseClickPoint.X - n.NodeSize.Width / 2, mouseClickPoint.Y - n.NodeSize.Height / 2);
            // open the editor
            bool ok = n.OpenEditor();
            if (ok)
            {
                // add the node & redraw
                Nodes.Add(n);
                Redraw();
            }
        }

        /// <summary>
        /// Returns the create context menu
        /// </summary>
        internal ContextMenuStrip CreateMenu { get { return mnuItems; } }

        /// <summary>
        /// Determines the way links should be drawn
        /// </summary>
        public LineTypeEnum LineType { get; set; }

        /// <summary>
        /// Draw the entire diagram to a bitmap and return it
        /// </summary>
        /// <returns>A bitmap of the entire diagram</returns>
        public Image AsImage()
        {
            // determine the bounding box
            var bbox = NodeExtensions.GetBoundingBox(Nodes.Select(n => n.Area));

            bbox = new Rectangle(0, 0, bbox.Right + 1, bbox.Bottom + 1);
            bbox.Inflate(NodeSize.Width, NodeSize.Height);
            Bitmap bmp = new Bitmap(bbox.Width, bbox.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                DrawContents(g, bbox, new Point(0, 0), 1);
            }
            return bmp;
        }

        /// <summary>
        /// Initializes the few components this control contains
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mnuItems = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuItems.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuItems
            // 
            this.mnuItems.Name = "mnuItems";
            this.mnuItems.Size = new System.Drawing.Size(181, 48);

            this.mnuItems.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private ContextMenuStrip mnuItems;
        private System.ComponentModel.IContainer components;

    }

    /// <summary>
    /// The available types of the links
    /// </summary>
    public enum LineTypeEnum
    {
        Bezier,
        FourWay,
        Straight
    }

}
