using System;
using System.Collections.Generic;

namespace Dargon.Vox {
   public abstract class VoxTypes {
      public abstract IReadOnlyDictionary<int, TypeContext> EnumerateTypes();
   }
}