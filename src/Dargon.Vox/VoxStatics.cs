using System;

namespace Dargon.Vox {
   public static class VoxStatics {
      public static TypeContext Something<T>() => Something(typeof(T));
      public static TypeContext Something(Type type) => Something(type, () => Activator.CreateInstance(type));
      public static TypeContext Something<T>(Func<T> activator) => Something(typeof(T), () => activator());
      public static TypeContext Something(Type type, Func<object> activator) {
         return new TypeContext(type, activator);
      }
   }
}