namespace Dargon.Vox.Data {
   public interface IForwardDataWriter {
      void WriteByte(byte val);
      void WriteBytes(byte[] val);
      void WriteBytes(byte[] val, int offset, int length);
   }
}