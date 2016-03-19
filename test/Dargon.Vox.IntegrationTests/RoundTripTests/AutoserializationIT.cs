using Dargon.Vox.Internals;
using NMockito;
using Xunit;

namespace Dargon.Vox.RoundTripTests {
   public class AutoserializationIT : NMockitoInstance {
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
         TypeRegistry.RegisterType((TypeId)1, typeof(StringLinkedListNode));
         TypeSerializerRegistry<StringLinkedListNode>.Register(
            AutoTypeSerializerFactory.Create<StringLinkedListNode>());

         RoundTripTest.Run(node);
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
