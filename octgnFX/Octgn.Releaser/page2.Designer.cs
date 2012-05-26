namespace Octgn.Releaser
{
	partial class page2
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
			this.label1 = new System.Windows.Forms.Label();
			this.tbOldVersion = new System.Windows.Forms.TextBox();
			this.tbNewVersion = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(79, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Current Version";
			// 
			// tbOldVersion
			// 
			this.tbOldVersion.Enabled = false;
			this.tbOldVersion.Location = new System.Drawing.Point(98, 10);
			this.tbOldVersion.Name = "tbOldVersion";
			this.tbOldVersion.Size = new System.Drawing.Size(323, 20);
			this.tbOldVersion.TabIndex = 1;
			// 
			// tbNewVersion
			// 
			this.tbNewVersion.Location = new System.Drawing.Point(98, 36);
			this.tbNewVersion.Name = "tbNewVersion";
			this.tbNewVersion.Size = new System.Drawing.Size(323, 20);
			this.tbNewVersion.TabIndex = 2;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(25, 39);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(67, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "New Version";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(346, 75);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 4;
			this.button1.Text = "Next";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// page2
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(446, 110);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.tbNewVersion);
			this.Controls.Add(this.tbOldVersion);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "page2";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Octgn Releaser";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.page2_FormClosed);
			this.Load += new System.EventHandler(this.page2_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbOldVersion;
		private System.Windows.Forms.TextBox tbNewVersion;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1;
	}
}