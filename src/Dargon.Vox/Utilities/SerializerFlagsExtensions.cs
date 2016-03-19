using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dargon.Vox.Utilities {
   public static class SerializerFlagsExtensions {
      public static bool IsPayloadLengthHeaderEnabled(this SerializerOptions options) {
         return (options.Flags & SerializerFlags.NoLengthHeader) == 0;
      }
   }
}
