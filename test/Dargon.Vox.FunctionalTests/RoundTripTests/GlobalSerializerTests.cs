using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dargon.Ryu;
using NMockito;

namespace Dargon.Vox.FunctionalTests.RoundTripTests {
   public class GlobalSerializerTests : NMockitoInstance {
      public void Run() {
         // Populate the global serializer by VoxRyuExtensionModule hook.
         var ryu = new RyuFactory().Create();

         // Test that global serializer is properly configured by roundtrip on custom type.
         var expected = new CustomType { X = CreatePlaceholder<string>() };
         var actual = expected.DeepCloneSerializable();
         AssertTrue(expected != actual);
         AssertEquals(expected.X, actual.X);
      }

      public class CustomVoxTypes : VoxTypes {
         public CustomVoxTypes() : base(100000) {
            Register(0, typeof(CustomType));
         }
      }

      [AutoSerializable]
      public class CustomType {
         public string X { get; set; }
      }
   }
}
