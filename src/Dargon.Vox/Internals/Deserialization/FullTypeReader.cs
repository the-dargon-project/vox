using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Dargon.Commons;
using Dargon.Vox.Data;
using Dargon.Vox.Utilities;

namespace Dargon.Vox.Internals.Deserialization {
   public static class FullTypeReader {
      private const int kInitialTypeIdBufferSize = 16;
      private const int kTypeIdBufferGrowthRate = 2;
      private const int kBufferSentinel = int.MinValue;

      private static readonly IncrementalDictionary<int[], Type> _fullTypeByTypeId = new IncrementalDictionary<int[], Type>(new CustomIntArrayComparer());
      [ThreadStatic] private static int[] _typeIdBuffer;

      public static Type ReadFullType(this IForwardDataReader reader) {
         int typeIdBufferIndicesUsed;
         var typeIdBuffer = GetTypeIdBufferWithReadTypeId(reader, out typeIdBufferIndicesUsed);
         var type = _fullTypeByTypeId.GetOrAdd(typeIdBuffer, () => CloneBuffer(typeIdBuffer), DeserializeType);
         for (var i = 0; i < typeIdBufferIndicesUsed; i++) {
            typeIdBuffer[i] = kBufferSentinel;
         }
         return type;
      }

      private static int[] GetTypeIdBufferWithReadTypeId(IForwardDataReader reader, out int bufferIndicesUsed) {
         var typeIdBuffer = GetTypeIdBuffer();
         int genericTypeIdDepth = 0;
         int nextIndex = 0;
         do {
            var typeId = reader.ReadTypeId();
            if (typeId == TypeId.GenericTypeIdBegin) {
               genericTypeIdDepth++;
            } else if (typeId == TypeId.GenericTypeIdEnd) {
               genericTypeIdDepth--;
            } else {
               if (nextIndex == typeIdBuffer.Length) {
                  typeIdBuffer = ResizeTypeIdBuffer();
               }
               typeIdBuffer[nextIndex] = (int)typeId;
               nextIndex++;
            }
         } while (genericTypeIdDepth > 0);
         bufferIndicesUsed = nextIndex;
         return typeIdBuffer;
      }

      private static int[] CloneBuffer(int[] typeIdBuffer) {
         var cutoffLength = Array.IndexOf(typeIdBuffer, kBufferSentinel);
         if (cutoffLength < 0) {
            cutoffLength = typeIdBuffer.Length;
         }
         return typeIdBuffer.SubArray(0, cutoffLength);
      }

      private static Type DeserializeType() {
         var typeIds = GetTypeIdBuffer();
         var s = new Stack<Type>();
         var startIndexExclusive = Array.IndexOf(typeIds, kBufferSentinel);
         if (startIndexExclusive < 0) {
            startIndexExclusive = typeIds.Length;
         }
         for (var i = startIndexExclusive - 1; i >= 0; i--) {
            var typeId = typeIds[i];
            var type = TypeRegistry.Lookup(typeId);
            if (type.IsGenericTypeDefinition) {
               var genericArgs = type.GetGenericArguments().Map(s.Pop);
               s.Push(type.MakeGenericType(genericArgs));
            } else {
               s.Push(type);
            }
         }
         if (s.Count != 1) {
            throw new SerializationException("Incomplete type serialization.");
         }
         return s.Pop();
      }

      private static int[] GetTypeIdBuffer() {
         if (_typeIdBuffer == null) {
            _typeIdBuffer = Util.Generate(kInitialTypeIdBufferSize, i => kBufferSentinel);
         }
         return _typeIdBuffer;
      }

      private static int[] ResizeTypeIdBuffer() {
         var newTypeIdBuffer = new int[_typeIdBuffer.Length * kTypeIdBufferGrowthRate];
         for (var i = 0; i < _typeIdBuffer.Length; i++) {
            newTypeIdBuffer[i] = _typeIdBuffer[i];
         }
         for (var i = _typeIdBuffer.Length; i < newTypeIdBuffer.Length; i++) {
            newTypeIdBuffer[i] = kBufferSentinel;
         }
         return _typeIdBuffer = newTypeIdBuffer;
      }

      public class CustomIntArrayComparer : IEqualityComparer<int[]> {
         public bool Equals(int[] x, int[] y) {
            if (x == null && y == null) {
               return true;
            } else if (x == null || y == null) {
               return false;
            } else {
               // Swap so X has lowest length
               if (x.Length > y.Length) {
                  var temp = x;
                  x = y;
                  y = temp;
               }
               
               for (var i = 0; i < x.Length; i++) {
                  if (x[i] != y[i]) {
                     return false;
                  }
               }
               if (y.Length > x.Length) {
                  if (y[x.Length] != kBufferSentinel) {
                     return false;
                  }
               }
               return true;
            }
         }

         public int GetHashCode(int[] arr) {
            int hash = 1;
            for (var i = 0; i < arr.Length && arr[i] != kBufferSentinel; i++) {
               hash = hash * 23 + arr[i];
            }
            return hash;
         }
      }
   }
}
