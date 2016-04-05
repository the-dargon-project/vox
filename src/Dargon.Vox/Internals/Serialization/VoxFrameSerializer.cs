using Dargon.Vox.Data;
using Dargon.Vox.Utilities;
using System;
using System.IO;

namespace Dargon.Vox.Internals.Serialization {
   public static class VoxFrameSerializer {
      private delegate void NonpolymorphicSerializeFunction(object subject, Stream output, SerializerOptions options);

      private static readonly IGenericFlyweightFactory<NonpolymorphicSerializeFunction> serializeFactory
         = GenericFlyweightFactory.ForMethod<NonpolymorphicSerializeFunction>(
            typeof(VoxFrameSerializer), nameof(SerializeNonpolymorphic));

      [ThreadStatic] private static ReusableStreamToForwardDataWriterAdapter writer;

      public static void Serialize(object subject, Stream output, SerializerOptions options) {
         serializeFactory.Get(subject.GetType())(subject, output, options);
      }

      public static void SerializeNonpolymorphic<T>(object subject, Stream output, SerializerOptions options) {
         writer = writer ?? new ReusableStreamToForwardDataWriterAdapter();
         writer.SetTarget(output);
         VoxFrameSerializer<T>.SerializeNonpolymorphic((T)subject, writer, options);
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
