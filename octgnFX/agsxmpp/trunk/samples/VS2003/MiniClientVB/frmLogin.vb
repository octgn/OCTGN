Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms

Imports agsXMPP
Imports agsXMPP.Xml
Imports agsXMPP.Xml.Dom

Namespace MiniClient

	Public Class frmLogin
	Inherits System.Windows.Forms.Form
		Private components As System.ComponentModel.Container = Nothing
		Private _connection As XmppClientConnection
		Private label3 As System.Windows.Forms.Label
		Private txtJid As System.Windows.Forms.TextBox
		Private label1 As System.Windows.Forms.Label
		Private label6 As System.Windows.Forms.Label
		Private label5 As System.Windows.Forms.Label
        Private WithEvents cmdLogin As System.Windows.Forms.Button
		Private numPriority As System.Windows.Forms.NumericUpDown
		Private label2 As System.Windows.Forms.Label
        Private WithEvents chkSSL As System.Windows.Forms.CheckBox
        Private WithEvents cmdCancel As System.Windows.Forms.Button
		Private label4 As System.Windows.Forms.Label
		Private txtPassword As System.Windows.Forms.TextBox
		Private txtResource As System.Windows.Forms.TextBox
		Private txtPort As System.Windows.Forms.TextBox
		
		Public Sub New(ByVal con As XmppClientConnection)
			InitializeComponent
			LoadSettings
			Me.DialogResult = DialogResult.Cancel
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
        Friend WithEvents chkRegisterAccount As System.Windows.Forms.CheckBox

		Private Sub InitializeComponent()
            Me.txtPort = New System.Windows.Forms.TextBox
            Me.txtResource = New System.Windows.Forms.TextBox
            Me.txtPassword = New System.Windows.Forms.TextBox
            Me.label4 = New System.Windows.Forms.Label
            Me.cmdCancel = New System.Windows.Forms.Button
            Me.chkSSL = New System.Windows.Forms.CheckBox
            Me.label2 = New System.Windows.Forms.Label
            Me.numPriority = New System.Windows.Forms.NumericUpDown
            Me.cmdLogin = New System.Windows.Forms.Button
            Me.label5 = New System.Windows.Forms.Label
            Me.label6 = New System.Windows.Forms.Label
            Me.label1 = New System.Windows.Forms.Label
            Me.txtJid = New System.Windows.Forms.TextBox
            Me.label3 = New System.Windows.Forms.Label
            Me.chkRegisterAccount = New System.Windows.Forms.CheckBox
            CType(Me.numPriority, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'txtPort
            '
            Me.txtPort.Location = New System.Drawing.Point(176, 88)
            Me.txtPort.MaxLength = 5
            Me.txtPort.Name = "txtPort"
            Me.txtPort.Size = New System.Drawing.Size(72, 20)
            Me.txtPort.TabIndex = 3
            Me.txtPort.Text = "5222"
            '
            'txtResource
            '
            Me.txtResource.Location = New System.Drawing.Point(80, 120)
            Me.txtResource.Name = "txtResource"
            Me.txtResource.Size = New System.Drawing.Size(168, 20)
            Me.txtResource.TabIndex = 4
            Me.txtResource.Text = "MiniClient"
            '
            'txtPassword
            '
            Me.txtPassword.Location = New System.Drawing.Point(80, 56)
            Me.txtPassword.Name = "txtPassword"
            Me.txtPassword.PasswordChar = Microsoft.VisualBasic.ChrW(42)
            Me.txtPassword.Size = New System.Drawing.Size(168, 20)
            Me.txtPassword.TabIndex = 1
            Me.txtPassword.Text = ""
            '
            'label4
            '
            Me.label4.Location = New System.Drawing.Point(8, 88)
            Me.label4.Name = "label4"
            Me.label4.Size = New System.Drawing.Size(56, 16)
            Me.label4.TabIndex = 8
            Me.label4.Text = "Priority:"
            '
            'cmdCancel
            '
            Me.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System
            Me.cmdCancel.Location = New System.Drawing.Point(32, 200)
            Me.cmdCancel.Name = "cmdCancel"
            Me.cmdCancel.Size = New System.Drawing.Size(88, 24)
            Me.cmdCancel.TabIndex = 7
            Me.cmdCancel.Text = "Cancel"
            '
            'chkSSL
            '
            Me.chkSSL.FlatStyle = System.Windows.Forms.FlatStyle.System
            Me.chkSSL.Location = New System.Drawing.Point(80, 152)
            Me.chkSSL.Name = "chkSSL"
            Me.chkSSL.Size = New System.Drawing.Size(160, 16)
            Me.chkSSL.TabIndex = 5
            Me.chkSSL.Text = "use SSL (old style SSL)"
            '
            'label2
            '
            Me.label2.Location = New System.Drawing.Point(80, 32)
            Me.label2.Name = "label2"
            Me.label2.Size = New System.Drawing.Size(176, 16)
            Me.label2.TabIndex = 4
            Me.label2.Text = "user@server.org"
            '
            'numPriority
            '
            Me.numPriority.Location = New System.Drawing.Point(80, 88)
            Me.numPriority.Name = "numPriority"
            Me.numPriority.Size = New System.Drawing.Size(40, 20)
            Me.numPriority.TabIndex = 2
            Me.numPriority.Value = New Decimal(New Integer() {10, 0, 0, 0})
            '
            'cmdLogin
            '
            Me.cmdLogin.FlatStyle = System.Windows.Forms.FlatStyle.System
            Me.cmdLogin.Location = New System.Drawing.Point(144, 200)
            Me.cmdLogin.Name = "cmdLogin"
            Me.cmdLogin.Size = New System.Drawing.Size(88, 24)
            Me.cmdLogin.TabIndex = 6
            Me.cmdLogin.Text = "Login"
            '
            'label5
            '
            Me.label5.Location = New System.Drawing.Point(8, 120)
            Me.label5.Name = "label5"
            Me.label5.Size = New System.Drawing.Size(64, 16)
            Me.label5.TabIndex = 9
            Me.label5.Text = "Resource:"
            '
            'label6
            '
            Me.label6.Location = New System.Drawing.Point(136, 88)
            Me.label6.Name = "label6"
            Me.label6.Size = New System.Drawing.Size(32, 16)
            Me.label6.TabIndex = 10
            Me.label6.Text = "Port:"
            '
            'label1
            '
            Me.label1.Location = New System.Drawing.Point(8, 8)
            Me.label1.Name = "label1"
            Me.label1.Size = New System.Drawing.Size(72, 16)
            Me.label1.TabIndex = 0
            Me.label1.Text = "Jabber ID:"
            '
            'txtJid
            '
            Me.txtJid.Location = New System.Drawing.Point(80, 8)
            Me.txtJid.Name = "txtJid"
            Me.txtJid.Size = New System.Drawing.Size(168, 20)
            Me.txtJid.TabIndex = 0
            Me.txtJid.Text = ""
            '
            'label3
            '
            Me.label3.Location = New System.Drawing.Point(8, 56)
            Me.label3.Name = "label3"
            Me.label3.Size = New System.Drawing.Size(64, 16)
            Me.label3.TabIndex = 6
            Me.label3.Text = "Password:"
            '
            'chkRegisterAccount
            '
            Me.chkRegisterAccount.FlatStyle = System.Windows.Forms.FlatStyle.System
            Me.chkRegisterAccount.Location = New System.Drawing.Point(80, 176)
            Me.chkRegisterAccount.Name = "chkRegisterAccount"
            Me.chkRegisterAccount.Size = New System.Drawing.Size(144, 16)
            Me.chkRegisterAccount.TabIndex = 11
            Me.chkRegisterAccount.Text = "register new account"
            '
            'frmLogin
            '
            Me.AcceptButton = Me.cmdLogin
            Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
            Me.ClientSize = New System.Drawing.Size(258, 232)
            Me.Controls.Add(Me.chkRegisterAccount)
            Me.Controls.Add(Me.chkSSL)
            Me.Controls.Add(Me.txtPort)
            Me.Controls.Add(Me.label6)
            Me.Controls.Add(Me.txtResource)
            Me.Controls.Add(Me.label5)
            Me.Controls.Add(Me.label4)
            Me.Controls.Add(Me.numPriority)
            Me.Controls.Add(Me.label3)
            Me.Controls.Add(Me.txtPassword)
            Me.Controls.Add(Me.label2)
            Me.Controls.Add(Me.cmdCancel)
            Me.Controls.Add(Me.cmdLogin)
            Me.Controls.Add(Me.txtJid)
            Me.Controls.Add(Me.label1)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
            Me.MaximizeBox = False
            Me.Name = "frmLogin"
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
            Me.Text = "Login form"
            CType(Me.numPriority, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)

        End Sub

       

        Private Sub cmdLogin_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdLogin.Click
            Dim jid As Jid = New Jid(txtJid.Text)
            _connection.Server = jid.Server
            _connection.Username = jid.User
            _connection.Password = txtPassword.Text
            _connection.Resource = txtResource.Text
            _connection.Priority = CType(numPriority.Value, Integer)
            _connection.Port = Integer.Parse(txtPort.Text)
            _connection.UseStartTLS = True
            _connection.UseSSL = chkSSL.Checked
            _connection.RegisterAccount = chkRegisterAccount.Checked
            _connection.UseCompression = False
            _connection.AutoResolveConnectServer = True

            Me.DialogResult = DialogResult.OK
            SaveSettings()
            Me.Close()
        End Sub

        Private ReadOnly Property SettingsFilename() As String
            Get
                Dim path As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.Location)
                Return path + "\Settings.xml"
            End Get
        End Property

        Private Sub LoadSettings()
            If System.IO.File.Exists(SettingsFilename) Then
                Dim doc As Document = New Document
                doc.LoadFile(SettingsFilename)
                Dim Login As Element = doc.RootElement.SelectSingleElement("Login")
                txtJid.Text = Login.GetTag("Jid")
                txtPassword.Text = Login.GetTag("Password")
                txtResource.Text = Login.GetTag("Resource")
                numPriority.Value = Login.GetTagInt("Priority")
                chkSSL.Checked = Login.GetTagBool("Ssl")
            End If
        End Sub

        Private Sub SaveSettings()
            Dim doc As Document = New Document
            Dim Settings As Element = New Element("Settings")
            Dim Login As Element = New Element("Login")
            Login.ChildNodes.Add(New Element("Jid", txtJid.Text))
            Login.ChildNodes.Add(New Element("Password", txtPassword.Text))
            Login.ChildNodes.Add(New Element("Resource", txtResource.Text))
            Login.ChildNodes.Add(New Element("Priority", numPriority.Value.ToString))
            Login.ChildNodes.Add(New Element("Port", txtPort.Text))
            Login.ChildNodes.Add(New Element("Ssl", chkSSL.Checked))
            doc.ChildNodes.Add(Settings)
            Settings.ChildNodes.Add(Login)
            doc.Save(SettingsFilename)
        End Sub

        Private Sub chkSSL_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSSL.CheckedChanged
            txtPort.Text = "5223"
        End Sub

        Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
            Me.DialogResult = DialogResult.Cancel
        End Sub
    End Class
End Namespace
