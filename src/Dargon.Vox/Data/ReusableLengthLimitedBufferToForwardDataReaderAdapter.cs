using System;

namespace Dargon.Vox.Data {
   public class ReusableLengthLimitedBufferToForwardDataReaderAdapter : ReusableLengthLimitedBase<byte[]>, ILengthLimitedForwardDataReader {
      private int offset;
      
      public void SetOffset(int value) {
         offset = value;
      }

      public byte ReadByte() {
         Advance(1);
         return _target[offset++];
      }

      public byte[] ReadBytes(int length) {
         var result = new byte[length];
         ReadBytes(length, result);
         return result;
      }

      public void ReadBytes(int length, byte[] target) {
         Advance(length);
         Array.Copy(_target, offset, target, 0, length);
         offset += length;
      }

      public void SkipBytes(int count) {
         Advance(count);
         offset += count;
      }
   }
}