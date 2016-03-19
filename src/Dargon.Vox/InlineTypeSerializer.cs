using System;

namespace Dargon.Vox {
   public class InlineTypeSerializer<T> : ITypeSerializer<T> {
      private readonly Action<ISlotWriter, T> write;
      private readonly Action<ISlotReader, T> read;

      public InlineTypeSerializer(Action<ISlotWriter, T> write, Action<ISlotReader, T> read) {
         this.write = write;
         this.read = read;
      }

      public Type Type => typeof(T);

      public void Serialize(ISlotWriter writer, T source) {
         write(writer, source);
      }

      public void Deserialize(ISlotReader reader, T target) {
         read(reader, target);
      }
   }
}
