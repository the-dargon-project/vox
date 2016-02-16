using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dargon.Messages.Serialization {
   public static class Serialize {
      public static void To<T>(Stream target) {
         var serializer = TypeSerializerRegistry<T>.Serializer;

      }
   }
}
