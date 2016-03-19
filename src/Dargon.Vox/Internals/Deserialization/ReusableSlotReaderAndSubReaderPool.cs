using System;
using System.Collections.Generic;
using Dargon.Vox.Data;
using Dargon.Vox.Slots;

namespace Dargon.Vox.Internals.Deserialization {
   public static class ReusableSlotReaderAndSubReaderPool {
      [ThreadStatic] private static bool _initialized;
      [ThreadStatic] private static Stack<Tuple<ReusableSlotReader, ReusableLengthLimitedSubForwardDataReaderProxy>> _readerPool;

      private static void EnsureInitialized() {
         if (!_initialized) {
            _initialized = true;
            _readerPool = new Stack<Tuple<ReusableSlotReader, ReusableLengthLimitedSubForwardDataReaderProxy>>();
         }
      }

      public static void Take(out ReusableSlotReader slotReader, out ReusableLengthLimitedSubForwardDataReaderProxy subReader) {
         EnsureInitialized();

         if (_readerPool.Count == 0) {
            slotReader = new ReusableSlotReader();
            subReader = new ReusableLengthLimitedSubForwardDataReaderProxy();
         } else {
            var tuple = _readerPool.Pop();
            slotReader = tuple.Item1;
            subReader = tuple.Item2;
         }
      }

      public static void Return(ReusableSlotReader reusableSlotReader, ReusableLengthLimitedSubForwardDataReaderProxy subReader) {
         EnsureInitialized();
         _readerPool.Push(Tuple.Create(reusableSlotReader, subReader));
      }
   }
}