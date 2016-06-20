using Dargon.Vox.FunctionalTests.RoundTripTests;
using Dargon.Vox.RoundTripTests;
using static Dargon.Commons.Channels.ChannelsExtensions;

namespace Dargon.Vox {
   public class Program {
      public static void Main() {
         new TypeRoundTripFT().TypeRoundTripTest();
         new TypeRoundTripFT().TypeArrayRoundTripTest();
         new TypeRoundTripFT().TypeArrayArrayRoundTripTest();
         new TypeRoundTripFT().TypeSimplificationTest();

         new VectorLikeRoundtripFT().ArrayEasyTest();
         new VectorLikeRoundtripFT().ArrayHardTest1();
         new VectorLikeRoundtripFT().ArrayHardTest2();
         new VectorLikeRoundtripFT().ListEasyTest();
         new VectorLikeRoundtripFT().ListHardTest1();
         new VectorLikeRoundtripFT().ListHardTest2();

         new BooleanRoundtripFT().TrueRoundTripTest();
         new BooleanRoundtripFT().FalseRoundTripTest();
         new BooleanRoundtripFT().ThisIsMaybeProbablyTotallyGoingToBreaktest();

         new StringRoundTripFT().StringRoundTripTest();
         new StringRoundTripFT().StringArrayRoundTripTest();

         new DictionaryLikeRoundtripFT().EasyTest();
         new DictionaryLikeRoundtripFT().HardTest1();
         new DictionaryLikeRoundtripFT().HardTest2();

         //         new BytesIT().ByteArrays();
         //         new BoolIT().True();
         //         new BoolIT().False();
         //         new GuidIT().Guids();
         //         new AutoserializationIT().LinkedList_String_SingleNode();
         //         new AutoserializationIT().LinkedList_String_Chain();
         //         new AutoserializationIT().Hodgepodge();
      }
   }
}
