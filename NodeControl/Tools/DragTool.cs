using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using NodeControl.Nodes;

namespace NodeControl.Tools
{
    class DragTool : Tool
    {

        public DragTool(NodeDiagram diagram)
            : base(diagram)
        {
            selectedObjectsPositions = new Dictionary<Node, Point>();
        }

        private Point orgPos;
        private Dictionary<Node, Point> selectedObjectsPositions;
        private bool mousedown;


        private ContainerNode[] suspendedContainers;

        private Node nodeAtDragPoint;
        public override bool OnMouseDown(MouseEventArgs e)
        {
            mousedown = true;
            selectedObjectsPositions.Clear();
            orgPos = new Point(e.X, e.Y);

            nodeAtDragPoint = diagram.NodeAt(e.X, e.Y);

            var objectsToMove = diagram.SelectedObjects.OfType<Node>();
            foreach (var obj in objectsToMove)
            {
                var n = ((Node)obj);
                if (!selectedObjectsPositions.ContainsKey(n))
                    selectedObjectsPositions.Add(n, n.Position);

                if (n is ContainerNode)
                {
                    foreach (var child in ((ContainerNode)n).GetAllChildren())
                    {
                        if (!selectedObjectsPositions.ContainsKey(child))
                            selectedObjectsPositions.Add(child, child.Position);
                    }
                }
            }

            suspendedContainers = selectedObjectsPositions.Keys.Where(n => n.Container != null)
                                                          .Select(n => n.Container)
                                                          .Distinct()
                                                          .ToArray();
            foreach (var c in suspendedContainers)
                c.SuspendFit();

            if (selectedObjectsPositions.Count > 0)
                return true;
            else
                return false;
        }

        private Point mousePos;
        public override bool OnMouseMove(MouseEventArgs e)
        {
            if (mousedown && selectedObjectsPositions.Count > 0)
            {
                directionRight = mousePos.X < e.X;
                mousePos = new Point(e.X, e.Y);
                MoveNode(e, false);
                return true;
            }

            return false;
        }

        public override bool OnMouseUp(MouseEventArgs e)
        {
            mousedown = false;
            MoveNode(e, true);


            HashSet<ContainerNode> containersThatWillChangeInBounds = new HashSet<ContainerNode>();
            foreach (var n in diagram.SelectedObjects.OfType<Node>())
            {
                ContainerNode cn = n.Container;
                while (cn != null)
                {
                    containersThatWillChangeInBounds.Add(cn);
                    cn = cn.Container;
                }
            }

            if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
            {
                var nodesIntersecting = new HashSet<Node>(diagram.NodesAt(e.X, e.Y));

                var nodesIntersectingThatAreNotPartOfSelection = nodesIntersecting.Except(selectedObjectsPositions.Keys.OfType<Node>());
                var nodesIntersectingThatArePartOfSelection = nodesIntersecting.Where(n => selectedObjectsPositions.ContainsKey(n));

                var targetNode = nodesIntersectingThatAreNotPartOfSelection.FirstOrDefault();
                if (targetNode != null)
                    targetNode.OnNodeDragDrop(diagram.SelectedObjects.OfType<Node>().Where(n => n != targetNode));
                else
                {

                    var nodesToCheckIfTheyNeedToBeRemovedFromTheirContainer = diagram.SelectedObjects.OfType<Node>().ToList();
                    if (nodeAtDragPoint is ContainerNode)
                    {
                        // the node we're dragging is a container, any child node shouldn't be removed just by dragging the its container
                        foreach (var child in ((ContainerNode)nodeAtDragPoint).GetAllChildren())
                            nodesToCheckIfTheyNeedToBeRemovedFromTheirContainer.Remove(child);
                    }

                    foreach (var n in nodesToCheckIfTheyNeedToBeRemovedFromTheirContainer)
                    {
                        if (n.Container != null && !nodesIntersecting.Contains(n.Container))
                        {
                            var container = n.Container;
                            container.Children.Remove(n);
                        }
                    }
                }
            }

            // everything is moved, allow bounds to be updated based on their children
            foreach (var c in suspendedContainers.OrderByDescending(cn => cn.Depth))
                c.ResumeFit();


            // add new containers (those that are referenced by the changed children)
            foreach (var n in diagram.SelectedObjects.OfType<Node>())
            {
                ContainerNode cn = n.Container;
                while (cn != null)
                {
                    containersThatWillChangeInBounds.Add(cn);
                    cn = cn.Container;
                }
            }


            foreach (var c in containersThatWillChangeInBounds.OrderByDescending(cn => cn.Depth))
            {
                c.UpdateBounds();
            }

            diagram.Invalidate();

            //var nodesIntersecting =  diagram.Nodes.Where(n => selectedObjectsPositions.OfType<Node>().Any(selNode => n.Area.IntersectsWith(selNode.Area)));

            return true;
        }

        private bool directionRight;

        private void MoveNode(MouseEventArgs e, bool snapToGrid)
        {
            int offX = e.X - orgPos.X;
            int offY = e.Y - orgPos.Y;


            var allOtherNodes = diagram.Nodes.Except(selectedObjectsPositions.Keys).ToArray();
            if (allOtherNodes.Length > 0 && (Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                int xClosestToCursor;
                Node closestNodeX;
                bool left;
                bool selLeft;
                int yClosestToCursor;
                Node closestNodeY;
                bool top;
                bool selTop;

                var bbox = NodeExtensions.GetBoundingBox(selectedObjectsPositions.Keys.Select(n => n.Area));
                GetClosestGuideline(bbox, allOtherNodes, out xClosestToCursor, out yClosestToCursor, out closestNodeX, out closestNodeY, out left, out selLeft, out top, out selTop);



                foreach (var pair in selectedObjectsPositions)
                {
                    int posX = pair.Value.X + offX;
                    int posY = pair.Value.Y + offY;
                    if (xClosestToCursor < 10)
                    {
                        if (left)
                        {
                            if (selLeft)
                                posX = closestNodeX.Area.Left;
                            else
                                posX = closestNodeX.Area.Left - pair.Key.Area.Width;
                        }
                        else
                        {
                            if (selLeft)
                                posX = closestNodeX.Area.Right;
                            else
                                posX = closestNodeX.Area.Right - pair.Key.Area.Width;
                        }
                    }
                    if (yClosestToCursor < 10)
                    {
                        if (top)
                        {
                            if (selTop)
                                posY = closestNodeY.Area.Top;
                            else
                                posY = closestNodeY.Area.Top - pair.Key.Area.Height;
                        }
                        else
                        {
                            if (selTop)
                                posY = closestNodeY.Area.Bottom;
                            else
                                posY = closestNodeY.Area.Bottom - pair.Key.Area.Height;
                        }
                    }


                    pair.Key.Position = new Point(posX, posY);
                }
            }
            else
            {
                foreach (var pair in selectedObjectsPositions)
                {
                    int posX = pair.Value.X + offX;
                    int posY = pair.Value.Y + offY;

                    if (snapToGrid)
                        pair.Key.Position = new Point(posX, posY).RoundTo(diagram.GridSize.Width, diagram.GridSize.Height);
                    else
                        pair.Key.Position = new Point(posX, posY);
                }
            }
            diagram.Redraw();
        }

        public override void OnDraw(Graphics g)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                var allOtherNodes = diagram.Nodes.Except(selectedObjectsPositions.Keys).ToArray();
                if (allOtherNodes.Length > 0)
                {
                    int xClosestToCursor;
                    Node closestNodeX;
                    bool left;
                    bool selLeft;
                    int yClosestToCursor;
                    Node closestNodeY;
                    bool top;
                    bool selTop;

                    var bbox = NodeExtensions.GetBoundingBox(selectedObjectsPositions.Keys.Select(n => n.Area));

                    GetClosestGuideline(bbox, allOtherNodes, out xClosestToCursor, out yClosestToCursor, out closestNodeX, out closestNodeY, out left, out selLeft, out top, out selTop);


                    Rectangle closestXArea = closestNodeX.Area;
                    closestXArea.Offset((int)(diagram.AutoScrollPosition.X / diagram.Zoom), (int)(diagram.AutoScrollPosition.Y / diagram.Zoom));
                    closestXArea = closestXArea.Zoom(diagram.Zoom);

                    Rectangle closestYArea = closestNodeY.Area;
                    closestYArea.Offset((int)(diagram.AutoScrollPosition.X / diagram.Zoom), (int)(diagram.AutoScrollPosition.Y / diagram.Zoom));
                    closestYArea = closestYArea.Zoom(diagram.Zoom);

                    if (xClosestToCursor < 10)
                    {
                        if (left)
                            g.DrawLine(Pens.LimeGreen, new Point(closestXArea.Left, 0), new Point(closestXArea.Left, diagram.Height));
                        else
                            g.DrawLine(Pens.LimeGreen, new Point(closestXArea.Right, 0), new Point(closestXArea.Right, diagram.Height));
                    }

                    if (yClosestToCursor < 10)
                    {
                        if (top)
                            g.DrawLine(Pens.LimeGreen, new Point(0, closestYArea.Top), new Point(diagram.Width, closestYArea.Top));
                        else
                            g.DrawLine(Pens.LimeGreen, new Point(0, closestYArea.Bottom), new Point(diagram.Width, closestYArea.Bottom));
                    }
                }
            }
        }

        private void GetClosestGuideline(Rectangle bbox, Node[] allOtherNodes, out int xClosestToCursor, out int yClosestToCursor, out Node closestNodeX, out Node closestNodeY, out bool left, out bool selLeft, out bool top, out bool selTop)
        {

            selTop = false;
            selLeft = false;
            xClosestToCursor = int.MaxValue;
            yClosestToCursor = int.MaxValue;

            closestNodeX = null;
            closestNodeY = null;
            left = false;
            top = false;
            try
            {

                foreach (var n in allOtherNodes)
                {
                    if (xClosestToCursor > Math.Abs(bbox.Right - n.Area.Left))
                    {
                        xClosestToCursor = Math.Abs(bbox.Right - n.Area.Left);
                        left = true;
                        selLeft = false;
                        closestNodeX = n;
                    }
                    if (xClosestToCursor > Math.Abs(bbox.Left - n.Area.Left))
                    {
                        xClosestToCursor = Math.Abs(bbox.Left - n.Area.Left);
                        left = true;
                        selLeft = true;
                        closestNodeX = n;
                    }
                    if (xClosestToCursor > Math.Abs(bbox.Left - n.Area.Right))
                    {
                        xClosestToCursor = Math.Abs(bbox.Left - n.Area.Right);
                        left = false;
                        selLeft = true;
                        closestNodeX = n;
                    }
                    if (xClosestToCursor > Math.Abs(bbox.Right - n.Area.Right))
                    {
                        xClosestToCursor = Math.Abs(bbox.Right - n.Area.Right);
                        left = false;
                        selLeft = false;
                        closestNodeX = n;
                    }



                    if (yClosestToCursor > Math.Abs(bbox.Bottom - n.Area.Top))
                    {
                        yClosestToCursor = Math.Abs(bbox.Bottom - n.Area.Top);
                        top = true;
                        selTop = false;
                        closestNodeY = n;
                    }
                    if (yClosestToCursor > Math.Abs(bbox.Top - n.Area.Top))
                    {
                        yClosestToCursor = Math.Abs(bbox.Top - n.Area.Top);
                        top = true;
                        selTop = true;
                        closestNodeY = n;
                    }
                    if (yClosestToCursor > Math.Abs(bbox.Top - n.Area.Bottom))
                    {
                        yClosestToCursor = Math.Abs(bbox.Top - n.Area.Bottom);
                        top = false;
                        selTop = true;
                        closestNodeY = n;
                    }
                    if (yClosestToCursor > Math.Abs(bbox.Bottom - n.Area.Bottom))
                    {
                        yClosestToCursor = Math.Abs(bbox.Bottom - n.Area.Bottom);
                        top = false;
                        selTop = false;
                        closestNodeY = n;
                    }
                }
            }
            catch (Exception)
            {
            }
        }



    }
}
