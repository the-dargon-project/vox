using Dargon.Messages.Slots;

namespace Dargon.Messages {
   public interface ISerializableType {
      void Serialize(ISlotWriter writer);
      void Deserialize(ISlotReader reader);
   }
}