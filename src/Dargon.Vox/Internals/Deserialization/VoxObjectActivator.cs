using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Dargon.Commons;

namespace Dargon.Vox.Internals.Deserialization {
   public static class VoxObjectActivator<T> {
      private static Func<T> _factory;

      static VoxObjectActivator() {
         var ctors = typeof(T).GetConstructors();
         foreach (var ctor in ctors) {
            if (ctor.GetParameters().All(p => p.HasDefaultValue)) {
               _factory = Expression.Lambda<Func<T>>(
                  Expression.New(typeof(T).GetConstructor(Type.EmptyTypes))).Compile();
            }
         }
      } 

      public static void SetFactory(Func<T> factory) {
         factory.ThrowIfNull(nameof(factory));
         _factory = factory;
      }

      public static T CreateInstance() {
         if (_factory == null) {
            throw new Exception($"No type serializer for {typeof(T).FullName} registered and could not find usable default constructor.");
         } else {
            return _factory();
         }
      }
   }
}
