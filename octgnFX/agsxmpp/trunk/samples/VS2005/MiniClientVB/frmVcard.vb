Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms

Imports agsXMPP
Imports agsXMPP.protocol
Imports agsXMPP.protocol.client
Imports agsXMPP.protocol.iq.vcard

Namespace MiniClient

	Public Class frmVcard
	Inherits System.Windows.Forms.Form
		Private components As System.ComponentModel.Container = Nothing
		Private _connection As XmppClientConnection
		Private packetId As String
		Private label3 As System.Windows.Forms.Label
		Private txtBirthday As System.Windows.Forms.TextBox
		Private label1 As System.Windows.Forms.Label
		Private label5 As System.Windows.Forms.Label
		Private txtFullname As System.Windows.Forms.TextBox
		Private label2 As System.Windows.Forms.Label
		Private picPhoto As System.Windows.Forms.PictureBox
		Private txtDescription As System.Windows.Forms.TextBox
		Private label4 As System.Windows.Forms.Label
		Private cmdClose As System.Windows.Forms.Button
		Private txtNickname As System.Windows.Forms.TextBox
		
		Public Sub New(ByVal jid As Jid, ByVal con As XmppClientConnection)
			InitializeComponent
			_connection = con
			Me.Text = "Vcard from: " + jid.Bare
			Dim viq As VcardIq = New VcardIq(IqType.get, New Jid(jid.Bare))
			packetId = viq.Id
			con.IqGrabber.SendIq(viq, AddressOf VcardResult, Nothing)
		End Sub

		Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
			If disposing Then
				If Not (components Is Nothing) Then
					components.Dispose
				End If
			End If
			MyBase.Dispose(disposing)
			If Not (_connection.IqGrabber Is Nothing) Then
				_connection.IqGrabber.Remove(packetId)
			End If
		End Sub

		Private Sub InitializeComponent()
			Me.txtNickname = New System.Windows.Forms.TextBox
			Me.cmdClose = New System.Windows.Forms.Button
			Me.label4 = New System.Windows.Forms.Label
			Me.txtDescription = New System.Windows.Forms.TextBox
			Me.picPhoto = New System.Windows.Forms.PictureBox
			Me.label2 = New System.Windows.Forms.Label
			Me.txtFullname = New System.Windows.Forms.TextBox
			Me.label5 = New System.Windows.Forms.Label
			Me.label1 = New System.Windows.Forms.Label
			Me.txtBirthday = New System.Windows.Forms.TextBox
			Me.label3 = New System.Windows.Forms.Label
			Me.SuspendLayout
			'
			'txtNickname
			'
			Me.txtNickname.Location = New System.Drawing.Point(128, 48)
			Me.txtNickname.Name = "txtNickname"
			Me.txtNickname.ReadOnly = true
			Me.txtNickname.Size = New System.Drawing.Size(176, 20)
			Me.txtNickname.TabIndex = 1
			Me.txtNickname.Text = ""
			'
			'cmdClose
			'
			Me.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.System
			Me.cmdClose.Location = New System.Drawing.Point(120, 304)
			Me.cmdClose.Name = "cmdClose"
			Me.cmdClose.Size = New System.Drawing.Size(96, 24)
			Me.cmdClose.TabIndex = 4
			Me.cmdClose.Text = "Close"
			AddHandler Me.cmdClose.Click, AddressOf Me.cmdClose_Click
			'
			'label4
			'
			Me.label4.Location = New System.Drawing.Point(16, 48)
			Me.label4.Name = "label4"
			Me.label4.Size = New System.Drawing.Size(96, 16)
			Me.label4.TabIndex = 3
			Me.label4.Text = "Nick Name:"
			'
			'txtDescription
			'
			Me.txtDescription.Location = New System.Drawing.Point(128, 112)
			Me.txtDescription.Multiline = true
			Me.txtDescription.Name = "txtDescription"
			Me.txtDescription.ReadOnly = true
			Me.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
			Me.txtDescription.Size = New System.Drawing.Size(176, 64)
			Me.txtDescription.TabIndex = 3
			Me.txtDescription.Text = ""
			'
			'picPhoto
			'
			Me.picPhoto.Location = New System.Drawing.Point(128, 192)
			Me.picPhoto.Name = "picPhoto"
			Me.picPhoto.Size = New System.Drawing.Size(176, 104)
			Me.picPhoto.TabIndex = 9
			Me.picPhoto.TabStop = false
			'
			'label2
			'
			Me.label2.Location = New System.Drawing.Point(16, 88)
			Me.label2.Name = "label2"
			Me.label2.Size = New System.Drawing.Size(104, 16)
			Me.label2.TabIndex = 1
			Me.label2.Text = "Birthday:"
			'
			'txtFullname
			'
			Me.txtFullname.Location = New System.Drawing.Point(128, 16)
			Me.txtFullname.Name = "txtFullname"
			Me.txtFullname.ReadOnly = true
			Me.txtFullname.Size = New System.Drawing.Size(176, 20)
			Me.txtFullname.TabIndex = 0
			Me.txtFullname.Text = ""
			'
			'label5
			'
			Me.label5.Location = New System.Drawing.Point(16, 192)
			Me.label5.Name = "label5"
			Me.label5.Size = New System.Drawing.Size(104, 16)
			Me.label5.TabIndex = 8
			Me.label5.Text = "Photo:"
			'
			'label1
			'
			Me.label1.Location = New System.Drawing.Point(16, 16)
			Me.label1.Name = "label1"
			Me.label1.Size = New System.Drawing.Size(96, 24)
			Me.label1.TabIndex = 0
			Me.label1.Text = "Full Name:"
			'
			'txtBirthday
			'
			Me.txtBirthday.Location = New System.Drawing.Point(128, 80)
			Me.txtBirthday.Name = "txtBirthday"
			Me.txtBirthday.ReadOnly = true
			Me.txtBirthday.Size = New System.Drawing.Size(176, 20)
			Me.txtBirthday.TabIndex = 2
			Me.txtBirthday.Text = ""
			'
			'label3
			'
			Me.label3.Location = New System.Drawing.Point(16, 120)
			Me.label3.Name = "label3"
			Me.label3.Size = New System.Drawing.Size(104, 16)
			Me.label3.TabIndex = 2
			Me.label3.Text = "Description:"
			'
			'frmVcard
			'
			Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
			Me.ClientSize = New System.Drawing.Size(328, 336)
			Me.Controls.Add(Me.cmdClose)
			Me.Controls.Add(Me.picPhoto)
			Me.Controls.Add(Me.label5)
			Me.Controls.Add(Me.txtNickname)
			Me.Controls.Add(Me.txtDescription)
			Me.Controls.Add(Me.txtBirthday)
			Me.Controls.Add(Me.txtFullname)
			Me.Controls.Add(Me.label4)
			Me.Controls.Add(Me.label3)
			Me.Controls.Add(Me.label2)
			Me.Controls.Add(Me.label1)
			Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
			Me.MaximizeBox = false
			Me.Name = "frmVcard"
			Me.Text = "frmVcard"
			Me.ResumeLayout(false)
		End Sub

        Private Sub VcardResult(ByVal sender As Object, ByVal iq As agsXMPP.protocol.client.IQ, ByVal data As Object)
            If InvokeRequired Then
                BeginInvoke(CType(AddressOf VcardResult, agsXMPP.IqCB), New Object() {sender, iq, data})
                Return
            End If

            If iq.Type = IqType.result Then
                Dim vcard As Vcard = iq.Vcard
                If Not (vcard Is Nothing) Then
                    txtFullname.Text = vcard.Fullname
                    txtNickname.Text = vcard.Nickname
                    txtBirthday.Text = vcard.Birthday.ToString
                    txtDescription.Text = vcard.Description
                    Dim photo As Photo = vcard.Photo
                    If Not (photo Is Nothing) Then
                        picPhoto.Image = vcard.Photo.Image
                    End If
                End If
            End If
        End Sub

		Private Sub cmdClose_Click(ByVal sender As Object, ByVal e As System.EventArgs)
			Me.Close
		End Sub
	End Class 
End Namespace
