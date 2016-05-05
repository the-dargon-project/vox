using System;
using System.Collections.Generic;
using Dargon.Commons;

namespace Dargon.Vox.Internals {
   public static class TypeSerializerRegistry<T> {
      private static ITypeSerializer<T> serializer;

      public static ITypeSerializer<T> Serializer => GetSerializer();
      public static object synchronization = new object();

      private static ITypeSerializer<T> GetSerializer() {
         if (serializer == null) {
            lock (synchronization) {
               if (typeof(T).GetAttributeOrNull<AutoSerializableAttribute>() != null) {
                  serializer = AutoTypeSerializerFactory.Create<T>();
               } else if (typeof(ISerializableType).IsAssignableFrom(typeof(T))) {
                  serializer = new InlineTypeSerializer<T>(
                     (writer, t) => ((ISerializableType)t).Serialize(writer),
                     (reader, t) => ((ISerializableType)t).Deserialize(reader));
               } else {
                  throw new KeyNotFoundException("No serializer for type " + typeof(T).FullName + " registered.");
               }
            }
         }
         return serializer;
      }

      public static void Register(ITypeSerializer<T> newSerializer) {
         if (newSerializer == null) {
            throw new ArgumentNullException(nameof(newSerializer));
         }
         if (serializer != null && !newSerializer.Equals(serializer)) {
            throw new InvalidOperationException($"Duplicate registration of type {typeof(T).FullName}.");
         }
         serializer = newSerializer;
      }
   }
}
