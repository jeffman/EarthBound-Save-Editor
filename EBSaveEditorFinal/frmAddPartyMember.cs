using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EBSaveEditorFinal
{
    public partial class frmAddPartyMember : Form
    {
        public int partyMemberResult = 0;

        public frmAddPartyMember(string[] memberNames)
        {
            InitializeComponent();
            cboPartyMember.Items.Clear();
            for (int i = 0; i < memberNames.Length; i++)
                cboPartyMember.Items.Add(memberNames[i]);
            cboPartyMember.SelectedIndex = 0;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            partyMemberResult = cboPartyMember.SelectedIndex;
            this.Close();
        }

        private void cboPartyMember_SelectedIndexChanged(object sender, EventArgs e)
        {
            partyMemberResult = cboPartyMember.SelectedIndex;
        }
    }
}