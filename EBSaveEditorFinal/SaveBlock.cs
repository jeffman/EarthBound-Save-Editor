using System;
using System.Collections.Generic;
using System.Text;

namespace EBSaveEditorFinal
{
    class SaveBlock
    {
        public string playerName = "";
        public string petName = "";
        public string favFood = "";
        public string favThing = "";

        public long moneyHand = 0;
        public long moneyATM = 0;

        public byte[] escargoItems = new byte[36];

        public int locX = 0;
        public int locY = 0;

        public byte[] partyMembers = new byte[6];
        public byte[] controlledMembers = new byte[4];
        public byte numPartyMembers = 0;
        public byte numControlledMembers = 0;

        public int exitMouseX = 0;
        public int exitMouseY = 0;

        public byte textSpeed = 0;
        public byte soundSetting = 0;

        public long inGameTimer = 0;
        public byte textPal = 0;

        public PartyMember[] chars = new PartyMember[4];

        public bool[] flags = new bool[1640];

        public SaveBlock()
        {
        }

        public void LoadBlock(byte[] buffer)
        {
            byte ch = 0;
            byte[] subbuffer;

            playerName = "";
            for (int i = 0; i < 24; i++)
            {
                ch = buffer[i + 0x2c];
                if (ch > 0)
                    playerName += EBMethods.GetChar(ch);
            }

            petName = "";
            for (int i = 0; i < 6; i++)
            {
                ch = buffer[i + 0x44];
                if (ch > 0)
                    petName += EBMethods.GetChar(ch);
            }

            favFood = "";
            for (int i = 0; i < 6; i++)
            {
                ch = buffer[i + 0x4a];
                if (ch > 0)
                    favFood += EBMethods.GetChar(ch);
            }

            favThing = "";
            for (int i = 0; i < 10; i++)
            {
                ch = buffer[i + 0x50];
                if (ch > 0)
                    favThing += EBMethods.GetChar(ch);
            }

            moneyHand = (buffer[0x5f] << 24) + (buffer[0x5e] << 16) + (buffer[0x5d] << 8) + buffer[0x5c];
            moneyATM = (buffer[0x63] << 24) + (buffer[0x62] << 16) + (buffer[0x61] << 8) + buffer[0x60];

            for (int i = 0; i < 36; i++)
                escargoItems[i] = buffer[i + 0x76];

            locX = (buffer[0xa3] << 8) + buffer[0xa2];
            locY = (buffer[0xa7] << 8) + buffer[0xa6];

            for (int i = 0; i < 6; i++)
                partyMembers[i] = buffer[i + 0xb6];

            for (int i = 0; i < 4; i++)
                controlledMembers[i] = buffer[i + 0xbc];

            numPartyMembers = buffer[0xce];
            numControlledMembers = buffer[0xcf];

            exitMouseX = (buffer[0xde] << 8) + buffer[0xdd];
            exitMouseY = (buffer[0xe0] << 8) + buffer[0xdf];

            textSpeed = buffer[0xe1];
            soundSetting = buffer[0xe2];
            inGameTimer = (buffer[0x1f7] << 24) + (buffer[0x1f6] << 16) + (buffer[0x1f5] << 8) + buffer[0x1f4];
            textPal = buffer[0x1f8];

            for (int i = 0; i < 4; i++)
            {
                subbuffer = new byte[0x5f];
                for (int j = 0; j < subbuffer.Length; j++)
                    subbuffer[j] = buffer[(i * 0x5f) + 0x1f9 + j];

                chars[i] = new PartyMember();
                chars[i].LoadBlock(subbuffer);
            }

            int flagCounter = 0;
            for (int i = 0; i < 205; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    flags[flagCounter++] = ((buffer[i + 0x433] & (1 << j)) == 0) ? false : true;
                }
            }
        }

        public void PutBlock(ref byte[] buffer)
        {
            EBMethods.PutString(playerName, ref buffer, 24, 0x2c);
            EBMethods.PutString(petName, ref buffer, 6, 0x44);
            EBMethods.PutString(favFood, ref buffer, 6, 0x4a);
            EBMethods.PutString(favThing, ref buffer, 10, 0x50);

            buffer[0x5c] = (byte)(moneyHand & 0xff);
            buffer[0x5d] = (byte)((moneyHand & 0xff00) >> 8);
            buffer[0x5e] = (byte)((moneyHand & 0xff0000) >> 16);
            buffer[0x5f] = (byte)((moneyHand & 0xff000000) >> 24);

            buffer[0x60] = (byte)(moneyATM & 0xff);
            buffer[0x61] = (byte)((moneyATM & 0xff00) >> 8);
            buffer[0x62] = (byte)((moneyATM & 0xff0000) >> 16);
            buffer[0x63] = (byte)((moneyATM & 0xff000000) >> 24);

            for (int i = 0; i < 36; i++)
                buffer[i + 0x76] = escargoItems[i];

            buffer[0xa2] = (byte)(locX & 0xff);
            buffer[0xa3] = (byte)((locX & 0xff00) >> 8);
            buffer[0xa6] = (byte)(locY & 0xff);
            buffer[0xa7] = (byte)((locY & 0xff00) >> 8);

            for (int i = 0; i < 6; i++)
                buffer[i + 0xb6] = partyMembers[i];

            for (int i = 0; i < 4; i++)
                buffer[i + 0xbc] = controlledMembers[i];

            buffer[0xce] = numPartyMembers;
            buffer[0xcf] = numControlledMembers;

            buffer[0xdd] = (byte)(exitMouseX & 0xff);
            buffer[0xde] = (byte)((exitMouseX & 0xff00) >> 8);
            buffer[0xdf] = (byte)(exitMouseY & 0xff);
            buffer[0xe0] = (byte)((exitMouseY & 0xff00) >> 8);

            buffer[0xe1] = textSpeed;
            buffer[0xe2] = soundSetting;

            buffer[0x1f4] = (byte)(inGameTimer & 0xff);
            buffer[0x1f5] = (byte)((inGameTimer & 0xff00) >> 8);
            buffer[0x1f6] = (byte)((inGameTimer & 0xff0000) >> 16);
            buffer[0x1f7] = (byte)((inGameTimer & 0xff000000) >> 24);

            buffer[0x1f8] = textPal;

            byte[] subbuffer;
            for (int i = 0; i < 4; i++)
            {
                subbuffer = new byte[0x5f];
                for (int j = 0; j < subbuffer.Length; j++)
                    subbuffer[j] = buffer[j + 0x1f9 + (i * 0x5f)];
                chars[i].PutBlock(ref subbuffer);
                for (int j = 0; j < subbuffer.Length; j++)
                    buffer[j + 0x1f9 + (i * 0x5f)] = subbuffer[j];
            }

            byte ch = 0;
            int flagCounter = 0;
            for (int i = 0; i < 205; i++)
            {
                ch = 0;
                for (int j = 0; j < 8; j++)
                {
                    ch += (byte)((flags[flagCounter++] ? 1 : 0) << j);
                }
                buffer[i + 0x433] = ch;
            }

        }

    }

}
