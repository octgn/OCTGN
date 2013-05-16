Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms

Imports agsXMPP

Namespace MiniClient

	Public Class frmAddRoster
	Inherits System.Windows.Forms.Form
		Private components As System.ComponentModel.Container = Nothing
		Private _connection As XmppClientConnection
		Private label2 As System.Windows.Forms.Label
		Private txtNickname As System.Windows.Forms.TextBox
		Private txtJid As System.Windows.Forms.TextBox
		Private cmdAdd As System.Windows.Forms.Button
		Private label1 As System.Windows.Forms.Label
		Private label3 As System.Windows.Forms.Label
		
		Public Sub New(ByVal con As XmppClientConnection)
			InitializeComponent
			_connection = con
		End Sub

		Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
			If disposing Then
				If Not (components Is Nothing) Then
					components.Dispose
				End If
			End If
			MyBase.Dispose(disposing)
		End Sub

		Private Sub InitializeComponent()
			Me.label3 = New System.Windows.Forms.Label
			Me.label1 = New System.Windows.Forms.Label
			Me.cmdAdd = New System.Windows.Forms.Button
			Me.txtJid = New System.Windows.Forms.TextBox
			Me.txtNickname = New System.Windows.Forms.TextBox
			Me.label2 = New System.Windows.Forms.Label
			Me.SuspendLayout
			'
			'label3
			'
			Me.label3.Location = New System.Drawing.Point(8, 56)
			Me.label3.Name = "label3"
			Me.label3.Size = New System.Drawing.Size(64, 16)
			Me.label3.TabIndex = 11
			Me.label3.Text = "Nickname:"
			'
			'label1
			'
			Me.label1.Location = New System.Drawing.Point(8, 8)
			Me.label1.Name = "label1"
			Me.label1.Size = New System.Drawing.Size(72, 16)
			Me.label1.TabIndex = 7
			Me.label1.Text = "Jabber ID:"
			'
			'cmdAdd
			'
			Me.cmdAdd.FlatStyle = System.Windows.Forms.FlatStyle.System
			Me.cmdAdd.Location = New System.Drawing.Point(96, 88)
			Me.cmdAdd.Name = "cmdAdd"
			Me.cmdAdd.Size = New System.Drawing.Size(80, 24)
			Me.cmdAdd.TabIndex = 12
			Me.cmdAdd.Text = "Add"
			AddHandler Me.cmdAdd.Click, AddressOf Me.cmdAdd_Click
			'
			'txtJid
			'
			Me.txtJid.Location = New System.Drawing.Point(80, 8)
			Me.txtJid.Name = "txtJid"
			Me.txtJid.Size = New System.Drawing.Size(168, 20)
			Me.txtJid.TabIndex = 8
			Me.txtJid.Text = ""
			'
			'txtNickname
			'
			Me.txtNickname.Location = New System.Drawing.Point(80, 56)
			Me.txtNickname.Name = "txtNickname"
			Me.txtNickname.Size = New System.Drawing.Size(168, 20)
			Me.txtNickname.TabIndex = 9
			Me.txtNickname.Text = ""
			'
			'label2
			'
			Me.label2.Location = New System.Drawing.Point(80, 32)
			Me.label2.Name = "label2"
			Me.label2.Size = New System.Drawing.Size(160, 16)
			Me.label2.TabIndex = 10
			Me.label2.Text = "user@server.org"
			'
			'frmAddRoster
			'
			Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
			Me.ClientSize = New System.Drawing.Size(264, 118)
			Me.Controls.Add(Me.cmdAdd)
			Me.Controls.Add(Me.txtNickname)
			Me.Controls.Add(Me.txtJid)
			Me.Controls.Add(Me.label3)
			Me.Controls.Add(Me.label2)
			Me.Controls.Add(Me.label1)
			Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
			Me.MaximizeBox = false
			Me.MinimizeBox = false
			Me.Name = "frmAddRoster"
			Me.Text = "Add Contact"
			Me.ResumeLayout(false)
		End Sub

		Private Sub cmdAdd_Click(ByVal sender As Object, ByVal e As System.EventArgs)
			Dim jid As Jid = New Jid(txtJid.Text)
			If txtNickname.Text.Length > 0 Then
				_connection.RosterManager.AddRosterItem(jid, txtNickname.Text)
			Else
				_connection.RosterManager.AddRosterItem(jid)
			End If
			_connection.PresenceManager.Subcribe(jid)
			Me.Close
		End Sub
	End Class 
End Namespace
