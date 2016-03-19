namespace Dargon.Vox {
   public interface ISlotWriter {
      void WriteBytes(int slot, byte[] bytes);
      void WriteBytes(int slot, byte[] bytes, int offset, int length);

      void WriteNull(int slot);
   }
}