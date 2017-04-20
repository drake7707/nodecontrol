using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodeControl.Nodes;
using System.Windows.Forms;

namespace NodeControl.Factories
{
    public class StartNodeFactory : NodeFactory
    {
        public StartNodeFactory()
            : base("Start node")
        {
        }

        public override Keys[] GetShortcutKeys()
        {
            return new Keys[] { Keys.N, Keys.S };
        }

        public override Type NodeType
        {
            get { return typeof(StartNode); }
        }

        public override Node CreateNode(NodeDiagram diagram)
        {
            if (diagram.Nodes.OfType<StartNode>().Any())
            {
                // allow only 1 start node
                return null;
            }
            else
                return new StartNode(diagram);
        }

    }
}
