using Dargon.Vox.Data;
using Dargon.Vox.Internals.TypePlaceholders;
using Dargon.Vox.Slots;

namespace Dargon.Vox.Internals.Deserialization {
   public class ReusableSlotReader : ISlotReader {
      private ILengthLimitedForwardDataReader _reader;
      private int _nextSlot;

      public void ResetWithInput(ILengthLimitedForwardDataReader reader) {
         _reader = reader;
         _nextSlot = 0;
      }

      public object ReadObject(int slot) {
         AdvanceUntil(slot);
         return VoxObjectDeserializer.Deserialize(_reader);
      }

      public T ReadObject<T>(int slot) => ReadNonpolymorphicHelper<T>(slot);
      public byte[] ReadBytes(int slot) => ReadNonpolymorphicHelper<byte[]>(slot);
      public string ReadString(int slot) => ReadNonpolymorphicHelper<string>(slot);
      public object ReadNull(int slot) => ReadNonpolymorphicHelper<NullType>(slot);

      private T ReadNonpolymorphicHelper<T>(int slot) {
         AdvanceUntil(slot);
         return VoxObjectDeserializer.DeserializeNonpolymorphic<T>(_reader);
      }

      private void AdvanceUntil(int slot) {
         while (_nextSlot < slot) {
            VoxObjectSkipper.Skip(_reader);
            _nextSlot++;
         }
         _nextSlot++;
      }
   }
}