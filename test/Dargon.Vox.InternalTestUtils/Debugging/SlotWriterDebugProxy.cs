using System;

namespace Dargon.Vox.InternalTestUtils.Debugging {
   public class SlotWriterDebugProxy : ISlotWriter {
      private readonly ISlotWriter target;
      private int indentation = 0;

      public SlotWriterDebugProxy(ISlotWriter target = null) {
         this.target = target ?? new NullSlotWriter();
      }

      public void WriteObject<T>(int slot, T x) {
         Out(slot, "Write Object: " + (x?.ToString() ?? "[Null]"));
         indentation++;
         target.WriteObject(slot, x);
         indentation--;
      }

      public void WriteBytes(int slot, byte[] bytes) {
         Out(slot, "Write Bytes: Length " + bytes.Length);
         target.WriteBytes(slot, bytes);
      }

      public void WriteBytes(int slot, byte[] bytes, int offset, int length) {
         Out(slot, "Write Bytes: Length " + bytes.Length + " Offset " + offset);
         target.WriteBytes(slot, bytes, offset, length);
      }

      public void WriteNumeric(int slot, int val) {
         Out(slot, "Write Numeric: " + val);
         target.WriteNumeric(slot, val);
      }

      public void WriteGuid(int slot, Guid guid) {
         Out(slot, "Write Guid: " + guid);
         target.WriteGuid(slot, guid);
      }

      public void WriteString(int slot, string s) {
         Out(slot, "Write String: " + s);
         target.WriteString(slot, s);
      }

      public void WriteNull(int slot) {
         Out(slot, "Write Null");
         target.WriteNull(slot);
      }

      private void Out(int slot, string desc) {
         Console.WriteLine(new string('\t', indentation) + slot + " => " + desc);
      }
   }
}