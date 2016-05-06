using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dargon.Vox.Utilities {
   public static class CollectionUnpacker<TCollection> {
      static CollectionUnpacker() {
         IsEnumerable = typeof(IEnumerable).IsAssignableFrom(typeof(TCollection));
         if (IsEnumerable) {
            EnumerableType = typeof(TCollection).GetInterfaces()
                                                .First(i => i.Name.Contains(nameof(IEnumerable)) && i.IsGenericType);
            ElementType = EnumerableType.GetGenericArguments()[0];

            IsDictionary = ElementType.IsGenericType && ElementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
            if (IsDictionary) {
               var genericArgs = ElementType.GetGenericArguments();
               KeyType = genericArgs[0];
               ValueType = genericArgs[1];
            }
         }
      }

      public static bool IsEnumerable { get; }
      public static Type EnumerableType { get; }
      public static Type ElementType { get; }

      public static bool IsDictionary { get; }
      public static Type KeyType { get; }
      public static Type ValueType { get; }
   }
}