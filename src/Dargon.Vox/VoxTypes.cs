using System;
using System.Collections.Generic;

namespace Dargon.Vox {
   public abstract class VoxTypes {
      private readonly int baseId;

      protected VoxTypes(int baseId = 0) {
         if (baseId < 0) {
            throw new ArgumentOutOfRangeException(nameof(baseId));
         }

         this.baseId = baseId;
      }

      public abstract IReadOnlyDictionary<int, TypeContext> EnumerateTypes();
   }
}