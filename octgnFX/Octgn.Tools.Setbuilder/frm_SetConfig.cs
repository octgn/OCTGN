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
    public partial class frm_SetConfig : Form
    {
        private readonly Form1 _parentForm;
        public frm_SetConfig(Form1 parentForm)
        {
            InitializeComponent();
            _parentForm = parentForm;
        }

        private void frm_SetConfig_Load(object sender, EventArgs e)
        {
            this.chk_SaveAll.Checked = _parentForm.config.saveAllProperties;
            if (this.chk_SaveAll.Checked) checkList.Enabled = false;
            this.checkList.Items.Clear();
            foreach (gameProperty prop in _parentForm.config.propertyList)
            {
                int index = checkList.Items.Add(prop.property);
                checkList.SetItemChecked(index, prop.alwaysSave);
            }
        }

        private void frm_SetConfig_FormClosed(object sender, FormClosedEventArgs e)
        {
            for (int i = 0; i < checkList.Items.Count; i++)
            {
                _parentForm.config.SetProperty(checkList.Items[i].ToString(), checkList.GetItemChecked(i));
            }
            _parentForm.Enabled = true;
            _parentForm.configfile.Save("settings.xml");
        }

        private void chk_SaveAll_CheckedChanged(object sender, EventArgs e)
        {
            _parentForm.config.saveAllProperties = chk_SaveAll.Checked;
            if (chk_SaveAll.Checked) checkList.Enabled = false;
            else checkList.Enabled = true;
        }

    }

}
