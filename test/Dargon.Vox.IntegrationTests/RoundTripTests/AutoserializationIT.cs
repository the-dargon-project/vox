using Dargon.Vox.InternalTestUtils;
using NMockito;
using Xunit;

namespace Dargon.Vox.RoundTripTests {
   public class AutoserializationIT : NMockitoInstance {
      [Fact]
      public void Hodgepodge() {
         TestTypeRegistrationHelpers.RegisterWithSerializer<HodgepodgeDto>();
         RoundTripTest.Run(new HodgepodgeDto {
            String = CreatePlaceholder<string>(),
            Int8 = CreatePlaceholder<sbyte>(),
            Int16 = CreatePlaceholder<short>(),
            Int32 = CreatePlaceholder<int>()
         });
      }

      [Fact]
      public void LinkedList_SingleNode() {
         LinkedList_RunTest(new StringLinkedListNode(CreatePlaceholder<string>()));
      }

      [Fact]
      public void LinkedList_Chain() {
         LinkedList_RunTest(
            new StringLinkedListNode(
               CreatePlaceholder<string>(),
               new StringLinkedListNode(
                  CreatePlaceholder<string>(),
                  new StringLinkedListNode(
                     CreatePlaceholder<string>()))));
      }

      private void LinkedList_RunTest(StringLinkedListNode node) {
         TestTypeRegistrationHelpers.RegisterWithSerializer<StringLinkedListNode>();
         RoundTripTest.Run(node);
      }

      internal class HodgepodgeDto {
         public string String { get; set; }
         public sbyte Int8 { get; set; }
         public short Int16 { get; set; }
         public int Int32 { get; set; }

         public override bool Equals(object obj) => Equals(obj as HodgepodgeDto);

         public bool Equals(HodgepodgeDto o) => o != null &&
                                                Equals(String, o.String) &&
                                                Int8 == o.Int8 &&
                                                Int16 == o.Int16 &&
                                                Int32 == o.Int32;
      }

      internal class StringLinkedListNode {
         public string Value { get; set; }
         public StringLinkedListNode Next { get; set; }

         public StringLinkedListNode() { }

         public StringLinkedListNode(string value, StringLinkedListNode next = null) {
            Value = value;
            Next = next;
         }

         public override bool Equals(object obj) {
            var other = obj as StringLinkedListNode;
            return other != null &&
                   Value == other.Value &&
                   Equals(Next, other.Next);
         }
      }
   }
}
