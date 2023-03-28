namespace AUZT_Authorization
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
            this.progressBar1 = new CCWin.SkinControl.SkinProgressBar();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Back = null;
            this.progressBar1.BackColor = System.Drawing.SystemColors.Control;
            this.progressBar1.BarBack = null;
            this.progressBar1.BarRadius = 6;
            this.progressBar1.BarRadiusStyle = CCWin.SkinClass.RoundStyle.All;
            this.progressBar1.Border = System.Drawing.Color.Transparent;
            this.progressBar1.ForeColor = System.Drawing.Color.Red;
            this.progressBar1.InnerBorder = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(153)))), ((int)(((byte)(218)))));
            this.progressBar1.Location = new System.Drawing.Point(7, 3);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Radius = 8;
            this.progressBar1.RadiusStyle = CCWin.SkinClass.RoundStyle.All;
            this.progressBar1.Size = new System.Drawing.Size(553, 15);
            this.progressBar1.TabIndex = 2;
            this.progressBar1.TrackBack = System.Drawing.SystemColors.Control;
            this.progressBar1.TrackFore = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(153)))), ((int)(((byte)(218)))));
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(558, 22);
            this.ControlBox = false;
            this.Controls.Add(this.progressBar1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinProgressBar progressBar1;
    }
}