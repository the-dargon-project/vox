﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dargon.Vox.Data {
   public static class IntegerSerializationStatics {
      public static uint ReadUInt32(this IForwardDataReader reader) {
         var a = (uint)reader.ReadByte();
         var b = (uint)reader.ReadByte();
         var c = (uint)reader.ReadByte();
         var d = (uint)reader.ReadByte();
         return a | (b << 8) | (c << 16) | (d << 24);
      }

      public static ulong ReadUInt64(this IForwardDataReader reader) {
         ulong low = ReadUInt32(reader);
         ulong high = ReadUInt32(reader);
         return low | (high << 32);
      }
   }
}