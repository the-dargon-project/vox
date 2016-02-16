using System;
using System.Collections.Generic;
using System.IO;

namespace Dargon.Messages.Internals {
   public interface ISlotWriter {
      void WriteBytes(int slot, byte[] bytes);
      void WriteBytes(int slot, byte[] bytes, int offset, int length);
   }

   public class SlotContainer {
      private const double kGrowthFactor = 1.5;
      protected ICoreBuffer[] _slots;

      public ICoreBuffer[] ReleaseSlots() {
         var result = _slots;
         _slots = null;
         return result;
      }

      protected void Resize(int slotCount) {
         var newSlots = new ICoreBuffer[slotCount];
         if (_slots != null) {
            Array.Copy(_slots, newSlots, _slots.Length);
         }
         _slots = newSlots;
      }

      protected void EnsureIndexable(int slot) {
         while (slot >= _slots.Length) {
            Resize((int)Math.Ceiling(_slots.Length * kGrowthFactor));
         }
      }
   }

   public class SlotWriter : SlotContainer, ISlotWriter {
      public SlotWriter(int slotCount) {
         Resize(slotCount);
      }

      public void WriteBytes(int slot, byte[] bytes) {
         WriteBytes(slot, bytes, 0, bytes.Length);
      }

      public void WriteBytes(int slot, byte[] bytes, int offset, int length) {
         Helper(slot, TypeId.ByteArray, b => b.WriteBytes(bytes, offset, length));
      }

      private void Helper(int slot, TypeId typeId, Action<ICoreBuffer> write) {
         EnsureIndexable(slot);
         var buffer = new CoreBuffer(new MemoryStream());
         CoreSerializer.WriteVariableInt(buffer, (int)typeId);
         write(buffer);
         _slots[slot] = buffer;
      }
   }

   public interface ISerializer {
      void Write<T>(object val);
      void Write<T>(T val);
   }
}