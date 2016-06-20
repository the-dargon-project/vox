using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dargon.Commons;
using Dargon.Commons.Collections;

namespace Dargon.Vox.Utilities {
   public static class EnumerableUtilities {
      private static readonly ConcurrentDictionary<Type, Type> typeSimplifyingMapThingNamingStuffIsHard = new ConcurrentDictionary<Type, Type>();

      public static Type GetGenericIEnumerableInterfaceTypeOrNull(Type collectionType) {
         if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
            return collectionType;
         }

         return collectionType.GetInterfaces()
                              .FirstOrDefault(i => i.Name.Contains(nameof(IEnumerable)) && i.IsGenericType);
      }

      public static Type GetEnumerableElementType(Type type) {
         if (type.IsArray) {
            return type.GetElementType();
         } else {
            var enumerableType = GetGenericIEnumerableInterfaceTypeOrNull(type);
            return enumerableType.GetGenericArguments()[0];
         }
      }

      public static Type SimplifyType(Type type) {
         if ((!type.IsGenericType && !type.IsArray) || type == typeof(string)) {
            return type;
         }
         if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)) {
            return typeof(KeyValuePair<,>).MakeGenericType(
               type.GetGenericArguments().Map(SimplifyType));
         }
         return typeSimplifyingMapThingNamingStuffIsHard.GetOrAdd(type, SimplifyCollectionType);
      }

      private static Type SimplifyCollectionType(Type genericType) {
         var enumerableInterfaceType = GetGenericIEnumerableInterfaceTypeOrNull(genericType);
         if (enumerableInterfaceType == null) {
            return genericType;
         }
         var elementType = SimplifyType(enumerableInterfaceType.GetGenericArguments()[0]);
         if (elementType.IsGenericType &&
             elementType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.KeyValuePair<,>)) {
            var kvpGenericArgs = elementType.GetGenericArguments();
            return typeof(Dictionary<,>).MakeGenericType(kvpGenericArgs);
         }
         return elementType.MakeArrayType();
      }

      public static bool IsDictionaryLikeType(Type enumerableType) {
         return enumerableType.GetInterfaces()
                              .Any(t => t == typeof(IDictionary) ||
                                        (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>)) ||
                                        (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
            );
      }
   }
}