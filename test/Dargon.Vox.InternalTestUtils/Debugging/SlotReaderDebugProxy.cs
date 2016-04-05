﻿using System;

namespace Dargon.Vox.InternalTestUtils.Debugging {
   public class SlotReaderDebugProxy : ISlotReader {
      private readonly ISlotReader target;
      private int indentation = 0;

      public SlotReaderDebugProxy(ISlotReader target = null) {
         this.target = target ?? new NullSlotReader();
      }

      public object ReadObject(int slot) => ProxyResult(slot, "Read Object", target.ReadObject);
      public T ReadObject<T>(int slot) => ProxyResult(slot, "Read Object of Type " + typeof(T).FullName, target.ReadObject<T>);
      public byte[] ReadBytes(int slot) => ProxyResult(slot, "Read Bytes", target.ReadBytes, arr => arr.Length + " bytes");
      public bool ReadBoolean(int slot) => ProxyResult(slot, "Read Boolean", target.ReadBoolean);
      public int ReadNumeric(int slot) => ProxyResult(slot, "Read Numeric", target.ReadNumeric);
      public string ReadString(int slot) => ProxyResult(slot, "Read String", target.ReadString);
      public Guid ReadGuid(int slot) => ProxyResult(slot, "Read Guid", target.ReadGuid);
      public object ReadNull(int slot) => ProxyResult(slot, "Read Null", target.ReadNull);

      private T ProxyResult<T>(int slot, string desc, Func<int, T> func, Func<T, string> stringify = null) {
         Out(slot, desc);
         indentation++;
         var result = func(slot);
         indentation--;
         var resultString = result == null ? "[Null]" : (stringify == null ? result.ToString() : stringify(result));
         Out(slot, "Result: " + resultString);
         return result;
      }

      private void Out(int slot, string s) {
         Console.WriteLine(new string('\t', indentation) + slot + " => " + s);
      }
   }
}