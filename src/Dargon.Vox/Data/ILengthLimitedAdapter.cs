namespace Dargon.Vox.Data {
   public interface ILengthLimitedAdapter {
      long BytesRemaining { get; }
      void SetBytesRemaining(long bytesRemaining);
   }
}