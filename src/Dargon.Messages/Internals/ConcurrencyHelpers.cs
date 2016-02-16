using System.Threading;

namespace Dargon.Messages.Internals {
   public static class ConcurrencyHelpers {
      public static void CompareSwapToMax(ref int dest, int value) {
         var destValue = dest;
         while (value > destValue) {
            destValue = Interlocked.CompareExchange(ref dest, value, destValue);
         }
      }
   }
}
