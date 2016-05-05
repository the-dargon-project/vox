using System;
using System.Collections.Generic;

namespace Dargon.Vox {
   public interface ISlotWriter {
      void WriteObject<T>(int slot, T x);

      void WriteBytes(int slot, byte[] bytes);
      void WriteBytes(int slot, byte[] bytes, int offset, int length);

      void WriteBoolean(int slot, bool val);
      void WriteNumeric(int slot, int val);
      void WriteGuid(int slot, Guid guid);
      void WriteString(int slot, string s);
      void WriteNull(int slot);
      void WriteCollection<TElement, TCollection>(int slot, TCollection collection) where TCollection : IEnumerable<TElement>;
   }
}