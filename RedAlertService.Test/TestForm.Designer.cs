namespace RedAlertService.Test
{
    partial class TestForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button_SendTestEmail = new System.Windows.Forms.Button();
            this.button_ProcessEmailRequests = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_SendTestEmail
            // 
            this.button_SendTestEmail.Location = new System.Drawing.Point(30, 28);
            this.button_SendTestEmail.Name = "button_SendTestEmail";
            this.button_SendTestEmail.Size = new System.Drawing.Size(255, 56);
            this.button_SendTestEmail.TabIndex = 1;
            this.button_SendTestEmail.Text = "Send Test Email";
            this.button_SendTestEmail.UseVisualStyleBackColor = true;
            this.button_SendTestEmail.Click += new System.EventHandler(this.button_SendTestEmail_Click);
            // 
            // button_ProcessEmailRequests
            // 
            this.button_ProcessEmailRequests.Location = new System.Drawing.Point(32, 106);
            this.button_ProcessEmailRequests.Name = "button_ProcessEmailRequests";
            this.button_ProcessEmailRequests.Size = new System.Drawing.Size(255, 56);
            this.button_ProcessEmailRequests.TabIndex = 2;
            this.button_ProcessEmailRequests.Text = "Process Email Requests";
            this.button_ProcessEmailRequests.UseVisualStyleBackColor = true;
            this.button_ProcessEmailRequests.Click += new System.EventHandler(this.button_ProcessEmailRequests_Click);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(319, 193);
            this.Controls.Add(this.button_ProcessEmailRequests);
            this.Controls.Add(this.button_SendTestEmail);
            this.Name = "TestForm";
            this.Text = "RedAlert Email Test";
            this.ResumeLayout(false);

        }

        #endregion
        private Button button_SendTestEmail;
        private Button button_ProcessEmailRequests;
    }
}