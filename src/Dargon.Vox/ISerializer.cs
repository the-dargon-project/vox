namespace Dargon.Vox {
   public interface ISerializer {
      void Write<T>(object val);
      void Write<T>(T val);
   }
}