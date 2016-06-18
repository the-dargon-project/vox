using System;

namespace Dargon.Vox.Utilities {
   public interface IGenericFlyweightFactory<T> {
      T Get(Type type);
   }

   public class GenericFlyweightFactoryImpl<T> : IGenericFlyweightFactory<T> {
      private readonly IncrementalDictionary<Type, T> _handlersByType = new IncrementalDictionary<Type, T>();
      private readonly Func<Type, T> _factory;

      public GenericFlyweightFactoryImpl(Func<Type, T> factory) {
         _factory = factory;
      }

      public T Get(Type type) {
         return _handlersByType.GetOrAdd(type, _factory);
      }
   }
}
