using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Octgn.Tools.SetBuilder
{
    public partial class frm_ItemEditor : Form
    {
        private readonly Form1 _parentForm;

        public frm_ItemEditor(Form1 parentForm)
        {
            InitializeComponent();
            _parentForm = parentForm;
        }

        public object item;
        public cardProperty cardprop;
        public cardAlternate cardalt;
        public cardObject card;
        public setObject set;
        public markerObject marker;
        public packObject pack;
        public optionObject option;
        public pickObject pick;

        private void frm_ItemEditor_Load(object sender, EventArgs e)
        {
            cmb_PropertyName.Items.Clear();
            foreach (string propname in _parentForm.propertyList)
            {
                cmb_PropertyName.Items.Add(propname);
            }
        }

        private void setupForm(string type)
        {
            ToolTip toolTips = new ToolTip();
            cmb_PropertyName.Visible = false;
            txt_Field1.Visible = false;
            txt_Field2.Visible = false;
            txt_Field3.Visible = false;
            txt_Field4.Visible = false;
            btn_GenerateGuid.Visible = false;
            if (type == "property")
            {
                cmb_PropertyName.Visible = true;
                txt_Field2.Visible = true;
                toolTips.SetToolTip(cmb_PropertyName, "Select the property");
                toolTips.SetToolTip(txt_Field2, "The value of the property");
            }
            else if (type == "alternate")
            {
                txt_Field1.Visible = true;
                txt_Field2.Visible = true;
                toolTips.SetToolTip(txt_Field1, "Card name for the alternate version.");
                toolTips.SetToolTip(txt_Field2, "Unique Identifier for the alternate version.");
            }
            else if (type == "option")
            {
                txt_Field1.Visible = true;
                toolTips.SetToolTip(txt_Field1, "Probability of option. 0.00 to 1.00");
            }
            else if (type == "pick")
            {
                cmb_PropertyName.Visible = true;
                txt_Field2.Visible = true;
                txt_Field3.Visible = true;
                toolTips.SetToolTip(txt_Field3, "Quantity of cards in pack or option.");
                toolTips.SetToolTip(cmb_PropertyName, "Property name to match against.");
                toolTips.SetToolTip(txt_Field2, "Property value to match.");
            }
            else if (type == "set")
            {
                txt_Field1.Visible = true;
                txt_Field2.Visible = true;
                txt_Field3.Visible = true;
                txt_Field4.Visible = true;
                btn_GenerateGuid.Visible = true;
                toolTips.SetToolTip(txt_Field1, "The name of the set");
                toolTips.SetToolTip(txt_Field2, "The unique GUID of the set");
                toolTips.SetToolTip(btn_GenerateGuid, "Generate a new unique GUID");
                toolTips.SetToolTip(txt_Field3, "The minimum game version for this set");
                toolTips.SetToolTip(txt_Field4, "The version number for this set");
            }
            else if ((type == "card") || (type == "marker") || type == "pack")
            {
                txt_Field1.Visible = true;
                txt_Field2.Visible = true;
                btn_GenerateGuid.Visible = true;
                toolTips.SetToolTip(txt_Field1, "Name");
                toolTips.SetToolTip(txt_Field2, "Unique GUIDt");
                toolTips.SetToolTip(btn_GenerateGuid, "Generate a new unique GUID");
            }
        }

        private void frm_ItemEditor_VisibleChanged(object sender, EventArgs e)
        {
            if (item is cardProperty)
            {
                setupForm("property");
                cardprop = (cardProperty)item;
                cmb_PropertyName.Text = cardprop.name;
                txt_Field2.Text = cardprop.value;
                this.ActiveControl = txt_Field2;
            }
            else if (item is cardAlternate)
            {
                setupForm("alternate");
                cardalt = (cardAlternate)item;
                txt_Field1.Text = cardalt.name;
                txt_Field2.Text = cardalt.type;
                this.ActiveControl = txt_Field1;
            }
            else if (item is cardObject)
            {
                setupForm("card");
                card = (cardObject)item;
                txt_Field1.Text = card.name;
                txt_Field2.Text = card.id.ToString();
                this.ActiveControl = txt_Field1;
            }
            else if (item is setObject)
            {
                setupForm("set");
                set = (setObject)item;
                txt_Field1.Text = set.name;
                txt_Field2.Text = set.id.ToString();
                txt_Field3.Text = set.gameVersion;
                txt_Field4.Text = set.version;
                this.ActiveControl = txt_Field1;
            }
            else if (item is markerObject)
            {
                setupForm("marker");
                marker = (markerObject)item;
                txt_Field1.Text = marker.name;
                txt_Field2.Text = marker.id.ToString();
                this.ActiveControl = txt_Field1;
            }
            else if (item is packObject)
            {
                setupForm("pack");
                pack = (packObject)item;
                txt_Field1.Text = pack.name;
                txt_Field2.Text = pack.id.ToString();
                this.ActiveControl = txt_Field1;
            }
            else if (item is optionObject)
            {
                setupForm("object");
                option = (optionObject)item;
                txt_Field1.Text = option.probability;
                this.ActiveControl = txt_Field1;
            }
            else if (item is pickObject)
            {
                setupForm("pick");
                pick = (pickObject)item;
                txt_Field3.Text = pick.qty;
                cmb_PropertyName.Text = pick.key;
                txt_Field2.Text = pick.value;
                this.ActiveControl = cmb_PropertyName;
            }
            else this.Close();
        }

        private void btn_Accept_Click(object sender, EventArgs e)
        {

            saveContents();
            Close();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frm_ItemEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            _parentForm.Enabled = true;
            if (item is gameItem) _parentForm.updateNode();
        }

        private void frm_ItemEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Right)       // Ctrl-Right Arrow
            {
                saveContents();
                Close();
                _parentForm.openNode("next","same");
                e.SuppressKeyPress = true;  // stops bing! also sets handeled which stop event bubbling
            }
            else if (e.Control && e.KeyCode == Keys.Left)
            {
                saveContents();
                Close();
                _parentForm.openNode("prev", "same");
                e.SuppressKeyPress = true;  // stops bing! also sets handeled which stop event bubbling
            }
            else if (e.Control && e.KeyCode == Keys.Up)
            {
                saveContents();
                Close();
                _parentForm.openNode("same", "prev");
                e.SuppressKeyPress = true;  // stops bing! also sets handeled which stop event bubbling
            }
            else if (e.Control && e.KeyCode == Keys.Down)
            {
                saveContents();
                Close();
                _parentForm.openNode("same", "next");
                e.SuppressKeyPress = true;  // stops bing! also sets handeled which stop event bubbling
            }
        }

        private void saveContents()
        {
            if (item is cardProperty)
            {
                if (cardprop.name != cmb_PropertyName.Text)
                {
                    cardprop.name = cmb_PropertyName.Text;
                    cardprop.changed = true;
                }
                if (cardprop.value != cmb_PropertyName.Text)
                {
                    cardprop.value = txt_Field2.Text;
                    cardprop.changed = true;
                }
            }
            else if (item is cardAlternate)
            {
                if (cardalt.name != txt_Field1.Text)
                {
                    cardalt.name = txt_Field1.Text;
                    cardalt.changed = true;
                }
                if (cardalt.type != txt_Field2.Text)
                {
                    cardalt.type = txt_Field2.Text;
                    cardalt.changed = true;
                }
            }
            else if (item is cardObject)
            {
                if (card.name != txt_Field1.Text)
                {
                    card.name = txt_Field1.Text;
                    card.changed = true;
                }
                Guid outGuid;
                if (Guid.TryParse(txt_Field2.Text, out outGuid))
                {
                    if (card.id != outGuid)
                    {
                        card.id = outGuid;
                        card.changed = true;
                    }
                }
            }
            else if (item is setObject)
            {
                if (set.name != txt_Field1.Text)
                {
                    set.name = txt_Field1.Text;
                    set.changed = true;
                }
                Guid outGuid;
                if (Guid.TryParse(txt_Field2.Text, out outGuid))
                {
                    if (set.id != outGuid)
                    {
                        set.id = outGuid;
                        set.changed = true;
                    }
                }
                if (set.gameVersion != txt_Field3.Text)
                {
                    set.gameVersion = txt_Field3.Text;
                    set.changed = true;
                }
                if (set.version != txt_Field4.Text)
                {
                    set.version = txt_Field4.Text;
                    set.changed = true;
                }
            }
            else if (item is markerObject)
            {
                if (marker.name != txt_Field1.Text)
                {
                    marker.name = txt_Field1.Text;
                    marker.changed = true;
                }
                Guid outGuid;
                if (Guid.TryParse(txt_Field2.Text, out outGuid))
                {
                    if (marker.id != outGuid)
                    {
                        marker.id = outGuid;
                        marker.changed = true;
                    }
                }

            }
            else if (item is packObject)
            {
                if (pack.name != txt_Field1.Text)
                {
                    pack.name = txt_Field1.Text;
                    pack.changed = true;
                }
                Guid outGuid;
                if (Guid.TryParse(txt_Field2.Text, out outGuid))
                {
                    if (pack.id != outGuid)
                    {
                        pack.id = outGuid;
                        pack.changed = true;
                    }
                }
            }
            else if (item is optionObject)
            {
                if (option.probability != txt_Field1.Text)
                {
                    option.probability = txt_Field1.Text;
                    option.changed = true;
                }
            }
            else if (item is pickObject)
            {
                if (pick.qty != txt_Field3.Text)
                {
                    pick.qty = txt_Field3.Text;
                    pick.changed = true;
                }
                if (pick.key != cmb_PropertyName.Text)
                {
                    pick.key = cmb_PropertyName.Text;
                    pick.changed = true;
                }
                if (pick.value != txt_Field2.Text)
                {
                    pick.value = txt_Field2.Text;
                    pick.changed = true;
                }
            }
        }

        private void btn_GenerateGuid_Click(object sender, EventArgs e)
        {
            if (txt_Field2.Visible)
            {
                Guid tmpGuid; 
                if (Guid.TryParse(txt_Field2.Text, out tmpGuid))
                {
                    DialogResult res = MessageBox.Show("You already have a valid ID, are you sure you want to change it?", "Change GUID?", MessageBoxButtons.YesNo);
                    if (res == DialogResult.Yes) tmpGuid = Guid.NewGuid();
                }
                else tmpGuid = Guid.NewGuid();
                txt_Field2.Text = tmpGuid.ToString();
            }
        }
    }
}
