using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodeControl.Nodes;
using System.Drawing;
using System.Drawing.Drawing2D;
using NodeControl.NodeEditor;

namespace NodeControl.Nodes
{
    /// <summary>
    /// Represents a node that contains other nodes
    /// </summary>
    class ContainerNode : Node
    {

        private int paddingForText = 20;

        private Size nodeSize;


        /// <summary>
        /// The nodes that the container contains
        /// </summary>
        public ContainerNodeCollection Children { get; private set; }

        public ContainerNode(NodeDiagram parent)
            : base(parent)
        {
            Children = new ContainerNodeCollection(this);
        }

        /// <summary>
        /// Recursively returns all the children of the container. If there are any containers in the children
        /// return all their children as well
        /// </summary>
        /// <returns>All the nodes in the container</returns>
        public IEnumerable<Node> GetAllChildren()
        {
            foreach (var c in Children)
            {
                yield return c;

                if (c is ContainerNode)
                {
                    foreach (var subc in ((ContainerNode)c).GetAllChildren())
                        yield return subc;
                }
            }
        }

        /// <summary>
        /// The depth of the container in the container chain
        /// </summary>
        public int Depth
        {
            get
            {
                if (Container == null)
                    return 0;
                else
                    return Container.Depth + 1;
            }
        }

        /// <summary>
        /// The container node never links to any nodes, so no nodes will ever be returned
        /// </summary>
        /// <returns>An empty collection</returns>
        public override IEnumerable<Node> GetLinkedNodes()
        {
            yield break;
        }

        /// <summary>
        /// Container nodes don't have links, so do nothing
        /// </summary>
        /// <param name="n"></param>
        protected internal override void RemoveLinkTo(Node n)
        {

        }

        /// <summary>
        /// Container nodes don't have links, so do nothing
        /// </summary>
        /// <param name="mouseDownInfo"></param>
        /// <param name="targetNode"></param>
        protected internal override void AddLink(MouseDownInfo mouseDownInfo, Node targetNode)
        {

        }

        /// <summary>
        /// Opens the editor for the container node, the container only contains text, so the default text node editor will be used
        /// </summary>
        /// <returns></returns>
        protected internal override bool OpenEditor()
        {
            using (TextNodeEditor dlg = new TextNodeEditor(this))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.Text = dlg.NodeText;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If there is a node dropped onto the container, add it to the current container
        /// </summary>
        /// <param name="otherNodes"></param>
        internal override void OnNodeDragDrop(IEnumerable<Node> otherNodes)
        {
            foreach (var n in otherNodes)
                Children.Add(n);

            // update the bounds of the container to fully wrap all the children nodes
            UpdateBounds();
        }


        private bool freezeBoundingBox;
        /// <summary>
        /// Suspends any bounds updating
        /// </summary>
        internal void SuspendFit()
        {
            freezeBoundingBox = true;
        }

        /// <summary>
        /// Resumes any bounds updating
        /// </summary>
        internal void ResumeFit()
        {
            freezeBoundingBox = false;
        }

        /// <summary>
        /// Draws the container node
        /// </summary>
        /// <param name="g">The graphics to use</param>
        /// <param name="font">The font to draw the text with</param>
        /// <param name="viewportRect">The viewport bounds</param>
        /// <param name="isSelected">True if the node is selected</param>
        protected internal override void Draw(System.Drawing.Graphics g, System.Drawing.Font font, System.Drawing.Rectangle viewportRect, bool isSelected)
        {
            Rectangle area = Area;

            if (viewportRect.IntersectsWith(area))
            {
                // draw the background
                using (LinearGradientBrush br = new LinearGradientBrush(area, Color.FromArgb(240, 240, 240), Color.FromArgb(200, 200, 200), LinearGradientMode.Vertical))
                    g.FillRectangle(br, area);

                // draw the border
                if (isSelected)
                    g.DrawRectangle(Pens.Red, area);
                else
                    g.DrawRectangle(Pens.Black, area);

                // draw the text as title
                g.DrawString((Text + ""), font, Brushes.Black, new RectangleF(area.Left, area.Top, area.Width, paddingForText), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                // draw the line under the title
                g.DrawLine(Pens.Black, new Point(area.Left, area.Top + paddingForText), new Point(area.Right, area.Top + paddingForText));
            }
        }

        /// <summary>
        /// The node size is dependent on the nodes it contains
        /// </summary>
        public override Size NodeSize
        {
            get
            {
                if (Children.Count > 0)
                    return nodeSize.RoundTo(parent.GridSize.Width, parent.GridSize.Height);
                else
                    return base.NodeSize;
            }
        }

        /// <summary>
        /// Update the bounds of the node to fully wrap all the children nodes
        /// </summary>
        public void UpdateBounds()
        {
            // if not suspended
            if (!freezeBoundingBox)
            {
                // adjust the position and size to fit
                if (Children.Count > 0)
                {
                    var bbox = NodeExtensions.GetBoundingBox(Children.Select(c => c.Area));
                    bbox.Inflate(parent.GridSize.Width, parent.GridSize.Height);

                    var padding = paddingForText / parent.GridSize.Height * parent.GridSize.Height;
                    this.Position = new Point(bbox.Location.X, bbox.Location.Y - padding).RoundTo(parent.GridSize.Width, parent.GridSize.Height);
                    this.nodeSize = new Size(bbox.Size.Width, bbox.Size.Height + padding);
                }
            }

            // if the current container has a parent container, update its bounds to fit (because the current container node size & position could be changed
            if (Container != null)
                Container.UpdateBounds();

        }
    }

    /// <summary>
    /// A collection of children in a container
    /// </summary>
    public class ContainerNodeCollection : IList<Node>
    {
        private ContainerNode owner;
        private List<Node> lst;
        private HashSet<Node> uniqueNodes;
        internal ContainerNodeCollection(ContainerNode owner)
        {
            this.lst = new List<Node>();
            this.uniqueNodes = new HashSet<Node>();

            this.owner = owner;
        }

        //internal void AddWithoutRefBack(Node itm)
        //{
        //    if (!uniqueNodes.Contains(itm))
        //    {
        //        lst.Add(itm);
        //        uniqueNodes.Add(itm);
        //    }

        //}
        //internal void RemoveWithoutRefBack(Node itm)
        //{
        //    lst.Remove(itm);
        //    uniqueNodes.Remove(itm);
        //}

        public int IndexOf(Node item)
        {
            return lst.IndexOf(item);
        }

        public void Insert(int index, Node item)
        {
            if (!uniqueNodes.Contains(item))
            {
                lst.Insert(index, item);
                uniqueNodes.Add(item);

                item.Container = owner;
            }
        }

        public void RemoveAt(int index)
        {
            Node n = lst[index];
            lst.RemoveAt(index);
            uniqueNodes.Remove(n);
            n.Container = null;
        }

        public Node this[int index]
        {
            get
            {
                return lst[index];
            }
            set
            {
                var n = lst[index];
                if (value != n)
                {
                    if (!uniqueNodes.Contains(value))
                    {
                        uniqueNodes.Remove(n);
                        n.Container = null;
                        lst[index] = value;
                        uniqueNodes.Add(value);
                        value.Container = owner;
                    }
                }
            }
        }

        public void Add(Node item)
        {
            if (!uniqueNodes.Contains(item))
            {
                if (item.Container != null && item.Container != owner)
                    item.Container.Children.Remove(item);

                lst.Add(item);
                uniqueNodes.Add(item);
                item.Container = owner;
            }
        }

        public void Clear()
        {
            foreach (var item in lst)
                item.Container = null;

            uniqueNodes.Clear();
            lst.Clear();
        }

        public bool Contains(Node item)
        {
            return uniqueNodes.Contains(item);
        }

        public void CopyTo(Node[] array, int arrayIndex)
        {
            lst.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return lst.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Node item)
        {
            item.Container = null;
            uniqueNodes.Remove(item);
            return lst.Remove(item);
        }

        public IEnumerator<Node> GetEnumerator()
        {
            return lst.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return lst.GetEnumerator();
        }
    }


}
