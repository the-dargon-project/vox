using System.IO;
using Dargon.Messages.Internals;

namespace Dargon.Messages {
   public static class Serialize {
      public static void To<T>(Stream target) {
         var serializer = TypeSerializerRegistry<T>.Serializer;

      }
   }
}
