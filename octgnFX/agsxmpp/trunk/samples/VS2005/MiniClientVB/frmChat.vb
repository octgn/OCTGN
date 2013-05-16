Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms
Imports agsXMPP
Imports agsXMPP.protocol
Imports agsXMPP.protocol.client
Imports agsXMPP.Collections
Namespace MiniClient

	Public Class frmChat
	Inherits System.Windows.Forms.Form
		Private components As System.ComponentModel.Container = Nothing
		Private _connection As XmppClientConnection
		Private m_Jid As Jid
		Private _nickname As String
		Private rtfChat As System.Windows.Forms.RichTextBox
		Private splitter1 As System.Windows.Forms.Splitter
        Private WithEvents cmdSend As System.Windows.Forms.Button
		Private statusBar1 As System.Windows.Forms.StatusBar
        Private rtfSend As System.Windows.Forms.RichTextBox
        Private pictureBox1 As System.Windows.Forms.PictureBox

        Public Sub New(ByVal jid As Jid, ByVal con As XmppClientConnection, ByVal nickname As String)
            m_Jid = jid
            _connection = con
            _nickname = nickname
            InitializeComponent()
            Me.Text = "Chat with " + nickname
            Util.Forms.Add(m_Jid.Bare.ToLower, Me)
            con.MessageGrabber.Add(jid, New BareJidComparer, AddressOf MessageCallback, Nothing)
        End Sub

        Public Property Jid() As Jid
            Get
                Return m_Jid
            End Get
            Set(ByVal value As Jid)
                m_Jid = value
            End Set
        End Property

        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                If Not (components Is Nothing) Then
                    components.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
            Util.Forms.Remove(m_Jid.Bare.ToLower)
            _connection.MessageGrabber.Remove(m_Jid)
            _connection = Nothing
        End Sub

        Private Sub InitializeComponent()
            Me.pictureBox1 = New System.Windows.Forms.PictureBox
            Me.rtfSend = New System.Windows.Forms.RichTextBox
            Me.statusBar1 = New System.Windows.Forms.StatusBar
            Me.cmdSend = New System.Windows.Forms.Button
            Me.splitter1 = New System.Windows.Forms.Splitter
            Me.rtfChat = New System.Windows.Forms.RichTextBox
            CType(Me.pictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'pictureBox1
            '
            Me.pictureBox1.Dock = System.Windows.Forms.DockStyle.Bottom
            Me.pictureBox1.Location = New System.Drawing.Point(0, 182)
            Me.pictureBox1.Name = "pictureBox1"
            Me.pictureBox1.Size = New System.Drawing.Size(424, 34)
            Me.pictureBox1.TabIndex = 6
            Me.pictureBox1.TabStop = False
            '
            'rtfSend
            '
            Me.rtfSend.Dock = System.Windows.Forms.DockStyle.Bottom
            Me.rtfSend.Location = New System.Drawing.Point(0, 138)
            Me.rtfSend.Name = "rtfSend"
            Me.rtfSend.Size = New System.Drawing.Size(424, 44)
            Me.rtfSend.TabIndex = 8
            Me.rtfSend.Text = ""
            '
            'statusBar1
            '
            Me.statusBar1.Location = New System.Drawing.Point(0, 216)
            Me.statusBar1.Name = "statusBar1"
            Me.statusBar1.Size = New System.Drawing.Size(424, 22)
            Me.statusBar1.TabIndex = 5
            '
            'cmdSend
            '
            Me.cmdSend.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.cmdSend.FlatStyle = System.Windows.Forms.FlatStyle.System
            Me.cmdSend.Location = New System.Drawing.Point(344, 188)
            Me.cmdSend.Name = "cmdSend"
            Me.cmdSend.Size = New System.Drawing.Size(72, 22)
            Me.cmdSend.TabIndex = 7
            Me.cmdSend.Text = "&Send"
            '
            'splitter1
            '
            Me.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom
            Me.splitter1.Location = New System.Drawing.Point(0, 130)
            Me.splitter1.Name = "splitter1"
            Me.splitter1.Size = New System.Drawing.Size(424, 8)
            Me.splitter1.TabIndex = 9
            Me.splitter1.TabStop = False
            '
            'rtfChat
            '
            Me.rtfChat.Dock = System.Windows.Forms.DockStyle.Fill
            Me.rtfChat.Location = New System.Drawing.Point(0, 0)
            Me.rtfChat.Name = "rtfChat"
            Me.rtfChat.Size = New System.Drawing.Size(424, 130)
            Me.rtfChat.TabIndex = 10
            Me.rtfChat.Text = ""
            '
            'frmChat
            '
            Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
            Me.ClientSize = New System.Drawing.Size(424, 238)
            Me.Controls.Add(Me.rtfChat)
            Me.Controls.Add(Me.splitter1)
            Me.Controls.Add(Me.rtfSend)
            Me.Controls.Add(Me.cmdSend)
            Me.Controls.Add(Me.pictureBox1)
            Me.Controls.Add(Me.statusBar1)
            Me.Name = "frmChat"
            Me.Text = "frmChat"
            CType(Me.pictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)

        End Sub

        Private Sub OutgoingMessage(ByVal msg As agsXMPP.protocol.client.Message)
            rtfChat.SelectionColor = Color.Blue
            rtfChat.AppendText("Me said: ")
            rtfChat.SelectionColor = Color.Black
            rtfChat.AppendText(msg.Body)
            rtfChat.AppendText("" & Microsoft.VisualBasic.Chr(13) & "" & Microsoft.VisualBasic.Chr(10) & "")
        End Sub

        Public Sub IncomingMessage(ByVal msg As agsXMPP.protocol.client.Message)
            rtfChat.SelectionColor = Color.Red
            rtfChat.AppendText(_nickname + " said: ")
            rtfChat.SelectionColor = Color.Black
            rtfChat.AppendText(msg.Body)
            rtfChat.AppendText("" & Microsoft.VisualBasic.Chr(13) & "" & Microsoft.VisualBasic.Chr(10) & "")
        End Sub

        Private Sub cmdSend_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdSend.Click
            Dim msg As agsXMPP.protocol.client.Message = New agsXMPP.protocol.client.Message
            msg.Type = MessageType.chat
            msg.To = m_Jid
            msg.Body = rtfSend.Text
            _connection.Send(msg)
            OutgoingMessage(msg)
            rtfSend.Text = ""
        End Sub

        Private Sub MessageCallback(ByVal sender As Object, ByVal msg As agsXMPP.protocol.client.Message, ByVal data As Object)
            If InvokeRequired Then
                BeginInvoke(CType(AddressOf MessageCallback, agsXMPP.MessageCB), New Object() {sender, msg, data})
                Return
            End If

            If Not msg.Body Is Nothing Then
                '// we received a norml message
                IncomingMessage(msg)
            Else
                '// a message with no body. Probalbly a message event or chat state notification
            End If

        End Sub

        Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        End Sub
    End Class
End Namespace
