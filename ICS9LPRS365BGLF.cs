using System.Collections.Generic;

namespace SetFSB{
    internal class ICS9LPRS365BGLF : PllBase, Pll{
      

        private const ushort SMBUS_DEVICE = 0x69;
        private const byte CMD = 0x00;
        private const byte BYTECOUNT = 22;
        private const byte CONTROLBYTE1 = 13;
        private const byte CONTROLBYTE2 = 14;

        
/* Table containing three values
 * FSB, CONTROLBYTE, CONTROLBYTE1 
 * Gathered by setting FSB using setfsb, then reviewing the Diagnosis
 * screen to identify what data changed
 */


        public ICS9LPRS365BGLF(smBus smb){
            this.smb = smb;
            ControlBytes.Add(new ControlByte(200, 0x48, 0x2F));
            ControlBytes.Add(new ControlByte(222, 0x48, 0x4B));
            ControlBytes.Add(new ControlByte(233, 0x48, 0x4B)); 
            ControlBytes.Add(new ControlByte(240, 0x48, 0x67));
        }

        

        //ICS9LPRS365BGLF_
        public int SetFSB(int fsb){
            byte[] PllControlByteBlock = {0x47, 0x85, 0xF0, 0x63, 0xFF, 0xF0, 0x30, 0x11, 0xD0, 0x25, 0x69, 0x80, 0x0D, 0xCF, 0xED, 0xEF, 0x2F, 0xFF, 0x70, 0xF2, 0x23, 0x03};

            foreach (var cb in ControlBytes){
                if (cb.fsb == fsb){
                    PllControlByteBlock[CONTROLBYTE1] = cb.Byte1;
                    PllControlByteBlock[CONTROLBYTE2] = cb.Byte2;
                    break;
                }
            }

            return smb.smbus_write_block_data(SMBUS_DEVICE, CMD, BYTECOUNT, PllControlByteBlock);
        }

        public int GetFSB(){
            /* Empty 22 byte buffer */
            byte[] buf = {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};

            int res = smb.smbus_read_block_data(SMBUS_DEVICE, CMD, buf);

            if (res < 0) return -1;

            foreach (var cb in ControlBytes)
                if (cb.Byte1 == buf[CONTROLBYTE1] && cb.Byte2 == buf[CONTROLBYTE2]) return cb.fsb;

            return -1;
        }
    }
}