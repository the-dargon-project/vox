using System;
using System.Collections;
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

      private static readonly IGenericFlyweightFactory<EmitterFunc> emitWriteCallFuncs
         = GenericFlyweightFactory.ForMethod<EmitterFunc>(
            typeof(ReaderWriterCallHelper<>), nameof(ReaderWriterCallHelper<object>.EmitWrite));


      private static readonly IGenericFlyweightFactory<EmitterFunc> emitReadCallFuncs
         = GenericFlyweightFactory.ForMethod<EmitterFunc>(
            typeof(ReaderWriterCallHelper<>), nameof(ReaderWriterCallHelper<object>.EmitRead));

      public static IReadOnlyDictionary<Type, ITypeSerializer> CreateMany(IReadOnlyList<Type> types) {
         return types.ToDictionary(type => type, Create);
      }

      public static ITypeSerializer<T> Create<T>() => (ITypeSerializer<T>)Create(typeof(T));

      public static ITypeSerializer Create(Type type) {
         var serialize = new DynamicMethod($"__serialize_{type.FullName}", typeof(void), new[] { typeof(ISlotWriter), type }, type);
         var deserialize = new DynamicMethod($"__deserialize_{type.FullName}", typeof(void), new[] { typeof(ISlotReader), type }, type);

         var serializeEmitter = serialize.GetILGenerator();
         var deserializeEmitter = deserialize.GetILGenerator();

         var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
         var fieldsToSerialize = type.GetFields(bindingFlags).Where(x => !x.IsInitOnly).Select((fi, i) => new { Index = i, FieldInfo = fi });
         foreach (var kvp in fieldsToSerialize) {
            EmitFieldSerialization(serializeEmitter, kvp.Index, kvp.FieldInfo);
            EmitFieldDeserialization(deserializeEmitter, kvp.Index, kvp.FieldInfo);
         }

         serializeEmitter.Emit(OpCodes.Ret);
         deserializeEmitter.Emit(OpCodes.Ret);

         var serializerType = typeof(AutoTypeSerializer<>).MakeGenericType(type);
         var serializerConstructor = serializerType.GetConstructors()[0];
         var ctorParams = serializerConstructor.GetParameters();
         var serializeMethodType = ctorParams[0].ParameterType;
         var deserializeMethodType = ctorParams[1].ParameterType;
         return (ITypeSerializer)Activator.CreateInstance(
            serializerType,
            serialize.CreateDelegate(serializeMethodType),
            deserialize.CreateDelegate(deserializeMethodType));
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

      private class AutoTypeSerializer<T> : InlineTypeSerializer<T> {
         public AutoTypeSerializer(Action<ISlotWriter, T> write, Action<ISlotReader, T> read) : base(write, read) {}

         public override bool Equals(object obj) {
            return GetType() == obj?.GetType();
         }
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
            } else if (typeof(T) == typeof(bool)) {
               writerMethod = slotWriter.GetMethod(nameof(ISlotWriter.WriteBoolean));
               readerMethod = slotReader.GetMethod(nameof(ISlotReader.ReadBoolean));
            } else if (typeof(T) == typeof(sbyte) || typeof(T) == typeof(short) || typeof(T) == typeof(int)) {
               writerMethod = slotWriter.GetMethod(nameof(ISlotWriter.WriteNumeric));
               readerMethod = slotReader.GetMethod(nameof(ISlotReader.ReadNumeric));
            } else if (typeof(T) == typeof(Guid)) {
               writerMethod = slotWriter.GetMethod(nameof(ISlotWriter.WriteGuid));
               readerMethod = slotReader.GetMethod(nameof(ISlotReader.ReadGuid));
            } else if (typeof(T) == typeof(string)) {
               writerMethod = slotWriter.GetMethod(nameof(ISlotWriter.WriteString));
               readerMethod = slotReader.GetMethod(nameof(ISlotReader.ReadString));
            } else if (typeof(IEnumerable).IsAssignableFrom(typeof(T))) {
               var elementType = typeof(T).GetInterfaces()
                                          .First(i => i.Name.Contains(nameof(IEnumerable)) && i.IsGenericType)
                                          .GenericTypeArguments[0];
               writerMethod = slotWriter.GetMethod(nameof(ISlotWriter.WriteCollection))
                                        .MakeGenericMethod(elementType, typeof(T));
               readerMethod = slotReader.GetMethod(nameof(ISlotReader.ReadCollection))
                                        .MakeGenericMethod(elementType, typeof(T));
            } else {
               writerMethod = slotWriter.GetMethods()
                                        .First(m => m.Name == nameof(ISlotWriter.WriteObject))
                                        .MakeGenericMethod(typeof(T));

               // TODO: Not Null + Sealed Constraint allows for nonpolymorphic call
               readerMethod = slotReader.GetMethods()
                                        .Where(m => m.Name == nameof(ISlotReader.ReadObject))
                                        .First(m => !m.IsGenericMethod);
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
