namespace Octgn.Releaser
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
			this.label1 = new System.Windows.Forms.Label();
			this.tbOctgnDirectory = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.p1 = new System.Windows.Forms.Panel();
			this.button2 = new System.Windows.Forms.Button();
			this.tbSiteDirectory = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.button3 = new System.Windows.Forms.Button();
			this.p1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(81, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Octgn Directory";
			// 
			// tbOctgnDirectory
			// 
			this.tbOctgnDirectory.Location = new System.Drawing.Point(114, 3);
			this.tbOctgnDirectory.Name = "tbOctgnDirectory";
			this.tbOctgnDirectory.Size = new System.Drawing.Size(264, 20);
			this.tbOctgnDirectory.TabIndex = 1;
			this.tbOctgnDirectory.Text = "F:\\Programming\\OCTGN";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(384, 1);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(24, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "...";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// p1
			// 
			this.p1.Controls.Add(this.button2);
			this.p1.Controls.Add(this.tbSiteDirectory);
			this.p1.Controls.Add(this.label2);
			this.p1.Controls.Add(this.label1);
			this.p1.Controls.Add(this.button1);
			this.p1.Controls.Add(this.tbOctgnDirectory);
			this.p1.Location = new System.Drawing.Point(12, 12);
			this.p1.Name = "p1";
			this.p1.Size = new System.Drawing.Size(434, 59);
			this.p1.TabIndex = 3;
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(384, 29);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(24, 23);
			this.button2.TabIndex = 5;
			this.button2.Text = "...";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// tbSiteDirectory
			// 
			this.tbSiteDirectory.Location = new System.Drawing.Point(114, 31);
			this.tbSiteDirectory.Name = "tbSiteDirectory";
			this.tbSiteDirectory.Size = new System.Drawing.Size(264, 20);
			this.tbSiteDirectory.TabIndex = 4;
			this.tbSiteDirectory.Text = "F:\\Programming\\OCTGN site";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 31);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "gh-pages Directory";
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(371, 77);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 4;
			this.button3.Text = "Next";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(461, 110);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.p1);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Octgn Releaser";
			this.p1.ResumeLayout(false);
			this.p1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbOctgnDirectory;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Panel p1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.TextBox tbSiteDirectory;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button3;
	}
}

