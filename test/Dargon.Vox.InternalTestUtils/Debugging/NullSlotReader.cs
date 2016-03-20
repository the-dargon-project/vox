using System;

namespace Dargon.Vox.InternalTestUtils.Debugging {
   public class NullSlotReader : ISlotReader {
      public object ReadObject(int slot) => null;
      public T ReadObject<T>(int slot) => default(T);
      public byte[] ReadBytes(int slot) => null;
      public int ReadNumeric(int slot) => 0;
      public string ReadString(int slot) => null;
      public Guid ReadGuid(int slot) => Guid.Empty;
      public object ReadNull(int slot) => null;
   }
}
