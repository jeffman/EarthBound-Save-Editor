using System;
using System.Collections.Generic;
using System.Text;

namespace EBSaveEditorFinal
{
    class PartyMember
    {
        public string name = "";
        public byte level = 0;
        public long exp = 0;
        public int maxHP = 0;
        public int maxPP = 0;
        public byte[] statsBefore = new byte[7];
        public byte[] statsAfter = new byte[7];
        public byte[] items = new byte[14];
        public byte[] equip = new byte[4];
        public int currHP = 0;
        public int rollHP = 0;
        public int currPP = 0;
        public int rollPP = 0;

        public PartyMember()
        {
        }

        public void LoadBlock(byte[] buffer)
        {
            byte ch = 0;

            name = "";
            for (int i = 0; i < 5; i++)
            {
                ch = buffer[i];
                if (ch > 0)
                    name += EBMethods.GetChar(ch);
            }

            level = buffer[5];
            exp = (buffer[9] << 24) + (buffer[8] << 16) + (buffer[7] << 8) + buffer[6];
            maxHP = (buffer[11] << 8) + buffer[10];
            maxPP = (buffer[13] << 8) + buffer[12];

            for (int i = 0; i < 7; i++)
            {
                statsAfter[i] = buffer[21 + i];
                statsBefore[i] = buffer[28 + i];
            }

            for (int i = 0; i < 14; i++)
                items[i] = buffer[i + 35];

            for (int i = 0; i < 4; i++)
                equip[i] = buffer[i + 49];

            currHP = (buffer[70] << 8) + buffer[69];
            rollHP = (buffer[72] << 8) + buffer[71];
            currPP = (buffer[76] << 8) + buffer[75];
            rollPP = (buffer[78] << 8) + buffer[77];

        }

        public void PutBlock(ref byte[] buffer)
        {
            EBMethods.PutString(name, ref buffer, 5, 0);

            buffer[5] = level;

            buffer[6] = (byte)(exp & 0xff);
            buffer[7] = (byte)((exp & 0xff00) >> 8);
            buffer[8] = (byte)((exp & 0xff0000) >> 16);
            buffer[9] = (byte)((exp & 0xff000000) >> 24);

            buffer[10] = (byte)(maxHP & 0xff);
            buffer[11] = (byte)((maxHP & 0xff00) >> 8);
            buffer[12] = (byte)(maxPP & 0xff);
            buffer[13] = (byte)((maxPP & 0xff00) >> 8);

            for (int i = 0; i < 7; i++)
            {
                buffer[21 + i] = statsAfter[i];
                buffer[28 + i] = statsBefore[i];
            }

            for (int i = 0; i < 14; i++)
                buffer[i + 35] = items[i];

            for (int i = 0; i < 4; i++)
                buffer[i + 49] = equip[i];

            buffer[69] = (byte)(currHP & 0xff);
            buffer[70] = (byte)((currHP & 0xff00) >> 8);
            buffer[71] = (byte)(rollHP & 0xff);
            buffer[72] = (byte)((rollHP & 0xff00) >> 8);

            buffer[75] = (byte)(currPP & 0xff);
            buffer[76] = (byte)((currPP & 0xff00) >> 8);
            buffer[77] = (byte)(rollPP & 0xff);
            buffer[78] = (byte)((rollPP & 0xff00) >> 8);
        }
    }
}
