using System;

namespace Dargon.Vox.Data {
   public class ReusableLengthLimitedBufferToForwardDataWriterAdapter : ReusableLengthLimitedBase<byte[]>, ILengthLimitedForwardDataWriter {
      private int offset;

      public void SetOffset(int offset) {
         this.offset = offset;
      }

      public void WriteByte(byte val) {
         Advance(1);
         _target[offset++] = val;
      }

      public void WriteBytes(byte[] val) {
         WriteBytes(val, 0, val.Length);
      }

      public void WriteBytes(byte[] val, int offset, int length) {
         Advance(length);
         Array.Copy(val, offset, _target, this.offset, length);
         this.offset += length;
      }
   }
}