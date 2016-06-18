using Dargon.Vox.Slots;

namespace Dargon.Vox {
   public interface ISerializableType {
      void Serialize(ISlotWriter writer);
      void Deserialize(ISlotReader reader);
   }
}