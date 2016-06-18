using System;
using System.Collections.Generic;
using Dargon.Commons;
using Dargon.Vox.Utilities;

namespace Dargon.Vox {
   public class FullTypeBinaryRepresentationCache {
      private readonly CopyOnAddDictionary<Type, byte[]> binaryRepresentationByType = new CopyOnAddDictionary<Type, byte[]>();
      private readonly TypeRegistry typeRegistry;

      public FullTypeBinaryRepresentationCache(TypeRegistry typeRegistry) {
         this.typeRegistry = typeRegistry;
      }

      public byte[] GetOrCompute(Type type) {
         return binaryRepresentationByType.GetOrAdd(type, ComputeBinaryRepresentation);
      }

      private byte[] ComputeBinaryRepresentation(Type arg) {
         var result = new List<byte>();
         var s = new Stack<Type>();
         s.Push(arg);
         while (s.Count != 0) {
            var t = s.Pop();
            if (t.IsArray) {
               s.Push(typeof(Array));
               s.Push(t.GetElementType());
            } else if (t.IsGenericType && !t.IsGenericTypeDefinition) {
               s.Push(t.GetGenericTypeDefinition());
               t.GetGenericArguments().ForEach(s.Push);
            } else {
               var typeId = typeRegistry.GetTypeIdOrThrow(t);
               VarIntSerializer.WriteVariableInt(result.Add, typeId);
            }
         }
         var lengthBuffer = new List<byte>();
         VarIntSerializer.WriteVariableInt(lengthBuffer.Add, result.Count);
         // shits given = 0
         for (var i = 0; i < lengthBuffer.Count; i++) {
            result.Insert(i, lengthBuffer[i]);
         }
         return result.ToArray();
      }
   }
}