/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 *																					 *
 * Copyright (c) 2003-2006 by AG-Software											 *
 * All Rights Reserved.																 *
 *																					 *
 * You should have received a copy of the AG-Software Shared Source License			 *
 * along with this library; if not, email gnauck@ag-software.de to request a copy.   *
 *																					 *
 * For general enquiries, email gnauck@ag-software.de or visit our website at:		 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace agsXMPP.ui.roster
{
    /// <summary>
    /// Tooltip form for Roster Tooltips. The .NET Tooltips are nice, but sealed and there is no way to set the 
    /// startup location. So we created our own simple tooltip class.
    /// </summary>
    public partial class Tooltip : Form
    {
        #region << Win32 API >>
        [DllImport("user32.dll")]
        protected static extern bool ShowWindow(IntPtr hWnd, Int32 flags);
        [DllImport("user32.dll")]
        protected static extern bool SetWindowPos(IntPtr hWnd, Int32 hWndInsertAfter, Int32 X, Int32 Y, Int32 cx, Int32 cy, uint uFlags);
        
        // SetWindowPos()
        protected const Int32 HWND_TOPMOST      = -1;
        protected const Int32 SWP_NOACTIVATE    = 0x0010;
        // ShowWindow()
        protected const Int32 SW_SHOWNOACTIVATE = 4;
        #endregion

        private int m_InitialDelay  = 2000;
        private int m_AutoPopDelay  = 2000 * 5;

        public Tooltip()
        {                        
            InitializeComponent();
        }
       
        // The new .NET 2.0 stuff doesnt work as expected, so go back to old Win32 API
        //protected override bool ShowWithoutActivation
        //{
        //    get
        //    {
        //        return true;
        //    }
        //}

        public bool Active
        {
            set
            {
                if (value)
                {
                    timerActivate.Interval = m_InitialDelay;
                    timerActivate.Enabled = true;
                }
                else
                {
                    this.Hide();
                    timerDelay.Enabled = false;
                    timerActivate.Enabled = false;
                }
            }            
        }       

        public int AutoPopDelay
        {
            get { return m_AutoPopDelay; }
            set { m_AutoPopDelay = value; }
        }

        public string Tooltiptext
        {
            set { lblText.Text = value; }
            get { return lblText.Text; }
        }       

        public int InitialDelay
        {
            get { return m_InitialDelay; }
            set { m_InitialDelay = value; }
        }

        private void timerActivate_Tick(object sender, EventArgs e)
        {            
            timerActivate.Enabled = false;

            Point point = Cursor.Position;
            point.Y += 20;

            // Use unmanaged ShowWindow() and SetWindowPos() instead of the managed Show() to display the window - this method will display
            // the window TopMost, but without stealing focus (namely the SW_SHOWNOACTIVATE and SWP_NOACTIVATE flags)
            SetWindowPos(this.Handle, HWND_TOPMOST, point.X, point.Y, lblText.Width, lblText.Height, SWP_NOACTIVATE);
            ShowWindow(Handle, SW_SHOWNOACTIVATE);            
           
            //Show();            
            //this.Location = Pos;
            //this.Size = lblText.Size;
            
            timerDelay.Interval = m_AutoPopDelay;
            timerDelay.Enabled = true;            
        }

        private void timerDelay_Tick(object sender, EventArgs e)
        { 
            timerDelay.Enabled = false;
            this.Hide();
        }          
    }
}