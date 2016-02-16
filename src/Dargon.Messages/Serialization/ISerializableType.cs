namespace Dargon.Messages.Internals {
   public interface ISerializableType {
      void Serialize(ISlotWriter writer);
      void Deserialize(ISlotReader reader);
   }
}