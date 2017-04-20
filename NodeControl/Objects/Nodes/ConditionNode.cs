using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using NodeControl.NodeEditor;

namespace NodeControl.Nodes
{
    /// <summary>
    /// A node that has conditional links to other nodes
    /// E.g Yes / No with each answer pointing to a different node
    /// </summary>
    public class ConditionNode : Node
    {
        public ConditionNode(NodeDiagram parent)
            : base(parent)
        {
            LinksTo = new ConditionCollection(this);
        }

        /// <summary>
        /// The node size of a conditional node is dependent on the number of conditions
        /// the node has
        /// </summary>
        public override Size NodeSize
        {
            get
            {
                if (LinksTo.Count == 0)
                    return base.NodeSize;

                // ensure there is enough room for the conditions
                if (Direction == DirectionEnum.Horizontal)
                {
                    if (base.NodeSize.Height / LinksTo.Count < 10)
                    {
                        int height = 15 * LinksTo.Count;
                        return new Size(base.NodeSize.Width, height);
                    }
                }
                else
                {
                    if (base.NodeSize.Width / LinksTo.Count < 10)
                    {
                        int width = 15 * LinksTo.Count;
                        return new Size(width, base.NodeSize.Height);
                    }
                }

                return base.NodeSize;
            }
        }

        /// <summary>
        /// All the conditional links to other nodes
        /// </summary>
        public ConditionCollection LinksTo { get; set; }

        /// <summary>
        /// Returns all the linked nodes
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Node> GetLinkedNodes()
        {
            foreach (var c in LinksTo)
                yield return c.LinksTo;
        }

        /// <summary>
        /// Override the standard drawing of the node to draw the text and conditions next (or beneath it)
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="font">The font to use for the text</param>
        /// <param name="viewportRect">The bounds of the viewport</param>
        /// <param name="isSelected">True if the node is selected</param>
        protected internal override void Draw(Graphics g, Font font, Rectangle viewportRect, bool isSelected)
        {
            Rectangle area = Area;

            if (viewportRect.IntersectsWith(area))
            {
                RectangleF textAreaRect;

                // if there are any conditions, half the space for the text
                if (LinksTo.Count > 0)
                {
                    if (Direction == DirectionEnum.Horizontal)
                        textAreaRect = new RectangleF(area.Left, area.Top, area.Width / 2, area.Height);
                    else
                        textAreaRect = new RectangleF(area.Left, area.Top, area.Height, area.Height / 2);
                }
                else
                    textAreaRect = new RectangleF(area.Left, area.Top, area.Width, area.Height);

                // draw the background
                using (LinearGradientBrush br = new LinearGradientBrush(area, Color.White, Color.FromArgb(229, 229, 229), LinearGradientMode.Vertical))
                    g.FillRectangle(br, area);

                // draw the border of the node
                if (isSelected)
                    g.DrawRectangle(Pens.Red, area);
                else
                    g.DrawRectangle(Pens.Black, area);

                // draw the node text
                g.DrawString((Text + ""), font, Brushes.Black, textAreaRect, new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });


                if (LinksTo.Count > 0)
                {
                    DrawConditions(g, font, area);
                }
            }
        }

        /// <summary>
        /// Draw the conditions of the node
        /// </summary>
        /// <param name="g">The graphics to draw with</param>
        /// <param name="font">The font to use for the node text</param>
        /// <param name="area">The bounds of the node</param>
        private void DrawConditions(Graphics g, Font font, Rectangle area)
        {
            // use a smaller font
            using (Font smallf = new Font(font.FontFamily, 6f))
            {
                // if horizontal draw the conditions at the right side
                if (Direction == DirectionEnum.Horizontal)
                {
                    float cellSize = area.Height / (float)LinksTo.Count;
                    float top = area.Top;
                    foreach (var condition in LinksTo)
                    {
                        RectangleF condRect = new RectangleF(area.Left + area.Width / 2, top, area.Width / 2, cellSize);
                        g.DrawRectangle(Pens.Black, condRect.X, 1 + condRect.Y, condRect.Width, condRect.Height - 2);
                        g.DrawString(condition.Text + "", smallf, Brushes.Black, condRect, new StringFormat() { Alignment = StringAlignment.Center, Trimming = StringTrimming.None, LineAlignment = StringAlignment.Center });
                        top += cellSize;
                    }
                }
                else
                {
                    // else if vertical at the bottom side
                    float cellSize = area.Width / (float)LinksTo.Count;
                    float left = area.Left;
                    foreach (var condition in LinksTo)
                    {
                        RectangleF condRect = new RectangleF(left, area.Top + area.Height / 2, cellSize, area.Height / 2);
                        g.DrawRectangle(Pens.Black, condRect.X, 1 + condRect.Y, condRect.Width, condRect.Height - 2);
                        g.DrawString(condition.Text + "", smallf, Brushes.Black, condRect, new StringFormat() { Alignment = StringAlignment.Center, Trimming = StringTrimming.None, LineAlignment = StringAlignment.Center });
                        left += cellSize;
                    }
                }
            }
        }

        /// <summary>
        /// Removes a link to a node
        /// </summary>
        /// <param name="n">The target node</param>
        internal protected override void RemoveLinkTo(Node n)
        {
            // find the condition that links to the given node
            var condition = LinksTo.Where(l => l.LinksTo == n).FirstOrDefault();
            if (condition != null && condition.LinksTo != null)
            {
                // remove the current node from the parent of the target node and set the condition link to null
                condition.LinksTo.ParentNodes.RemoveWithoutRefBack(this);
                condition.LinksTo = null;
            }
        }

        /// <summary>
        /// Add additional info of which condition is clicked on the mouse down
        /// </summary>
        /// <param name="e">The mouse event args</param>
        /// <returns>Returns the info about where the mouse is down</returns>
        internal override MouseDownInfo GetMouseDownInfo(System.Windows.Forms.MouseEventArgs e)
        {
            var area = this.Area;
            if (Direction == DirectionEnum.Horizontal)
            {
                float top = area.Top;
                float cellSize = area.Height / (float)LinksTo.Count;
                foreach (var condition in LinksTo)
                {
                    RectangleF condRect = new RectangleF(area.Left + area.Width / 2, top, area.Width / 2, cellSize);
                    // if the point falls inside the condition, return the condition info
                    if (condRect.Contains(new PointF(e.X, e.Y)))
                        return new ConditionMouseDownInfo() { StartNode = this, Condition = condition };
                    
                    top += cellSize;
                }
            }
            else
            {
                float left = area.Left;
                float cellSize = area.Width / (float)LinksTo.Count;
                foreach (var condition in LinksTo)
                {
                    RectangleF condRect = new RectangleF(left, area.Top + area.Height / 2, cellSize, area.Height / 2);
                    // if the point falls inside the condition, return the condition info
                    if (condRect.Contains(new PointF(e.X, e.Y)))
                        return new ConditionMouseDownInfo() { StartNode = this, Condition = condition };

                    left += cellSize;
                }

            }

            return null;
        }

        /// <summary>
        /// Adds a link to a condition to the target node
        /// </summary>
        /// <param name="mouseDownInfo">The mouse down info that contains the condition info</param>
        /// <param name="targetNode">The target node to link to</param>
        internal protected override void AddLink(MouseDownInfo mouseDownInfo, Node targetNode)
        {
            // if there is info
            if (mouseDownInfo != null)
            {
                ConditionMouseDownInfo conditionInfo = (ConditionMouseDownInfo)mouseDownInfo;
                // check if the condition linked to another node, if so remove the link first
                var oldLink = conditionInfo.Condition.LinksTo;
                if (oldLink != null)
                    oldLink.ParentNodes.RemoveWithoutRefBack(this);

                // add the link to the condition
                conditionInfo.Condition.LinksTo = targetNode;
                if (targetNode != null)
                    targetNode.ParentNodes.AddWithoutRefBack(this);
            }
        }

        /// <summary>
        /// Opens the condition node editor
        /// </summary>
        /// <returns>True if something was changed</returns>
        internal protected override bool OpenEditor()
        {
            using (ConditionNodeEditor dlg = new ConditionNodeEditor(this))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.Text = dlg.NodeText;

                    string[] nodeConditions = dlg.NodeConditions;
                    for (int i = 0; i < nodeConditions.Length; i++)
                    {
                        // add new conditions if there are new ones
                        if (i >= LinksTo.Count)
                            LinksTo.Add(new Condition() { Text = nodeConditions[i], LinksTo = null });
                        else // otherwise just change the text
                            LinksTo[i].Text = nodeConditions[i];
                    }

                    // if some conditions are removed, remove all the links that are invalid
                    for (int i = LinksTo.Count - 1; i >= nodeConditions.Length; i--)
                    {
                        var condition = LinksTo[i];
                        LinksTo.RemoveAt(i);
                        if (condition.LinksTo != null)
                        {
                            condition.LinksTo.ParentNodes.RemoveWithoutRefBack(this);
                            condition.LinksTo = null;
                        }
                    }

                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Represents a collection of conditions of a node
    /// </summary>
    public class ConditionCollection : IList<Condition>
    {
        /// <summary>
        /// The node that owns this collection
        /// </summary>
        private Node owner;

        /// <summary>
        /// The conditions
        /// </summary>
        private List<Condition> lst;
        internal ConditionCollection(Node owner)
        {
            this.lst = new List<Condition>();
            this.owner = owner;
        }

        public int IndexOf(Condition item)
        {
            return lst.IndexOf(item);
        }

        public void Insert(int index, Condition item)
        {
            lst.Insert(index, item);
            item.LinksTo.ParentNodes.AddWithoutRefBack(owner);
        }

        public void RemoveAt(int index)
        {
            Condition n = lst[index];
            lst.RemoveAt(index);
            if (n.LinksTo != null)
                n.LinksTo.ParentNodes.RemoveWithoutRefBack(owner);
        }

        public Condition this[int index]
        {
            get
            {
                return lst[index];
            }
            set
            {
                var n = lst[index];
                if (value.LinksTo != n.LinksTo)
                {
                    if (n.LinksTo != null)
                        n.LinksTo.ParentNodes.RemoveWithoutRefBack(owner);
                    lst[index] = value;
                    if (value.LinksTo != null)
                        value.LinksTo.ParentNodes.AddWithoutRefBack(owner);
                }
            }
        }

        public void Add(Condition item)
        {
            lst.Add(item);
            if (item.LinksTo != null)
                item.LinksTo.ParentNodes.AddWithoutRefBack(owner);
        }

        public void Clear()
        {
            foreach (var item in lst)
                item.LinksTo.ParentNodes.RemoveWithoutRefBack(owner);
            lst.Clear();
        }

        public bool Contains(Condition item)
        {
            return lst.Contains(item);
        }

        public void CopyTo(Condition[] array, int arrayIndex)
        {
            lst.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return lst.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Condition item)
        {
            item.LinksTo.ParentNodes.RemoveWithoutRefBack(owner);
            return lst.Remove(item);
        }

        public IEnumerator<Condition> GetEnumerator()
        {
            return lst.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return lst.GetEnumerator();
        }
    }


    /// <summary>
    /// A condition of a node 
    /// </summary>
    public class Condition
    {
        /// <summary>
        /// The text of the condition
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The node the condition links to, null if there is no linked node
        /// </summary>
        public Node LinksTo { get; set; }

        /// <summary>
        /// A tag that contains additional info about the condition
        /// </summary>
        public object Tag { get; set; }
    }

    /// <summary>
    /// Additional info of the condition where the mouse is clicked
    /// </summary>
    internal class ConditionMouseDownInfo : MouseDownInfo
    {
        public Condition Condition { get; set; }
    }

}
