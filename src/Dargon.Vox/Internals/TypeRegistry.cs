using System;
using System.Collections.Generic;
using System.Linq;
using Dargon.Commons.Collections;
using Dargon.Vox.Internals.TypePlaceholders;
using Dargon.Vox.Utilities;

namespace Dargon.Vox.Internals {
   public static class TypeRegistry {
      private static readonly IncrementalDictionary<TypeId, Type> typesById = new IncrementalDictionary<TypeId, Type>();
      private static readonly IncrementalDictionary<Type, TypeId> idsByType = new IncrementalDictionary<Type, TypeId>();

      static TypeRegistry() {
         RegisterTypes(new Dictionary<TypeId, Type> {
            [TypeId.ByteArray] = typeof(byte[]),
            [TypeId.Null] = typeof(NullType)
         });
      }

      public static Type Lookup(int typeId) => Lookup((TypeId)typeId);
      public static Type Lookup(TypeId typeId) => typesById[typeId];
      public static TypeId Lookup<T>() => Lookup(typeof(T));
      public static TypeId Lookup(Type t) => idsByType[t];

      public static void RegisterType(TypeId typeId, Type type) {
         RegisterTypes(ImmutableDictionary.Of(typeId, type));
      }

      public static void RegisterTypes(IReadOnlyDictionary<TypeId, Type> additionalTypesById) {
         typesById.Merge(additionalTypesById);
         idsByType.Merge(additionalTypesById.ToDictionary(x => x.Value, x => x.Key));
      }
   }
}
