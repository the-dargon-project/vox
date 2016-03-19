using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dargon.Vox.Internals.Deserialization {
   public static class VoxObjectActivator<T> {
      private static Func<T> _factory;

      public static void SetFactory(Func<T> factory) {
         _factory = factory;
      }

      public static T CreateInstance() {
         return _factory();
      }
   }
}
