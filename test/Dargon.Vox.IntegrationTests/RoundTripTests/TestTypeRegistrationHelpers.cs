using Dargon.Commons.Collections;
using System;
using System.Threading;
using Dargon.Vox.Internals;

namespace Dargon.Vox.RoundTripTests {
   public static class TestTypeRegistrationHelpers {
      private static readonly ConcurrentDictionary<Type, int> typeIdsByType = new ConcurrentDictionary<Type, int>();
      private static int typeIdCounter = 1;

      public static void RegisterWithSerializer<T>() {
         typeIdsByType.GetOrAdd(
            typeof(T),
            add => {
               var typeId = Interlocked.Increment(ref typeIdCounter);
               TypeRegistry.RegisterType((TypeId)typeId, typeof(T));
               var serializer = AutoTypeSerializerFactory.Create(typeof(T));
               typeof(TypeSerializerRegistry<>).MakeGenericType(typeof(T))
                                               .GetMethod(nameof(TypeSerializerRegistry<object>.Register))
                                               .Invoke(null, new object [] { serializer });
               return typeId;
            });
      }
   }
}
