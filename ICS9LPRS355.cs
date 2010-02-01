namespace SetFSB {
    class ICS9LPRS355 : PllBase, Pll {

        private const ushort SMBUS_DEVICE = 0x69;
        private const byte CMD = 0x00;
        private const byte BYTECOUNT = 22;
        private const byte CONTROLBYTE1 = 13;
        private const byte CONTROLBYTE2 = 14;

        public ICS9LPRS355(smBus smb) {
            this.smb = smb;
            ControlBytes.Add( new ControlByte(112, 0x88, 0x71));
            ControlBytes.Add( new ControlByte(117, 0x88, 0x87));
            ControlBytes.Add( new ControlByte(125, 0x88, 0xA3));
            ControlBytes.Add( new ControlByte(133, 0x88, 0xBF));
            ControlBytes.Add( new ControlByte(142, 0x88, 0xDB));
            ControlBytes.Add( new ControlByte(150, 0x88, 0xF7));
            ControlBytes.Add( new ControlByte(158, 0x48, 0x13));
            ControlBytes.Add( new ControlByte(167, 0x48, 0x2F));
            ControlBytes.Add( new ControlByte(175, 0x48, 0x4B));
            ControlBytes.Add( new ControlByte(183, 0x48, 0x67));
        
        }

        public int SetFSB(int fsb) {
            byte[] PllControlByteBlock = { 0x47, 0x85, 0xF0, 0x63, 0xFF, 0xF0, 0x30, 0x11, 0xD0, 0x25, 0x69, 0x80, 0x0D, 0xCF, 0xED, 0xEF, 0x2F, 0xFF, 0x70, 0xF2, 0x23, 0x03 };

            if (fsb < 0) return -1;

            foreach (var cb in ControlBytes) {
                if (cb.fsb == fsb) {
                    PllControlByteBlock[CONTROLBYTE1] = cb.Byte1;
                    PllControlByteBlock[CONTROLBYTE2] = cb.Byte2;
                    break;
                }
            }

            return smb.smbus_write_block_data(SMBUS_DEVICE, CMD, BYTECOUNT, PllControlByteBlock);
        }

        public int GetFSB() {
            /* Empty 22 byte buffer */
            byte[] buf = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            int res = smb.smbus_read_block_data(SMBUS_DEVICE, CMD, buf);

            if (res < 0) return -1;

            foreach (var cb in ControlBytes)
                if (cb.Byte1 == buf[CONTROLBYTE1] && cb.Byte2 == buf[CONTROLBYTE2]) return cb.fsb;

            return -1;
        }


    }
}
