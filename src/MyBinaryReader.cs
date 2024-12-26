using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tja2fumen
{
    internal class MyBinaryReader : BinaryReader
    {
        internal MyBinaryReader(System.IO.Stream stream) : base(stream)
        {
        }
        internal bool isLittleEndian;

        public override int ReadInt32()
        {
            bool shouldReverse = (this.isLittleEndian != BitConverter.IsLittleEndian);
            var data = base.ReadBytes(4);
            if (shouldReverse)
            {
                Array.Reverse(data);
            }
            return BitConverter.ToInt32(data, 0);
        }

        public override UInt32 ReadUInt32()
        {
            bool shouldReverse = (this.isLittleEndian != BitConverter.IsLittleEndian);
            var data = base.ReadBytes(4);
            if (shouldReverse)
            {
                Array.Reverse(data);
            }
            return BitConverter.ToUInt32(data, 0);
        }

        public override Int16 ReadInt16()
        {
            bool shouldReverse = (this.isLittleEndian != BitConverter.IsLittleEndian);
            var data = base.ReadBytes(2);
            if (shouldReverse)
            {
                Array.Reverse(data);
            }
            return BitConverter.ToInt16(data, 0);
        }

        public override UInt16 ReadUInt16()
        {
            bool shouldReverse = (this.isLittleEndian != BitConverter.IsLittleEndian);
            var data = base.ReadBytes(2);
            if (shouldReverse)
            {
                Array.Reverse(data);
            }
            return BitConverter.ToUInt16(data, 0);
        }
    }
}
