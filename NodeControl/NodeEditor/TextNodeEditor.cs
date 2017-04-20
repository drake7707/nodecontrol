using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NodeControl.Nodes;

namespace NodeControl.NodeEditor
{
    public partial class TextNodeEditor : Form
    {
        public TextNodeEditor(Node n)
        {
            InitializeComponent();
            NodeText = n.Text;
        }

        public string NodeText
        {
            get { return txtText.Text; }
            set { txtText.Text = value; }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                DialogResult = System.Windows.Forms.DialogResult.Cancel;
                return true;
            }
            else if (keyData == (Keys.Enter))
            {
                DialogResult = System.Windows.Forms.DialogResult.OK;
                return true;
            }
            else
                return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

       
    }
}
