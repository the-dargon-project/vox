using Dargon.Vox.Data;
using Dargon.Vox.Utilities;

namespace Dargon.Vox.Internals.Deserialization {
   public static class VoxObjectSkipper {
      private delegate void SkipFunc(ILengthLimitedForwardDataReader input);

      private static readonly GenericFlyweightFactory<SkipFunc> objectBodySkipperByType
         = GenericFlyweightFactory.ForMethod<SkipFunc>(
            typeof(VoxObjectBodyDeserializerAndSkipper<>),
            nameof(VoxObjectBodyDeserializerAndSkipper<object>.Skip));

      public static void Skip(ILengthLimitedForwardDataReader input) {
         var type = input.ReadFullType();
         objectBodySkipperByType.Get(type)(input);
      }
   }
}