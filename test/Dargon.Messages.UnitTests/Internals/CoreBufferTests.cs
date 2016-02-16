using System;
using NMockito;
using NMockito.Fluent;
using System.IO;
using Xunit;

namespace Dargon.Messages.Internals {
   public class CoreBufferTests : NMockitoInstance {
      [Fact]
      public void ReadByte_ThrowsOnEndOfStream() {
         var stream = CreateSpy<MemoryStream>();
         Expect(stream.ReadByte()).ThenReturn(-1);

         var testObj = new CoreBuffer(stream);

         AssertThrows<EndOfStreamException>(() => testObj.ReadByte());
         VerifyExpectationsAndNoMoreInteractions();
      }

      [Fact]
      public void ReadByte_HappyPath() {
         var stream = CreateSpy<MemoryStream>();
         Expect(stream.ReadByte()).ThenReturn(1, 2, 3);

         var testObj = new CoreBuffer(stream);

         testObj.ReadByte().IsEqualTo((byte)1);
         testObj.ReadByte().IsEqualTo((byte)2);
         testObj.ReadByte().IsEqualTo((byte)3);

         VerifyExpectationsAndNoMoreInteractions();
      }
   }
}
