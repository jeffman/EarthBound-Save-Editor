using System;
using System.Collections.Generic;
using System.Text;

namespace EBSaveEditorFinal
{
    class EBMethods
    {
        public static char GetChar(byte ch)
        {
            if (ch < 0x30) return '\u0000';
            return (char)(ch - 0x30);
        }

        public static byte PutChar(char ch)
        {
            return (byte)((byte)ch + 0x30);
        }

        public static void PutString(string s, ref byte[] buffer, byte length, byte offset)
        {
            char[] c = s.ToCharArray();
            for (int i = 0; i < Math.Min(length, c.Length); i++)
                buffer[i + offset] = PutChar(c[i]);
            for (int i = Math.Min(length, c.Length); i < length; i++)
                buffer[i + offset] = 0;
        }
    }
}
