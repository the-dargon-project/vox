using System;
using Dargon.Messages.Internals;

namespace Dargon.Messages.Serialization {
   public static class TypeSerializerRegistry<T> {
      private static ITypeSerializer<T> _serializer;

      public static ITypeSerializer<T> Serializer => _serializer;

      public static void RegisterType(ITypeSerializer<T> serializer) {
         if (serializer == null) {
            throw new ArgumentNullException(nameof(serializer));
         } else if (_serializer != null) {
            if (!_serializer.Equals(serializer)) {
               throw new InvalidOperationException($"Duplicate registration of type {typeof(T).FullName}.");
            }
         }
         _serializer = serializer;
      }
   }
}
