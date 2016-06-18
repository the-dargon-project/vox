using Dargon.Vox.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dargon.Vox.Utilities;

namespace Dargon.Vox.Internals.Serialization {
   public static class FullTypeToBinaryRepresentationCache<T> {
      public static byte[] Serialization { get; private set; }

      private delegate void CollectionTypeTraverseFunc(Action<TypeId> callback);

      private static IGenericFlyweightFactory<CollectionTypeTraverseFunc> collectionTypeTraversers
         = GenericFlyweightFactory.ForMethod<CollectionTypeTraverseFunc>(
            typeof(FullTypeToBinaryRepresentationCache<T>),
            nameof(CollectionTypeTraverse));

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
         if (type.IsArray || (type.IsGenericType && !type.IsGenericTypeDefinition)) {
            if (typeof(IEnumerable).IsAssignableFrom(type)) {
               collectionTypeTraversers.Get(type)(callback);
            } else {
               callback(TypeId.GenericTypeIdBegin);
               TypeTraverse(type.GetGenericTypeDefinition(), callback);
               foreach (var genericArgument in type.GenericTypeArguments) {
                  TypeTraverse(genericArgument, callback);
               }
               callback(TypeId.GenericTypeIdEnd);
            }
         } else {
            TypeId typeId;
            if (TypeRegistry.TryLookup(type, out typeId)) {
               callback(typeId);
            } else {
               throw new KeyNotFoundException($"Type {type}: typeId not registered.");
            }
         }
      }

      private static void CollectionTypeTraverse<TCollection>(Action<TypeId> callback) {
         if (CollectionUnpacker<TCollection>.IsDictionary) {
            callback(TypeId.GenericTypeIdBegin);
            callback(TypeId.Map);
            TypeTraverse(CollectionUnpacker<TCollection>.KeyType, callback);
            TypeTraverse(CollectionUnpacker<TCollection>.ValueType, callback);
            callback(TypeId.GenericTypeIdEnd);
         } else {
            callback(TypeId.GenericTypeIdBegin);
            callback(TypeId.Array);
            TypeTraverse(CollectionUnpacker<TCollection>.ElementType, callback);
            callback(TypeId.GenericTypeIdEnd);
         }
      }
   }
}
