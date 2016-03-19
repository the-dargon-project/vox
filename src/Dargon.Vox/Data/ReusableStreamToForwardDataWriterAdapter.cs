using System.IO;

namespace Dargon.Vox.Data {
   public class ReusableStreamToForwardDataWriterAdapter : ReusableBase<Stream>, IForwardDataWriter {
      public void WriteByte(byte val) {
         _target.WriteByte(val);
      }

      public void WriteBytes(byte[] val) {
         _target.Write(val, 0, val.Length);
      }

      public void WriteBytes(byte[] val, int offset, int length) {
         _target.Write(val, offset, length);
      }
   }
}