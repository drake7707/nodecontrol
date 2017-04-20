using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodeControl.Nodes;

namespace NodeControl
{
    /// <summary>
    /// A link between 2 nodes
    /// </summary>
    public struct Link : INodeObject
    {
        private Node from;
        /// <summary>
        /// The node where the link comes from
        /// </summary>
        public Node From { get { return from; } }

        private Node to;
        /// <summary>
        /// The node where the link goes to
        /// </summary>
        public Node To { get { return to; } }

        private int index;
        /// <summary>
        /// The index of the link in the collection of child nodes the from node links to
        /// </summary>
        public int Index { get { return index; } }

        public Link(Node from, Node to, int index)
        {
            this.from = from;
            this.to = to;
            this.index = index;
        }
    }

}
