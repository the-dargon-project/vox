using Dargon.Messages.Internals;
using Dargon.Messages.Slots;

namespace Dargon.Messages {
   public interface ITypeSerializer<T> {
      TypeId TypeId { get; set; }
      void Serialize(ISlotWriter writer, T source);
      void Deserialize(ISlotReader reader, T target);
      int MaxSlotCount { get; set; }
   }
}