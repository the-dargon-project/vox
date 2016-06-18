using System.Collections.Generic;

namespace Dargon.Vox {
   public class InlineVoxTypes : VoxTypes {
      private readonly IReadOnlyDictionary<int, TypeContext> typeContextsById;

      public InlineVoxTypes(IReadOnlyDictionary<int, TypeContext> typeContextsById) {
         this.typeContextsById = typeContextsById;
      }

      public override IReadOnlyDictionary<int, TypeContext> EnumerateTypes() {
         return typeContextsById;
      }
   }
}