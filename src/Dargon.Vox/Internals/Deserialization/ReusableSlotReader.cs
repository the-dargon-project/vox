using Dargon.Vox.Data;
using Dargon.Vox.Slots;

namespace Dargon.Vox.Internals.Deserialization {
   public class ReusableSlotReader : ISlotReader {
      private ILengthLimitedForwardDataReader _reader;
      private int _nextSlot;

      public void ResetWithInput(ILengthLimitedForwardDataReader reader) {
         _reader = reader;
         _nextSlot = 0;
      }

      public byte[] ReadBytes(int slot) {
         AdvanceUntil(slot);
         return VoxObjectDeserializer.DeserializeNonpolymorphic<byte[]>(_reader);
      }

      private void AdvanceUntil(int slot) {
         while (_nextSlot < slot) {
            VoxObjectSkipper.Skip(_reader);
            _nextSlot++;
         }
      }
   }
}