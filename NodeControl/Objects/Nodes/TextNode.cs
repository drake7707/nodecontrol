using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodeControl.NodeEditor;

namespace NodeControl.Nodes
{
    public class TextNode : Node
    {
        public TextNode(NodeDiagram parent)
            : base(parent)
        {

        }

        private Node linksTo;

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


        public override IEnumerable<Node> GetLinkedNodes()
        {
            yield return LinksTo;
        }

        internal protected override void RemoveLinkTo(Node n)
        {
            if (linksTo == n)
            {
                LinksTo = null;
            }
        }

        internal protected override void AddLink(MouseDownInfo mouseDownInfo, Node targetNode)
        {
            LinksTo = targetNode;
        }

        internal protected override bool OpenEditor()
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
    }

}
