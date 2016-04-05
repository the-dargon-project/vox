using System;

namespace Dargon.Vox.InternalTestUtils.Debugging {
   public class NullSlotWriter : ISlotWriter {
      public void WriteObject<T>(int slot, T x) {}
      public void WriteBytes(int slot, byte[] bytes) {}
      public void WriteBytes(int slot, byte[] bytes, int offset, int length) {}
      public void WriteBoolean(int slot, bool val) {} 
      public void WriteNumeric(int slot, int val) {}
      public void WriteString(int slot, string s) {}
      public void WriteGuid(int slot, Guid guid) {}
      public void WriteNull(int slot) {}
   }
}