using Dargon.Vox.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dargon.Vox.Internals.Serialization {
   public static class FullTypeToBinaryRepresentationCache<T> {
      public static byte[] Serialization { get; private set; }

      static FullTypeToBinaryRepresentationCache() {
         var flattenedTypes = new List<TypeId>();
         TypeTraverse(typeof(T), flattenedTypes.Add);

         using (var ms = new MemoryStream()) {
            var writer = new ReusableStreamToForwardDataWriterAdapter();
            writer.SetTarget(ms);
            flattenedTypes.ForEach(writer.WriteTypeId);
            Serialization = ms.ToArray();
         }
      }

      private static void TypeTraverse(Type type, Action<TypeId> callback) {
         if (type.IsGenericType && !type.IsGenericTypeDefinition) {
            callback(TypeId.GenericTypeIdBegin);
            TypeTraverse(type.GetGenericTypeDefinition(), callback);
            foreach (var genericArgument in type.GenericTypeArguments) {
               TypeTraverse(genericArgument, callback);
            }
            callback(TypeId.GenericTypeIdEnd);
         } else {
            callback(TypeRegistry.Lookup(type));
         }
      }
   }
}
