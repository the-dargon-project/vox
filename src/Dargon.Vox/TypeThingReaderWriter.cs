using System;
using System.IO;
using System.Text;
using Dargon.Commons.Exceptions;
using Dargon.Vox.Internals.TypePlaceholders;

namespace Dargon.Vox {
   public class TypeThingReaderWriter : IThingReaderWriter {
      private readonly FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache;
      private readonly TypeReader typeReader;

      public TypeThingReaderWriter(FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache, TypeReader typeReader) {
         this.fullTypeBinaryRepresentationCache = fullTypeBinaryRepresentationCache;
         this.typeReader = typeReader;
      }

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         var type = (Type)subject;
         var typeBinaryRepresentation = fullTypeBinaryRepresentationCache.GetOrCompute(type);
         dest.Write(fullTypeBinaryRepresentationCache.GetOrCompute(typeof(Type)));
         dest.Write(typeBinaryRepresentation);
      }

      public object ReadBody(BinaryReader reader) {
         return typeReader.ReadType(reader.ReadByte);
      }
   }

   public class StringThingReaderWriter : IThingReaderWriter {
      private readonly FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache;

      public StringThingReaderWriter(FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache) {
         this.fullTypeBinaryRepresentationCache = fullTypeBinaryRepresentationCache;
      }

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         dest.Write(fullTypeBinaryRepresentationCache.GetOrCompute(typeof(string)));

         var str = (string)subject;
         using(dest.ReserveLength()) {
            var bytes = Encoding.UTF8.GetBytes(str);
            dest.Write(bytes);
         }
      }

      public object ReadBody(BinaryReader reader) {
         var length = VarIntSerializer.ReadVariableInt(reader.ReadByte);
         var bytes = reader.ReadBytes(length);
         return Encoding.UTF8.GetString(bytes);
      }
   }

   public class BoolThingReaderWriter : IThingReaderWriter {
      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         throw new InvalidStateException($"WriteThing invoked on {nameof(BoolThingReaderWriter)}");
      }

      public object ReadBody(BinaryReader reader) {
         throw new InvalidStateException($"ReadBody invoked on {nameof(BoolThingReaderWriter)}");
      }
   }

   public class BoolTrueThingReaderWriter : IThingReaderWriter {
      private readonly FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache;

      public BoolTrueThingReaderWriter(FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache) {
         this.fullTypeBinaryRepresentationCache = fullTypeBinaryRepresentationCache;
      }

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         dest.Write(fullTypeBinaryRepresentationCache.GetOrCompute(typeof(TBoolTrue)));
      }

      public object ReadBody(BinaryReader reader) {
         return true;
      }
   }

   public class BoolFalseThingReaderWriter : IThingReaderWriter {
      private readonly FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache;

      public BoolFalseThingReaderWriter(FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache) {
         this.fullTypeBinaryRepresentationCache = fullTypeBinaryRepresentationCache;
      }

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         dest.Write(fullTypeBinaryRepresentationCache.GetOrCompute(typeof(TBoolFalse)));
      }

      public object ReadBody(BinaryReader reader) {
         return false;
      }
   }

   public class ObjectThingReaderWriter : IThingReaderWriter {

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         throw new NotImplementedException();
      }

      public object ReadBody(BinaryReader reader) {
         throw new NotImplementedException();
      }
   }
}