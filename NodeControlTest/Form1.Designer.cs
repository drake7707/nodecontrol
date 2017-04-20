namespace NodeControlTest
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSaveImage = new System.Windows.Forms.Button();
            this.rdbBezier = new System.Windows.Forms.RadioButton();
            this.rdb4Way = new System.Windows.Forms.RadioButton();
            this.btnAutoLayout = new System.Windows.Forms.Button();
            this.pnl = new System.Windows.Forms.Panel();
            this.rdbStraight = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.32432F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80.67567F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pnl, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(740, 385);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdbStraight);
            this.groupBox1.Controls.Add(this.btnSaveImage);
            this.groupBox1.Controls.Add(this.rdbBezier);
            this.groupBox1.Controls.Add(this.rdb4Way);
            this.groupBox1.Controls.Add(this.btnAutoLayout);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(136, 379);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // btnSaveImage
            // 
            this.btnSaveImage.Location = new System.Drawing.Point(10, 178);
            this.btnSaveImage.Name = "btnSaveImage";
            this.btnSaveImage.Size = new System.Drawing.Size(75, 23);
            this.btnSaveImage.TabIndex = 3;
            this.btnSaveImage.Text = "Save image";
            this.btnSaveImage.UseVisualStyleBackColor = true;
            this.btnSaveImage.Click += new System.EventHandler(this.btnSaveImage_Click);
            // 
            // rdbBezier
            // 
            this.rdbBezier.AutoSize = true;
            this.rdbBezier.Checked = true;
            this.rdbBezier.Location = new System.Drawing.Point(6, 95);
            this.rdbBezier.Name = "rdbBezier";
            this.rdbBezier.Size = new System.Drawing.Size(54, 17);
            this.rdbBezier.TabIndex = 2;
            this.rdbBezier.TabStop = true;
            this.rdbBezier.Text = "Bezier";
            this.rdbBezier.UseVisualStyleBackColor = true;
            this.rdbBezier.CheckedChanged += new System.EventHandler(this.rdbBezier_CheckedChanged);
            // 
            // rdb4Way
            // 
            this.rdb4Way.AutoSize = true;
            this.rdb4Way.Location = new System.Drawing.Point(6, 118);
            this.rdb4Way.Name = "rdb4Way";
            this.rdb4Way.Size = new System.Drawing.Size(77, 17);
            this.rdb4Way.TabIndex = 1;
            this.rdb4Way.Text = "4 way links";
            this.rdb4Way.UseVisualStyleBackColor = true;
            this.rdb4Way.CheckedChanged += new System.EventHandler(this.rdb4Way_CheckedChanged);
            // 
            // btnAutoLayout
            // 
            this.btnAutoLayout.Location = new System.Drawing.Point(10, 29);
            this.btnAutoLayout.Name = "btnAutoLayout";
            this.btnAutoLayout.Size = new System.Drawing.Size(75, 23);
            this.btnAutoLayout.TabIndex = 0;
            this.btnAutoLayout.Text = "Auto layout";
            this.btnAutoLayout.UseVisualStyleBackColor = true;
            this.btnAutoLayout.Click += new System.EventHandler(this.btnAutoLayout_Click);
            // 
            // pnl
            // 
            this.pnl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl.Location = new System.Drawing.Point(145, 3);
            this.pnl.Name = "pnl";
            this.pnl.Size = new System.Drawing.Size(592, 379);
            this.pnl.TabIndex = 1;
            // 
            // rdbStraight
            // 
            this.rdbStraight.AutoSize = true;
            this.rdbStraight.Location = new System.Drawing.Point(6, 141);
            this.rdbStraight.Name = "rdbStraight";
            this.rdbStraight.Size = new System.Drawing.Size(61, 17);
            this.rdbStraight.TabIndex = 4;
            this.rdbStraight.Text = "Straight";
            this.rdbStraight.UseVisualStyleBackColor = true;
            this.rdbStraight.CheckedChanged += new System.EventHandler(this.rdbStraight_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(740, 385);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Node diagram test";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnAutoLayout;
        private System.Windows.Forms.Panel pnl;
        private System.Windows.Forms.RadioButton rdbBezier;
        private System.Windows.Forms.RadioButton rdb4Way;
        private System.Windows.Forms.Button btnSaveImage;
        private System.Windows.Forms.RadioButton rdbStraight;
    }
}

