using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace NodeControl.Tools
{
    class MultiSelectTool : Tool
    {
        public MultiSelectTool(NodeDiagram diagram)
            : base(diagram)
        {

        }

        private Point orgPos;

        public override void OnDraw(System.Drawing.Graphics g)
        {
            if (mouseDown)
            {
                Rectangle drawingArea = area;
                drawingArea.Offset((int)(diagram.AutoScrollPosition.X / diagram.Zoom), (int)(diagram.AutoScrollPosition.Y / diagram.Zoom));

                drawingArea = drawingArea.Zoom(diagram.Zoom);
                g.DrawRectangle(Pens.Green, drawingArea);
            }
                
        }

        private bool mouseDown;
        public override bool OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            mouseDown = true;
            orgPos = new Point(e.X, e.Y);
            return true;
        }

        private Rectangle area;
        public override bool OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            if (mouseDown)
            {
                SelectMultipleNodes(e);
                return true;
            }
            return false;
        }

        private void SelectMultipleNodes(System.Windows.Forms.MouseEventArgs e)
        {

            area = new Rectangle(orgPos.X < e.X ? orgPos.X : e.X,
                                          orgPos.Y < e.Y ? orgPos.Y : e.Y,
                                          Math.Abs(orgPos.X - e.X),
                                          Math.Abs(orgPos.Y - e.Y));
            diagram.Invalidate();

            if (Math.Abs(orgPos.X - e.X) < 10 && Math.Abs(orgPos.Y - e.Y) < 10)
            {

            }
            else
            {
                var selectedNodes = diagram.Nodes.Where(n => n.Area.IntersectsWith(area));
                diagram.SelectedObjects = new HashSet<INodeObject>(selectedNodes.Cast<INodeObject>());
                diagram.Redraw();
            }
        }

        public override bool OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            mouseDown = false;
            diagram.Invalidate();

            if (Math.Abs(orgPos.X - e.X) < 10 && Math.Abs(orgPos.Y - e.Y) < 10)
            {
                var n = diagram.NodeAt(e.X, e.Y);
                if (n != null)
                {
                    if (n.IsSelected)
                        diagram.SelectedObjects.Remove(n);
                    else
                        diagram.SelectedObjects.Add(n);

                    diagram.Redraw();
                }
            }
            else
            {
                SelectMultipleNodes(e);
            }

            return true;
        }
    }
}
