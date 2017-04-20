using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodeControl.Nodes;
using System.Windows.Forms;

namespace NodeControl.Factories
{
    public class ConditionNodeFactory : NodeFactory
    {
        public ConditionNodeFactory()
            : base("Condition node")
        {
        }

        public override Keys[] GetShortcutKeys()
        {
            return new Keys[] { Keys.N, Keys.D };
        }

        public override Type NodeType
        {
            get { return typeof(ConditionNode); }
        }

        public override Node CreateNode(NodeDiagram diagram)
        {
            return new ConditionNode(diagram);
        }
    }
}
