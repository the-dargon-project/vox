namespace Dargon.Vox.Data {
   public interface IForwardDataReader {
      byte ReadByte();
      byte[] ReadBytes(int length);
      void ReadBytes(int length, byte[] target);
      void SkipBytes(int count);
   }
}
