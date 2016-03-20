using Dargon.Vox.Internals;
using System.IO;
using Dargon.Vox.Internals.Deserialization;
using Dargon.Vox.Internals.Serialization;

namespace Dargon.Vox {
   public static class Serialize {
      private static readonly SerializerOptions defaultSerializerOptions = new SerializerOptions();

      public static void To<T>(Stream target, T what) {
         VoxFrameSerializer.SerializeNonpolymorphic<T>(what, target, defaultSerializerOptions);
      }
   }

   public static class Deserialize {
      private static readonly SerializerOptions defaultSerializerOptions = new SerializerOptions();

      public static T From<T>(Stream target) {
         var result = From(target);
         return (T)result;
      }

      public static object From(Stream target) {
         return VoxFrameDeserializer.Deserialize(target, defaultSerializerOptions);
      }
   }
}
