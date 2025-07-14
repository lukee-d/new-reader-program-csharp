namespace rfidapp
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
            this.components = new System.ComponentModel.Container();
            this.textBoxIP = new System.Windows.Forms.TextBox();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.buttonStartReading = new System.Windows.Forms.Button();
            this.buttonStopReading = new System.Windows.Forms.Button();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.timerRead = new System.Windows.Forms.Timer(this.components);
            this.groupBoxModes = new System.Windows.Forms.GroupBox();
            this.radioButtonAnswer = new System.Windows.Forms.RadioButton();
            this.radioButtonActive = new System.Windows.Forms.RadioButton();
            this.textBoxNewEPC = new System.Windows.Forms.TextBox();
            this.buttonWriteTag = new System.Windows.Forms.Button();
            this.labelEPC = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.trackBarPower = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.labelPower = new System.Windows.Forms.Label();
            this.buttonSetPower = new System.Windows.Forms.Button();
            this.textBoxEPCRead = new System.Windows.Forms.TextBox();
            this.buttonRead = new System.Windows.Forms.Button();
            this.buttonSaveLog = new System.Windows.Forms.Button();
            this.groupBoxModes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPower)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxIP
            // 
            this.textBoxIP.Location = new System.Drawing.Point(33, 35);
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.Size = new System.Drawing.Size(100, 22);
            this.textBoxIP.TabIndex = 4;
            this.textBoxIP.Text = "192.168.1.178";
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(179, 35);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(100, 22);
            this.textBoxPort.TabIndex = 5;
            this.textBoxPort.Text = "60000";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(33, 81);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(95, 23);
            this.buttonConnect.TabIndex = 6;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Enabled = false;
            this.buttonDisconnect.Location = new System.Drawing.Point(171, 81);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(108, 23);
            this.buttonDisconnect.TabIndex = 7;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // buttonStartReading
            // 
            this.buttonStartReading.Enabled = false;
            this.buttonStartReading.Location = new System.Drawing.Point(21, 127);
            this.buttonStartReading.Name = "buttonStartReading";
            this.buttonStartReading.Size = new System.Drawing.Size(123, 23);
            this.buttonStartReading.TabIndex = 8;
            this.buttonStartReading.Text = "Start Reading";
            this.buttonStartReading.UseVisualStyleBackColor = true;
            this.buttonStartReading.Click += new System.EventHandler(this.buttonStartReading_Click);
            // 
            // buttonStopReading
            // 
            this.buttonStopReading.Enabled = false;
            this.buttonStopReading.Location = new System.Drawing.Point(162, 127);
            this.buttonStopReading.Name = "buttonStopReading";
            this.buttonStopReading.Size = new System.Drawing.Size(130, 23);
            this.buttonStopReading.TabIndex = 9;
            this.buttonStopReading.Text = "Stop Reading";
            this.buttonStopReading.UseVisualStyleBackColor = true;
            this.buttonStopReading.Click += new System.EventHandler(this.buttonStopReading_Click);
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Location = new System.Drawing.Point(33, 198);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ReadOnly = true;
            this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxOutput.Size = new System.Drawing.Size(246, 126);
            this.textBoxOutput.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(45, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 16);
            this.label1.TabIndex = 11;
            this.label1.Text = "IP Address";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(214, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 16);
            this.label2.TabIndex = 12;
            this.label2.Text = "Port";
            // 
            // groupBoxModes
            // 
            this.groupBoxModes.Controls.Add(this.radioButtonAnswer);
            this.groupBoxModes.Controls.Add(this.radioButtonActive);
            this.groupBoxModes.Location = new System.Drawing.Point(348, 35);
            this.groupBoxModes.Name = "groupBoxModes";
            this.groupBoxModes.Size = new System.Drawing.Size(200, 83);
            this.groupBoxModes.TabIndex = 13;
            this.groupBoxModes.TabStop = false;
            this.groupBoxModes.Text = "Select Reader Mode";
            // 
            // radioButtonAnswer
            // 
            this.radioButtonAnswer.AutoSize = true;
            this.radioButtonAnswer.Location = new System.Drawing.Point(7, 49);
            this.radioButtonAnswer.Name = "radioButtonAnswer";
            this.radioButtonAnswer.Size = new System.Drawing.Size(110, 20);
            this.radioButtonAnswer.TabIndex = 1;
            this.radioButtonAnswer.TabStop = true;
            this.radioButtonAnswer.Text = "Answer Mode";
            this.radioButtonAnswer.UseVisualStyleBackColor = true;
            // 
            // radioButtonActive
            // 
            this.radioButtonActive.AutoSize = true;
            this.radioButtonActive.Location = new System.Drawing.Point(7, 22);
            this.radioButtonActive.Name = "radioButtonActive";
            this.radioButtonActive.Size = new System.Drawing.Size(103, 20);
            this.radioButtonActive.TabIndex = 0;
            this.radioButtonActive.TabStop = true;
            this.radioButtonActive.Text = "Active Mode";
            this.radioButtonActive.UseVisualStyleBackColor = true;
            // 
            // textBoxNewEPC
            // 
            this.textBoxNewEPC.Location = new System.Drawing.Point(594, 57);
            this.textBoxNewEPC.Name = "textBoxNewEPC";
            this.textBoxNewEPC.Size = new System.Drawing.Size(200, 22);
            this.textBoxNewEPC.TabIndex = 14;
            this.textBoxNewEPC.TextChanged += new System.EventHandler(this.textBoxNewEPC_TextChanged);
            // 
            // buttonWriteTag
            // 
            this.buttonWriteTag.Enabled = false;
            this.buttonWriteTag.Location = new System.Drawing.Point(659, 85);
            this.buttonWriteTag.Name = "buttonWriteTag";
            this.buttonWriteTag.Size = new System.Drawing.Size(75, 23);
            this.buttonWriteTag.TabIndex = 15;
            this.buttonWriteTag.Text = "Write Tag";
            this.buttonWriteTag.UseVisualStyleBackColor = true;
            this.buttonWriteTag.Click += new System.EventHandler(this.buttonWriteTag_Click);
            // 
            // labelEPC
            // 
            this.labelEPC.AutoSize = true;
            this.labelEPC.Location = new System.Drawing.Point(667, 35);
            this.labelEPC.Name = "labelEPC";
            this.labelEPC.Size = new System.Drawing.Size(67, 16);
            this.labelEPC.TabIndex = 16;
            this.labelEPC.Text = "New EPC:";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.ForeColor = System.Drawing.Color.Red;
            this.labelStatus.Location = new System.Drawing.Point(108, 168);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(90, 16);
            this.labelStatus.TabIndex = 17;
            this.labelStatus.Text = "Disconnected";
            this.labelStatus.Click += new System.EventHandler(this.labelStatus_Click);
            // 
            // trackBarPower
            // 
            this.trackBarPower.Location = new System.Drawing.Point(348, 186);
            this.trackBarPower.Maximum = 30;
            this.trackBarPower.Name = "trackBarPower";
            this.trackBarPower.Size = new System.Drawing.Size(200, 56);
            this.trackBarPower.TabIndex = 18;
            this.trackBarPower.TickFrequency = 5;
            this.trackBarPower.Value = 15;
            this.trackBarPower.Scroll += new System.EventHandler(this.trackBarPower_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 16);
            this.label3.TabIndex = 19;
            this.label3.Text = "label3";
            // 
            // labelPower
            // 
            this.labelPower.AutoSize = true;
            this.labelPower.Location = new System.Drawing.Point(397, 158);
            this.labelPower.Name = "labelPower";
            this.labelPower.Size = new System.Drawing.Size(96, 16);
            this.labelPower.TabIndex = 20;
            this.labelPower.Text = "Power: 15 dBm";
            this.labelPower.Click += new System.EventHandler(this.labelPower_Click);
            // 
            // buttonSetPower
            // 
            this.buttonSetPower.Location = new System.Drawing.Point(400, 229);
            this.buttonSetPower.Name = "buttonSetPower";
            this.buttonSetPower.Size = new System.Drawing.Size(93, 23);
            this.buttonSetPower.TabIndex = 21;
            this.buttonSetPower.Text = "Set Power";
            this.buttonSetPower.UseVisualStyleBackColor = true;
            this.buttonSetPower.Click += new System.EventHandler(this.buttonSetPower_Click);
            // 
            // textBoxEPCRead
            // 
            this.textBoxEPCRead.Location = new System.Drawing.Point(633, 198);
            this.textBoxEPCRead.Multiline = true;
            this.textBoxEPCRead.Name = "textBoxEPCRead";
            this.textBoxEPCRead.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxEPCRead.Size = new System.Drawing.Size(140, 97);
            this.textBoxEPCRead.TabIndex = 22;
            this.textBoxEPCRead.TextChanged += new System.EventHandler(this.textBoxEPCRead_TextChanged);
            // 
            // buttonRead
            // 
            this.buttonRead.Location = new System.Drawing.Point(670, 301);
            this.buttonRead.Name = "buttonRead";
            this.buttonRead.Size = new System.Drawing.Size(75, 23);
            this.buttonRead.TabIndex = 23;
            this.buttonRead.Text = "button1";
            this.buttonRead.UseVisualStyleBackColor = true;
            this.buttonRead.Click += new System.EventHandler(this.buttonRead_Click);
            // 
            // buttonSaveLog
            // 
            this.buttonSaveLog.Location = new System.Drawing.Point(297, 301);
            this.buttonSaveLog.Name = "buttonSaveLog";
            this.buttonSaveLog.Size = new System.Drawing.Size(109, 23);
            this.buttonSaveLog.TabIndex = 24;
            this.buttonSaveLog.Text = "Save Log";
            this.buttonSaveLog.UseVisualStyleBackColor = true;
            this.buttonSaveLog.Click += new System.EventHandler(this.buttonSaveLog_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(831, 450);
            this.Controls.Add(this.buttonSaveLog);
            this.Controls.Add(this.buttonRead);
            this.Controls.Add(this.textBoxEPCRead);
            this.Controls.Add(this.buttonSetPower);
            this.Controls.Add(this.labelPower);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.trackBarPower);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelEPC);
            this.Controls.Add(this.buttonWriteTag);
            this.Controls.Add(this.textBoxNewEPC);
            this.Controls.Add(this.groupBoxModes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.buttonStopReading);
            this.Controls.Add(this.buttonStartReading);
            this.Controls.Add(this.buttonDisconnect);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.textBoxPort);
            this.Controls.Add(this.textBoxIP);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBoxModes.ResumeLayout(false);
            this.groupBoxModes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPower)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.Button buttonStartReading;
        private System.Windows.Forms.Button buttonStopReading;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timerRead;
        private System.Windows.Forms.GroupBox groupBoxModes;
        private System.Windows.Forms.RadioButton radioButtonActive;
        private System.Windows.Forms.RadioButton radioButtonAnswer;
        private System.Windows.Forms.TextBox textBoxNewEPC;
        private System.Windows.Forms.Button buttonWriteTag;
        private System.Windows.Forms.Label labelEPC;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.TrackBar trackBarPower;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelPower;
        private System.Windows.Forms.Button buttonSetPower;
        private System.Windows.Forms.TextBox textBoxEPCRead;
        private System.Windows.Forms.Button buttonRead;
        private System.Windows.Forms.Button buttonSaveLog;
    }
}

