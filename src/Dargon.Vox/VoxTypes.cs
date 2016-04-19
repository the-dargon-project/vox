using System;
using Dargon.Vox.Internals;

namespace Dargon.Vox {
   public class VoxTypes {
      private readonly int baseId;

      public VoxTypes(int baseId = 0) {
         if (baseId < 0) {
            throw new ArgumentOutOfRangeException(nameof(baseId));
         }

         this.baseId = baseId;
      }

      public void Register<T>(int typeId) => Register(typeId, typeof(T));

      public void Register(int typeId, Type type) {
         typeId += baseId;

         if (typeId < 0) {
            throw new ArgumentOutOfRangeException(nameof(typeId));
         }

         TypeRegistry.RegisterType((TypeId)typeId, type);
      }
   }
}
