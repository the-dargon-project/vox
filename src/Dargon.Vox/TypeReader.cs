using System;
using System.Collections.Generic;
using Dargon.Commons;
using Dargon.Commons.Exceptions;

namespace Dargon.Vox {
   public class TypeReader {
      private readonly TypeRegistry typeRegistry;

      public TypeReader(TypeRegistry typeRegistry) {
         this.typeRegistry = typeRegistry;
      }

      public Type ReadType(Func<byte> readByte) {
         var typeCount = VarIntSerializer.ReadVariableInt(readByte);
         var types = Util.Generate(typeCount, () => {
            var typeId = VarIntSerializer.ReadVariableInt(readByte);
            return typeRegistry.GetTypeOrThrow(typeId);
         });
         
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
}