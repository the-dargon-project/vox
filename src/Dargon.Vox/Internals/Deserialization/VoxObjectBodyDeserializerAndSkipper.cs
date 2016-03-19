using System.Text;
using Dargon.Vox.Data;
using Dargon.Vox.Internals.TypePlaceholders;
using Dargon.Vox.Slots;
using SubReader = Dargon.Vox.Data.ReusableLengthLimitedSubForwardDataReaderProxy;

namespace Dargon.Vox.Internals.Deserialization {
   public static class VoxObjectBodyDeserializerAndSkipper<T> {
      public static T Deserialize(ILengthLimitedForwardDataReader input) {
         if (typeof(T) == typeof(byte[])) {
            var count = input.ReadVariableInt();
            return (T)(object)input.ReadBytes(count);
         } else if (typeof(T) == typeof(string)) {
            var length = input.ReadVariableInt();
            var buffer = input.ReadBytes(length);
            return (T)(object)Encoding.ASCII.GetString(buffer, 0, length);
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
         } else if (typeof(T) == typeof(string)) {
            var count = input.ReadVariableInt();
            input.SkipBytes(count);
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
   }
}