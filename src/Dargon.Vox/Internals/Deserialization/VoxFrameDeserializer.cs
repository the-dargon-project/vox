using Dargon.Vox.Data;
using Dargon.Vox.Utilities;
using System;
using System.IO;

namespace Dargon.Vox.Internals.Deserialization {
   public static class VoxFrameDeserializer {
      private delegate object DeserializeHelperFunc(IForwardDataReader reader, SerializerOptions options);

      [ThreadStatic] private static bool _initialized;
      [ThreadStatic] private static ReusableLengthLimitedStreamToForwardDataReaderAdapter _streamAdapter;
      [ThreadStatic] private static ReusableLengthLimitedBufferToForwardDataReaderAdapter _bufferAdapter;

      private static void EnsureInitialized() {
         if (!_initialized) {
            _initialized = true;
            _streamAdapter = new ReusableLengthLimitedStreamToForwardDataReaderAdapter();
            _bufferAdapter = new ReusableLengthLimitedBufferToForwardDataReaderAdapter();
         }
      }

      public static object Deserialize(Stream input, SerializerOptions options) {
         EnsureInitialized();
         _streamAdapter.SetTarget(input);
         _streamAdapter.SetBytesRemaining(long.MaxValue);
         return Deserialize(_streamAdapter, options);
      }

      public static object Deserialize(byte[] input, int offset, int length, SerializerOptions options) {
         EnsureInitialized();
         _bufferAdapter.SetTarget(input);
         _bufferAdapter.SetOffset(offset);
         _bufferAdapter.SetBytesRemaining(length);
         return Deserialize(_bufferAdapter, options);
      }

      private static object Deserialize(ILengthLimitedForwardDataReader input, SerializerOptions options) {
         if (options.IsPayloadLengthHeaderEnabled()) {
            var frameLength = input.ReadVariableInt();
            input.SetBytesRemaining(frameLength);
            input.ReadUInt32(); // reserved
         }
         return VoxObjectDeserializer.Deserialize(input);
      }
   }
}
