﻿using Dargon.Vox.Data;
using Dargon.Vox.Internals.TypePlaceholders;
using Dargon.Vox.Slots;

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

      public T ReadObject<T>(int slot) => ReadNonpolymorphicHelper<T>(slot);
      public byte[] ReadBytes(int slot) => ReadNonpolymorphicHelper<byte[]>(slot);
      public int ReadNumeric(int slot) {
         var typeId = _reader.ReadTypeId();
         if (typeId == TypeId.Int8) {
            return _reader.ReadByte();
         } else if (typeId == TypeId.Int16) {
            return _reader.ReadByte() | (_reader.ReadByte() << 8);
         } else if (typeId == TypeId.Int32) {
            return _reader.ReadByte() | (_reader.ReadByte() << 8) | (_reader.ReadByte() << 16) | (_reader.ReadByte() << 24);
         } else {
            throw new SlotTypeMismatchException(TypeId.Int32, typeId);
         }
      }
      public string ReadString(int slot) => ReadNonpolymorphicHelper<string>(slot);
      public object ReadNull(int slot) => ReadNonpolymorphicHelper<NullType>(slot);

      private T ReadNonpolymorphicHelper<T>(int slot) {
         AdvanceUntil(slot);
         return VoxObjectDeserializer.DeserializeNonpolymorphic<T>(_reader);
      }

      private void AdvanceUntil(int slot) {
         while (_nextSlot < slot) {
            VoxObjectSkipper.Skip(_reader);
            _nextSlot++;
         }
         _nextSlot++;
      }
   }
}