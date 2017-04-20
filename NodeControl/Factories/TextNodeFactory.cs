using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodeControl.Nodes;
using System.Windows.Forms;

namespace NodeControl.Factories
{
    public class TextNodeFactory : NodeFactory
    {
        public TextNodeFactory()
            : base("Text node")
        {
        }

        public override Keys[] GetShortcutKeys()
        {
            return new Keys[] { Keys.N, Keys.T };
        }

        public override Type NodeType
        {
            get { return typeof(TextNode); }
        }

        public override Node CreateNode(NodeDiagram diagram)
        {
            return new TextNode(diagram);
        }
    }
}
