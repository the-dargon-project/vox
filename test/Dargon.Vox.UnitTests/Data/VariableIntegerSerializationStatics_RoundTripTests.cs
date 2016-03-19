using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dargon.Commons;
using Dargon.Vox.Data;
using NMockito;
using Xunit;

namespace Dargon.Messages.Data {
   public class VariableIntegerSerializationStatics_RoundTripTests : NMockitoInstance {
      [Fact]
      public void Randomized() {
         var random = new Random(0);
         var numbers = Util.Generate(1000, i => random.Next(int.MinValue, int.MaxValue));
         RunRoundTripTest(numbers);
      }

      [Fact]
      public void EdgeCases() {
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
         RunRoundTripTest(nums);
      }

      private void RunRoundTripTest(int[] numbers) {
         var ms = new MemoryStream();
         var writer = new ReusableStreamToForwardDataWriterAdapter();
         writer.SetTarget(ms);
         foreach (var number in numbers) {
            writer.WriteVariableInt(number);
         }
         ms.Position = 0;
         var reader = new ReusableLengthLimitedStreamToForwardDataReaderAdapter();
         reader.SetTarget(ms);
         reader.SetBytesRemaining(ms.Length);
         foreach (var number in numbers) {
            AssertEquals(number, reader.ReadVariableInt());
         }
      }
   }
}
