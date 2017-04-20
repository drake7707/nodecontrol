using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeControl.Tools
{
    abstract class Tool : ITool
    {
        protected NodeDiagram diagram;
        public Tool(NodeDiagram diagram)
        {
            this.diagram = diagram;
        }


        public abstract void OnDraw(System.Drawing.Graphics g);

        public abstract bool OnMouseDown(System.Windows.Forms.MouseEventArgs e);

        public abstract bool OnMouseMove(System.Windows.Forms.MouseEventArgs e);

        public abstract bool OnMouseUp(System.Windows.Forms.MouseEventArgs e);

    }
}
