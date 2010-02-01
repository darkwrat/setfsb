using System.Collections.Generic;


namespace SetFSB {
    public interface Pll{
        int SetFSB(int fsb);
        int GetFSB();
        List<int> SupportedFSBs { get;  }
    }
    public class PllBase {

        public  smBus smb;
        public  List<ControlByte> ControlBytes = new List<ControlByte>();

         public List<int> SupportedFSBs{
            get{
                var fsbs = new List<int>();
                foreach (var cb in ControlBytes){
                    fsbs.Add(cb.fsb);
                }
                return fsbs;
            }
        }
    }

    public class ControlByte{
            public readonly byte fsb;
            public readonly byte Byte1;
            public readonly byte Byte2;

            public ControlByte(byte Fsb, byte byte1, byte byte2){
                fsb = Fsb;
                Byte1 = byte1;
                Byte2 = byte2;
            }
        } ;


   
}
