using System.IO;
using System.Linq;
using System.Threading;
using Dargon.Commons;
using Dargon.Messages.Slots;

// ReSharper disable StaticMemberInGenericType
namespace Dargon.Messages.Internals {
   public static class ObjectSerializer<T> {
      private static int serializeSlotCount = 1;
      private static int deserializeSlotCount = 1;
      private static readonly ThreadLocal<BwToCswAdapter> binaryToCoreWriterAdapter = new ThreadLocal<BwToCswAdapter>(() => new BwToCswAdapter());

      private static ITypeSerializer<T> TypeSerializer => TypeSerializerRegistry<T>.Serializer;
      private static BwToCswAdapter BinaryWriterAdapter => binaryToCoreWriterAdapter.Value;

      public static void Serialize(T t, BinaryWriter target) {
         // Serialize target to slots
         var slotWriter = new SlotWriter(serializeSlotCount);
         TypeSerializer.Serialize(slotWriter, t);

         // Update anticipated slot count.
         var slots = slotWriter.ReleaseSlots();
         ConditionallyIncreaseDefaultSlotCount(slots);

         // Forward compute slot lengths, sizes, and body length
         var slotLengths = slots.Map<ICoreBuffer, int>(s => (int)s.Length);
         var slotLengthSizes = slotLengths.Map(CoreSerializer.ComputeVariableIntLength);
         var typeLength = CoreSerializer.ComputeTypeIdLength(TypeSerializer.TypeId);
         var bodyLength = typeLength + slotLengthSizes.Sum() + slotLengthSizes.Sum();

         // Write to target stream
         var coreSerializerWriter = BinaryWriterAdapter;
         coreSerializerWriter.SetWriter(target);
         CoreSerializer.WriteVariableInt(coreSerializerWriter, bodyLength);
         CoreSerializer.WriteTypeId(coreSerializerWriter, TypeSerializer.TypeId);
         for (var i = 0; i < slots.Length; i++) {
            CoreSerializer.WriteVariableInt(coreSerializerWriter, slotLengths[i]);
            slots[i].CopyTo(coreSerializerWriter);
         }
         coreSerializerWriter.SetWriter(null);
      }

      public static void Deserialize(T t, BinaryWriter target) {

      }

      private static void ConditionallyIncreaseDefaultSlotCount(ICoreBuffer[] slots) {
         if (slots.Length <= serializeSlotCount) {
            int lastNonNullIndex = slots.Length - 1;
            while (slots[lastNonNullIndex] == null && lastNonNullIndex > 0) {
               lastNonNullIndex--;
            }
            SetCounterToMaximum(ref serializeSlotCount, lastNonNullIndex + 1);
         }
      }

      private static void SetCounterToMaximum(ref int slotCount, int value) {
         int readValue = slotCount;
         while (readValue != value) {
            readValue = Interlocked.CompareExchange(ref slotCount, value, readValue);
         }
      }

      public class BwToCswAdapter : ICoreSerializerWriter {
         private BinaryWriter _writer;

         public void SetWriter(BinaryWriter writer) {
            _writer = writer;
         }

         public void WriteByte(byte val) {
            _writer.Write(val);
         }

         public void WriteBytes(byte[] val) {
            _writer.Write(val);
         }

         public void WriteBytes(byte[] val, int offset, int length) {
            _writer.Write(val, offset, length);
         }
      }
   }
}
