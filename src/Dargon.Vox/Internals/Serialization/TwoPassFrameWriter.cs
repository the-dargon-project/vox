using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Dargon.Vox.Data;
using Dargon.Vox.Internals.TypePlaceholders.Boxes;
using Dargon.Vox.Utilities;

namespace Dargon.Vox.Internals.Serialization {
   public class TwoPassFrameWriter<T> : ISlotWriter {
      private readonly ObjectLengthCollection objectLengthCollection = new ObjectLengthCollection();
      private readonly Queue<int> stringLengthQueue = new Queue<int>(); 
      private readonly FakeForwardDataWriter fakeWriter = new FakeForwardDataWriter();
      private bool isDryPass;
      private IForwardDataWriter output;
      private byte[] stringBuffer = new byte[16];
      
      public void RunPasses(T subject, IForwardDataWriter writer) {
         PrepareDryPass();
         WriteObject<T>(0, subject);
         PrepareRealPass(writer);
         WriteObject<T>(0, subject);
         Trace.Assert(objectLengthCollection.Count == 0);
      }

      private void PrepareDryPass() {
         objectLengthCollection.Clear();
         isDryPass = true;
         output = fakeWriter;
         fakeWriter.Position = 0;
      }

      private void PrepareRealPass(IForwardDataWriter writer) {
         isDryPass = false;
         output = writer;

         // write frame header
         const int kReservedSize = sizeof(uint);
         writer.WriteVariableInt(fakeWriter.Position + kReservedSize);
         writer.WriteByte(0);
         writer.WriteByte(0);
         writer.WriteByte(0);
         writer.WriteByte(0);
      }

      public void WriteObject<U>(int slot, U subject) {
         if (typeof(U) == typeof(byte[])) {
            WriteBytes(slot, (byte[])(object)subject);
         } else if (typeof(U) == typeof(bool)) {
            WriteBoolean(slot, (bool)(object)subject);
         } else if (typeof(U) == typeof(sbyte)) {
            WriteNumeric(slot, (sbyte)(object)subject);
         } else if (typeof(U) == typeof(short)) {
            WriteNumeric(slot, (short)(object)subject);
         } else if (typeof(U) == typeof(int)) {
            WriteNumeric(slot, (int)(object)subject);
         } else if (typeof(U) == typeof(string)) {
            WriteString(slot, (string)(object)subject);
         } else if (typeof(U) == typeof(Type)) {
            WriteType(slot, (Type)(object)subject);
         } else if (typeof(U) == typeof(Guid)) {
            WriteGuid(slot, (Guid)(object)subject);
         } else if (typeof(IEnumerable).IsAssignableFrom(typeof(U))) {
            WriteCollectionVisitor<U>.Process(this, slot, subject);
         } else {
            if (subject == null) {
               WriteNull(slot);
            } else {
               // Custom object writer pushes to length queue on its own
               WriteCustomObject(slot, subject);
            }
         }
      }

      public void WriteCustomObject<U>(int slot, U subject) {
         if (isDryPass) {
            var lengthIndex = objectLengthCollection.Reserve();
            int savedWriterPosition = fakeWriter.Position;
            TypeSerializerRegistry<U>.Serializer.Serialize(this, subject);
            var length = fakeWriter.Position - savedWriterPosition;
            objectLengthCollection.Put(lengthIndex, length);
            fakeWriter.Position += FullTypeToBinaryRepresentationCache<U>.Serialization.Length;
            fakeWriter.Position += length.ComputeVariableIntLength();
         } else {
            output.WriteBytes(FullTypeToBinaryRepresentationCache<U>.Serialization);
            output.WriteVariableInt(objectLengthCollection.Take());
            TypeSerializerRegistry<U>.Serializer.Serialize(this, subject);
         }
      }

      public void WriteBytes(int slot, byte[] bytes) {
         if (bytes == null) {
            WriteNull(slot);
         } else {
            WriteBytes(slot, bytes, 0, bytes.Length);
         }
      }

      public void WriteBytes(int slot, byte[] bytes, int offset, int length) {
         if (isDryPass) {
            fakeWriter.Position += TypeId.ByteArray.ComputeTypeIdLength() + length.ComputeVariableIntLength() + length;
         } else {
            output.WriteTypeId(TypeId.ByteArray);
            output.WriteVariableInt(length);
            output.WriteBytes(bytes, offset, length);
         }
      }

      public void WriteBoolean(int slot, bool val) {
         var typeId = val ? TypeId.BoolTrue : TypeId.BoolFalse;
         if (isDryPass) {
            fakeWriter.Position += typeId.ComputeTypeIdLength();
         } else {
            output.WriteTypeId(typeId);
         }
      }

      public void WriteNumeric(int slot, int value) {
         if (value >= 0) {
            if (value <= sbyte.MaxValue) {
               WriteNumeric_Int8Range(slot, value);
            } else if (value <= short.MaxValue) {
               WriteNumeric_Int16Range(slot, value);
            } else {
               WriteNumeric_Int32Range(slot, value);
            }
         } else {
            if (sbyte.MinValue <= value) {
               WriteNumeric_Int8Range(slot, value);
            } else if (short.MinValue <= value) {
               WriteNumeric_Int16Range(slot, value);
            } else {
               WriteNumeric_Int32Range(slot, value);
            }
         }
      }

      private void WriteNumeric_Int8Range(int slot, int value) {
         if (isDryPass) {
            fakeWriter.Position += TypeId.Int8.ComputeTypeIdLength() + sizeof(sbyte);
         } else {
            output.WriteTypeId(TypeId.Int8);
            output.WriteByte((byte)value);
         }
      }

      private void WriteNumeric_Int16Range(int slot, int value) {
         if (isDryPass) {
            fakeWriter.Position += TypeId.Int16.ComputeTypeIdLength() + sizeof(short);
         } else {
            output.WriteTypeId(TypeId.Int16);
            output.WriteByte((byte)(value & 0xFF));
            output.WriteByte((byte)((value >> 8) & 0xFF));
         }
      }

      private void WriteNumeric_Int32Range(int slot, int value) {
         if (isDryPass) {
            fakeWriter.Position += TypeId.Int32.ComputeTypeIdLength() + sizeof(short);
         } else {
            output.WriteTypeId(TypeId.Int32);
            output.WriteByte((byte)(value & 0xFF));
            output.WriteByte((byte)((value >> 8) & 0xFF));
            output.WriteByte((byte)((value >> 16) & 0xFF));
            output.WriteByte((byte)((value >> 24) & 0xFF));
         }
      }

      public void WriteString(int slot, string str) {
         if (str == null) {
            WriteNull(slot);
         } else if (isDryPass) {
            var bodyByteCount = Encoding.UTF8.GetByteCount(str);
            stringLengthQueue.Enqueue(bodyByteCount);
            fakeWriter.Position += TypeId.String.ComputeTypeIdLength() + bodyByteCount.ComputeVariableIntLength() + bodyByteCount;
         } else {
            var bodyByteCount = stringLengthQueue.Dequeue();
            EnsureSize(ref stringBuffer, bodyByteCount);
            Encoding.UTF8.GetBytes(str, 0, str.Length, stringBuffer, 0);
            output.WriteTypeId(TypeId.String);
            output.WriteVariableInt(bodyByteCount);
            output.WriteBytes(stringBuffer, 0, bodyByteCount);
         }
      }

      public void WriteType(int slot, Type type) {
         WriteTypeVisitor.Visit(this, slot, type);
      }

      public void WriteType<T>(int slot) {
         if (isDryPass) {
            var typeSerialization = FullTypeToBinaryRepresentationCache<T>.Serialization;
            fakeWriter.Position += TypeId.Type.ComputeTypeIdLength()
               + typeSerialization.Length.ComputeVariableIntLength()
               + typeSerialization.Length;
         } else {
            var typeSerialization = FullTypeToBinaryRepresentationCache<T>.Serialization;
            output.WriteTypeId(TypeId.Type);
            output.WriteVariableInt(typeSerialization.Length);
            output.WriteBytes(typeSerialization);
         }
      }

      public void WriteGuid(int slot, Guid guid) {
         const int kGuidLength = 16;
         if (isDryPass) {
            fakeWriter.Position += TypeId.Guid.ComputeTypeIdLength() + kGuidLength;
         } else {
            output.WriteTypeId(TypeId.Guid);
            output.WriteBytes(guid.ToByteArray(), 0, kGuidLength);
         }
      }

      public void WriteNull(int slot) {
         if (isDryPass) {
            fakeWriter.Position += TypeId.Null.ComputeTypeIdLength();
         } else {
            output.WriteTypeId(TypeId.Null);
         }
      }

      public void WriteCollection<TElement, TCollection>(int slot, TCollection collection) where TCollection : IEnumerable<TElement> {
         if (collection == null) {
            WriteNull(slot);
         } else if (GenericTypeUnpacker<TElement>.Definition == typeof(KeyValuePair<,>)) {
            MapBoxBuilderAndWriteObjectVisitor<TElement>.Process(this, slot, collection);
         } else {
            WriteObject(slot, new ArrayBox<TElement>(collection));
         }
      }

      private void EnsureSize(ref byte[] buffer, int length) {
         if (buffer.Length < length) {
            buffer = new byte[length];
         }
      }

      public class FakeForwardDataWriter : IForwardDataWriter {
         public int Position { get; set; }

         public void WriteByte(byte val) => Position++;
         public void WriteBytes(byte[] val) => WriteBytes(val, 0, val.Length);
         public void WriteBytes(byte[] val, int offset, int length) => Position += length;
      }

      private class ObjectLengthCollection {
         private readonly List<int> storage = new List<int>();
         private int nextTakenIndex;

         public int Count => storage.Count - nextTakenIndex;

         public void Clear() {
            storage.Clear();
            nextTakenIndex = 0;
         }

         public int Reserve() {
            int index = storage.Count;
            storage.Add(int.MinValue);
            return index;
         }

         public void Put(int index, int size) => storage[index] = size;
         public int Take() => storage[nextTakenIndex++];
      }

      private static class WriteCollectionVisitor<TCollection> {
         private delegate void InvokerFunc(TwoPassFrameWriter<T> writer, int slot, TCollection collection);

         private static readonly InvokerFunc invoker;

         static WriteCollectionVisitor() {
            var method = new DynamicMethod(
               "write_collection_visitor_" + typeof(TCollection).Name,
               typeof(void), new[] { typeof(TwoPassFrameWriter<T>), typeof(int), typeof(TCollection) },
               typeof(TwoPassFrameWriter<T>), true);
            var emitter = method.GetILGenerator();
            var elementType = typeof(TCollection).GetInterfaces()
                                                 .First(i => i.Name.Contains(nameof(IEnumerable)) && i.IsGenericType)
                                                 .GetGenericArguments()[0];
            var writeCollectionMethod = typeof(TwoPassFrameWriter<T>).GetMethod(nameof(TwoPassFrameWriter<object>.WriteCollection))
                                                                     .MakeGenericMethod(elementType, typeof(TCollection));
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldarg_1);
            emitter.Emit(OpCodes.Ldarg_2);
            emitter.Emit(OpCodes.Call, writeCollectionMethod);
            emitter.Emit(OpCodes.Ret);
            invoker = (InvokerFunc)method.CreateDelegate(typeof(InvokerFunc));
         }

         public static void Process(TwoPassFrameWriter<T> writer, int slot, TCollection collection) {
            invoker(writer, slot, collection);
         }
      }

      private static class MapBoxBuilderAndWriteObjectVisitor<TKeyValuePair> {
         private delegate void InvokerFunc(TwoPassFrameWriter<T> writer, int slot, IEnumerable collection);

         private static readonly InvokerFunc invoker;

         static MapBoxBuilderAndWriteObjectVisitor() {
            var method = new DynamicMethod(
               "mapbox_visitor_" + typeof(TKeyValuePair).Name,
               typeof(void), new[] { typeof(TwoPassFrameWriter<T>), typeof(int), typeof(IEnumerable) },
               typeof(TwoPassFrameWriter<T>), true);
            var emitter = method.GetILGenerator();
            var mapBoxType = GenericTypeUnpacker<TKeyValuePair>.Build(typeof(MapBox<,>));
            var writeObjectMethod = typeof(TwoPassFrameWriter<T>).GetMethod("WriteObject").MakeGenericMethod(mapBoxType);
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldarg_1);
            emitter.Emit(OpCodes.Ldarg_2);
            emitter.Emit(OpCodes.Castclass, typeof(IEnumerable<TKeyValuePair>));
            emitter.Emit(OpCodes.Newobj, mapBoxType.GetConstructors().First(c => c.GetParameters().Length == 1));
            emitter.Emit(OpCodes.Call, writeObjectMethod);
            emitter.Emit(OpCodes.Ret);
            invoker = (InvokerFunc)method.CreateDelegate(typeof(InvokerFunc));
         } 

         public static void Process(TwoPassFrameWriter<T> writer, int slot, IEnumerable mapBox) {
            invoker(writer, slot, mapBox);
         }
      }

      private static class WriteTypeVisitor {
         private delegate void InvokerFunc(TwoPassFrameWriter<T> writer, int slot);

         private static readonly IGenericFlyweightFactory<InvokerFunc> invokers
            = GenericFlyweightFactory.ForMethod<InvokerFunc>(
               typeof(WriteTypeVisitor), nameof(Visitor));

         private static void Visitor<T>(TwoPassFrameWriter<T> writer, int slot) {
            writer.WriteType<T>(slot);
         }

         public static void Visit(TwoPassFrameWriter<T> writer, int slot, Type type) {
            invokers.Get(type)(writer, slot);
         }
      }
   }
}