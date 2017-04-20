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
    public partial class ConditionNodeEditor : Form
    {
        public ConditionNodeEditor(ConditionNode n)
        {
            InitializeComponent();
            NodeText = n.Text;
            NodeConditions = n.LinksTo.Select(c => c.Text).ToArray();
        }

        public string NodeText
        {
            get { return txtText.Text; }
            set { txtText.Text = value; }
        }


        public string[] NodeConditions
        {
            get { return txtConditions.Lines; }
            set { txtConditions.Lines = value; }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                DialogResult = System.Windows.Forms.DialogResult.Cancel;
                return true;
            }
            else if (keyData == (Keys.Control | Keys.Enter))
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
