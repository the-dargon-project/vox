using Dargon.Vox.RoundTripTests;

namespace Dargon.Vox {
   public class Program {
      public static void Main() {
         new BytesIT().ByteArrays();
         new GuidIT().Guids();
         new AutoserializationIT().LinkedList_SingleNode();
         new AutoserializationIT().LinkedList_Chain();
         new AutoserializationIT().Hodgepodge();
      }
   }
}
