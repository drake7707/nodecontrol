using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NodeControl.Nodes;
using NodeControl;

namespace NodeControlTest
{
    public partial class Form1 : Form
    {
        private NodeDiagram d;
        public Form1()
        {
            InitializeComponent();

            d= new NodeDiagram();
            d.Dock = DockStyle.Fill;
            pnl.Controls.Add(d);

            var n = new TextNode(d) { Text = "Test" };

            var subn = new ConditionNode(d) { Text = "OK?" };
            n.LinksTo = subn;

            var yesNode = new TextNode(d) { Text = "Alright then", LinksTo = n };
            subn.LinksTo.Add(new Condition() { Text = "Yes", LinksTo = yesNode });

            subn.LinksTo.Add(new Condition() { Text = "Come again?", LinksTo = subn });

            var noNode = new TextNode(d) { Text = "Your loss", LinksTo = subn };
            subn.LinksTo.Add(new Condition() { Text = "No", LinksTo = noNode });

            d.Nodes.Add(n);
            d.Nodes.Add(subn);
            d.Nodes.Add(yesNode);
            d.Nodes.Add(noNode);

            d.Nodes.Add(n);

            var unlinkedNode = new TextNode(d) { Text = "Unlinked " };
            d.Nodes.Add(unlinkedNode);

            foreach (var node in d.Nodes)
            {
                node.Direction = Node.DirectionEnum.Vertical;
            }

            d.AutoLayout(true);
        }

        private void rdbBezier_CheckedChanged(object sender, EventArgs e)
        {
            d.LineType = LineTypeEnum.Bezier;
            d.Redraw();
        }

        private void rdb4Way_CheckedChanged(object sender, EventArgs e)
        {
            d.LineType = LineTypeEnum.FourWay;
            d.Redraw();
        }


        private void rdbStraight_CheckedChanged(object sender, EventArgs e)
        {
            d.LineType = LineTypeEnum.Straight;
            d.Redraw();
        }

        private void btnAutoLayout_Click(object sender, EventArgs e)
        {
            d.AutoLayout(false);
        }

        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            using (Form frm = new Form())
            {
                frm.BackgroundImage =  d.AsImage();
                frm.BackgroundImageLayout = ImageLayout.None;
                frm.ShowDialog();
            }
        }

    }

   


}
