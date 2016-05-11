using System;
using Dargon.Vox.Data;
using Dargon.Vox.Internals.Serialization;
using Dargon.Vox.Internals.TypePlaceholders;
using Dargon.Vox.Slots;
using Dargon.Vox.Utilities;

namespace Dargon.Vox.Internals.Deserialization {
   public static class VoxObjectDeserializer {
      private delegate object DeserializeFunc(ILengthLimitedForwardDataReader input);

      private static readonly IGenericFlyweightFactory<DeserializeFunc> objectBodyDeserializerByType
         = GenericFlyweightFactory.ForMethod<DeserializeFunc>(
            typeof(VoxObjectBodyDeserializerAndSkipper<>),
            nameof(VoxObjectBodyDeserializerAndSkipper<object>.Deserialize));

      public static object Deserialize(ILengthLimitedForwardDataReader input) {
         var type = input.ReadFullType();
         if (type == typeof(BoolTrue)) {
            return true;
         } else if (type == typeof(BoolFalse)) {
            return false;
         } else {
            return objectBodyDeserializerByType.Get(type)(input);
         }
      }

      public static unsafe T DeserializeNonpolymorphic<T>(ILengthLimitedForwardDataReader input) {
         var type = input.ReadFullType();
         if (type != typeof(T)) {
            throw new Exception($"WTF {type} {typeof(T)}");
         }
//         var expectedTypeSerialization = FullTypeToBinaryRepresentationCache<T>.Serialization;
//         byte* actualTypeSerialization = stackalloc byte[expectedTypeSerialization.Length];
//         for (var i = 0; i < expectedTypeSerialization.Length; i++) {
//            actualTypeSerialization[i] = input.ReadByte();
//            if (actualTypeSerialization[i] != expectedTypeSerialization[i]) {
//               // todo: push read bytes back onto a data reader, deserialize actual type for exception.
//               throw new NonpolymorphicTypeMismatchException("Deserialized Nonpolymorphic Type is not " + typeof(T).FullName);
//            }
//         }
         return VoxObjectBodyDeserializerAndSkipper<T>.Deserialize(input);
      }

      public class NonpolymorphicTypeMismatchException : Exception {
         public NonpolymorphicTypeMismatchException(string message) : base(message) { }
      }

      public class SpecialTypeReroutingGenericFlyweightFactory<T> : IGenericFlyweightFactory<T> {
         private readonly IGenericFlyweightFactory<T> inner;

         public SpecialTypeReroutingGenericFlyweightFactory(IGenericFlyweightFactory<T> inner) {
            this.inner = inner;
         }

         public T Get(Type type) {
            if (type == typeof(BoolTrue) || type == typeof(BoolFalse)) {
               return inner.Get(typeof(bool));
            } else {
               return inner.Get(type);
            }
         }
      }
   }
}