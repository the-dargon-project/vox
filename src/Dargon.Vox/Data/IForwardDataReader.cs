namespace Dargon.Vox.Data {
   public interface IForwardDataReader {
      byte ReadByte();
      byte[] ReadBytes(int length);
      void SkipBytes(int count);
   }
}
