using Dargon.Commons.Pooling;
using System.IO;

namespace Dargon.Vox {
   public static class VoxUtils {
      private static readonly IObjectPool<MemoryStream> memoryStreamPool = ObjectPool.Create(() => new MemoryStream());

      public static T DeepCloneSerializable<T>(this T x) {
         var ms = memoryStreamPool.TakeObject();
         try {
            Serialize.To(ms, x);
            ms.Position = 0;
            return Deserialize.From<T>(ms);
         } finally {
            memoryStreamPool.ReturnObject(ms);
         }
      }
   }
}
