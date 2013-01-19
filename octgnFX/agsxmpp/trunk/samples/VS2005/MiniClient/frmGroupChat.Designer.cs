namespace MiniClient
{
    partial class frmGroupChat
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

            Util.GroupChatForms.Remove(m_RoomJid.Bare.ToLower());
            
            // Remove the Message Callback in the MessageGrabber
            m_XmppCon.MessageGrabber.Remove(m_RoomJid);

            // Remove the Presence Callback in the MessageGrabber
            m_XmppCon.PresenceGrabber.Remove(m_RoomJid);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGroupChat));
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lvwRoster = new System.Windows.Forms.ListView();
            this.headerNickname = new System.Windows.Forms.ColumnHeader();
            this.headerStatus = new System.Windows.Forms.ColumnHeader();
            this.headerRole = new System.Windows.Forms.ColumnHeader();
            this.headerAffiliation = new System.Windows.Forms.ColumnHeader();
            this.ilsRoster = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.rtfChat = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdChangeSubject = new System.Windows.Forms.Button();
            this.txtSubject = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.rtfSend = new System.Windows.Forms.RichTextBox();
            this.cmdSend = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 330);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(623, 24);
            this.statusBar1.TabIndex = 6;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lvwRoster);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(623, 330);
            this.splitContainer1.SplitterDistance = 188;
            this.splitContainer1.TabIndex = 7;
            // 
            // lvwRoster
            // 
            this.lvwRoster.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.headerNickname,
            this.headerStatus,
            this.headerRole,
            this.headerAffiliation});
            this.lvwRoster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvwRoster.Location = new System.Drawing.Point(0, 0);
            this.lvwRoster.Name = "lvwRoster";
            this.lvwRoster.Size = new System.Drawing.Size(188, 330);
            this.lvwRoster.SmallImageList = this.ilsRoster;
            this.lvwRoster.TabIndex = 0;
            this.lvwRoster.UseCompatibleStateImageBehavior = false;
            this.lvwRoster.View = System.Windows.Forms.View.Details;
            // 
            // headerNickname
            // 
            this.headerNickname.Text = "Nickname";
            this.headerNickname.Width = 82;
            // 
            // headerStatus
            // 
            this.headerStatus.Text = "Status";
            this.headerStatus.Width = 73;
            // 
            // headerRole
            // 
            this.headerRole.Text = "Role";
            // 
            // headerAffiliation
            // 
            this.headerAffiliation.Text = "Affiliation";
            // 
            // ilsRoster
            // 
            this.ilsRoster.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilsRoster.ImageStream")));
            this.ilsRoster.TransparentColor = System.Drawing.Color.Transparent;
            this.ilsRoster.Images.SetKeyName(0, "");
            this.ilsRoster.Images.SetKeyName(1, "");
            this.ilsRoster.Images.SetKeyName(2, "");
            this.ilsRoster.Images.SetKeyName(3, "");
            this.ilsRoster.Images.SetKeyName(4, "");
            this.ilsRoster.Images.SetKeyName(5, "");
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tableLayoutPanel2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer2.Size = new System.Drawing.Size(431, 330);
            this.splitContainer2.SplitterDistance = 215;
            this.splitContainer2.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 302F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 71F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.rtfChat, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmdChangeSubject, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.txtSubject, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(431, 215);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // rtfChat
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.rtfChat, 3);
            this.rtfChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfChat.Location = new System.Drawing.Point(3, 33);
            this.rtfChat.Name = "rtfChat";
            this.rtfChat.Size = new System.Drawing.Size(425, 179);
            this.rtfChat.TabIndex = 3;
            this.rtfChat.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 30);
            this.label1.TabIndex = 4;
            this.label1.Text = "Subject:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmdChangeSubject
            // 
            this.cmdChangeSubject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdChangeSubject.Location = new System.Drawing.Point(363, 3);
            this.cmdChangeSubject.Name = "cmdChangeSubject";
            this.cmdChangeSubject.Size = new System.Drawing.Size(65, 24);
            this.cmdChangeSubject.TabIndex = 6;
            this.cmdChangeSubject.Text = "change";
            this.cmdChangeSubject.UseVisualStyleBackColor = true;
            this.cmdChangeSubject.Click += new System.EventHandler(this.cmdChangeSubject_Click);
            // 
            // txtSubject
            // 
            this.txtSubject.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSubject.Location = new System.Drawing.Point(61, 3);
            this.txtSubject.Multiline = true;
            this.txtSubject.Name = "txtSubject";
            this.txtSubject.Size = new System.Drawing.Size(296, 24);
            this.txtSubject.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.rtfSend, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmdSend, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(431, 111);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // rtfSend
            // 
            this.rtfSend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfSend.Location = new System.Drawing.Point(3, 3);
            this.rtfSend.Name = "rtfSend";
            this.rtfSend.Size = new System.Drawing.Size(425, 75);
            this.rtfSend.TabIndex = 0;
            this.rtfSend.Text = "";
            // 
            // cmdSend
            // 
            this.cmdSend.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdSend.Location = new System.Drawing.Point(351, 84);
            this.cmdSend.Name = "cmdSend";
            this.cmdSend.Size = new System.Drawing.Size(77, 24);
            this.cmdSend.TabIndex = 1;
            this.cmdSend.Text = "&Send";
            this.cmdSend.UseVisualStyleBackColor = true;
            this.cmdSend.Click += new System.EventHandler(this.cmdSend_Click);
            // 
            // frmGroupChat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(623, 354);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusBar1);
            this.Name = "frmGroupChat";
            this.Text = "Group Chat";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmGroupChat_FormClosed);
            this.Load += new System.EventHandler(this.frmGroupChat_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.StatusBar statusBar1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RichTextBox rtfSend;
        private System.Windows.Forms.ListView lvwRoster;
        private System.Windows.Forms.ColumnHeader headerNickname;
        private System.Windows.Forms.ColumnHeader headerStatus;
        private System.Windows.Forms.Button cmdSend;
        private System.Windows.Forms.ImageList ilsRoster;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TextBox txtSubject;
        private System.Windows.Forms.RichTextBox rtfChat;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cmdChangeSubject;
        private System.Windows.Forms.ColumnHeader headerRole;
        private System.Windows.Forms.ColumnHeader headerAffiliation;

    }
}