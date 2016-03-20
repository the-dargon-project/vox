using System.IO;

namespace Dargon.Vox.Data {
   public class ReusableLengthLimitedStreamToForwardDataReaderAdapter : ReusableLengthLimitedBase<Stream>, ILengthLimitedForwardDataReader {
      public byte ReadByte() {
         Advance(1);
         var b = _target.ReadByte();
         if (b == -1) {
            throw new EndOfStreamException();
         }
         return (byte)(b & 0xFF);
      }
      
      public byte[] ReadBytes(int length) {
         var buffer = new byte[length];
         ReadBytes(length, buffer);
         return buffer;
      }

      public void ReadBytes(int length, byte[] buffer) {
         Advance(length);
         int offset = 0;
         while (offset < length) {
            offset += _target.Read(buffer, offset, length - offset);
         }
      }

      public void SkipBytes(int count) {
         Advance(count);
         _target.Seek(count, SeekOrigin.Current);
      }
   }
}