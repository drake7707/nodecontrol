using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodeControl.Nodes;
using System.Drawing;

namespace NodeControl.Nodes
{
    /// <summary>
    /// An end node, end nodes can be linked to but can not link to other nodes
    /// </summary>
    public class EndNode : Node
    {
        public EndNode(NodeDiagram parent)
            : base(parent)
        {

        }

        /// <summary>
        /// The node size of an end node is always square and half the size of the default node size
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
        /// Draws the end node
        /// </summary>
        /// <param name="g">The graphics to draw the node with</param>
        /// <param name="f">The font to use</param>
        /// <param name="viewportRect">The viewport bounds</param>
        /// <param name="isSelected">The node selection state</param>
        protected internal override void Draw(System.Drawing.Graphics g, System.Drawing.Font f, System.Drawing.Rectangle viewportRect, bool isSelected)
        {
            Rectangle area = Area;
            Rectangle innerArea = area;
            innerArea.Inflate(-5, -5);

            // if it is (partly) visible
            if (viewportRect.IntersectsWith(area))
            {
                Brush br;
                Pen p;
                if (isSelected)
                {
                    br = Brushes.Red;
                    p = Pens.Red;
                }
                else
                {
                    br = Brushes.Black;
                    p = Pens.Black;
                }

                // draw outer ellipse
                g.DrawEllipse(p, area);

                // draw inner ellipse
                g.FillEllipse(br, innerArea);
            }
        }

        /// <summary>
        /// The end node can't link to other nodes so never return linked nodes
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Node> GetLinkedNodes()
        {
            yield break;
        }


        /// <summary>
        /// Removes a link to a node. End nodes can't link to other nodes so this does nothing
        /// </summary>
        /// <param name="n">The node to remove the link to</param>
        internal protected override void RemoveLinkTo(Node n)
        {
        }

        /// <summary>
        /// Adds a link to a given target node. End nodes can't have links to other nodes
        /// </summary>
        /// <param name="mouseDownInfo"></param>
        /// <param name="targetNode"></param>
        internal protected override void AddLink(MouseDownInfo mouseDownInfo, Node targetNode)
        {
        }

        /// <summary>
        /// Opens the editor. End nodes don't have editors so always return true
        /// </summary>
        /// <returns></returns>
        internal protected override bool OpenEditor()
        {
            return true;
        }

        /// <summary>
        /// End nodes can't link to other nodes
        /// </summary>
        public override bool CanLink
        {
            get
            {
                return false;
            }
        }


    }
}
