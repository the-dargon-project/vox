using System.Collections.Generic;
using System.Linq;
using Dargon.Commons;

namespace Dargon.Vox.Internals.TypePlaceholders.Boxes {
   public class MapBox<TKey, TValue> : ISerializableType, ISerializationBox {
      private IEnumerable<KeyValuePair<TKey, TValue>> inner;

      public MapBox() { }
      public MapBox(IEnumerable<KeyValuePair<TKey, TValue>> enumerable) {
         inner = enumerable;
      }

      public void Serialize(ISlotWriter writer) {
         var count = inner.Count();
         var i = 0;
         writer.WriteNumeric(i++, count);
         foreach (var kvp in inner) {
            writer.WriteObject(i++, kvp.Key);
            writer.WriteObject(i++, kvp.Value);
         }
      }

      public void Deserialize(ISlotReader reader) {
         var i = 0;
         var count = reader.ReadNumeric(i++);
         var result = Util.Generate(
            count,
            _ => new KeyValuePair<TKey, TValue>(
               reader.ReadObject<TKey>(i++),
               reader.ReadObject<TValue>(i++)));
         inner = result;
      }

      public object Unbox() {
         return inner;
      }
   }
}