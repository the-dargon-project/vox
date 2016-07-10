using System;
using System.Collections.Generic;
using Dargon.Commons;
using Dargon.Commons.Collections;
using Dargon.Commons.Comparers;
using Dargon.Commons.Exceptions;

namespace Dargon.Vox {
   public class TypeReader {
      private readonly TypeRegistry typeRegistry;
      private readonly CopyOnAddDictionary<int[], Type> typesByTypeIdParts = new CopyOnAddDictionary<int[], Type>(new IntArrayEqualityComparator());

      public TypeReader(TypeRegistry typeRegistry) {
         this.typeRegistry = typeRegistry;
      }

      public Type ReadType(Func<byte> readByte) {
         var typeCount = VarIntSerializer.ReadVariableInt(readByte);
         var typeIds = Util.Generate(typeCount, () => VarIntSerializer.ReadVariableInt(readByte));

         return typesByTypeIdParts.GetOrAdd(
            typeIds,
            UnpackTypes);
      }

      public Type UnpackTypes(int[] typeIds) {
         var types = typeIds.Map(typeRegistry.GetTypeOrThrow);

         // special case: if types has 1 element and it's a generic type definition, return it
         if (types.Length == 1 && types[0].IsGenericTypeDefinition) {
            return types[0];
         }

         var s = new Stack<Type>();
         foreach (var type in types) {
            if (type == typeof(Array)) {
               s.Push(s.Pop().MakeArrayType());
            } else if (type.IsGenericTypeDefinition) {
               var genericArgs = Util.Generate(type.GetGenericArguments().Length, s.Pop);
               s.Push(type.MakeGenericType(genericArgs));
            } else {
               s.Push(type);
            }
         }
         if (s.Count != 1) {
            throw new InvalidStateException($"Expected s.Count == 1 but found {s.Count}.");
         }
         return s.Pop();
      }
   }

   public class IntArrayEqualityComparator : IEqualityComparer<int[]> {
      public bool Equals(int[] x, int[] y) {
         if (x.Length != y.Length) {
            return false;
         }
         for (var i = 0; i < x.Length && i < y.Length; i++) {
            if (x[i] != y[i]) {
               return false;
            }
         }
         return true;
      }

      public int GetHashCode(int[] obj) {
         var h = 13;
         for (var i = 0; i < obj.Length; i++) {
            h = h * 17 + obj[i];
         }
         return h;
      }
   }
}