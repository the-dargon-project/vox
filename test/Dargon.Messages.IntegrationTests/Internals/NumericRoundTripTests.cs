using Dargon.Commons;
using NMockito;
using System;
using System.IO;
using Xunit;

namespace Dargon.Messages.Internals {
   public class NumericRoundTripTests : NMockitoInstance {
      [Fact]
      public void Randomized_NumericRoundTripTest() {
         var random = new Random(0);
         var numbers = Util.Generate(1000, i => random.Next(int.MinValue, int.MaxValue));
         NumericRoundTripTest(numbers);
      }

      [Fact]
      public void Hardcoded_NumericRoundTripTest() {
         var nums = new[] {
            int.MinValue,
            int.MinValue + 1,
            short.MinValue - 1,
            short.MinValue,
            short.MinValue + 1,
            byte.MinValue - 1,
            byte.MinValue,
            byte.MinValue + 1,
            -(2 << 7) - 1,
            -(2 << 7),
            -(2 << 7) + 1,
            -(2 << 6) - 1,
            -(2 << 6),
            -(2 << 6) + 1,
            (2 << 6) - 1,
            (2 << 6),
            (2 << 6) + 1,
            (2 << 7) - 1,
            (2 << 7),
            (2 << 7) + 1,
            byte.MaxValue - 1,
            byte.MaxValue,
            byte.MaxValue + 1,
            short.MaxValue - 1,
            short.MaxValue,
            short.MaxValue + 1,
            int.MaxValue - 1,
            int.MaxValue,
         };
         NumericRoundTripTest(nums);
      }

      private void NumericRoundTripTest(int[] numbers) {
         var ms = new MemoryStream();
         var buffer = new CoreBuffer(ms);
         foreach (var number in numbers) {
            CoreSerializer.WriteVariableInt(buffer, number);
         }
         ms.Position = 0;
         foreach (var number in numbers) {
            AssertEquals(number, CoreSerializer.ReadVariableInt(buffer));
         }
      }
   }
}
