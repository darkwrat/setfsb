using System.Threading;
using OpenLibSys;

namespace SetFSB{
    public class smBus{
        private static Ols ols;
        public smBus (Ols openLibSys){
            ols = openLibSys;
        }


        private const byte STATUS_FLAGS = SMBHSTSTS_BYTE_DONE | SMBHSTSTS_FAILED | SMBHSTSTS_BUS_ERR | SMBHSTSTS_DEV_ERR | SMBHSTSTS_INTR;


        private const ushort SMBUS_IO_BASE = 0x400;

        private const ushort SMBHSTSTS = SMBUS_IO_BASE;
        private const ushort SMBHSTCNT = SMBUS_IO_BASE + 2;
        private const ushort SMBHSTCMD = SMBUS_IO_BASE + 3;
        private const ushort SMBHSTADD = SMBUS_IO_BASE + 4;
        private const ushort SMBHSTDAT0 = SMBUS_IO_BASE + 5;
/*
static private ushort  SMBHSTDAT1  = SMBUS_IO_BASE + 6;
*/
        private const ushort SMBBLKDAT = SMBUS_IO_BASE + 7;
//static private ushort  SMBPEC     =  SMBUS_IO_BASE + 8;
//static private ushort  SMBAUXSTS  =  SMBUS_IO_BASE + 12;
        private const ushort SMBAUXCTL = SMBUS_IO_BASE + 13;

/* Auxillary controlregister bits, ICH4+ only */
/*
static private uint  SMBAUXCTL_CRC  =   1;
*/
        private const uint SMBAUXCTL_E32B = 2;

/* I801 command constants */
/*
static private byte  I801_BYTE_DATA  =  0x8;
*/
        private const byte I801_BLOCK_DATA = 0x14;
        private const byte I801_BLOCK_LAST = 0x34;
        private const byte I801_START = 0x40;

/* I801 Hosts Status register bits */
        private const byte SMBHSTSTS_BYTE_DONE = 0x80;
        private const byte SMBHSTSTS_INUSE_STS = 0x40;
/*
static private byte  SMBHSTSTS_SMBALERT_STS  = 0x20;
*/
        private const byte SMBHSTSTS_FAILED = 0x10;
        private const byte SMBHSTSTS_BUS_ERR = 0x08;
        private const byte SMBHSTSTS_DEV_ERR = 0x04;
        private const byte SMBHSTSTS_INTR = 0x02;
        private const byte SMBHSTSTS_HOST_BUSY = 0x1;


        private const byte SMBHSTCNT_KILL = 2;

        private const uint MAX_TIMEOUT = 100;


       uint grub_pci_make_address (uint bus, uint device, uint function)
{
           return  (bus << 8) | (device << 3) | (function );

  //return (uint)((1 << 31) | (bus << 16) | (device << 11) | (function << 8) | (reg << 2));
}

        private const byte PCI_VENDOR_ID = 0x00;

         private const byte  PCI_DEVICE_ID   =        0x02;
 private const uint  PCI_BASE_ADDRESS_SPACE_IO = 0x01;
 private const byte  PCI_COMMAND           =     0x04 ;   /* 16 bits */
 private const ushort   PCI_COMMAND_IO       =      0x1  ;   /* Enable response in I/O space */

 private const byte  PCI_BASE_ADDRESS_4    =  0x20;


        private const uint INTEL_PCI_VENDOR_ID = 0x8086;
        private const int ICH_FUNC_DISABLE = 0x3418;   // offset for root complex base address
        private const uint SMBUS_DISABLE_BIT = 3;
        private const uint ICH8_PCI_ID = 0x283e;
        private const uint ICH7_PCI_ID = 0x27da;
        private const uint ICH9_PCI_ID = 0x2930;
            
        private const byte SMBHSTCFG = 0x40;
        private const byte SMBHSTCFG_HST_EN = 1;
        private const uint SMBAUXCTL_CRC = 1;

        public void enable_smbus()
{  
  // some BIOSes disable SMBus. Turn it back on, even though the ICH8 manual says
  // you shouldn't, maybe because they never saw the need and don't to proper reinitialization,
  // leaving the device in an inconsistent state?
  uint rcba = ols.ReadPciConfigDword (grub_pci_make_address(0, 0x1f, 0) , 0xf0);

  // if RCBA isn't memory mapped, there's little we can do - we could
  // set the bit that turns on the mapping, but that's chipset specific
  // and usually is locked (D_LCK bit set)
  if ((rcba & 1) != 0u)   // if mapped
  {
    rcba &= 0xffffc000;
    //grub_printf("rcba: %x\n", rcba);
   /* int func_disable = (int) (rcba + ICH_FUNC_DISABLE);
    func_disable &=  ~(1 <<  (int) SMBUS_DISABLE_BIT);    // enable SMBUS
    * does not work in c#  */
  }

  /* Set the SMBus device statically. */
  uint dev = grub_pci_make_address(0x0, 0x1f, 0x3);

  if (ols.ReadPciConfigWord (dev , PCI_VENDOR_ID) != INTEL_PCI_VENDOR_ID)
  {
    //grub_printf("unsupported SMBus controller\n");
    return;
  }
  /* Check to make sure we've got the right device. */
  uint pci_id = ols.ReadPciConfigWord(dev , PCI_DEVICE_ID);
  if (pci_id != ICH9_PCI_ID && pci_id != ICH8_PCI_ID && pci_id != ICH7_PCI_ID)
  {
    //grub_printf("unsupported SMBus controller\n");
    return;
  }
  
  /* Set SMBus I/O base.  grub_pci_write */
  ols.WritePciConfigDword (dev , PCI_BASE_ADDRESS_4, SMBUS_IO_BASE | PCI_BASE_ADDRESS_SPACE_IO);

  /* Set SMBus enable, disable I2C_EN, disable SMB_SMI_EN  grub_pci_write_byte*/
  ols.WritePciConfigByte (dev , SMBHSTCFG, SMBHSTCFG_HST_EN);

  /* Set SMBus I/O space enable.  grub_pci_write_word*/
  ols.WritePciConfigWord (dev , PCI_COMMAND, PCI_COMMAND_IO);
  
  /* Disable interrupt generation.  grub_outb*/
  ols.WriteIoPortByte (SMBHSTCNT,0);

  /* Clear any lingering errors, so transactions can run. grub_outb*/
  ols.WriteIoPortByte ( SMBHSTSTS,ols.ReadIoPortByte (SMBHSTSTS));

  // disable block mode & packet checking
  //ols.WriteIoPortByte (SMBAUXCTL,(byte) (ols.ReadIoPortByte(SMBAUXCTL) & ~(SMBAUXCTL_E32B | SMBAUXCTL_CRC)));
 
  // comment this out to use byte by byte block operations
  ols.WriteIoPortByte(SMBAUXCTL,(byte) (ols.ReadIoPortByte(SMBAUXCTL) | SMBAUXCTL_E32B));  
}


        public  int smbus_read_block_data(ushort device, byte command, byte[] values){
            int result;
            enable_smbus();

            if ((result = smbus_wait_until_ready()) < 0)
                return result;

            // are these 2 lines really needed? - not in Linux driver

            //ols.WriteIoPortByte(SMBHSTSTS, ols.ReadIoPortByte(SMBHSTSTS));
            //while ((ols.ReadIoPortByte(SMBHSTSTS) & SMBHSTSTS_INUSE_STS) == 0){
            //    Thread.Sleep(0);
            //}
           
            ols.WriteIoPortByte(SMBHSTADD, (byte) (((device & 0x7f) << 1) | 0x1));
            //ols.WriteIoPortByte(SMBHSTADD, (byte) ((device & 0x7f) << 1));
            ols.WriteIoPortByte(SMBHSTCMD, command);

            if ((ols.ReadIoPortByte(SMBAUXCTL) & SMBAUXCTL_E32B) != 0u)
                return i801_read_block_as_block(values);
            else
                return i801_read_block_byte_by_byte(values);
        }


        private static int i801_read_block_byte_by_byte(byte[] buf){
            int result, i, size = 32;
            if ((result = i801_check_pre()) < 0)
                return result;

            for (i = 0; i < size; ++i){
                byte smbcmd = ((i == (size - 1)) ? I801_BLOCK_LAST : I801_BLOCK_DATA);
                ols.WriteIoPortByte(SMBHSTCNT, smbcmd);
                if (i == 0)
                    ols.WriteIoPortByte(SMBHSTCNT, (byte) (ols.ReadIoPortByte(SMBHSTCNT) | I801_START));
                ushort tries = 0;
                ushort status;
                do{
                    Thread.Sleep(1);
                    status = ols.ReadIoPortByte(SMBHSTSTS);
                } while ((status & SMBHSTSTS_BYTE_DONE) == 0 && (tries++ < MAX_TIMEOUT));

                if ((result = i801_check_post(status, tries > MAX_TIMEOUT ? 1 : 0)) < 0)
                    return result;

                if (i == 0)
                    size = ols.ReadIoPortByte(SMBHSTDAT0);
                buf[i] = ols.ReadIoPortByte(SMBBLKDAT);
                ols.WriteIoPortByte(SMBHSTSTS, (byte) (SMBHSTSTS_BYTE_DONE | SMBHSTSTS_INTR));
            }
            return size;
        }


        private static int i801_read_block_as_block(byte[] buf){
            int i, status;
            ols.ReadIoPortByte(SMBHSTCNT); // reset data buffer index

            if ((status = i801_transaction(I801_BLOCK_DATA)) < 0)
                return status;
            int size = ols.ReadIoPortByte(SMBHSTDAT0);
            for (i = 0; i < size; ++i)
                buf[i] = ols.ReadIoPortByte(SMBBLKDAT);
            return size;
        }


        public  int smbus_write_block_data(ushort device, byte command, byte size, byte[] values){
            int result;
            if ((result = smbus_wait_until_ready()) < 0)
                return result;
            // are these 2 lines really needed? - not in Linux driver
            ols.WriteIoPortByte(SMBHSTSTS, ols.ReadIoPortByte(SMBHSTSTS));
            while ((ols.ReadIoPortByte(SMBHSTSTS) & SMBHSTSTS_INUSE_STS) == 0){}

            ols.WriteIoPortByte(SMBHSTADD, (byte) ((device & 0x7f) << 1));
            ols.WriteIoPortByte(SMBHSTCMD, command);

            if ((ols.ReadIoPortByte(SMBAUXCTL) & SMBAUXCTL_E32B) != 0u)
                return i801_write_block_as_block(values, size);
            else
                return i801_write_block_byte_by_byte(values, size);
        }


        private static int smbus_wait_until_ready(){
            byte mbyte;
            uint tries = 0;
            do{
                Thread.Sleep(1);
                mbyte = ols.ReadIoPortByte(SMBHSTSTS);
            } while (mbyte != 0 && (tries++ < MAX_TIMEOUT));
            return tries > MAX_TIMEOUT ? -1 : 0;
        }

        private static int i801_write_block_as_block(byte[] buf, byte size){
            uint i;
            int status;
            ols.ReadIoPortByte(SMBHSTCNT); // reset data buffer index

            // Use 32-byte buffer to process this transaction
            ols.WriteIoPortByte(SMBHSTDAT0, size);
            for (i = 0; i < size; ++i)
                ols.WriteIoPortByte(SMBBLKDAT, buf[i]);
            if ((status = i801_transaction(I801_BLOCK_DATA)) < 0)
                return status;
            return 0;
        }


        private static int i801_write_block_byte_by_byte(byte[] buf, byte size){
            uint i;
            int result;
            if ((result = i801_check_pre()) < 0)
                return result;

            ols.WriteIoPortByte(SMBHSTDAT0, size);
            ols.WriteIoPortByte(SMBBLKDAT, buf[0]);

            for (i = 0; i < size; ++i){
                ols.WriteIoPortByte(SMBHSTCNT, I801_BLOCK_DATA);
                if (i == 0)
                    ols.WriteIoPortByte(SMBHSTCNT, (byte) (ols.ReadIoPortByte(SMBHSTCNT) | I801_START));

                byte tries = 0;
                int status;
                do{
                    Thread.Sleep(1);
                    status = ols.ReadIoPortByte(SMBHSTSTS);
                } while ((status & SMBHSTSTS_BYTE_DONE) == 0 && (tries++ < MAX_TIMEOUT));

                if ((result = i801_check_post(status, tries > MAX_TIMEOUT ? 1 : 0)) < 0)
                    return result;

                if ((i + 1) < size)
                    ols.WriteIoPortByte(SMBBLKDAT, buf[i + 1]);

                ols.WriteIoPortByte(SMBHSTSTS, (byte) (SMBHSTSTS_BYTE_DONE | SMBHSTSTS_INTR));
            }
            return 0;
        }


        private static int i801_transaction(int xact){
            int result = i801_check_pre(), status, tries = 0;
            if (result < 0)
                return result;

            ols.WriteIoPortByte(SMBHSTCNT, (byte) (xact | I801_START));
            do{
                Thread.Sleep(1);
                status = ols.ReadIoPortByte(SMBHSTSTS);
            } while (((status & SMBHSTSTS_HOST_BUSY) != 0l) && (tries++ < MAX_TIMEOUT));
            if ((result = i801_check_post(status, tries > MAX_TIMEOUT ? 1 : 0)) < 0)
                return result;
            ols.WriteIoPortByte(SMBHSTSTS, SMBHSTSTS_INTR);
            return 0;
        }

        private static int i801_check_post(int status, int timeout){
            int error = 0;
            if (timeout != 0){
                // abort transaction
                ols.WriteIoPortByte(SMBHSTCNT, (byte) (ols.ReadIoPortByte(SMBHSTCNT) | SMBHSTCNT_KILL));
                Thread.Sleep(1);
                ols.WriteIoPortByte(SMBHSTCNT, (byte) (ols.ReadIoPortByte(SMBHSTCNT) & (~SMBHSTCNT_KILL)));
                ols.WriteIoPortByte(SMBHSTSTS, STATUS_FLAGS);
                return -2;
            }
            if ((status & SMBHSTSTS_FAILED) != 0l)
                error = -3;
            if ((status & SMBHSTSTS_DEV_ERR) != 0l)
                error = -4;
            if ((status & SMBHSTSTS_BUS_ERR) != 0l)
                error = -5;
            if (error != 0){
                // clear error flags
                ols.WriteIoPortByte(SMBHSTSTS, (byte) (status & STATUS_FLAGS));
            }
            return error;
        }

        private static int i801_check_pre(){
            int status = ols.ReadIoPortByte(SMBHSTSTS);
            if ((status & SMBHSTSTS_HOST_BUSY) != 0l)
                return -10;
            status &= STATUS_FLAGS;
            if (status != 0u){
                // clear status flags (some devices expect this as an acknowledge
                ols.WriteIoPortByte(SMBHSTSTS, (byte) status);
                status = ols.ReadIoPortByte(SMBHSTSTS) & STATUS_FLAGS;
                if (status != 0u)
                    return -20;
            }
            return 0;
        }
    }
}