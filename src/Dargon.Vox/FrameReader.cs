using System;
using System.IO;
using System.Text;
using Dargon.Vox.Utilities;

namespace Dargon.Vox {
   public class FrameReader {
      private readonly ThisIsTotesTheRealLegitThingReaderWriterThing thisIsTotesTheRealLegitThingReaderWriterThing;

      public FrameReader(ThisIsTotesTheRealLegitThingReaderWriterThing thisIsTotesTheRealLegitThingReaderWriterThing) {
         this.thisIsTotesTheRealLegitThingReaderWriterThing = thisIsTotesTheRealLegitThingReaderWriterThing;
      }

      public object ReadFrame(Stream stream, Type hintType) {
         using (var streamReader = new BinaryReader(stream, Encoding.UTF8, true)) {
            var frameLength = VarIntSerializer.ReadVariableInt(streamReader.ReadByte);
            var thingBuffer = new MemoryStream(streamReader.ReadBytes(frameLength));

            using (var thingReader = new BinaryReader(thingBuffer)) {
               return thisIsTotesTheRealLegitThingReaderWriterThing.ReadThing(thingReader, hintType);
            }
         }
      }
   }
}