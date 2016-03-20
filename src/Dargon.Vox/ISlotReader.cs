using System;

namespace Dargon.Vox {
   public interface ISlotReader {
      object ReadObject(int slot);
      T ReadObject<T>(int slot);

      byte[] ReadBytes(int slot);

      int ReadNumeric(int slot);
      string ReadString(int slot);
      Guid ReadGuid(int slot);
      object ReadNull(int slot);
   }
}
