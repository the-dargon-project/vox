using System;
using NMockito;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dargon.Commons;

namespace Dargon.Vox.RoundTripTests {
   public static class RoundTripTest {
      public static void Run<T>(T val, string benchmarkName = null, int benchmarkCount = -1) {
         if (benchmarkCount < 0) {
            new Runner().Run(val, 1);
         } else {
            var sw = new Stopwatch();
            sw.Start();
            new Runner().Run(val, 1);
            Console.WriteLine($"`{benchmarkName}` benchmark completed {benchmarkCount} trials in {sw.ElapsedMilliseconds} ms");
         }
      }

      public class Runner : NMockitoInstance {
         public void Run<T>(T val, int count) {
            using (var ms = new MemoryStream()) {
               for (var i = 0; i < count; i++) {
                  Serialize.To(ms, val);
               }
               ms.Position = 0;
               T[] results = Util.Generate(count, i => Deserialize.From<T>(ms));
               foreach (var val2 in results) {
                  if (val == null) {
                     AssertNull(val2);
                  } else if (val is IEnumerable) {
                     AssertSequenceEquals(((IEnumerable)val).Cast<object>(), ((IEnumerable)val2).Cast<object>());
                  } else {
                     AssertEquals(val, val2);
                  }
               }
            }
         }
      }
   }
}
