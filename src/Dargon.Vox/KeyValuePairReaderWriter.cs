using System;
using System.Diagnostics;
using System.IO;
using Dargon.Vox.Utilities;

namespace Dargon.Vox {
   public class KeyValuePairReaderWriter : IThingReaderWriter {
      private readonly FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache;
      private readonly ThisIsTotesTheRealLegitThingReaderWriterThing thisIsTotesTheRealLegitThingReaderWriterThing;
      private readonly Type userKvpType;
      private readonly Type userKeyType;
      private readonly Type userValueType;
      private readonly Type simplifiedKvpType;

      public KeyValuePairReaderWriter(FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache, ThisIsTotesTheRealLegitThingReaderWriterThing thisIsTotesTheRealLegitThingReaderWriterThing, Type userKvpType) {
         this.fullTypeBinaryRepresentationCache = fullTypeBinaryRepresentationCache;
         this.thisIsTotesTheRealLegitThingReaderWriterThing = thisIsTotesTheRealLegitThingReaderWriterThing;

         Trace.Assert(userKvpType.IsGenericType && userKvpType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.KeyValuePair<,>));
         this.userKvpType = userKvpType;
         var genericArguments = userKvpType.GetGenericArguments();
         this.userKeyType = genericArguments[0];
         this.userValueType = genericArguments[1];
         this.simplifiedKvpType = TypeSimplifier.SimplifyType(userKvpType);
      }

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         dest.Write(fullTypeBinaryRepresentationCache.GetOrCompute(simplifiedKvpType));

         using (dest.ReserveLength()) {
            dynamic s = subject;
            thisIsTotesTheRealLegitThingReaderWriterThing.WriteThing(dest, s.Key);
            thisIsTotesTheRealLegitThingReaderWriterThing.WriteThing(dest, s.Value);
         }
      }

      public object ReadBody(BinaryReader reader) {
         var length = VarIntSerializer.ReadVariableInt(reader.ReadByte);
         using (var ms = new MemoryStream(reader.ReadBytes(length)))
         using (var msReader = new BinaryReader(ms)) {
            var key = thisIsTotesTheRealLegitThingReaderWriterThing.ReadThing(msReader, userKeyType);
            var value = thisIsTotesTheRealLegitThingReaderWriterThing.ReadThing(msReader, userValueType);
            return Activator.CreateInstance(userKvpType, key, value);
         }
      }
   }
}