using Dargon.Vox.RoundTripTests;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Xunit;

namespace Dargon.Vox.FunctionalTests.RoundTripTests {
   public class AutoSerializableRoundTripTest : RoundTripTest {
      [Fact]
      public void EasyTest() {
         var subject = new Box<int> { Value = 10 };
         MultiThreadedRoundTripTest(new object[] { subject }, 1000, 8);
      }

      [Fact]
      public void HardTest1() {
         var subject = new Box<Box<int>> { Value = new Box<int> { Value = 10 } };
         MultiThreadedRoundTripTest(new object[] { subject }, 1000, 8);
      }

      [Fact]
      public void HardTest2() {
         var subject = new Box<ConcurrentDictionary<string, HashSet<Box<int>>>> {
            Value = new ConcurrentDictionary<string, HashSet<Box<int>>> {
               ["Xwth"] = new HashSet<Box<int>> {
                  new Box<int> { Value = 1 },
                  new Box<int> { Value = 2 },
                  new Box<int> { Value = 3 }
               }
            }
         };
         MultiThreadedRoundTripTest(new object[] { subject }, 1000, 8);
      }

      [AutoSerializable]
      public class Box<T> {
         public T Value { get; set; }

         public override int GetHashCode() => Value?.GetHashCode() ?? 0;

         public override bool Equals(object obj) {
            var box = obj as Box<T>;
            return box != null && Value.Equals(box.Value);
         }
      }
   }
}
