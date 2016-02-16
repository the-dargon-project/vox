using System;
using Dargon.Messages.Internals;

namespace Dargon.Messages.Slots {
   public interface ISlotReader {
      byte[] ReadBytes(int slot);
   }

   public class SlotReader : ISlotReader {
      private readonly ICoreBuffer[] _slots;

      public SlotReader(ICoreBuffer[] slots) {
         _slots = slots;
      }

      public byte[] ReadBytes(int slot) {
         return Helper(slot, TypeId.ByteArray, (buffer) => buffer.ReadRemaining());
      }

      public T Helper<T>(int slot, TypeId typeId, Func<ICoreBuffer, T> read) {
         if (slot >= _slots.Length) {
            throw new IndexOutOfRangeException();
         }
         var buffer = _slots[slot];
         var actualTypeId = (TypeId)CoreSerializer.ReadVariableInt(buffer);
         if (typeId != actualTypeId) {
            throw new SlotTypeMismatchException(typeId, actualTypeId);
         }
         return read(buffer);
      }
   }
}
