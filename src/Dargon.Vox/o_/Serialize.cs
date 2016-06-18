using Dargon.Vox.Internals;
using System.IO;
using System.Reflection;
using Dargon.Vox.Internals.Deserialization;
using Dargon.Vox.Internals.Serialization;
using Dargon.Vox.Utilities;

namespace Dargon.Vox {
   public static class Serialize {
      private static readonly SerializerOptions defaultSerializerOptions = new SerializerOptions();

      private delegate void ToStreamFunc(Stream target, object what);

      private static readonly IGenericFlyweightFactory<ToStreamFunc> toStreamFactory
         = GenericFlyweightFactory.ForMethod<ToStreamFunc>(
            typeof(Serialize), nameof(ToStreamInternal));

      private delegate void ToByteArrayFunc(byte[] target, int offset, int length, object what);

      private static readonly IGenericFlyweightFactory<ToByteArrayFunc> toByteArrayFactory
         = GenericFlyweightFactory.ForMethod<ToByteArrayFunc>(
            typeof(Serialize), nameof(ToByteArrayInternal));

      public static void To<T>(Stream target, T what) {
         if (typeof(T) == typeof(object)) {
            toStreamFactory.Get(what.GetType())(target, what);
         } else {
            ToStreamInternal(target, what);
         }
      }

      private static void ToStreamInternal<T>(Stream target, T what) {
         VoxFrameSerializer.SerializeNonpolymorphicStream<T>(what, target, defaultSerializerOptions);
      }

      public static void To<T>(byte[] target, T what) => To(target, 0, target.Length, what);

      public static void To<T>(byte[] target, int offset, int length, T what) {
         if (typeof(T) == typeof(object)) {
            toByteArrayFactory.Get(what.GetType())(target, offset, length, what);
         } else {
            ToByteArrayInternal(target, offset, length, what);
         }
      }

      private static void ToByteArrayInternal<T>(byte[] target, int offset, int length, T what) {
         VoxFrameSerializer.SerializeNonpolymorphicBuffer<T>(what, target, offset, length, defaultSerializerOptions);
      }
   }

   public static class Deserialize {
      private static readonly SerializerOptions defaultSerializerOptions = new SerializerOptions();

      public static T From<T>(Stream target) => (T)From(target);

      public static object From(Stream target) {
         return VoxFrameDeserializer.Deserialize(target, defaultSerializerOptions);
      }

      public static T From<T>(byte[] target) => From<T>(target, 0, target.Length);

      public static object From(byte[] target) => From(target, 0, target.Length);

      public static T From<T>(byte[] target, int offset, int length) => (T)From(target, offset, length);

      public static object From(byte[] target, int offset, int length) {
         return VoxFrameDeserializer.Deserialize(target, offset, length, defaultSerializerOptions);
      }
   }
}
