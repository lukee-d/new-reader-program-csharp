namespace rfidapp2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.labelLastEPC = new System.Windows.Forms.Label();
            this.textBoxNewEPC = new System.Windows.Forms.TextBox();
            this.labelMatchStatus = new System.Windows.Forms.Label();
            this.labelConnectionStatus = new System.Windows.Forms.Label();
            this.buttonReconnect = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // labelLastEPC
            // 
            this.labelLastEPC.Location = new System.Drawing.Point(249, 9);
            this.labelLastEPC.Name = "labelLastEPC";
            this.labelLastEPC.Size = new System.Drawing.Size(229, 40);
            this.labelLastEPC.TabIndex = 0;
            this.labelLastEPC.Text = "Last EPC: ";
            this.labelLastEPC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxNewEPC
            // 
            this.textBoxNewEPC.Location = new System.Drawing.Point(252, 90);
            this.textBoxNewEPC.MaxLength = 9;
            this.textBoxNewEPC.Name = "textBoxNewEPC";
            this.textBoxNewEPC.Size = new System.Drawing.Size(226, 22);
            this.textBoxNewEPC.TabIndex = 1;
            this.textBoxNewEPC.TextChanged += new System.EventHandler(this.textBoxNewEPC_TextChanged);
            // 
            // labelMatchStatus
            // 
            this.labelMatchStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMatchStatus.Location = new System.Drawing.Point(144, 142);
            this.labelMatchStatus.Name = "labelMatchStatus";
            this.labelMatchStatus.Size = new System.Drawing.Size(450, 50);
            this.labelMatchStatus.TabIndex = 2;
            this.labelMatchStatus.Text = "NO MATCH";
            this.labelMatchStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelConnectionStatus
            // 
            this.labelConnectionStatus.ForeColor = System.Drawing.Color.DimGray;
            this.labelConnectionStatus.Location = new System.Drawing.Point(22, 192);
            this.labelConnectionStatus.Name = "labelConnectionStatus";
            this.labelConnectionStatus.Size = new System.Drawing.Size(168, 23);
            this.labelConnectionStatus.TabIndex = 4;
            this.labelConnectionStatus.Text = "Disconnected";
            this.labelConnectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonReconnect
            // 
            this.buttonReconnect.Location = new System.Drawing.Point(57, 218);
            this.buttonReconnect.Name = "buttonReconnect";
            this.buttonReconnect.Size = new System.Drawing.Size(94, 23);
            this.buttonReconnect.TabIndex = 5;
            this.buttonReconnect.Text = "Reconnect";
            this.buttonReconnect.UseVisualStyleBackColor = true;
            this.buttonReconnect.Click += new System.EventHandler(this.buttonReconnect_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(25, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(189, 50);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(736, 334);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonReconnect);
            this.Controls.Add(this.labelConnectionStatus);
            this.Controls.Add(this.labelMatchStatus);
            this.Controls.Add(this.textBoxNewEPC);
            this.Controls.Add(this.labelLastEPC);
            this.Name = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label labelLastEPC;
        private System.Windows.Forms.TextBox textBoxNewEPC;
        private System.Windows.Forms.Label labelMatchStatus;
        private System.Windows.Forms.Label labelConnectionStatus;
        private System.Windows.Forms.Button buttonReconnect;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
#endregion

