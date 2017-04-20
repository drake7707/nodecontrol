using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace NodeControl
{
    interface ITool
    {
        void OnDraw(Graphics g);
        bool OnMouseDown(MouseEventArgs e);
        bool OnMouseMove(MouseEventArgs e);
        bool OnMouseUp(MouseEventArgs e);
    }
}
