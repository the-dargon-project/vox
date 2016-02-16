using System;
using System.IO;
using Dargon.Messages.Internals;

namespace Dargon.Messages.Slots {
   public interface ISlotWriter {
      void WriteBytes(int slot, byte[] bytes);
      void WriteBytes(int slot, byte[] bytes, int offset, int length);
   }

   /// <summary>
   /// Performs a dry-run of serialization on a given object
   /// </summary>
   public class DrySlotWriter : ISlotWriter {
      private const int kPrimitiveTypeIdSize = 1;

      private int expectedSlot = 0;
      private int slotTableSize;
      private int dataTableSize;

      public int SlotTableSize => slotTableSize;
      public int DataTableSize => dataTableSize;

      public void WriteBytes(int slot, byte[] bytes) {
         WriteBytes(slot, bytes, 0, bytes.Length);
      }

      public void WriteBytes(int slot, byte[] bytes, int offset, int length) {
         InnerHelper(
            slot,
            kPrimitiveTypeIdSize + CoreSerializer.ComputeVariableIntLength(bytes.Length),
            bytes.Length
         );
      }

      private void InnerHelper(int slot, int slotTableEntrySize, int dataTableEntrySize) {
         if (slot < expectedSlot) {
            throw new InvalidOperationException("Slot serialization must occur sequentially.");
         }

         // We'll be serializing 0-length slots for skipped slots
         int slotsToSkip = slot - expectedSlot;
         slotTableSize += slotsToSkip * kPrimitiveTypeIdSize;

         // Then we'll serialize the actual slot
         slotTableSize += slotTableEntrySize;
         dataTableSize += dataTableEntrySize;
      }
   }

   public interface ISerializer {
      void Write<T>(object val);
      void Write<T>(T val);
   }
}