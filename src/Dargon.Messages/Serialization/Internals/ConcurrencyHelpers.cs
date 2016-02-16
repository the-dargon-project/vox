using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dargon.Messages.Serialization.Internals {
   public static class ConcurrencyHelpers {
      public static void CompareSwapToMax(ref int dest, int value) {
         var destValue = dest;
         while (value > destValue) {
            destValue = Interlocked.CompareExchange(ref dest, value, destValue);
         }
      }
   }
}
