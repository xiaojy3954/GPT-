using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AUZT_Authorization
{
    partial class UpdateLog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

      

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.probar = new System.Windows.Forms.ProgressBar();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblMsg = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // probar
            // 
            this.probar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.probar.Location = new System.Drawing.Point(0, 1);
            this.probar.Name = "probar";
            this.probar.Size = new System.Drawing.Size(401, 17);
            this.probar.TabIndex = 1;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblMsg});
            this.statusStrip1.Location = new System.Drawing.Point(0, 18);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(402, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblMsg
            // 
            this.lblMsg.Name = "lblMsg";
            this.lblMsg.Size = new System.Drawing.Size(56, 17);
            this.lblMsg.Text = "准备就绪";
            // 
            // UpdateLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(402, 40);
            this.Controls.Add(this.probar);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateLog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "自动更新";
            this.Load += new System.EventHandler(this.UpdateLog_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        // Token: 0x04000628 RID: 1576
        private ToolStripStatusLabel lblMsg;

        // Token: 0x04000625 RID: 1573
        private IContainer ljuoKgOdRs;

        // Token: 0x04000626 RID: 1574
        private ProgressBar probar;

        // Token: 0x04000627 RID: 1575
        private StatusStrip statusStrip1;

        // Token: 0x04000624 RID: 1572
        private string TfXoYcYdx7;

        // Token: 0x020000C1 RID: 193
        // Token: 0x060008CF RID: 2255
        public delegate void SetBool(bool b);

        // Token: 0x020000C2 RID: 194
        // Token: 0x060008D3 RID: 2259
        public delegate void SetText(string text);

        // Token: 0x020000C3 RID: 195
        // Token: 0x060008D7 RID: 2263
        public delegate void SetValue(int i);
    }
}