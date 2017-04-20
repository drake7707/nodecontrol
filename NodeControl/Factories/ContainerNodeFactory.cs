using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodeControl.Nodes;
using System.Windows.Forms;

namespace NodeControl.Factories
{
    class ContainerNodeFactory : NodeFactory
    {
        public ContainerNodeFactory()
            : base("Container node")
        {
        }

        public override Keys[] GetShortcutKeys()
        {
            return new Keys[] { Keys.N, Keys.C };
        }

        public override Type NodeType
        {
            get { return typeof(ContainerNode); }
        }

        public override Node CreateNode(NodeDiagram diagram)
        {
            return new ContainerNode(diagram);
        }
    }
    
    
}
