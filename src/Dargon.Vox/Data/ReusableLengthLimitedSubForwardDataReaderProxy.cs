namespace Dargon.Vox.Data {
   public class ReusableLengthLimitedSubForwardDataReaderProxy : ReusableLengthLimitedBase<ILengthLimitedForwardDataReader>, ILengthLimitedForwardDataReader {
      public byte ReadByte() {
         Advance(1);
         return _target.ReadByte();
      }

      public byte[] ReadBytes(int length) {
         Advance(length);
         return _target.ReadBytes(length);
      }

      public void ReadBytes(int length, byte[] target) {
         Advance(length);
         _target.ReadBytes(length, target);
      }

      public void SkipBytes(int count) {
         Advance(count);
         _target.SkipBytes(count);
      }
   }
}