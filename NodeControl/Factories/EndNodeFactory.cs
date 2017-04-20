using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodeControl.Nodes;
using System.Windows.Forms;

namespace NodeControl.Factories
{
    public class EndNodeFactory : NodeFactory
    {
        public EndNodeFactory()
            : base("End node")
        {
        }

        public override Keys[] GetShortcutKeys()
        {
            return new Keys[] { Keys.N, Keys.E };
        }

        public override Type NodeType
        {
            get { return typeof(EndNode); }
        }

        public override Node CreateNode(NodeDiagram diagram)
        {
            return new EndNode(diagram);
        }

    }
}
