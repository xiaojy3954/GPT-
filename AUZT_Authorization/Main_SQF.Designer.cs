﻿namespace AUZT_Authorization
{
    partial class Main_SQF
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main_SQF));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.添加ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.修改ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.txt_COMPANY = new System.Windows.Forms.TextBox();
            this.txt_COMPANY_EN = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(248)))));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridView1.Location = new System.Drawing.Point(3, 136);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(153)))), ((int)(((byte)(218)))));
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.ShowEditingIcon = false;
            this.dataGridView1.ShowRowErrors = false;
            this.dataGridView1.Size = new System.Drawing.Size(743, 363);
            this.dataGridView1.TabIndex = 109;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.添加ToolStripMenuItem,
            this.修改ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(101, 48);
            // 
            // 添加ToolStripMenuItem
            // 
            this.添加ToolStripMenuItem.Name = "添加ToolStripMenuItem";
            this.添加ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.添加ToolStripMenuItem.Text = "添加";
            this.添加ToolStripMenuItem.Click += new System.EventHandler(this.添加ToolStripMenuItem_Click);
            // 
            // 修改ToolStripMenuItem
            // 
            this.修改ToolStripMenuItem.Name = "修改ToolStripMenuItem";
            this.修改ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.修改ToolStripMenuItem.Text = "修改";
            this.修改ToolStripMenuItem.Click += new System.EventHandler(this.修改ToolStripMenuItem_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(248)))));
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.txt_COMPANY);
            this.groupBox1.Controls.Add(this.txt_COMPANY_EN);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(3, 78);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(743, 52);
            this.groupBox1.TabIndex = 110;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "搜索";
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(153)))), ((int)(((byte)(218)))));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button2.ForeColor = System.Drawing.Color.White;
            this.button2.Location = new System.Drawing.Point(629, 11);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(106, 29);
            this.button2.TabIndex = 100;
            this.button2.TabStop = false;
            this.button2.Text = "搜索";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // txt_COMPANY
            // 
            this.txt_COMPANY.Location = new System.Drawing.Point(98, 14);
            this.txt_COMPANY.Name = "txt_COMPANY";
            this.txt_COMPANY.Size = new System.Drawing.Size(172, 21);
            this.txt_COMPANY.TabIndex = 6;
            // 
            // txt_COMPANY_EN
            // 
            this.txt_COMPANY_EN.Location = new System.Drawing.Point(407, 14);
            this.txt_COMPANY_EN.Name = "txt_COMPANY_EN";
            this.txt_COMPANY_EN.Size = new System.Drawing.Size(176, 21);
            this.txt_COMPANY_EN.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(300, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "授权方英文名称：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "授权方名称：";
            // 
            // Main_SQF
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(750, 502);
            this.CloseDownBack = global::AUZT_Authorization.Properties.Resources.btn_close_down;
            this.CloseMouseBack = global::AUZT_Authorization.Properties.Resources.btn_close_highlight;
            this.CloseNormlBack = global::AUZT_Authorization.Properties.Resources.btn_close_disable;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dataGridView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main_SQF";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "授权方维护";
            this.TitleCenter = true;
            this.TitleColor = System.Drawing.Color.White;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_SQF_FormClosing);
            this.Load += new System.EventHandler(this.Main_SQF_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Main_SQF_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 添加ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 修改ToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txt_COMPANY;
        private System.Windows.Forms.TextBox txt_COMPANY_EN;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
    }
}