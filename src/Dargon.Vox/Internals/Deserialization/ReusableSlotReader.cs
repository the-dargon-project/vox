using Dargon.Vox.Data;
using Dargon.Vox.Internals.TypePlaceholders;
using Dargon.Vox.Internals.TypePlaceholders.Boxes;
using Dargon.Vox.Slots;
using Dargon.Vox.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dargon.Vox.Internals.Deserialization {
   public class ReusableSlotReader : ISlotReader {
      private ILengthLimitedForwardDataReader _reader;
      private int _nextSlot;

      public void ResetWithInput(ILengthLimitedForwardDataReader reader) {
         _reader = reader;
         _nextSlot = 0;
      }

      public object ReadObject(int slot) {
         AdvanceUntil(slot);
         return VoxObjectDeserializer.Deserialize(_reader);
      }

      public T ReadObject<T>(int slot) {
         if (typeof(T) == typeof(int)) {
            return (T)(object)ReadNumeric(slot);
         } else if (typeof(T) == typeof(bool)) {
            return (T)(object)ReadBoolean(slot);
         } else if (typeof(T) != typeof(string) && typeof(IEnumerable).IsAssignableFrom(typeof(T))) {
            return ReadCollectionVisitor<T>.Visit(this, slot);
         } else {
            return ReadHelper<T>(slot);
         }
      }

      public byte[] ReadBytes(int slot) => ReadHelper<byte[]>(slot);
      public bool ReadBoolean(int slot) {
         AdvanceUntil(slot);
         var typeId = _reader.ReadTypeId();
         if (typeId == TypeId.BoolTrue) {
            return true;
         } else if (typeId == TypeId.BoolFalse) {
            return false;
         } else {
            throw new SlotTypeMismatchException(TypeId.BoolTrue, typeId);
         }
      }

      public unsafe int ReadNumeric(int slot) {
         AdvanceUntil(slot);
         var typeId = _reader.ReadTypeId();
         if (typeId == TypeId.Int8) {
            return (sbyte)_reader.ReadByte();
         } else if (typeId == TypeId.Int16) {
            byte* pBuff = stackalloc byte[2];
            pBuff[0] = _reader.ReadByte();
            pBuff[1] = _reader.ReadByte();
            return *(short*)pBuff;
         } else if (typeId == TypeId.Int32) {
            byte* pBuff = stackalloc byte[4];
            pBuff[0] = _reader.ReadByte();
            pBuff[1] = _reader.ReadByte();
            pBuff[2] = _reader.ReadByte();
            pBuff[3] = _reader.ReadByte();
            return *(int*)pBuff;
         } else {
            throw new SlotTypeMismatchException(TypeId.Int32, typeId);
         }
      }
      public string ReadString(int slot) => ReadHelper<string>(slot);
      public Type ReadType(int slot) => ReadHelper<Type>(slot);
      public DateTime ReadDateTime(int slot) => ReadHelper<DateTime>(slot);
      public float ReadFloat(int slot) => ReadHelper<float>(slot);
      public double ReadDouble(int slot) => ReadHelper<double>(slot);

      public Guid ReadGuid(int slot) => ReadHelper<Guid>(slot);
      public object ReadNull(int slot) => ReadHelper<NullType>(slot);

      public TCollection ReadCollection<TElement, TCollection>(int slot) where TCollection : IEnumerable<TElement> {
         ISerializationBox box;
         if (GenericTypeUnpacker<TElement>.Definition == typeof(KeyValuePair<,>)) {
            box = ReadNonpolymorphicHelperMapBoxVisitor<TElement>.Visit(this, slot);
         } else {
            box = ReadHelper<ArrayBox<TElement>>(slot);
         }
         var elements = (TElement[])box.Unbox();
         if (typeof(TCollection).IsAssignableFrom(typeof(TElement[]))) {
            return (TCollection)(object)elements;
         }
         var constructor = typeof(TCollection).GetConstructor(new[] { typeof(IEnumerable<TElement>) });
         if (constructor != null) {
            return (TCollection)Activator.CreateInstance(typeof(TCollection), elements);
         }
         var type = typeof(TCollection);
         var instance = (TCollection)Activator.CreateInstance(typeof(TCollection));
         var add = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                       .FirstOrDefault(m => m.Name.Contains("Add") && m.GetParameters().Length == 1) ?? type.GetMethod("Enqueue");
         foreach (var element in elements) {
            add.Invoke(instance, new object[] { element });
         }
         return instance;
      }

      private T ReadHelper<T>(int slot) {
         AdvanceUntil(slot);
         if (typeof(T) == typeof(object)) {
            return (T)VoxObjectDeserializer.Deserialize(_reader);
         } else {
            return VoxObjectDeserializer.DeserializeNonpolymorphic<T>(_reader);
         }
      }

      private void AdvanceUntil(int slot) {
         while (_nextSlot < slot) {
            VoxObjectSkipper.Skip(_reader);
            _nextSlot++;
         }
         _nextSlot++;
      }

      private static class ReadCollectionVisitor<TCollection> {
         private delegate TCollection InvokerFunc(ReusableSlotReader reader, int slot);

         private static readonly InvokerFunc invoker;

         static ReadCollectionVisitor() {
            var method = new DynamicMethod(
               "read_collection_visitor_" + typeof(TCollection).Name,
               typeof(TCollection), new[] { typeof(ReusableSlotReader), typeof(int) },
               typeof(ReusableSlotReader), true);
            var emitter = method.GetILGenerator();
            var elementType = CollectionUnpacker<TCollection>.ElementType;
            var readCollectionMethod = typeof(ReusableSlotReader).GetMethod(nameof(ReadCollection), BindingFlags.Instance | BindingFlags.Public)
                                                                           .MakeGenericMethod(elementType, typeof(TCollection));
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldarg_1);
            emitter.Emit(OpCodes.Call, readCollectionMethod);
            emitter.Emit(OpCodes.Ret);

            invoker = (InvokerFunc)method.CreateDelegate(typeof(InvokerFunc));
         }

         public static TCollection Visit(ReusableSlotReader reader, int slot) => invoker(reader, slot);
      }

      private static class ReadNonpolymorphicHelperMapBoxVisitor<TKeyValuePair> {
         private delegate ISerializationBox InvokerFunc(ReusableSlotReader reader, int slot);

         private static readonly InvokerFunc invoker;

         static ReadNonpolymorphicHelperMapBoxVisitor() {
            var method = new DynamicMethod(
               "mapbox_read_visitor_" + typeof(TKeyValuePair).Name,
               typeof(ISerializationBox), new[] { typeof(ReusableSlotReader), typeof(int) },
               typeof(ReusableSlotReader), true);
            var emitter = method.GetILGenerator();
            var mapBoxType = GenericTypeUnpacker<TKeyValuePair>.Build(typeof(MapBox<,>));
            var readNonpolymorphicHelperMethod = typeof(ReusableSlotReader).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                                                           .First(m => m.Name == nameof(ReusableSlotReader.ReadObject) && m.IsGenericMethod)
                                                                           .MakeGenericMethod(mapBoxType);
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldarg_1);
            emitter.Emit(OpCodes.Call, readNonpolymorphicHelperMethod);
            emitter.Emit(OpCodes.Ret);

            invoker = (InvokerFunc)method.CreateDelegate(typeof(InvokerFunc));
         }

         public static ISerializationBox Visit(ReusableSlotReader reader, int slot) => invoker(reader, slot);
      }
   }
}