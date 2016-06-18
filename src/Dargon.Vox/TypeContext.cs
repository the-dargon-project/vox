using System;

namespace Dargon.Vox {
   public class TypeContext {
      public TypeContext(Type type, Func<object> activator) {
         Type = type;
         Activator = activator;
      }

      public Type Type { get; }
      public Func<object> Activator { get; }
   }
}