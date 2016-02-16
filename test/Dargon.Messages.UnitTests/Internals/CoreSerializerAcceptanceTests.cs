using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NMockito;
using Xunit;

namespace Dargon.Messages.Internals {
   public class CoreSerializerAcceptanceTests : NMockitoInstance {
      [Fact]
      public void Serialized_Zero_Is_Zero_Byte() {
         var ms = new MemoryStream();
         var buffer = new CoreBuffer(ms);
         CoreSerializer.WriteVariableInt(buffer, 0);
         AssertEquals(1, ms.Length);
         AssertEquals(0, ms.GetBuffer()[0]);
      }

      [Fact]
      public void Numbers_From_Neg2Pow6_To_2Pow6Min1_Take_One_Byte() => ValidateRange(-(2 << 5), (2 << 5) - 1, 1);

      [Fact]
      public void Numbers_From_NegTwoPow13_To_Neg2Pow6Min1_Take_Two_Bytes() => ValidateRange(-(2 << 12), -(2 << 5) - 1, 2);

      [Fact]
      public void Numbers_From_2Pow6_To_TwoPow13Min1_Take_Two_Bytes() => ValidateRange((2 << 5), (2 << 12) - 1, 2);

      private void ValidateRange(int lower, int upper, int size) {
         for (int i = lower; i <= upper; i++) {
            AssertEquals(size, ComputeSize(i));
            AssertEquals(size, CoreSerializer.ComputeVariableIntLength(i));
         }
      }

      public int ComputeSize(int n) {
         var ms = new MemoryStream();
         var buffer = new CoreBuffer(ms);
         CoreSerializer.WriteVariableInt(buffer, n);
         return (int)ms.Length;
      }
   }
}
