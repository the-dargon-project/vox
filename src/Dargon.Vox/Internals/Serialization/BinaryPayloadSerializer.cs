//using Dargon.Vox.Data;
//using Dargon.Vox.Utilities;
//using System;
//using System.IO;
//
//// ReSharper disable StaticMemberInGenericType
//namespace Dargon.Vox.Internals.Serialization {
//   public static class BinaryPayloadSerializer<T> {
//      [ThreadStatic] private static BwToCswAdapter binaryWriterAdapter;
//      [ThreadStatic] private static DrySlotWriter drySlotWriter;
//      [ThreadStatic] private static SlotBlockWriter slotBlockWriter;
//      [ThreadStatic] private static DataBlockWriter dataBlockWriter;
//
//      private static readonly ITypeSerializer<T> TypeSerializer = TypeSerializerRegistry<T>.Serializer;
//      private static BwToCswAdapter BinaryWriterAdapter => binaryWriterAdapter ?? (binaryWriterAdapter = new BwToCswAdapter());
//      private static DrySlotWriter DrySlotWriter => drySlotWriter ?? (drySlotWriter = new DrySlotWriter());
//      private static ISlotWriter SlotBlockWriter => slotBlockWriter ?? (slotBlockWriter = new SlotBlockWriter(BinaryWriterAdapter));
//      private static ISlotWriter DataBlockWriter => dataBlockWriter ?? (dataBlockWriter = new DataBlockWriter(BinaryWriterAdapter));
//
//      public static void Serialize(T t, BinaryWriter writer, SerializerOptions options) {
//         // Update TLS State
//         BinaryWriterAdapter.SetWriter(writer);
//
//         // Perform dry serialization run to determine output length
//         TypeSerializer.Serialize(DrySlotWriter, t);
//
//         // Determine lengths of typeId, slotBlockBodyLength, and dataBlockBodyLength vals.
//         var typeIdLength = TypeSerializer.TypeId.ComputeTypeIdLength();
//         var slotBlockLength = DrySlotWriter.SlotBlockBodyLength.ComputeVariableIntLength();
//         var dataBlockLength = DrySlotWriter.DataBlockBodyLength.ComputeVariableIntLength();
//         var payloadLength = typeIdLength +
//                             slotBlockLength +
//                             DrySlotWriter.SlotBlockBodyLength +
//                             dataBlockLength +
//                             DrySlotWriter.DataBlockBodyLength;
//
//         // Conditionally output payload length
//         if (options.IsPayloadLengthHeaderEnabled()) {
//            BinaryWriterAdapter.WriteVariableInt(payloadLength);
//         }
//
//         // Output type block
//         BinaryWriterAdapter.WriteBytes(TypeIdToBinaryRepresentationCache<T>.Serialization);
//
//         // Serialize slot block
//         BinaryWriterAdapter.WriteVariableInt(slotBlockLength);
//         TypeSerializer.Serialize(SlotBlockWriter, t);
//
//         // Serialize data block
//         BinaryWriterAdapter.WriteVariableInt(dataBlockLength);
//         TypeSerializer.Serialize(DataBlockWriter, t);
//      }
//
//      public class BwToCswAdapter : IForwardDataWriter {
//         private BinaryWriter _writer;
//
//         public void SetWriter(BinaryWriter writer) {
//            _writer = writer;
//         }
//
//         public void WriteByte(byte val) {
//            _writer.Write(val);
//         }
//
//         public void WriteBytes(byte[] val) {
//            _writer.Write(val);
//         }
//
//         public void WriteBytes(byte[] val, int offset, int length) {
//            _writer.Write(val, offset, length);
//         }
//      }
//   }
//}
