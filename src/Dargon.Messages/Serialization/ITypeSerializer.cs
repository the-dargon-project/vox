namespace Dargon.Messages.Internals {
   public interface ITypeSerializer<T> {
      TypeId TypeId { get; set; }
      void Serialize(ISlotWriter writer, T source);
      void Deserialize(ISlotReader reader, T target);
   }
}