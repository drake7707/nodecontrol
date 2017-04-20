using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace NodeControl.Nodes
{
    /// <summary>
    /// A start node. Start nodes can only link to a single node and cannot be linked to
    /// </summary>
    public class StartNode : Node
    {
        public StartNode(NodeDiagram parent)
            : base(parent)
        {

        }

        private Node linksTo;

        /// <summary>
        /// The node the start node links to, or null when it doesn't link to anything
        /// </summary>
        public Node LinksTo
        {
            get { return linksTo; }
            set
            {

                if (linksTo != value)
                {
                    if (linksTo != null)
                        linksTo.ParentNodes.RemoveWithoutRefBack(this);
                    linksTo = value;
                    if (value != null)
                        linksTo.ParentNodes.AddWithoutRefBack(this);
                }
            }
        }

        /// <summary>
        /// The node size of the start node is always a square and half the size of a standard node
        /// </summary>
        public override System.Drawing.Size NodeSize
        {
            get
            {
                if (base.NodeSize.Width < base.NodeSize.Height)
                    return new System.Drawing.Size(base.NodeSize.Width / 2, base.NodeSize.Width / 2);
                else
                    return new System.Drawing.Size(base.NodeSize.Height / 2, base.NodeSize.Height / 2);
            }
        }

        /// <summary>
        /// Draws the start node (a single black ellipse)
        /// </summary>
        /// <param name="g">The graphics to draw the node with</param>
        /// <param name="f">The font to use</param>
        /// <param name="viewportRect">The viewport bounds</param>
        /// <param name="isSelected">The node selection state</param>
        protected internal override void Draw(System.Drawing.Graphics g, System.Drawing.Font f, System.Drawing.Rectangle viewportRect, bool isSelected)
        {
            Rectangle area = Area;
            // if it's visible
            if (viewportRect.IntersectsWith(area))
            {
                Brush br;
                if (isSelected)
                    br = Brushes.Red;
                else
                    br = Brushes.Black;

                g.FillEllipse(br, area);
            }
        }

        /// <summary>
        /// Returns the nodes it links to, only give the one node it can link to
        /// </summary>
        /// <returns>The node it can link to</returns>
        public override IEnumerable<Node> GetLinkedNodes()
        {
            yield return LinksTo;
        }

        /// <summary>
        /// Removes the link to a given node
        /// </summary>
        /// <param name="n">The node to remove the link to</param>
        internal protected override void RemoveLinkTo(Node n)
        {
            // if the start node links to the given node , remove it
            if (linksTo == n)
                LinksTo = null;
        }

        /// <summary>
        /// Adds a link to the given target node
        /// </summary>
        /// <param name="mouseDownInfo">The info when the mouse was pressed</param>
        /// <param name="targetNode">The target node to link to</param>
        internal protected override void AddLink(MouseDownInfo mouseDownInfo, Node targetNode)
        {
            LinksTo = targetNode;
        }

        /// <summary>
        /// Opens the editor, there is no editor of the start node so always return true
        /// </summary>
        /// <returns>Returns true</returns>
        internal protected override bool OpenEditor()
        {
            return true;
        }

        /// <summary>
        /// The start node can't be linked to, so return false
        /// </summary>
        public override bool CanBeLinkedTo
        {
            get
            {
                return false;
            }
        }
    }
}
