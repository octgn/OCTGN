Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Data

Imports agsXMPP
Imports agsXMPP.protocol
Imports agsXMPP.protocol.iq
Imports agsXMPP.protocol.iq.disco
Imports agsXMPP.protocol.iq.roster
Imports agsXMPP.protocol.client

Imports agsXMPP.Xml.Dom


Namespace MiniClient

	Public Class frmMain
	Inherits System.Windows.Forms.Form

		Private components As System.ComponentModel.IContainer
        Private WithEvents ctxMnuRosterDelete As System.Windows.Forms.MenuItem
		Private tabRoster As System.Windows.Forms.TabPage
		Private statusBar1 As System.Windows.Forms.StatusBar
		Private mainMenu1 As System.Windows.Forms.MainMenu
        Private WithEvents cboStatus As System.Windows.Forms.ComboBox
		Private tabControl1 As System.Windows.Forms.TabControl
        Private WithEvents ctxMnuRosterVcard As System.Windows.Forms.MenuItem
        Private WithEvents toolBar As System.Windows.Forms.ToolBar
        Private WithEvents mnuFileDisconnect As System.Windows.Forms.MenuItem
		Private tabDebug As System.Windows.Forms.TabPage
        Private WithEvents mnuFileExit As System.Windows.Forms.MenuItem
		Private ctxMnuRoster As System.Windows.Forms.ContextMenu
        Private tbbAdd As System.Windows.Forms.ToolBarButton
		Private menuItem3 As System.Windows.Forms.MenuItem
		Private menuItem1 As System.Windows.Forms.MenuItem
		Private rtfDebug As System.Windows.Forms.RichTextBox
        Private lvwRoster As System.Windows.Forms.ListView
        Private WithEvents mnuFileConnect As System.Windows.Forms.MenuItem
        Private WithEvents ctxMnuRosterChat As System.Windows.Forms.MenuItem

        Private WithEvents XmppCon As XmppClientConnection

		Delegate Sub OnMessageDelegate(ByVal sender As Object, ByVal msg As agsXMPP.protocol.client.Message)

		Delegate Sub OnPresenceDelegate(ByVal sender As Object, ByVal pres As Presence)

		Public Sub New()
			InitializeComponent
			cboStatus.Items.AddRange(New Object() {ShowType.away.ToString, ShowType.xa.ToString, ShowType.chat.ToString, ShowType.dnd.ToString, "online"})
			cboStatus.Text = "offline"
		End Sub

		Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
			If disposing Then
				If Not (components Is Nothing) Then
					components.Dispose
				End If
			End If
			MyBase.Dispose(disposing)
		End Sub
        Friend WithEvents ilsRoster As System.Windows.Forms.ImageList
        Friend WithEvents ils16 As System.Windows.Forms.ImageList

        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
            Me.ctxMnuRosterChat = New System.Windows.Forms.MenuItem
            Me.mnuFileConnect = New System.Windows.Forms.MenuItem
            Me.lvwRoster = New System.Windows.Forms.ListView
            Me.ctxMnuRoster = New System.Windows.Forms.ContextMenu
            Me.ctxMnuRosterVcard = New System.Windows.Forms.MenuItem
            Me.ctxMnuRosterDelete = New System.Windows.Forms.MenuItem
            Me.ilsRoster = New System.Windows.Forms.ImageList(Me.components)
            Me.rtfDebug = New System.Windows.Forms.RichTextBox
            Me.menuItem1 = New System.Windows.Forms.MenuItem
            Me.mnuFileDisconnect = New System.Windows.Forms.MenuItem
            Me.menuItem3 = New System.Windows.Forms.MenuItem
            Me.mnuFileExit = New System.Windows.Forms.MenuItem
            Me.tbbAdd = New System.Windows.Forms.ToolBarButton
            Me.tabDebug = New System.Windows.Forms.TabPage
            Me.toolBar = New System.Windows.Forms.ToolBar
            Me.ils16 = New System.Windows.Forms.ImageList(Me.components)
            Me.tabControl1 = New System.Windows.Forms.TabControl
            Me.tabRoster = New System.Windows.Forms.TabPage
            Me.cboStatus = New System.Windows.Forms.ComboBox
            Me.mainMenu1 = New System.Windows.Forms.MainMenu(Me.components)
            Me.statusBar1 = New System.Windows.Forms.StatusBar
            Me.tabDebug.SuspendLayout()
            Me.tabControl1.SuspendLayout()
            Me.tabRoster.SuspendLayout()
            Me.SuspendLayout()
            '
            'ctxMnuRosterChat
            '
            Me.ctxMnuRosterChat.Index = 0
            Me.ctxMnuRosterChat.Text = "Chat"
            '
            'mnuFileConnect
            '
            Me.mnuFileConnect.Index = 0
            Me.mnuFileConnect.Text = "Connect"
            '
            'lvwRoster
            '
            Me.lvwRoster.ContextMenu = Me.ctxMnuRoster
            Me.lvwRoster.Dock = System.Windows.Forms.DockStyle.Fill
            Me.lvwRoster.Location = New System.Drawing.Point(0, 21)
            Me.lvwRoster.Name = "lvwRoster"
            Me.lvwRoster.Size = New System.Drawing.Size(285, 261)
            Me.lvwRoster.SmallImageList = Me.ilsRoster
            Me.lvwRoster.TabIndex = 1
            Me.lvwRoster.UseCompatibleStateImageBehavior = False
            Me.lvwRoster.View = System.Windows.Forms.View.Details
            '
            'ctxMnuRoster
            '
            Me.ctxMnuRoster.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.ctxMnuRosterChat, Me.ctxMnuRosterVcard, Me.ctxMnuRosterDelete})
            '
            'ctxMnuRosterVcard
            '
            Me.ctxMnuRosterVcard.Index = 1
            Me.ctxMnuRosterVcard.Text = "Vcard"
            '
            'ctxMnuRosterDelete
            '
            Me.ctxMnuRosterDelete.Index = 2
            Me.ctxMnuRosterDelete.Text = "delete"
            '
            'ilsRoster
            '
            Me.ilsRoster.ImageStream = CType(resources.GetObject("ilsRoster.ImageStream"), System.Windows.Forms.ImageListStreamer)
            Me.ilsRoster.TransparentColor = System.Drawing.Color.Transparent
            Me.ilsRoster.Images.SetKeyName(0, "")
            Me.ilsRoster.Images.SetKeyName(1, "")
            Me.ilsRoster.Images.SetKeyName(2, "")
            Me.ilsRoster.Images.SetKeyName(3, "")
            Me.ilsRoster.Images.SetKeyName(4, "")
            Me.ilsRoster.Images.SetKeyName(5, "")
            '
            'rtfDebug
            '
            Me.rtfDebug.BackColor = System.Drawing.SystemColors.Window
            Me.rtfDebug.Dock = System.Windows.Forms.DockStyle.Fill
            Me.rtfDebug.Location = New System.Drawing.Point(0, 0)
            Me.rtfDebug.Name = "rtfDebug"
            Me.rtfDebug.ReadOnly = True
            Me.rtfDebug.Size = New System.Drawing.Size(288, 351)
            Me.rtfDebug.TabIndex = 0
            Me.rtfDebug.Text = ""
            '
            'menuItem1
            '
            Me.menuItem1.Index = 0
            Me.menuItem1.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuFileConnect, Me.mnuFileDisconnect, Me.menuItem3, Me.mnuFileExit})
            Me.menuItem1.Text = "File"
            '
            'mnuFileDisconnect
            '
            Me.mnuFileDisconnect.Enabled = False
            Me.mnuFileDisconnect.Index = 1
            Me.mnuFileDisconnect.Text = "Disconnect"
            '
            'menuItem3
            '
            Me.menuItem3.Index = 2
            Me.menuItem3.Text = "-"
            '
            'mnuFileExit
            '
            Me.mnuFileExit.Index = 3
            Me.mnuFileExit.Text = "&Exit"
            '
            'tbbAdd
            '
            Me.tbbAdd.ImageIndex = 0
            Me.tbbAdd.Name = "tbbAdd"
            Me.tbbAdd.ToolTipText = "add contact"
            '
            'tabDebug
            '
            Me.tabDebug.Controls.Add(Me.rtfDebug)
            Me.tabDebug.ImageIndex = 2
            Me.tabDebug.Location = New System.Drawing.Point(4, 23)
            Me.tabDebug.Name = "tabDebug"
            Me.tabDebug.Size = New System.Drawing.Size(288, 351)
            Me.tabDebug.TabIndex = 1
            Me.tabDebug.Text = "Debug"
            Me.tabDebug.UseVisualStyleBackColor = True
            Me.tabDebug.Visible = False
            '
            'toolBar
            '
            Me.toolBar.Buttons.AddRange(New System.Windows.Forms.ToolBarButton() {Me.tbbAdd})
            Me.toolBar.DropDownArrows = True
            Me.toolBar.ImageList = Me.ils16
            Me.toolBar.Location = New System.Drawing.Point(0, 0)
            Me.toolBar.Name = "toolBar"
            Me.toolBar.ShowToolTips = True
            Me.toolBar.Size = New System.Drawing.Size(293, 28)
            Me.toolBar.TabIndex = 3
            '
            'ils16
            '
            Me.ils16.ImageStream = CType(resources.GetObject("ils16.ImageStream"), System.Windows.Forms.ImageListStreamer)
            Me.ils16.TransparentColor = System.Drawing.Color.Transparent
            Me.ils16.Images.SetKeyName(0, "")
            Me.ils16.Images.SetKeyName(1, "user.png")
            Me.ils16.Images.SetKeyName(2, "application_xp_terminal.png")
            '
            'tabControl1
            '
            Me.tabControl1.Controls.Add(Me.tabRoster)
            Me.tabControl1.Controls.Add(Me.tabDebug)
            Me.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill
            Me.tabControl1.ImageList = Me.ils16
            Me.tabControl1.Location = New System.Drawing.Point(0, 28)
            Me.tabControl1.Name = "tabControl1"
            Me.tabControl1.SelectedIndex = 0
            Me.tabControl1.Size = New System.Drawing.Size(293, 309)
            Me.tabControl1.TabIndex = 4
            '
            'tabRoster
            '
            Me.tabRoster.Controls.Add(Me.lvwRoster)
            Me.tabRoster.Controls.Add(Me.cboStatus)
            Me.tabRoster.ImageIndex = 1
            Me.tabRoster.Location = New System.Drawing.Point(4, 23)
            Me.tabRoster.Name = "tabRoster"
            Me.tabRoster.Size = New System.Drawing.Size(285, 282)
            Me.tabRoster.TabIndex = 0
            Me.tabRoster.Text = "Roster"
            Me.tabRoster.UseVisualStyleBackColor = True
            '
            'cboStatus
            '
            Me.cboStatus.Dock = System.Windows.Forms.DockStyle.Top
            Me.cboStatus.Location = New System.Drawing.Point(0, 0)
            Me.cboStatus.Name = "cboStatus"
            Me.cboStatus.Size = New System.Drawing.Size(285, 21)
            Me.cboStatus.TabIndex = 0
            '
            'mainMenu1
            '
            Me.mainMenu1.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.menuItem1})
            '
            'statusBar1
            '
            Me.statusBar1.Location = New System.Drawing.Point(0, 337)
            Me.statusBar1.Name = "statusBar1"
            Me.statusBar1.Size = New System.Drawing.Size(293, 24)
            Me.statusBar1.TabIndex = 1
            Me.statusBar1.Text = "Offline"
            '
            'frmMain
            '
            Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
            Me.ClientSize = New System.Drawing.Size(293, 361)
            Me.Controls.Add(Me.tabControl1)
            Me.Controls.Add(Me.toolBar)
            Me.Controls.Add(Me.statusBar1)
            Me.Menu = Me.mainMenu1
            Me.Name = "frmMain"
            Me.Text = "Mini Client"
            Me.tabDebug.ResumeLayout(False)
            Me.tabControl1.ResumeLayout(False)
            Me.tabRoster.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

		<STAThread()> _
		Shared Sub Main()
			Application.EnableVisualStyles
			Application.DoEvents
			Application.Run(New frmMain)
		End Sub

		Private Sub XmppCon_OnError(ByVal sender As Object, ByVal ex As Exception)
		End Sub

        Private Sub mnuFileDisconnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFileDisconnect.Click
            XmppCon.Close()
        End Sub

        Private Sub mnuFileConnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFileConnect.Click
            XmppCon = Nothing
            XmppCon = New XmppClientConnection

            Dim f As frmLogin = New frmLogin(XmppCon)
            If f.ShowDialog = Windows.Forms.DialogResult.OK Then
                InitRosterListView()
                XmppCon.Open()
            End If
        End Sub

        Private Sub mnuFileExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFileExit.Click
            Me.Close()
        End Sub

        Private Sub cboStatus_SelectedValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboStatus.SelectedValueChanged
            If XmppCon.Authenticated Then
                If cboStatus.Text = "online" Then
                    XmppCon.Show = ShowType.NONE
                Else
                    XmppCon.Show = CType(System.Enum.Parse(GetType(ShowType), cboStatus.Text), ShowType)
                End If
                XmppCon.SendMyPresence()
            End If
        End Sub

        Private Function FindRosterListViewItem(ByVal jid As Jid) As ListViewItem
            For Each lvi As ListViewItem In lvwRoster.Items
                If jid.Bare.ToLower = lvi.Tag.ToString.ToLower Then
                    Return lvi
                End If
            Next
            Return Nothing
        End Function

        Private Sub InitRosterListView()
            lvwRoster.Clear()
            lvwRoster.MultiSelect = False
            lvwRoster.Columns.Add("name", 100, HorizontalAlignment.Left)
            lvwRoster.Columns.Add("status", 75, HorizontalAlignment.Left)
            lvwRoster.Columns.Add("resource", 75, HorizontalAlignment.Left)
        End Sub

        Private Function GetSelectedListViewItem() As ListViewItem
            If lvwRoster.SelectedItems.Count > 0 Then
                Return lvwRoster.SelectedItems(0)
            End If
            Return Nothing
        End Function

        Private Function GetRosterImageIndex(ByVal pres As Presence) As Integer
            If pres.Type = PresenceType.unavailable Then
                Return 0
            Else
                If pres.Type = PresenceType.error Then
                Else
                    Select Case pres.Show
                        Case ShowType.NONE
                            Return 1
                        Case ShowType.away
                            Return 2
                        Case ShowType.chat
                            Return 4
                        Case ShowType.xa
                            Return 3
                        Case ShowType.dnd
                            Return 5
                    End Select
                End If
            End If
            Return 0
        End Function

        Private Sub ctxMnuRosterVcard_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ctxMnuRosterVcard.Click
            Dim itm As ListViewItem = GetSelectedListViewItem()
            If Not (itm Is Nothing) Then
                Dim f As frmVcard = New frmVcard(New Jid(itm.Tag.ToString), XmppCon)
                f.Show()
            End If
        End Sub

        Private Sub ctxMnuRosterChat_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ctxMnuRosterChat.Click
            Dim itm As ListViewItem = GetSelectedListViewItem()
            If Not (itm Is Nothing) Then
                If Not Util.Forms.ContainsKey(itm.Tag) Then
                    Dim f As frmChat = New frmChat(New Jid(CType(itm.Tag, String)), XmppCon, itm.Text)
                    f.Show()
                End If
            End If
        End Sub

        Private Sub ctxMnuRosterDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ctxMnuRosterDelete.Click
            Dim itm As ListViewItem = GetSelectedListViewItem()
            If Not (itm Is Nothing) Then
                Dim riq As RosterIq = New RosterIq
                riq.Type = IqType.set
                XmppCon.RosterManager.RemoveRosterItem(New Jid(CType(itm.Tag, String)))
            End If
        End Sub

        Private Sub toolBar_ButtonClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ToolBarButtonClickEventArgs) Handles toolBar.ButtonClick
            If e.Button Is tbbAdd Then
                Dim f As frmAddRoster = New frmAddRoster(XmppCon)
                f.Show()
            End If
        End Sub

        Private Sub XmppCon_OnWriteXml(ByVal sender As Object, ByVal xml As String) Handles XmppCon.OnWriteXml
            If InvokeRequired Then
                BeginInvoke(CType(AddressOf XmppCon_OnWriteXml, agsXMPP.XmlHandler), New Object() {sender, xml})
                Return
            End If

            rtfDebug.SelectionStart = rtfDebug.Text.Length
            rtfDebug.SelectionLength = 0
            rtfDebug.SelectionColor = Color.Blue
            rtfDebug.AppendText("SEND: ")
            rtfDebug.SelectionColor = Color.Black
            rtfDebug.AppendText(xml)
            rtfDebug.AppendText("" & Microsoft.VisualBasic.Chr(13) & "" & Microsoft.VisualBasic.Chr(10) & "")
        End Sub

        Private Sub XmppCon_OnReadXml(ByVal sender As Object, ByVal xml As String) Handles XmppCon.OnReadXml
            If InvokeRequired Then
                BeginInvoke(CType(AddressOf XmppCon_OnReadXml, agsXMPP.XmlHandler), New Object() {sender, xml})
                Return
            End If

            rtfDebug.SelectionStart = rtfDebug.Text.Length
            rtfDebug.SelectionLength = 0
            rtfDebug.SelectionColor = Color.Red
            rtfDebug.AppendText("RECV: ")
            rtfDebug.SelectionColor = Color.Black
            rtfDebug.AppendText(xml)
            rtfDebug.AppendText("" & Microsoft.VisualBasic.Chr(13) & "" & Microsoft.VisualBasic.Chr(10) & "")
        End Sub

        Private Sub XmppCon_OnRosterStart(ByVal sender As Object) Handles XmppCon.OnRosterStart
            If InvokeRequired Then
                BeginInvoke(CType(AddressOf XmppCon_OnRosterStart, agsXMPP.ObjectHandler), New Object() {sender})
                Return
            End If

            lvwRoster.BeginUpdate()
        End Sub

        Private Sub XmppCon_OnRosterEnd(ByVal sender As Object) Handles XmppCon.OnRosterEnd
            If InvokeRequired Then
                BeginInvoke(CType(AddressOf XmppCon_OnRosterEnd, agsXMPP.ObjectHandler), New Object() {sender})
                Return
            End If

            lvwRoster.EndUpdate()
            cboStatus.Text = "online"
        End Sub

        Private Sub XmppCon_OnRosterItem(ByVal sender As Object, ByVal item As agsXMPP.protocol.iq.roster.RosterItem) Handles XmppCon.OnRosterItem
            If InvokeRequired Then
                BeginInvoke(CType(AddressOf XmppCon_OnRosterItem, agsXMPP.XmppClientConnection.RosterHandler), New Object() {sender, item})
                Return
            End If
            If Not (item.Subscription = SubscriptionType.remove) Then
                Dim li As ListViewItem
                li = FindRosterListViewItem(item.Jid)
                If li Is Nothing Then
                    li = New ListViewItem
                    li.Text = Microsoft.VisualBasic.IIf(Not item.Name Is Nothing, item.Name, item.Jid.ToString())
                    li.ImageIndex = 0
                    li.Tag = item.Jid.ToString
                    li.SubItems.AddRange(New String() {"", ""})
                    lvwRoster.Items.Add(li)
                Else
                    li.Text = Microsoft.VisualBasic.IIf(Not (item.Name Is Nothing), item.Name, item.Jid.ToString)
                End If
            Else
                Dim li As ListViewItem = FindRosterListViewItem(item.Jid)
                If Not (li Is Nothing) Then
                    li.Remove()
                End If
            End If
        End Sub

        Private Sub XmppCon_OnAgentStart(ByVal sender As Object) Handles XmppCon.OnAgentStart
        End Sub

        Private Sub XmppCon_OnAgentEnd(ByVal sender As Object) Handles XmppCon.OnAgentEnd
        End Sub

        Private Sub XmppCon_OnAgentItem(ByVal sender As Object, ByVal agent As agsXMPP.protocol.iq.agent.Agent) Handles XmppCon.OnAgentItem
        End Sub

        Private Sub XmppCon_OnLogin(ByVal sender As Object) Handles XmppCon.OnLogin
            If InvokeRequired Then
                BeginInvoke(CType(AddressOf XmppCon_OnLogin, agsXMPP.ObjectHandler), New Object() {sender})
                Return
            End If

            mnuFileConnect.Enabled = False
            mnuFileDisconnect.Enabled = True
        End Sub

        Private Sub XmppCon_OnPresence(ByVal sender As Object, ByVal pres As Presence) Handles XmppCon.OnPresence
            If InvokeRequired Then
                BeginInvoke(CType(AddressOf XmppCon_OnPresence, PresenceHandler), New Object() {sender, pres})
                Return
            End If

            If pres.Type = PresenceType.subscribe Then
                Dim f As frmSubscribe = New frmSubscribe(XmppCon, pres.From)
                f.Show()
            Else
                If pres.Type = PresenceType.subscribed Then
                Else
                    If pres.Type = PresenceType.unsubscribe Then
                    Else
                        If pres.Type = PresenceType.unsubscribed Then
                        Else
                            Dim lvi As ListViewItem = FindRosterListViewItem(pres.From)
                            If Not (lvi Is Nothing) Then
                                Dim imageIdx As Integer = GetRosterImageIndex(pres)
                                lvi.ImageIndex = imageIdx
                                lvi.SubItems(1).Text = pres.Status
                                lvi.SubItems(2).Text = pres.From.Resource
                            End If
                        End If
                    End If
                End If
            End If
        End Sub

        Private Sub XmppCon_OnMessage(ByVal sender As Object, ByVal msg As agsXMPP.protocol.client.Message) Handles XmppCon.OnMessage
            If InvokeRequired Then
                BeginInvoke(CType(AddressOf XmppCon_OnMessage, MessageHandler), New Object() {sender, msg})
                Return
            End If

            If Not Util.Forms.ContainsKey(msg.From.Bare) Then
                Dim itm As ListViewItem = FindRosterListViewItem(msg.From)
                Dim nick As String = Microsoft.VisualBasic.IIf(itm Is Nothing, msg.From.Bare, itm.Text)
                Dim f As frmChat = New frmChat(msg.From, XmppCon, nick)
                f.Show()
                f.IncomingMessage(msg)
            End If
        End Sub

        Private Sub XmppCon_OnIq(ByVal sender As Object, ByVal iq As agsXMPP.protocol.client.IQ) Handles XmppCon.OnIq
            If InvokeRequired Then
                BeginInvoke(CType(AddressOf XmppCon_OnIq, IqHandler), New Object() {sender, iq})
                Return
            End If

            Dim query As Element = iq.Query

            If Not query Is Nothing Then

                If query.GetType() Is GetType(agsXMPP.protocol.iq.version.Version) Then
                    '// its a version IQ VersionIQ
                    Dim vers As agsXMPP.protocol.iq.version.Version = CType(query, agsXMPP.protocol.iq.version.Version)
                    If iq.Type = IqType.get Then

                        '// Somebody wants to know our client version, so send it back
                        iq.SwitchDirection()
                        iq.Type = IqType.result

                        vers.Name = "MiniClient"
                        vers.Ver = "0.5"
                        vers.Os = Environment.OSVersion.ToString()

                        XmppCon.Send(iq)
                    End If
                End If
            End If
        End Sub

        Private Sub XmppCon_OnClose(ByVal sender As Object) Handles XmppCon.OnClose
            If InvokeRequired Then
                BeginInvoke(CType(AddressOf XmppCon_OnClose, agsXMPP.ObjectHandler), New Object() {sender})
                Return
            End If

            mnuFileConnect.Enabled = True
            mnuFileDisconnect.Enabled = False
            cboStatus.Text = "offline"
            InitRosterListView()
        End Sub
    End Class
End Namespace
