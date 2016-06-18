using System;
using Dargon.Vox.Data;
using Dargon.Vox.Internals.TypePlaceholders;
using System.Text;
using Dargon.Vox.Internals.TypePlaceholders.Boxes;
using SubReader = Dargon.Vox.Data.ReusableLengthLimitedSubForwardDataReaderProxy;

namespace Dargon.Vox.Internals.Deserialization {
   public static class VoxObjectBodyDeserializerAndSkipper<T> {
      [ThreadStatic] private static byte[] stringBuffer;
      [ThreadStatic] private static byte[] guidBuffer;
      private static ReusableLengthLimitedSubForwardDataReaderProxy typeDataReaderProxy = new ReusableLengthLimitedSubForwardDataReaderProxy();

      public unsafe static T Deserialize(ILengthLimitedForwardDataReader input) {
         if (typeof(T) == typeof(byte[])) {
            var count = input.ReadVariableInt();
            return (T)(object)input.ReadBytes(count);
         } else if (typeof(T) == typeof(bool)) {
            throw new InvalidOperationException("Impossible case - should be handled in " + nameof(VoxObjectDeserializer));
         } else if (typeof(T) == typeof(sbyte)) {
            return (T)(object)(sbyte)input.ReadByte();
         } else if (typeof(T) == typeof(short)) {
            return (T)(object)(short)(input.ReadByte() | (input.ReadByte() << 8));
         } else if (typeof(T) == typeof(int)) {
            return (T)(object)(input.ReadByte() | (input.ReadByte() << 8) | (input.ReadByte() << 16) | (input.ReadByte() << 24));
         } else if (typeof(T) == typeof(string)) {
            var length = input.ReadVariableInt();
            EnsureSize(ref stringBuffer, length);
            input.ReadBytes(length, stringBuffer);
            return (T)(object)Encoding.UTF8.GetString(stringBuffer, 0, length);
         } else if (typeof(T) == typeof(Type)) {
            var length = input.ReadVariableInt();
            typeDataReaderProxy.SetTarget(input);
            typeDataReaderProxy.SetBytesRemaining(length);
            var type = typeDataReaderProxy.ReadFullType();
            return (T)(object)type;
         } else if (typeof(T) == typeof(DateTime)) {
            return (T)(object)new DateTime((long)input.ReadUInt64());
         } else if (typeof(T) == typeof(float)) {
            var data = input.ReadUInt32();
            return (T)(object)*(float*)&data;
         } else if (typeof(T) == typeof(double)) {
            var data = input.ReadUInt64();
            return (T)(object)*(double*)&data;
         } else if (typeof(T) == typeof(Guid)) {
            EnsureSize(ref guidBuffer, 16);
            input.ReadBytes(16, guidBuffer);
            return (T)(object)new Guid(guidBuffer);
         } else if (typeof(T) == typeof(NullType)) {
            return (T)(object)null;
         } else {
            return (T)DeserializeObject(input);
         }
      }

      public static void Skip(ILengthLimitedForwardDataReader input) {
         if (typeof(T) == typeof(byte[])) {
            var count = input.ReadVariableInt();
            input.SkipBytes(count);
         } else if (typeof(T) == typeof(bool) || typeof(T) == typeof(BoolFalse)) {
            // no body
         } else if (typeof(T) == typeof(sbyte)) {
            input.SkipBytes(sizeof(sbyte));
         } else if (typeof(T) == typeof(short)) {
            input.SkipBytes(sizeof(short));
         } else if (typeof(T) == typeof(int)) {
            input.SkipBytes(sizeof(int));
         } else if (typeof(T) == typeof(string)) {
            var count = input.ReadVariableInt();
            input.SkipBytes(count);
         } else if (typeof(T) == typeof(DateTime)) {
            input.SkipBytes(8);
         } else if (typeof(T) == typeof(float)) {
            input.SkipBytes(4);
         } else if (typeof(T) == typeof(double)) {
            input.SkipBytes(8);
         } else if (typeof(T) == typeof(Guid)) {
            input.SkipBytes(16);
         } else if (typeof(T) == typeof(NullType)) {
            // no body
         }
      }

      private static object DeserializeObject(ILengthLimitedForwardDataReader input) {
         var bodyLength = input.ReadVariableInt();

         ReusableSlotReader slotReader;
         SubReader subReader;
         ReusableSlotReaderAndSubReaderPool.Take(out slotReader, out subReader);
         try {
            subReader.SetTarget(input);
            subReader.SetBytesRemaining(bodyLength);
            slotReader.ResetWithInput(subReader);

            var instance = VoxObjectActivator<T>.CreateInstance();
            var serializer = TypeSerializerRegistry<T>.Serializer;
            serializer.Deserialize(slotReader, instance);
            return instance;
         } finally {
            ReusableSlotReaderAndSubReaderPool.Return(slotReader, subReader);
         }
      }

      private static void EnsureSize(ref byte[] buffer, int length) {
         if (buffer == null || buffer.Length < length) {
            buffer = new byte[length];
         }
      }
   }
}