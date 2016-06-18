using System;
using Dargon.Vox.Internals;
using Dargon.Vox.Slots;

namespace Dargon.Vox {
   public interface ITypeSerializer {
      Type Type { get; }
   }

   public interface ITypeSerializer<T> : ITypeSerializer {
      void Serialize(ISlotWriter writer, T source);
      void Deserialize(ISlotReader reader, T target);
   }
}