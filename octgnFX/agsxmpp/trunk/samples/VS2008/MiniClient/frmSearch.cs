using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using agsXMPP;
using agsXMPP.protocol;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.search;

namespace MiniClient
{
    public partial class frmSearch : Form
    {
        private XmppClientConnection m_XmppCon;        
        private string _IdFieldRequest = null;
        private string _IdSearchRequest = null;
        private bool _IsOldSearch = false;

        private DataTable _dataTable = new DataTable();


        public frmSearch(XmppClientConnection con)
        {
            InitializeComponent();

            m_XmppCon = con;
            // Fill combo with search services
            foreach (Jid jid in Util.Services.Search)
            {
                cboServices.Items.Add(jid.Bare);
            }

            dataGridView1.DataSource = _dataTable;
            dataGridView1.RowTemplate.Height = 20;
            dataGridView1.RowHeadersWidth = 24;
            dataGridView1.MultiSelect = false;
        }

        private void cboServices_SelectedValueChanged(object sender, EventArgs e)
        {
            RequestSearchFields();
        }

        private void cboServices_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                string newService = cboServices.Text;
                cboServices.SelectedIndex = cboServices.Items.Add(newService);
            }
        }

        #region << Request Search Fields >>
        private void RequestSearchFields()
        {
            //Example 1. Requesting Search Fields

            //<iq type='get'
            //    from='romeo@montague.net/home'
            //    to='characters.shakespeare.lit'
            //    id='search1'
            //    xml:lang='en'>
            //  <query xmlns='jabber:iq:search'/>
            //</iq>

            // Disable the Search Button until we habe a Form to submit
            cmdSearch.Enabled = false;

            SearchIq siq = new SearchIq();
            siq.Type = agsXMPP.protocol.client.IqType.get;
            siq.To = new Jid(cboServices.SelectedItem.ToString());

            // dicard the pending requests if there is one
            if (_IdFieldRequest != null)
                m_XmppCon.IqGrabber.Remove(_IdFieldRequest);

            // and cache the id of the new pending request
            _IdFieldRequest = siq.Id;

            // finally send the query
            m_XmppCon.IqGrabber.SendIq(siq, new IqCB(OnSearchFieldsResult), null);
        }

        private void OnSearchFieldsResult(object sender, IQ iq, object data)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new IqCB(OnSearchFieldsResult), new object[] { sender, iq, data });
                return;
            }

            if (iq.Type == IqType.result)
            {
                if (iq.Query is Search)
                {
                    Search search = iq.Query as Search;
                    if (search.Data != null)
                    {
                        xDataControl.CreateForm(search.Data);
                        // Enable the Search Button                        
                    }
                    else
                    {
                        // no xdata form returned from the search service.
                        // TODO should we add the old jabber search without xdata too? I don't think so.
                        //toolStripLabelForm.Text = String.Format("{0} returned no data form", iq.From.ToString());                        
                        xDataControl.CreateForm(search);
                        _IsOldSearch = true;
                    }
                    xDataControl.From = iq.From;
                    cmdSearch.Enabled = true;

                    toolStripStatusLabel1.Text = String.Format("search form from {0}", iq.From.ToString());
                }
            }
            else if (iq.Type == IqType.error)
            {
                toolStripStatusLabel1.Text = String.Format("{0} returned an error", iq.From.ToString());
            }
        }
        #endregion

        #region << Submit search request >>
        private void cmdSearch_Click(object sender, EventArgs e)
        {
            /*
            
             Example 8. Entity Submits Search Form

             <iq type='set'
                 from='juliet@capulet.com/balcony'
                 to='characters.shakespeare.lit'
                 id='search4'
                 xml:lang='en'>
               <query xmlns='jabber:iq:search'>
                 <x xmlns='jabber:x:data' type='submit'>
                   <field type='hidden' var='FORM_TYPE'>
                     <value>jabber:iq:search</value>
                   </field>
                   <field var='gender'>
                     <value>male</value>
                   </field>
                 </x>
               </query>
             </iq>
             */

            IQ siq = null;
            if (!_IsOldSearch)
            {
                // Validate the Form before we submit it
                if (xDataControl.Validate())
                {
                    siq = new SearchIq();
                    //siq.To = xDataControl.From;
                    //siq.Type = IqType.set;
                    ((SearchIq)siq).Query.Data = xDataControl.CreateResponse();
                }
            }
            else
            {
                siq = new IQ();
                siq.GenerateId();
                siq.Query = xDataControl.CreateSearchResponse();
            }

            siq.To = xDataControl.From;
            siq.Type = IqType.set;
            // dicard the pending requests if there is one
            if (_IdSearchRequest != null)
                m_XmppCon.IqGrabber.Remove(_IdSearchRequest);

            // and cache the id of the new pending request
            _IdSearchRequest = siq.Id;

            // finally send the query
            m_XmppCon.IqGrabber.SendIq(siq, new IqCB(OnSearchResult), null);
        }

        private void OnSearchResult(object sender, IQ iq, object data)
        {
            // We will upate the GUI from here, so invoke
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new IqCB(OnSearchResult), new object[] { sender, iq, data });
                return;
            }

            /*
             Example 9. Service Returns Search Results

            <iq type='result'
                from='characters.shakespeare.lit'
                to='juliet@capulet.com/balcony'
                id='search4'
                xml:lang='en'>
              <query xmlns='jabber:iq:search'>
                <x xmlns='jabber:x:data' type='result'>
                  <field type='hidden' var='FORM_TYPE'>
                    <value>jabber:iq:search</value>
                  </field>
                  <reported>
                    <field var='first' label='First Name'/>
                    <field var='last' label='Last Name'/>
                    <field var='jid' label='Jabber ID'/>
                    <field var='gender' label='Gender'/>
                  </reported>
                  <item>
                    <field var='first'><value>Benvolio</value></field>
                    <field var='last'><value>Montague</value></field>
                    <field var='jid'><value>benvolio@montague.net</value></field>
                    <field var='gender'><value>male</value></field>
                  </item>
                  <item>
                    <field var='first'><value>Romeo</value></field>
                    <field var='last'><value>Montague</value></field>
                    <field var='jid'><value>romeo@montague.net</value></field>
                    <field var='gender'><value>male</value></field>
                  </item>
                </x>
              </query>
            </iq>            
            */

            if (iq.Type == IqType.result)
            {
                if (iq.Query is Search)
                {
                    agsXMPP.protocol.x.data.Data xdata = ((Search)iq.Query).Data;
                    if (xdata != null)
                    {
                        ShowData(xdata);
                    }
                    else
                    {
                        ShowData(iq.Query as Search);
                    }
                }
            }
            else
            {
                ClearGridAndDataTable();
                toolStripStatusLabel1.Text = "an error occured in the search request";
            }
        }

        /// <summary>
        /// Clear the DataTable and the Grid headers
        /// </summary>
        private void ClearGridAndDataTable()
        {
            dataGridView1.SuspendLayout();

            _dataTable.Rows.Clear();
            _dataTable.Columns.Clear();

            dataGridView1.Columns.Clear();

            dataGridView1.ResumeLayout();
        }

        /// <summary>
        /// Shows the search result in the UI
        /// </summary>
        /// <param name="xdata"></param>
        private void ShowData(agsXMPP.protocol.x.data.Data xdata)
        {
            lock (this)
            {
                //ClearGridAndDataTable();

                dataGridView1.SuspendLayout();

                _dataTable.Rows.Clear();
                _dataTable.Columns.Clear();
                dataGridView1.Columns.Clear();

                agsXMPP.protocol.x.data.Reported reported = xdata.Reported;
                if (reported != null)
                {
                    foreach (agsXMPP.protocol.x.data.Field f in reported.GetFields())
                    {
                        // Create header
                        DataGridViewTextBoxColumn header = new DataGridViewTextBoxColumn();
                        header.DataPropertyName = f.Var;
                        header.HeaderText = f.Label;
                        header.Name = f.Var;

                        dataGridView1.Columns.Add(header);

                        // Create dataTable Col
                        _dataTable.Columns.Add(f.Var, typeof(string));
                    }
                }

                agsXMPP.protocol.x.data.Item[] items = xdata.GetItems();
                foreach (agsXMPP.protocol.x.data.Item item in items)
                {
                    DataRow dataRow = _dataTable.Rows.Add();
                    foreach (agsXMPP.protocol.x.data.Field field in item.GetFields())
                    {
                        dataRow[field.Var] = field.GetValue();
                    }
                }

                if (_dataTable.Rows.Count == 0)
                    toolStripStatusLabel1.Text = "no items found";
                else
                    toolStripStatusLabel1.Text = String.Format("{0} items found", _dataTable.Rows.Count.ToString());

                dataGridView1.ResumeLayout();
            }
        }

        private void AddColumnHeader(string name, string label)
        {
            // Create header
            DataGridViewTextBoxColumn header = new DataGridViewTextBoxColumn();
            header.DataPropertyName = name;
            header.HeaderText = label;
            header.Name = name;

            dataGridView1.Columns.Add(header);
            _dataTable.Columns.Add(name, typeof(string));
        }

        private void ShowData(Search search)
        {
            lock (this)
            {
                //ClearGridAndDataTable();

                dataGridView1.SuspendLayout();

                _dataTable.Rows.Clear();
                _dataTable.Columns.Clear();
                dataGridView1.Columns.Clear();


                // Create headers                
                AddColumnHeader("jid", "Jid");
                AddColumnHeader("last", "Lastname");
                AddColumnHeader("first", "Firstname");
                AddColumnHeader("nick", "Nickname");
                AddColumnHeader("email", "Email");

                SearchItem[] items = search.GetItems();
                foreach (SearchItem item in items)
                {
                    DataRow dataRow = _dataTable.Rows.Add();

                    if (item.Jid != null)
                        dataRow["jid"] = item.Jid;

                    if (item.Lastname != null)
                        dataRow["last"] = item.Lastname;

                    if (item.Firstname != null)
                        dataRow["first"] = item.Firstname;

                    if (item.Nickname != null)
                        dataRow["nick"] = item.Nickname;

                    if (item.Email != null)
                        dataRow["email"] = item.Email;
                }

                if (_dataTable.Rows.Count == 0)
                    toolStripStatusLabel1.Text = "no items found";
                else
                    toolStripStatusLabel1.Text = String.Format("{0} items found", _dataTable.Rows.Count.ToString());

                dataGridView1.ResumeLayout();
            }
        }

        #endregion



        #region << context menu events >>
        private void tsi_Click(object sender, EventArgs e)
        {

            if (dataGridView1.CurrentRow.Index >= 0)
            {
                int jidCol = GetJidCol();
                GetNickCol();

                if (jidCol != -1)
                {
                    Jid jid = new Jid(dataGridView1.CurrentRow.Cells[jidCol].Value.ToString());
                    if (sender == tsiAddContact)
                    {
                        string nick = null;
                        int nickCol = GetNickCol();
                        if (nickCol != -1)
                            nick = dataGridView1.CurrentRow.Cells[nickCol].Value.ToString();

                        frmAddRoster frmAdd = new frmAddRoster(jid, nick, m_XmppCon);
                        frmAdd.Show();
                    }
                    else if (sender == tsiVcard)
                    {
                        frmVcard frmCard = new frmVcard(jid, m_XmppCon);
                        frmCard.Show();
                    }
                }
            }

            //}
        }

        private int GetNamedCol(string[] arr)
        {
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                int idx = Array.IndexOf(arr, col.Name.ToLower());
                if (idx >= 0)
                    return col.Index;
            }
            return -1;
        }
        private int GetNickCol()
        {
            string[] find = new string[] { "name", "nickname", "nick" };

            return GetNamedCol(find);
        }

        private int GetJidCol()
        {
            string[] find = new string[] { "jid" };

            return GetNamedCol(find);
        }
        #endregion


        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int rowindex = dataGridView1.HitTest(e.X, e.Y).RowIndex;
                if (rowindex >= 0)
                    dataGridView1.CurrentCell = dataGridView1.Rows[rowindex].Cells[0];
            }
        }





    }
}