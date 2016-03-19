using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Dargon.Vox.Utilities;

namespace Dargon.Vox.Internals {
   public static class AutoTypeSerializerFactory {
      private delegate void EmitterFunc(ILGenerator ilGenerator);

      private static readonly GenericFlyweightFactory<EmitterFunc> emitWriteCallFuncs
         = GenericFlyweightFactory.ForMethod<EmitterFunc>(
            typeof(ReaderWriterCallHelper<>), nameof(ReaderWriterCallHelper<object>.EmitWrite));


      private static readonly GenericFlyweightFactory<EmitterFunc> emitReadCallFuncs
         = GenericFlyweightFactory.ForMethod<EmitterFunc>(
            typeof(ReaderWriterCallHelper<>), nameof(ReaderWriterCallHelper<object>.EmitRead));

      public static IReadOnlyDictionary<Type, ITypeSerializer> CreateMany(IReadOnlyList<Type> types) {
         return types.ToDictionary(type => type, type => CreateTypeSerializer(type));
      }

      private static ITypeSerializer CreateTypeSerializer(Type type) {
         var serialize = new DynamicMethod($"__serialize_{type.FullName}", typeof(void), new[] { typeof(ISlotWriter), type }, type);
         var deserialize = new DynamicMethod($"__deserialize_{type.FullName}", typeof(void), new[] { typeof(ISlotWriter), type }, type);

         var serializeEmitter = serialize.GetILGenerator();
         var deserializeEmitter = deserialize.GetILGenerator();

         var fieldsToSerialize = type.GetFields().Where(x => !x.IsInitOnly).Select((fi, i) => new { Index = i, FieldInfo = fi });
         foreach (var kvp in fieldsToSerialize) {
            EmitFieldSerialization(serializeEmitter, kvp.Index, kvp.FieldInfo);
            EmitFieldDeserialization(deserializeEmitter, kvp.Index, kvp.FieldInfo);
         }

         var serializerType = typeof(InlineTypeSerializer<>).MakeGenericType(type);
         return (ITypeSerializer)Activator.CreateInstance(serializerType, serialize, deserialize);
      }

      private static void EmitFieldSerialization(ILGenerator serializeEmitter, int slot, FieldInfo field) {
         // Push invocation target (slot writer) and slot number onto evaluation stack.
         serializeEmitter.Emit(OpCodes.Ldarg_0);
         serializeEmitter.Emit(OpCodes.Ldc_I4, slot);

         // Push subject-field value onto evaluation stack.
         serializeEmitter.Emit(OpCodes.Ldarg_1);
         serializeEmitter.Emit(OpCodes.Ldfld, field);

         // Push call of Write-To-Slot method.
         emitWriteCallFuncs.Get(field.FieldType)(serializeEmitter);
      }

      private static void EmitFieldDeserialization(ILGenerator deserializeEmitter, int slot, FieldInfo field) {
         // Push subject onto stack
         deserializeEmitter.Emit(OpCodes.Ldarg_1);
         
         // Push invocation target (slot reader) and slot number onto evaluation stack
         deserializeEmitter.Emit(OpCodes.Ldarg_0);
         deserializeEmitter.Emit(OpCodes.Ldc_I4, slot);

         // Invoke reader method, which places read result onto evaluation stack
         emitReadCallFuncs.Get(field.FieldType)(deserializeEmitter);

         // Store the read value to subject's field
         deserializeEmitter.Emit(OpCodes.Stfld, field);
      }

      private static class ReaderWriterCallHelper<T> {
         private static readonly MethodInfo writerMethod;
         private static readonly MethodInfo readerMethod;

         static ReaderWriterCallHelper() {
            var slotWriter = typeof(ISlotWriter);
            var slotReader = typeof(ISlotReader);
            if (typeof(T) == typeof(byte[])) {
               writerMethod = slotWriter.GetMethod(nameof(ISlotWriter.WriteBytes));
               readerMethod = slotReader.GetMethod(nameof(ISlotReader.ReadBytes));
            } else {
               throw new NotImplementedException();
            }
         }

         public static void EmitWrite(ILGenerator ilGenerator) {
            ilGenerator.Emit(OpCodes.Call, writerMethod);
         }

         public static void EmitRead(ILGenerator ilGenerator) {
            ilGenerator.Emit(OpCodes.Call, readerMethod);
         }
      }
   }
}
