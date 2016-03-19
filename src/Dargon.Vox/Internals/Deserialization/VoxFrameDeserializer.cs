using Dargon.Vox.Data;
using Dargon.Vox.Utilities;
using System;
using System.IO;

namespace Dargon.Vox.Internals.Deserialization {
   public static class VoxFrameDeserializer {
      private delegate object DeserializeHelperFunc(IForwardDataReader reader, SerializerOptions options);

      [ThreadStatic] private static bool _initialized;
      [ThreadStatic] private static ReusableLengthLimitedStreamToForwardDataReaderAdapter _streamAdapter;

      private static void EnsureInitialized() {
         if (!_initialized) {
            _initialized = true;
            _streamAdapter = new ReusableLengthLimitedStreamToForwardDataReaderAdapter();
         }
      }

      public static object Deserialize(Stream input, SerializerOptions options) {
         EnsureInitialized();
         _streamAdapter.SetTarget(input);
         return Deserialize(_streamAdapter, options);
      }

      private static object Deserialize(ILengthLimitedForwardDataReader input, SerializerOptions options) {
         input.SetBytesRemaining(long.MaxValue);
         if (options.IsPayloadLengthHeaderEnabled()) {
            var frameLength = input.ReadVariableInt();
            input.SetBytesRemaining(frameLength);
            input.ReadUInt32(); // reserved
         }
         return VoxObjectDeserializer.Deserialize(input);
      }
   }
}
