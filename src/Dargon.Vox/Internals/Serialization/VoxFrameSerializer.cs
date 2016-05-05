using Dargon.Vox.Data;
using Dargon.Vox.Utilities;
using System;
using System.IO;

namespace Dargon.Vox.Internals.Serialization {
   public static class VoxFrameSerializer {
      private delegate void NonpolymorphicSerializeFunction(object subject, Stream output, SerializerOptions options);

      private static readonly IGenericFlyweightFactory<NonpolymorphicSerializeFunction> serializeFactory
         = GenericFlyweightFactory.ForMethod<NonpolymorphicSerializeFunction>(
            typeof(VoxFrameSerializer), nameof(SerializeNonpolymorphicStream));

      [ThreadStatic] private static ReusableStreamToForwardDataWriterAdapter _streamAdapter;
      [ThreadStatic] private static ReusableLengthLimitedBufferToForwardDataWriterAdapter _bufferAdapter;

      public static void Serialize(object subject, Stream output, SerializerOptions options) {
         serializeFactory.Get(subject.GetType())(subject, output, options);
      }

      public static void SerializeNonpolymorphicStream<T>(object subject, Stream output, SerializerOptions options) {
         _streamAdapter = _streamAdapter ?? new ReusableStreamToForwardDataWriterAdapter();
         _streamAdapter.SetTarget(output);
         VoxFrameSerializer<T>.SerializeNonpolymorphic((T)subject, _streamAdapter, options);
      }

      public static void SerializeNonpolymorphicBuffer<T>(object subject, byte[] output, int offset, int length, SerializerOptions options) {
         _bufferAdapter = _bufferAdapter ?? new ReusableLengthLimitedBufferToForwardDataWriterAdapter();
         _bufferAdapter.SetTarget(output);
         _bufferAdapter.SetOffset(offset);
         _bufferAdapter.SetBytesRemaining(length);
         VoxFrameSerializer<T>.SerializeNonpolymorphic((T)subject, _bufferAdapter, options);
      }
   }

   public static class VoxFrameSerializer<T> {
      [ThreadStatic] private static TwoPassFrameWriter<T> twoPassFrameWriter; 

      public static void SerializeNonpolymorphic(T subject, IForwardDataWriter writer, SerializerOptions options) {
         twoPassFrameWriter = twoPassFrameWriter ?? new TwoPassFrameWriter<T>();
         twoPassFrameWriter.RunPasses(subject, writer);
      }
   }
}
