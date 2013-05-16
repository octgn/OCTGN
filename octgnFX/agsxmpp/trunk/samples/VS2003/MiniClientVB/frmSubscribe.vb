Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms

Imports agsXMPP
Imports agsXMPP.protocol.client

Namespace MiniClient

	Public Class frmSubscribe
	Inherits System.Windows.Forms.Form
		Private components As System.ComponentModel.Container = Nothing
		Private _connection As XmppClientConnection
		Private _from As Jid
		Private label1 As System.Windows.Forms.Label
		Private cmdApprove As System.Windows.Forms.Button
		Private lblFrom As System.Windows.Forms.Label
		Private cmdRefuse As System.Windows.Forms.Button
		
		Public Sub New(ByVal con As XmppClientConnection, ByVal jid As Jid)
			InitializeComponent
			_connection = con
			_from = jid
			lblFrom.Text = jid.ToString
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
			Me.cmdRefuse = New System.Windows.Forms.Button
			Me.lblFrom = New System.Windows.Forms.Label
			Me.cmdApprove = New System.Windows.Forms.Button
			Me.label1 = New System.Windows.Forms.Label
			Me.SuspendLayout
			'
			'cmdRefuse
			'
			Me.cmdRefuse.FlatStyle = System.Windows.Forms.FlatStyle.System
			Me.cmdRefuse.Location = New System.Drawing.Point(56, 56)
			Me.cmdRefuse.Name = "cmdRefuse"
			Me.cmdRefuse.Size = New System.Drawing.Size(72, 24)
			Me.cmdRefuse.TabIndex = 1
			Me.cmdRefuse.Text = "Refuse"
			AddHandler Me.cmdRefuse.Click, AddressOf Me.cmdRefuse_Click
			'
			'lblFrom
			'
			Me.lblFrom.Location = New System.Drawing.Point(56, 16)
			Me.lblFrom.Name = "lblFrom"
			Me.lblFrom.Size = New System.Drawing.Size(224, 32)
			Me.lblFrom.TabIndex = 3
			Me.lblFrom.Text = "jid"
			'
			'cmdApprove
			'
			Me.cmdApprove.FlatStyle = System.Windows.Forms.FlatStyle.System
			Me.cmdApprove.Location = New System.Drawing.Point(168, 56)
			Me.cmdApprove.Name = "cmdApprove"
			Me.cmdApprove.Size = New System.Drawing.Size(72, 24)
			Me.cmdApprove.TabIndex = 0
			Me.cmdApprove.Text = "Approve"
			AddHandler Me.cmdApprove.Click, AddressOf Me.cmdApprove_Click
			'
			'label1
			'
			Me.label1.Location = New System.Drawing.Point(8, 16)
			Me.label1.Name = "label1"
			Me.label1.Size = New System.Drawing.Size(48, 16)
			Me.label1.TabIndex = 2
			Me.label1.Text = "From:"
			'
			'frmSubscribe
			'
			Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
			Me.ClientSize = New System.Drawing.Size(292, 88)
			Me.Controls.Add(Me.lblFrom)
			Me.Controls.Add(Me.label1)
			Me.Controls.Add(Me.cmdRefuse)
			Me.Controls.Add(Me.cmdApprove)
			Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
			Me.MaximizeBox = false
			Me.MinimizeBox = false
			Me.Name = "frmSubscribe"
			Me.Text = "Subscription Request"
			Me.ResumeLayout(false)
		End Sub

		Private Sub cmdRefuse_Click(ByVal sender As Object, ByVal e As System.EventArgs)
			Dim pm As PresenceManager = New PresenceManager(_connection)
			pm.RefuseSubscriptionRequest(_from)
			Me.Close
		End Sub

		Private Sub cmdApprove_Click(ByVal sender As Object, ByVal e As System.EventArgs)
			Dim pm As PresenceManager = New PresenceManager(_connection)
			pm.ApproveSubscriptionRequest(_from)
			Me.Close
		End Sub
	End Class 
End Namespace
