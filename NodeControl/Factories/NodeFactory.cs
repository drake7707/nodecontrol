using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NodeControl.Nodes;

namespace NodeControl
{
    /// <summary>
    /// A factory that can create nodes
    /// </summary>
    public abstract class NodeFactory
    {
        public string NodeName { get; private set; }
        public NodeFactory(string nodeName)
        {
            this.NodeName = nodeName;
        }

        /// <summary>
        /// A combination of shortcut keys to create a node
        /// </summary>
        /// <returns></returns>
        public virtual Keys[] GetShortcutKeys()
        {
            return new Keys[0];
        }

        /// <summary>
        /// The type of the node
        /// </summary>
        public abstract Type NodeType
        {
            get;
        }

        /// <summary>
        /// Creates a node for the given node diagram
        /// </summary>
        /// <param name="diagram">The diagram that will own the node</param>
        /// <returns>A created node</returns>
        public abstract Node CreateNode(NodeDiagram diagram);
    }


    /// <summary>
    /// A collection of node factories of a given node diagram
    /// </summary>
    public class FactoryCollection : IList<NodeFactory>
    {
        /// <summary>
        /// The no
        /// </summary>
        private NodeDiagram owner;
        private List<NodeFactory> lst;
        internal FactoryCollection(NodeDiagram owner)
        {
            this.lst = new List<NodeFactory>();
            this.owner = owner;
        }


        public int IndexOf(NodeFactory item)
        {
            return lst.IndexOf(item);
        }


        public void Insert(int index, NodeFactory item)
        {
            lst.Insert(index, item);

            ToolStripMenuItem mnu = CreateMenuItem(item);
            owner.CreateMenu.Items.Insert(index, mnu);
        }

        /// <summary>
        /// Creates a context menu item to create a node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private ToolStripMenuItem CreateMenuItem(NodeFactory item)
        {
            ToolStripMenuItem mnu = new ToolStripMenuItem("New " + item.NodeName);
            mnu.Tag = item;

            var keys = item.GetShortcutKeys();
            if (keys.Length > 0)
                mnu.ShortcutKeyDisplayString = "Ctrl+" + string.Join(",", item.GetShortcutKeys().Select(k => k.ToString()).ToArray());

            mnu.Click += (s, ev) =>
            {
                var factory = ((NodeFactory)((ToolStripMenuItem)s).Tag);
                var n = factory.CreateNode(owner);
                if (n != null)
                    owner.AddNewNode(n);
            };
            return mnu;
        }

        public void RemoveAt(int index)
        {
            NodeFactory n = lst[index];
            lst.RemoveAt(index);
            owner.CreateMenu.Items.RemoveAt(index);
        }

        public NodeFactory this[int index]
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
                    owner.CreateMenu.Items.RemoveAt(index);
                    lst[index] = value;
                    ToolStripMenuItem mnu = CreateMenuItem(n);
                    owner.CreateMenu.Items.Insert(index, mnu);
                }
            }
        }

        public void Add(NodeFactory item)
        {
            lst.Add(item);
            var mnu = CreateMenuItem(item);
            owner.CreateMenu.Items.Add(mnu);
        }

        public void Clear()
        {
            lst.Clear();
            owner.CreateMenu.Items.Clear();
        }

        public bool Contains(NodeFactory item)
        {
            return lst.Contains(item);
        }

        public void CopyTo(NodeFactory[] array, int arrayIndex)
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

        public bool Remove(NodeFactory item)
        {
            var mnu = owner.CreateMenu.Items.Cast<ToolStripMenuItem>().Where(itm => itm.Tag == item).FirstOrDefault();
            if (mnu != null)
                owner.CreateMenu.Items.Remove(mnu);

            return lst.Remove(item);
        }

        public IEnumerator<NodeFactory> GetEnumerator()
        {
            return lst.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return lst.GetEnumerator();
        }
    }


}
