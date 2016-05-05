using Dargon.Commons;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dargon.Vox.Internals.TypePlaceholders.Boxes {
   public class IEnumerableBox<TElement> : ISerializableType, ISerializationBox {
      private IEnumerable<TElement> inner;

      public IEnumerableBox() {}

      public IEnumerableBox(IEnumerable enumerable) {
         inner = enumerable.Cast<TElement>();
      }

      public void Serialize(ISlotWriter writer) {
         var count = inner.Count();
         var i = 0;
         writer.WriteNumeric(i++, count);
         foreach (var element in inner) {
            writer.WriteObject(i++, element);
         }
      }

      public void Deserialize(ISlotReader reader) {
         var count = reader.ReadNumeric(0);
         inner = Util.Generate(count, i => reader.ReadObject<TElement>(i + 1));
      }

      public object Unbox() {
         return inner;
      }
   }
}
