using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace EBSaveEditorFinal
{
    public partial class frmMain : Form
    {
        SaveBlock SAVE = new SaveBlock();
        private byte[] filebuffer;
        private bool SRMmode = false;
        private bool fileOpen = false;
        private int SRMslot = -1;
        private bool updating = false;
        private int curItemSel = -1;
        private int curEscSel = -1;
        TextBox[] txtStatBefore = new TextBox[7];
        TextBox[] txtStatAfter = new TextBox[7];
        Label[] lblItem = new Label[14];
        Label[] lblEscargo = new Label[12];
        PictureBox[] picEquip = new PictureBox[4];
        ComboBox[] cboEquip = new ComboBox[4];
        string[] itemNames;
        string[] partyMembers;
        string[] eventDescs = new string[1640];
        private byte[,] bestItems = { { 177, 181, 196, 26, 62, 72, 86, 1, 130, 130, 97, 97, 97, 97 }, { 34, 62, 72, 80, 3, 3, 130, 130, 97, 97, 148, 148, 148, 148 }, { 48, 62, 72, 86, 134, 146, 146, 146, 146, 146, 146, 146, 146, 146 }, { 35, 63, 73, 87, 185, 208, 100, 100, 100, 100, 246, 246, 130, 130 } };
        private byte[,] bestEquips ={ { 4, 5, 6, 7 }, { 1, 2, 3, 4 }, { 1, 2, 3, 4 }, { 1, 2, 3, 4 } };

        public frmMain()
        {
            InitializeComponent();
            this.Text = Application.ProductName;
            MakeEBStrings();
            LoadEventDescs();
            this.FormClosing += frmMain_FormClosing;
            cboName.GotFocus += cboName_Click;
            cboName2.GotFocus += cboName2_Click;
            picItemBG.MouseClick += picItemBG_MouseClick;
            picEscargo.MouseClick += picEscargo_MouseClick;
            //tab1.Leave += cboName_Click;
            //tab2.Leave += cboName_Click;
            //tab3.Leave += cboName_Click;
            //tab4.Leave += cboName_Click;

            for (int i = 0; i < 7; i++)
            {
                txtStatBefore[i] = new TextBox();
                txtStatBefore[i].Visible = true;
                txtStatBefore[i].Size = new Size(46, 20);
                txtStatBefore[i].Left = 303;
                txtStatBefore[i].Top = 19 + (i * 26);
                grpParty.Controls.Add(txtStatBefore[i]);

                txtStatAfter[i] = new TextBox();
                txtStatAfter[i].Visible = true;
                txtStatAfter[i].Size = new Size(46, 20);
                txtStatAfter[i].Left = 430;
                txtStatAfter[i].Top = 19 + (i * 26);
                grpParty.Controls.Add(txtStatAfter[i]);
            }

            for (int i = 0; i < 4; i++)
            {
                picEquip[i] = new PictureBox();
                picEquip[i].Visible = false;
                picEquip[i].Image = Properties.Resources.equip;
                picEquip[i].Size = new Size(7, 7);

                cboEquip[i] = new ComboBox();
                cboEquip[i].Size = new Size(203, 21);
                cboEquip[i].DropDownStyle = ComboBoxStyle.DropDownList;
                cboEquip[i].SelectedIndexChanged += cboEquip_SelectedIndexChanged;
                cboEquip[i].Items.Add("(None)");
                for (int j = 0; j < 14; j++)
                    cboEquip[i].Items.Add(" ");
                grpItems.Controls.Add(picEquip[i]);
                grpItems.Controls.Add(cboEquip[i]);
            }
            cboEquip[0].Left = lblWeapon.Left + lblWeapon.Width + 12;
            cboEquip[0].Top = lblWeapon.Top - 3;
            cboEquip[1].Left = lblBody.Left + lblBody.Width + 12;
            cboEquip[1].Top = lblBody.Top - 3;
            cboEquip[2].Left = lblArms.Left + lblArms.Width + 12;
            cboEquip[2].Top = lblArms.Top - 3;
            cboEquip[3].Left = lblOther.Left + lblOther.Width + 12;
            cboEquip[3].Top = lblOther.Top - 3;

            picItemBG.Image = Properties.Resources.itembg;
            picEscargo.Image = Properties.Resources.escargobg;

            int flag = 0;
            for (int i = 0; i < 14; i++)
            {
                lblItem[i] = new Label();
                lblItem[i].Visible = true;
                lblItem[i].Text = ""; // "Text " + i.ToString();
                lblItem[i].ForeColor = Color.White;
                lblItem[i].BackColor = Color.FromArgb(16, 16, 16);
                lblItem[i].Left = picItemBG.Left + 13 + ((picItemBG.Width - 16) / 2 * flag);
                lblItem[i].Top = picItemBG.Top + 12 + ((picItemBG.Height - 16) / 7 * (i / 2));
                lblItem[i].Width = 100;
                lblItem[i].Height = 16;
                flag++;
                if (flag == 2)
                    flag = 0;
                grpItems.Controls.Add(lblItem[i]);
                lblItem[i].BringToFront();
                lblItem[i].Click += this.lblItem_Click;
            }

            for (int i = 0; i < 12; i++)
            {
                lblEscargo[i] = new Label();
                lblEscargo[i].Visible = true;
                lblEscargo[i].ForeColor = Color.White;
                lblEscargo[i].BackColor = Color.FromArgb(16, 16, 16);
                lblEscargo[i].Left = picEscargo.Left + 5;
                lblEscargo[i].Top = picEscargo.Top + 8 + ((picEscargo.Height - 12) / 12 * i);
                lblEscargo[i].Width = 114;
                lblEscargo[i].Height = 16;
                grpEscargo.Controls.Add(lblEscargo[i]);
                lblEscargo[i].BringToFront();
                lblEscargo[i].Click += this.lblEscargo_Click;
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CheckCancel())
                e.Cancel = true;
        }

        private bool CheckByteString(byte[] buffer, int length, string s)
        {
            string ss = "";
            for (int i = 0; i < length; i++)
                ss += (char)buffer[i];
            if (ss.Equals(s)) return true;
            return false;
        }

        private void LoadFile(string pathName)
        {
            FileStream f = File.Open(pathName, FileMode.Open);
            filebuffer = new byte[f.Length];
            f.Read(filebuffer, 0, filebuffer.Length);
            f.Close();

            if (pathName.Substring(pathName.Length - 3, 3).ToUpper().Equals("SRM") && (filebuffer.Length == 8192 || filebuffer.Length == 16384))
            {
                SRMmode = true;
                mnuFileSlot.Enabled = true;
                mnuSlot1.Checked = true;
                mnuSlot2.Checked = false;
                mnuSlot3.Checked = false;

                // Load SRM save
                SRMslot = 0;
                LoadSRMFile(filebuffer, SRMslot);

                // Update GUI
                UpdateGUI();

                fileOpen = true;
            }
            else if ((
                //(filebuffer.Length == 282459) ||
                //(filebuffer.Length == 274047) ||
                //(filebuffer.Length == 266879)
                //))
                CheckByteString(filebuffer, 0x15, "ZSNES Save State File")))
            {
                SRMmode = false;
                mnuFileSlot.Enabled = false;

                // Load ZS save
                LoadZSTFile(filebuffer);

                // Update GUI
                UpdateGUI();

                fileOpen = true;
            }
            else
            {
                MessageBox.Show("This isn't a valid save file!");
            }
        }

        private void SaveFile(string pathName)
        {
            ApplyChanges();

            if (SRMmode)
            {
                SaveSRMFile(ref filebuffer, SRMslot);
            }
            else
            {
                SaveZSTFile(ref filebuffer);
            }

            FileStream f = File.Open(pathName, FileMode.Create, FileAccess.Write);
            f.Write(filebuffer, 0, filebuffer.Length);
            f.Close();
        }

        private void LoadSRMFile(byte[] buffer, int fileSlot)
        {
            int fileStart = (fileSlot * 0xA00);
            byte[] subbuffer = new byte[0x500];
            for (int i = 0; i < subbuffer.Length; i++)
            {
                subbuffer[i] = buffer[i + fileStart];
            }

            SAVE = new SaveBlock();
            SAVE.LoadBlock(subbuffer);
        }

        private void LoadZSTFile(byte[] buffer)
        {
            int fileStart = 0xA3E8;
            byte[] subbuffer = new byte[0x500];
            for (int i = 0; i < subbuffer.Length; i++)
            {
                subbuffer[i] = buffer[i + fileStart];
            }

            SAVE = new SaveBlock();
            SAVE.LoadBlock(subbuffer);
        }

        private void SaveSRMFile(ref byte[] buffer, int fileSlot)
        {
            int fileStart = (fileSlot * 0xA00);
            byte[] subbuffer = new byte[0x500];
            for (int i = 0; i < subbuffer.Length; i++)
                subbuffer[i] = buffer[i + fileStart];
            SAVE.PutBlock(ref subbuffer);

            // Re-calculate the checksums
            int chk = 0;
            for (int i = 0x20; i < 0x4E0; i++)
            {
                chk += subbuffer[i];
                chk &= 0xffff;
            }

            // Complement
            int cmp = 0;
            int ch = 0;
            for (int i = 0x20; i < 0x4e0; i += 2)
            {
                ch = subbuffer[i] + (subbuffer[i + 1] << 8);
                cmp = ch ^ cmp;
            }
            cmp &= 0xffff;

            subbuffer[0x1c] = (byte)(chk & 0xff);
            subbuffer[0x1d] = (byte)((chk & 0xff00) >> 8);
            subbuffer[0x1e] = (byte)(cmp & 0xff);
            subbuffer[0x1f] = (byte)((cmp & 0xff00) >> 8);

            for (int i = 0; i < subbuffer.Length; i++)
            {
                buffer[i + fileStart] = subbuffer[i];
                buffer[i + fileStart + 0x500] = subbuffer[i];
            }

        }

        private void SaveZSTFile(ref byte[] buffer)
        {
            int fileStart = 0xA3E8;
            byte[] subbuffer = new byte[0x500];
            for (int i = 0; i < subbuffer.Length; i++)
                subbuffer[i] = buffer[i + fileStart];
            SAVE.PutBlock(ref subbuffer);

            subbuffer[0x1c] = 0; //(byte)(chk & 0xff);
            subbuffer[0x1d] = 0; //(byte)((chk & 0xff00) >> 8);
            subbuffer[0x1e] = 0; //(byte)(cmp & 0xff);
            subbuffer[0x1f] = 0; //(byte)((cmp & 0xff00) >> 8);

            for (int i = 0; i < subbuffer.Length; i++)
            {
                buffer[i + fileStart] = subbuffer[i];
            }
        }

        private void ApplyChanges()
        {
            updating = true;

            int charIndex = cboName.SelectedIndex;

            SAVE.chars[charIndex].name = txtName.Text;
            cboName.Items[charIndex] = txtName.Text;
            cboName2.Items[charIndex] = txtName.Text;
            RefreshPartyMembers();
            SAVE.chars[charIndex].currHP = int.Parse(txtCurHP.Text);
            SAVE.chars[charIndex].currPP = int.Parse(txtCurPP.Text);
            SAVE.chars[charIndex].rollHP = int.Parse(txtRolHP.Text);
            SAVE.chars[charIndex].rollPP = int.Parse(txtRolPP.Text);
            SAVE.chars[charIndex].maxHP = int.Parse(txtMaxHP.Text);
            SAVE.chars[charIndex].maxPP = int.Parse(txtMaxPP.Text);
            SAVE.chars[charIndex].level = byte.Parse(txtLevel.Text);
            SAVE.chars[charIndex].exp = long.Parse(txtEXP.Text);
            for (int i = 0; i < 7; i++)
            {
                SAVE.chars[charIndex].statsBefore[i] = byte.Parse(txtStatBefore[i].Text);
                SAVE.chars[charIndex].statsAfter[i] = byte.Parse(txtStatAfter[i].Text);
            }

            SAVE.moneyHand = long.Parse(txtMoney.Text);
            SAVE.moneyATM = long.Parse(txtATM.Text);
            SAVE.playerName = txtPlayerName.Text;
            SAVE.petName = txtPetName.Text;
            SAVE.favFood = txtFavFood.Text;
            SAVE.favThing = txtFavThing.Text;

            for (int i = 0; i < 1024; i++)
                SAVE.flags[i] = lst.Items[i].Checked;

            SAVE.locX = int.Parse(txtLocX.Text);
            SAVE.locY = int.Parse(txtLocY.Text);
            SAVE.exitMouseX = int.Parse(txtExitX.Text);
            SAVE.exitMouseY = int.Parse(txtExitY.Text);

            SAVE.textSpeed = (byte)(cboTextSpeed.SelectedIndex + 1);
            SAVE.soundSetting = (byte)(cboSoundMode.SelectedIndex + 1);
            SAVE.textPal = (byte)(cboWindowFlavour.SelectedIndex + 1);

            /*byte p = 0;
            byte c = 0;
            for (int i = 0; i < 4; i++)
                SAVE.controlledMembers[i] = 0;
            for (int i = 0; i < 6; i++)
            {
                if (SAVE.partyMembers[i] >= 1 && SAVE.partyMembers[i] <= 4)
                {
                    SAVE.controlledMembers[p] = (byte)(SAVE.partyMembers[i] - 1);
                    p++;
                    if (p == 4)
                    {
                        i = 6;
                    }
                }
            }
            SAVE.numControlledMembers = p;
            for (int i = 0; i < 6; i++)
            {
                if (SAVE.partyMembers[i] > 0)
                    c++;
            }
            SAVE.numPartyMembers = c;*/

            updating = false;

            cboName_SelectedIndexChanged(null, new EventArgs());
        }

        private void LoadEventDescs()
        {
            string[] e = new string[1];
            e[0] = "" + (char)13 + (char)10;
            string[] ch = Properties.Resources.EventDescs.Split(e, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < 1640; i++)
            {
                eventDescs[i] = "";
                for (int j = 0; j < ch.Length; j++)
                {
                    if (ch[j].Substring(1, 4).ToUpper().Equals((i + 1).ToString("X4")))
                    {
                        eventDescs[i] = ch[j].Substring(6);
                        j = ch.Length;
                    }
                }
            }
        }

        private void MakeEBStrings()
        {
            partyMembers = new string[]{"(None)", "Ness", "Paula", "Jeff", "Poo",
                "Pokey", "Picky", "King", "Tony", "Bubble Monkey", "Dungeon Man",
                "Flying Man 1", "Flying Man 2", "Flying Man 3", "Flying Man 4", "Flying Man 5",
                "Teddy Bear", "Super Plush Bear"};

            itemNames = new string[] { "Null", "Franklin badge", "Teddy bear", 
                "Super plush bear", "Broken machine", "Broken gadget", 
                "Broken air gun", "Broken spray can", "Broken laser", 
                "Broken iron", "Broken pipe", "Broken cannon", "Broken tube", 
                "Broken bazooka", "Broken trumpet", "Broken harmonica", 
                "Broken antenna", "Cracked bat", "Tee ball bat", 
                "Sand lot bat", "Minor league bat", "Mr. Baseball bat", 
                "Big league bat", "Hall of fame bat", "Magicant bat", 
                "Legendary bat", "Gutsy bat", "Casey bat", "Fry pan", 
                "Thick fry pan", "Deluxe fry pan", "Chef's fry pan", 
                "French fry pan", "Magic fry pan", "Holy fry pan", 
                "Sword of kings", "Pop gun", "Stun gun", "Toy air gun", 
                "Magnum air gun", "Zip gun", "Laser gun", "Hyper beam", 
                "Crusher beam", "Spectrum beam", "Death ray", "Baddest beam", 
                "Moon beam gun", "Gaia beam", "Yo-yo", "Slingshot", 
                "Bionic slingshot", "Trick yo-yo", "Combat yo-yo", 
                "Travel charm", "Great charm", "Crystal charm", 
                "Rabbit's foot", "Flame pendant", "Rain pendant", 
                "Night pendant", "Sea pendant", "Star pendant", 
                "Cloak of kings", "Cheap bracelet", "Copper bracelet", 
                "Silver bracelet", "Gold bracelet", "Platinum band", 
                "Diamond band", "Pixie's bracelet", "Cherub's band", 
                "Goddess band", "Bracer of kings", "Baseball cap", 
                "Holmes hat", "Mr. Baseball cap", "Hard hat", "Ribbon", 
                "Red ribbon", "Goddess ribbon", "Coin of slumber", 
                "Coin of defense", "Lucky coin", "Talisman coin", 
                "Shiny coin", "Souvenir coin", "Diadem of kings", 
                "Cookie", "Bag of fries", "Hamburger", "Boiled egg", 
                "Fresh Egg", "Picnic lunch", "Pasta di Summers", "Pizza", 
                "Chef's special", "Large pizza", "PSI caramel", 
                "Magic truffle", "Brain food lunch", "Rock candy", 
                "Croissant", "Bread roll", "Pak of bubble gum", 
                "Jar of Fly Honey", "Can of fruit juice", "Royal iced tea", 
                "Protein drink", "Kraken soup", "Bottle of water", 
                "Cold remedy", "Vial of serum", "IQ capsule", 
                "Guts capsule", "Speed capsule", "Vital capsule", 
                "Luck capsule", "Ketchup packet", "Sugar packet", 
                "Tin of Cocoa", "Carton of cream", "Sprig of parsley", 
                "Jar of hot sauce", "Salt packet", "Backstage pass", 
                "Jar of delisauce", "Wet towel", "Refreshing herb", 
                "Secret herb", "Horn of life", "Counter-PSI unit", 
                "Shield killer", "Bazooka", "Heavy bazooka", "HP-sucker", 
                "Hungry HP-sucker", "Xterminator spray", "Slime generator", 
                "Yogurt dispenser", "Ruler", "Snake bag", "Mummy wrap", 
                "Protractor", "Bottle rocket", "Big bottle rocket", 
                "Multi bottle rocket", "Bomb", "Super bomb", 
                "Insecticide spray", "Rust promoter", "Rust promoter DX", 
                "Pair of dirty socks", "Stag beetle", "Toothbrush", 
                "Handbag strap", "Pharaoh's curse", "Defense shower", 
                "Letter from mom", "Sudden guts pill", "Bag of Dragonite", 
                "Defense spray", "Piggy nose", "For Sale sign", 
                "Shyness book", "Picture postcard", "King banana", 
                "Letter from Tony", "Chick", "Chicken", "Key to the shack", 
                "Key to the cabin", "Bad key machine", "Temporary goods", 
                "Zombie paper", "Hawk eye", "Bicycle", "ATM card", 
                "Show ticket", "Letter from kids", "Wad of bills", 
                "Receiver phone", "Diamond", "Signed banana", 
                "Pencil eraser", "Hieroglyph copy", "Meteotite", 
                "Contact lens", "Hand-Aid", "Trout yogurt", "Banana", 
                "Calorie stick", "Key to the tower", "Meteorite piece", 
                "Earth pendant", "Neutralizer", "Sound Stone", "Exit mouse", 
                "Gelato de resort", "Snake", "Viper", "Brain stone", 
                "Town map", "Video relaxant", "Suporma", "Key to the locker", 
                "Insignificant item", "Magic tart", "Tiny ruby", 
                "Monkey's love", "Eraser eraser", "Tendakraut", 
                "T-rex's bat", "Big league bat", "Ultimate bat", 
                "Double beam", "Platinum band", "Diamond band", 
                "Defense ribbon", "Talisman ribbon", "Saturn ribbon", 
                "Coin of silence", "Charm coin", "Cup of noodles", 
                "Skip sandwich", "Skip sandwich DX", "Lucky sandwich", 
                "Lucky sandwich", "Lucky sandwich", "Lucky sandwich", 
                "Lucky sandwich", "Lucky sandwich", "Cup of coffee", 
                "Double burger", "Peanut cheese bar", "Piggy jelly", 
                "Bowl of rice gruel", "Bean croquette", "Molokheiya soup", 
                "Plain roll", "Kabob", "Plain yogurt", "Beef jerky", 
                "Mammoth burger", "Spicy jerky", "Luxury jerky", 
                "Bottle of DXwater", "Magic pudding", "Non-stick frypan", 
                "Mr. Saturn coin", "Meteornium", "Popsicle", 
                "Cup of Lifenoodles", "Carrot key", "-", "-" };
        }

        private void UpdateGUI()
        {
            updating = true;

            tabControl.Enabled = true;
            mnuApplyChanges.Enabled = true;
            mnuSaveROM.Enabled = true;
            mnuSaveAs.Enabled = true;
            mnuCloseROM.Enabled = true;
            btnApply.Enabled = true;
            btnSave.Enabled = true;

            cboName.Items.Clear();
            cboName2.Items.Clear();
            for (int i = 0; i < 4; i++)
            {
                cboName.Items.Add(SAVE.chars[i].name);
                cboName2.Items.Add(SAVE.chars[i].name);
            }
            cboName.SelectedIndex = 0;
            cboName2.SelectedIndex = 0;

            cboItems.Items.Clear();
            cboEscItems.Items.Clear();
            for (int i = 0; i < itemNames.Length; i++)
            {
                cboItems.Items.Add(itemNames[i] + " [" + i.ToString("X2") + "]");
                cboEscItems.Items.Add(itemNames[i] + " [" + i.ToString("X2") + "]");
            }

            txtMoney.Text = SAVE.moneyHand.ToString();
            txtATM.Text = SAVE.moneyATM.ToString();
            txtPlayerName.Text = SAVE.playerName;
            txtPetName.Text = SAVE.petName;
            txtFavFood.Text = SAVE.favFood;
            txtFavThing.Text = SAVE.favThing;

            cboEscargo.SelectedIndex = 0;

            if (lst.Items.Count == 0)
            {
                for (int i = 0; i < 1024; i++)
                {
                    lst.Items.Add(new ListViewItem("$" + (i + 1).ToString("X4")));
                    lst.Items[i].SubItems.Add("");
                }
            }
            for (int i = 0; i < 1024; i++)
            {
                lst.Items[i].Checked = SAVE.flags[i]; //SRM.GetFlag(fileSlot, i);
                lst.Items[i].SubItems[1] = new ListViewItem.ListViewSubItem(lst.Items[i], eventDescs[i]);
            }

            txtLocX.Text = SAVE.locX.ToString();
            txtLocY.Text = SAVE.locY.ToString();
            txtExitX.Text = SAVE.exitMouseX.ToString();
            txtExitY.Text = SAVE.exitMouseY.ToString();

            cboTextSpeed.SelectedIndex = SAVE.textSpeed - 1;
            cboSoundMode.SelectedIndex = SAVE.soundSetting - 1;
            cboWindowFlavour.SelectedIndex = SAVE.textPal - 1;

            RefreshPartyMembers();

            updating = false;

            cboName_SelectedIndexChanged(null, new EventArgs());
            cboEscargo_SelectedIndexChanged(null, new EventArgs());

        }

        private void mnuOpenROM_Click(object sender, EventArgs e)
        {
            if (CheckCancel()) return;

            DialogResult res = dlgOpen.ShowDialog();
            if (res == DialogResult.OK)
            {
                LoadFile(dlgOpen.FileName);
                this.Text = Application.ProductName + " - " + dlgOpen.FileName.Substring(dlgOpen.FileName.LastIndexOf("\\") + 1);
            }
        }

        private void RefreshPartyMembers()
        {
            for (int i = 1; i < 5; i++)
            {
                partyMembers[i] = SAVE.chars[i - 1].name;
            }
            partyMembers[7] = SAVE.petName;

            lstPartyMembers.Items.Clear();
            for (int i = 0; i < 6; i++)
                lstPartyMembers.Items.Add(partyMembers[SAVE.partyMembers[i]]);
        }

        private bool CheckCancel()
        {
            if (!fileOpen) return false;

            DialogResult res = MessageBox.Show("Do you want to save?", "Save", MessageBoxButtons.YesNoCancel);
            if (res == DialogResult.Yes)
            {
                btnSave_Click(null, new EventArgs());
            }
            else if (res == DialogResult.Cancel)
            {
                return true;
            }
            return false;
        }

        private void lblItem_Click(object sender, EventArgs e)
        {
            int index = Array.IndexOf(lblItem, sender);

            if (curItemSel >= 0)
            {
                lblItem[curItemSel].BackColor = Color.Black;
            }
            curItemSel = index;
            lblItem[index].BackColor = Color.Blue;

            updating = true;
            cboItems.SelectedIndex = findItemIndex(SAVE.chars[cboName2.SelectedIndex].items[index]);
            updating = false;
        }

        private int findItemIndex(int i)
        {
            string s = "";
            for (int j = 0; j < cboItems.Items.Count; j++)
            {
                s = cboItems.Items[j].ToString();
                if (s.Substring(s.LastIndexOf('[') + 1, 2).Equals(i.ToString("X2")))
                    return j;
            }
            return -1;
        }

        private byte getItemIndex(string s)
        {
            return byte.Parse(s.Substring(s.LastIndexOf('[') + 1, 2), System.Globalization.NumberStyles.HexNumber);
        }

        private void lblEscargo_Click(object sender, EventArgs e)
        {
            int index = Array.IndexOf(lblEscargo, sender);

            if (curEscSel >= 0)
            {
                lblEscargo[curEscSel].BackColor = Color.Black;
            }
            curEscSel = index;
            lblEscargo[index].BackColor = Color.Blue;

            int escIndex = index + (cboEscargo.SelectedIndex * 12);

            updating = true;
            cboEscItems.SelectedIndex = findItemIndex(SAVE.escargoItems[escIndex]);
            updating = false;
        }

        private void cboName_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        private void cboName2_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        private void picItemBG_MouseClick(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;

            if ((x >= 214) && (x <= 223) && (y >= 0) && (y <= 8))
            {
                if (cboName2.SelectedIndex > 0)
                {
                    ApplyChanges();
                    cboName2.SelectedIndex--;
                }
            }

            else if ((x >= 224) && (x <= 233) && (y >= 0) && (y <= 8))
            {
                if (cboName2.SelectedIndex < 3)
                {
                    ApplyChanges();
                    cboName2.SelectedIndex++;
                }
            }
        }
        
        private void picEscargo_MouseClick(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;

            if ((x >= 90) && (x <= 99) && (y >= 0) && (y <= 8))
            {
                if (cboEscargo.SelectedIndex > 0)
                {
                    ApplyChanges();
                    cboEscargo.SelectedIndex--;
                }
            }

            else if ((x >= 100) && (x <= 109) && (y >= 0) && (y <= 8))
            {
                if (cboEscargo.SelectedIndex < 2)
                {
                    ApplyChanges();
                    cboEscargo.SelectedIndex++;
                }
            }
        }

        private void cboName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (updating) return;

            updating = true;
            int i = cboName.SelectedIndex;
            cboName2.SelectedIndex = i;
            updating = false;

            txtName.Text = SAVE.chars[i].name;
            txtLevel.Text = SAVE.chars[i].level.ToString();
            txtEXP.Text = SAVE.chars[i].exp.ToString();
            txtCurHP.Text = SAVE.chars[i].currHP.ToString();
            txtCurPP.Text = SAVE.chars[i].currPP.ToString();
            txtMaxHP.Text = SAVE.chars[i].maxHP.ToString();
            txtMaxPP.Text = SAVE.chars[i].maxPP.ToString();
            txtRolHP.Text = SAVE.chars[i].rollHP.ToString();
            txtRolPP.Text = SAVE.chars[i].rollPP.ToString();

            for (int j = 0; j < 7; j++)
            {
                txtStatAfter[j].Text = SAVE.chars[i].statsAfter[j].ToString();
                txtStatBefore[j].Text = SAVE.chars[i].statsBefore[j].ToString();
            }
            /*
            for (int j = 0; j < 14; j++)
                lblItem[j].Text = itemNames[SAVE.chars[i].items[j]];

            for (int j = 0; j < 4; j++)
            {
                if (SAVE.chars[i].equip[j] == 0)
                {
                    picEquip[j].Visible = false;
                }
                else
                {
                    picEquip[j].Visible = true;
                    picEquip[j].Left = lblItem[SAVE.chars[i].equip[j] - 1].Left - 8;
                    picEquip[j].Top = lblItem[SAVE.chars[i].equip[j] - 1].Top + 3;
                }

                updating = true;
                cboEquip[j].Items.Clear();
                cboEquip[j].Items.Add("(None)");
                for (int k = 0; k < 14; k++)
                {
                    cboEquip[j].Items.Add(itemNames[SAVE.chars[i].items[k]]);
                }
                cboEquip[j].SelectedIndex = SAVE.chars[i].equip[j];
                updating = false;

            }
            picItemBG.SendToBack();

            if (curItemSel >= 0)
                lblItem_Click(lblItem[curItemSel], new EventArgs());*/
            UpdateItemGUI(i);
        }

        private void cboName2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboName2.SelectedIndex == cboName.SelectedIndex) return;
            cboName.SelectedIndex = cboName2.SelectedIndex;
            cboName_SelectedIndexChanged(sender, e);
        }

        private void cboEscargo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (updating) return;

            int index = cboEscargo.SelectedIndex;

            for (int i = 0; i < 12; i++)
                lblEscargo[i].Text = itemNames[SAVE.escargoItems[i + (index * 12)]];

            if (curEscSel >= 0)
                lblEscargo_Click(lblEscargo[curEscSel], new EventArgs());
        }

        private void cboEscItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (curEscSel >= 0 && !updating)
            {
                byte itemIndex = getItemIndex(cboEscItems.Text); //int.Parse(cboEscItems.SelectedItem.ToString().Substring(cboEscItems.SelectedItem.ToString().Length - 4, 3));
                SAVE.escargoItems[curEscSel + (cboEscargo.SelectedIndex * 12)] = itemIndex; //SRM.PutEscargoItem(fileSlot, curEscSel + (cboEscargo.SelectedIndex * 12), (byte)itemIndex);
                lblEscargo[curEscSel].Text = itemNames[itemIndex];
            }
        }

        private void cboItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (curItemSel >= 0 && !updating)
            {
                byte itemIndex = getItemIndex(cboItems.Text); //int.Parse(cboItems.SelectedItem.ToString().Substring(cboItems.SelectedItem.ToString().Length - 4, 3));
                SAVE.chars[cboName.SelectedIndex].items[curItemSel] = itemIndex; //SRM.PutItem(fileSlot, cboName.SelectedIndex, curItemSel, (byte)itemIndex);
                lblItem[curItemSel].Text = itemNames[itemIndex]; //itemNames[SRM.GetItem(fileSlot, cboName.SelectedIndex, curItemSel)];
                updating = true;
                for (int i = 0; i < 4; i++)
                    cboEquip[i].Items[curItemSel + 1] = itemNames[itemIndex];
                updating = false;
            }
        }

        private void cboEquip_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (updating) return;

            int index = Array.IndexOf(cboEquip, sender);
            SAVE.chars[cboName.SelectedIndex].equip[index] = (byte)cboEquip[index].SelectedIndex;

            if (SAVE.chars[cboName.SelectedIndex].equip[index] == 0)
            {
                picEquip[index].Visible = false;
            }
            else
            {
                picEquip[index].Visible = true;
                picEquip[index].Left = lblItem[SAVE.chars[cboName.SelectedIndex].equip[index] - 1].Left - 8;
                picEquip[index].Top = lblItem[SAVE.chars[cboName.SelectedIndex].equip[index] - 1].Top + 3;
            }
            picItemBG.SendToBack();
        }

        private void mnuApplyChanges_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //DialogResult res = dlgSave.ShowDialog();
            //if (res == DialogResult.OK)
            //{
                SaveFile(dlgOpen.FileName);
            //}
        }

        private void mnuSlot1_Click(object sender, EventArgs e)
        {
            if (SRMslot != 0)
            {
                ApplyChanges();
                SaveSRMFile(ref filebuffer, SRMslot);
                SRMslot = 0;
                mnuSlot1.Checked = true;
                mnuSlot2.Checked = false;
                mnuSlot3.Checked = false;
                LoadSRMFile(filebuffer, SRMslot);
                UpdateGUI();
            }
        }

        private void mnuSlot2_Click(object sender, EventArgs e)
        {
            if (SRMslot != 1)
            {
                ApplyChanges();
                SaveSRMFile(ref filebuffer, SRMslot);
                SRMslot = 1;
                mnuSlot2.Checked = true;
                mnuSlot1.Checked = false;
                mnuSlot3.Checked = false;
                LoadSRMFile(filebuffer, SRMslot);
                UpdateGUI();
            }
        }

        private void mnuSlot3_Click(object sender, EventArgs e)
        {
            if (SRMslot != 2)
            {
                ApplyChanges();
                SaveSRMFile(ref filebuffer, SRMslot);
                SRMslot = 2;
                mnuSlot3.Checked = true;
                mnuSlot2.Checked = false;
                mnuSlot1.Checked = false;
                LoadSRMFile(filebuffer, SRMslot);
                UpdateGUI();
            }
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            if (CheckCancel()) return;
            Application.Exit();
        }

        private void mnuSaveROM_Click(object sender, EventArgs e)
        {
            btnSave_Click(null, new EventArgs());
        }

        private void mnuCloseROM_Click(object sender, EventArgs e)
        {
            if (CheckCancel()) return;

            tabControl.Enabled = false;
            mnuApplyChanges.Enabled = false;
            mnuSaveROM.Enabled = false;
            mnuCloseROM.Enabled = false;
            btnApply.Enabled = false;
            btnSave.Enabled = false;
            mnuSaveAs.Enabled = false;
            mnuFileSlot.Enabled = false;
            fileOpen = false;
        }

        private void mnuSaveAs_Click(object sender, EventArgs e)
        {
            DialogResult res = dlgSave.ShowDialog();
            if (res == DialogResult.OK)
            {
                SaveFile(dlgSave.FileName);
                dlgOpen.FileName = dlgSave.FileName;
                this.Text = Application.ProductName + " - " + dlgSave.FileName.Substring(dlgSave.FileName.LastIndexOf("\\") + 1);
            }
        }

        private void btnDelPartyMember_Click(object sender, EventArgs e)
        {
            if (lstPartyMembers.SelectedIndex < 0) return;
            int i = lstPartyMembers.SelectedIndex;
            if (i < 5)
            {
                for (int j = i; j < 5; j++)
                    SAVE.partyMembers[j] = SAVE.partyMembers[j + 1];
            }
            SAVE.partyMembers[5] = 0;
            RefreshPartyMembers();
        }

        private void btnAddPartyMember_Click(object sender, EventArgs e)
        {
            frmAddPartyMember f = new frmAddPartyMember(partyMembers);
            DialogResult res = f.ShowDialog();
            if (res == DialogResult.Cancel) return;

            for (int i = 0; i < 6; i++)
            {
                if (SAVE.partyMembers[i] == 0)
                {
                    SAVE.partyMembers[i] = (byte)f.partyMemberResult;
                    i = 6;
                }
            }
            RefreshPartyMembers();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Find the first null
            int charIndex = cboName2.SelectedIndex;
            byte itemIndex = getItemIndex(cboItems.Text);
            for (int i = 0; i < 14; i++)
            {
                if (SAVE.chars[charIndex].items[i] == 0)
                {
                    SAVE.chars[charIndex].items[i] = itemIndex;
                    lblItem[i].Text = itemNames[itemIndex]; //itemNames[SRM.GetItem(fileSlot, cboName.SelectedIndex, curItemSel)];
                    updating = true;
                    for (int j = 0; j < 4; j++)
                        cboEquip[j].Items[i + 1] = itemNames[itemIndex];
                    updating = false;
                    return;
                }
            }
        }

        private void btnBest_Click(object sender, EventArgs e)
        {
            int charIndex = cboName2.SelectedIndex;
            byte[] theItems = new byte[14];
            for (int i = 0; i < 14; i++)
                SAVE.chars[charIndex].items[i] = bestItems[charIndex, i];
            byte[] theEquips = new byte[4];
            for (int i = 0; i < 4; i++)
                SAVE.chars[charIndex].equip[i] = bestEquips[charIndex, i];

            UpdateItemGUI(charIndex);
        }

        private void UpdateItemGUI(int charIndex)
        {
            for (int j = 0; j < 14; j++)
                lblItem[j].Text = itemNames[SAVE.chars[charIndex].items[j]];

            for (int j = 0; j < 4; j++)
            {
                if (SAVE.chars[charIndex].equip[j] == 0)
                {
                    picEquip[j].Visible = false;
                }
                else
                {
                    picEquip[j].Visible = true;
                    picEquip[j].Left = lblItem[SAVE.chars[charIndex].equip[j] - 1].Left - 8;
                    picEquip[j].Top = lblItem[SAVE.chars[charIndex].equip[j] - 1].Top + 3;
                }

                updating = true;
                for (int k = 0; k < 14; k++)
                {
                    cboEquip[j].Items[k + 1] = itemNames[SAVE.chars[charIndex].items[k]]; ;
                }
                cboEquip[j].SelectedIndex = SAVE.chars[charIndex].equip[j];
                updating = false;

            }
            picItemBG.SendToBack();

            if (curItemSel >= 0)
                lblItem_Click(lblItem[curItemSel], new EventArgs());
        }

        private void btnRem_Click(object sender, EventArgs e)
        {
            if (curItemSel >= 0)
            {
                int charIndex = cboName2.SelectedIndex;
                for (int i = 0; i < 4; i++)
                {
                    if (SAVE.chars[charIndex].equip[i] == curItemSel + 1)
                        SAVE.chars[charIndex].equip[i] = 0;
                }
                for (int i = 0; i < 4; i++)
                {
                    if (SAVE.chars[charIndex].equip[i] - 1 > curItemSel)
                        SAVE.chars[charIndex].equip[i]--;
                }
                SAVE.chars[charIndex].items[curItemSel] = 0;
                if (curItemSel < 13)
                {
                    for (int i = curItemSel; i < 13; i++)
                        SAVE.chars[charIndex].items[i] = SAVE.chars[charIndex].items[i + 1];
                }
                SAVE.chars[charIndex].items[13] = 0;
                UpdateItemGUI(charIndex);
            }
        }

        private void btnRemAll_Click(object sender, EventArgs e)
        {
            int charIndex = cboName2.SelectedIndex;
            for (int i = 0; i < 14; i++)
                SAVE.chars[charIndex].items[i] = 0;
            for (int i = 0; i < 4; i++)
                SAVE.chars[charIndex].equip[i] = 0;
            UpdateItemGUI(charIndex);
        }

        private void btnNumeric_Click(object sender, EventArgs e)
        {
            byte tmp = 0;
            int charIndex=cboName2.SelectedIndex;
            for (int i = 0; i < 13; i++)
            {
                for (int j = i + 1; j < 14; j++)
                {
                    if (
                        (SAVE.chars[charIndex].items[i] == 0 ? 256 : SAVE.chars[charIndex].items[i])
                        > 
                        (SAVE.chars[charIndex].items[j] == 0 ? 256 : SAVE.chars[charIndex].items[j]))
                    {
                        tmp = SAVE.chars[charIndex].items[i];
                        SAVE.chars[charIndex].items[i] = SAVE.chars[charIndex].items[j];
                        SAVE.chars[charIndex].items[j] = tmp;

                        for (int k = 0; k < 4; k++)
                        {
                            if (SAVE.chars[charIndex].equip[k] == i + 1)
                            {
                                SAVE.chars[charIndex].equip[k] = (byte)(j + 1);
                            }
                            else if (SAVE.chars[charIndex].equip[k] == j + 1)
                            {
                                SAVE.chars[charIndex].equip[k] = (byte)(i + 1);
                            }
                        }
                    }
                }
            }
            UpdateItemGUI(charIndex);
        }

        private void btnAlpha_Click(object sender, EventArgs e)
        {
            byte tmp = 0;
            int charIndex = cboName2.SelectedIndex;
            for (int i = 0; i < 13; i++)
            {
                for (int j = i + 1; j < 14; j++)
                {
                    if ((
                    (itemNames[SAVE.chars[charIndex].items[i]].CompareTo(
                    itemNames[SAVE.chars[charIndex].items[j]]) > 0) ||
                    (SAVE.chars[charIndex].items[i] == 0)) &&
                    !(SAVE.chars[charIndex].items[j] == 0))
                    {
                        tmp = SAVE.chars[charIndex].items[i];
                        SAVE.chars[charIndex].items[i] = SAVE.chars[charIndex].items[j];
                        SAVE.chars[charIndex].items[j] = tmp;

                        for (int k = 0; k < 4; k++)
                        {
                            if (SAVE.chars[charIndex].equip[k] == i + 1)
                            {
                                SAVE.chars[charIndex].equip[k] = (byte)(j + 1);
                            }
                            else if (SAVE.chars[charIndex].equip[k] == j + 1)
                            {
                                SAVE.chars[charIndex].equip[k] = (byte)(i + 1);
                            }
                        }
                    }
                }
            }
            UpdateItemGUI(charIndex);
        }

    }
}