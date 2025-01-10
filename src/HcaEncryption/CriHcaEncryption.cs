using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VGAudio.Codecs.CriHca;
using VGAudio.Utilities;

/*
 * Original File can be found at: https://github.com/Thealexbarney/VGAudio/blob/master/src/VGAudio/Containers/Hca/HcaWriter.cs
 * Since the latest release of VGAudio doesnt include Hca Encryption, i have to manually encrypt it myself
 */

namespace tja2fumen.HcaEncryption
{
    public static partial class CriHcaEncryption
    {
        private const int FramesToTest = 10;

        private static Crc16 Crc { get; } = new Crc16(0x8005);

        public static void Crypt(HcaInfo hca, byte[][] audio, CriHcaKey key, bool doDecrypt)
        {
            for (int frame = 0; frame < hca.FrameCount; frame++)
            {
                CryptFrame(hca, audio[frame], key, doDecrypt);
            }
        }

        public static void CryptFrame(HcaInfo hca, byte[] audio, CriHcaKey key, bool doDecrypt)
        {
            byte[] substitutionTable = doDecrypt ? key.DecryptionTable : key.EncryptionTable;

            for (int b = 0; b < hca.FrameSize - 2; b++)
            {
                audio[b] = substitutionTable[audio[b]];
            }

            ushort crc = Crc.Compute(audio, hca.FrameSize - 2);
            audio[hca.FrameSize - 2] = (byte)(crc >> 8);
            audio[hca.FrameSize - 1] = (byte)crc;
        }

        private static int FindFirstNonEmptyFrame(byte[][] frames)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                if (!FrameEmpty(frames[i]))
                {
                    return i;
                }
            }
            return 0;
        }

        private static bool FrameEmpty(byte[] frame)
        {
            for (int i = 2; i < frame.Length - 2; i++)
            {
                if (frame[i] != 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
