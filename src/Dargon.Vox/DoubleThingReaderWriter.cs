using System;
using System.IO;

namespace Dargon.Vox {
   public unsafe class DoubleThingReaderWriter : IThingReaderWriter {
      private readonly FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache;
      [ThreadStatic] private static byte[] writerBuffers;

      public DoubleThingReaderWriter(FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache) {
         this.fullTypeBinaryRepresentationCache = fullTypeBinaryRepresentationCache;
      }

      public byte[] GetWriterBuffer() {
         var result = writerBuffers;
         if (result == null) {
            return writerBuffers = new byte[8];
         }
         return result;
      }

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         dest.Write(fullTypeBinaryRepresentationCache.GetOrCompute(typeof(double)));

         var buffer = GetWriterBuffer();
         fixed (byte* pBuffer = buffer) {
            *(double*)pBuffer = (double)subject;
         }
         dest.Write(buffer);
      }

      public object ReadBody(BinaryReader reader) {
         var pBuffer = stackalloc byte[8];
         pBuffer[0] = reader.ReadByte();
         pBuffer[1] = reader.ReadByte();
         pBuffer[2] = reader.ReadByte();
         pBuffer[3] = reader.ReadByte();
         pBuffer[4] = reader.ReadByte();
         pBuffer[5] = reader.ReadByte();
         pBuffer[6] = reader.ReadByte();
         pBuffer[7] = reader.ReadByte();
         return *(double*)pBuffer;
      }
   }

   public class GuidThingReaderWriter : IThingReaderWriter {
      private readonly FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache;

      public GuidThingReaderWriter(FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache) {
         this.fullTypeBinaryRepresentationCache = fullTypeBinaryRepresentationCache;
      }

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         dest.Write(fullTypeBinaryRepresentationCache.GetOrCompute(typeof(Guid)));

         dest.Write(((Guid)subject).ToByteArray());
      }

      public object ReadBody(BinaryReader reader) {
         return new Guid(reader.ReadBytes(16));
      }
   }

   public unsafe class DateTimeThingReaderWriter : IThingReaderWriter {
      private readonly FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache;
      [ThreadStatic]
      private static byte[] writerBuffer;

      public DateTimeThingReaderWriter(FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache) {
         this.fullTypeBinaryRepresentationCache = fullTypeBinaryRepresentationCache;
      }


      public byte[] GetWriterBuffer() {
         var result = writerBuffer;
         if (result == null) {
            return writerBuffer = new byte[8];
         }
         return result;
      }

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         dest.Write(fullTypeBinaryRepresentationCache.GetOrCompute(typeof(DateTime)));

         var buffer = GetWriterBuffer();
         fixed (byte* pBuffer = buffer)
         {
            *(long*)pBuffer = ((DateTime)subject).Ticks;
         }
         dest.Write(buffer);
      }

      public object ReadBody(BinaryReader reader) {
         return new DateTime(reader.ReadInt64());
      }
   }

   public unsafe class TimeSpanThingReaderWriter : IThingReaderWriter {
      private readonly FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache;
      [ThreadStatic] private static byte[] writerBuffer;

      public TimeSpanThingReaderWriter(FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache) {
         this.fullTypeBinaryRepresentationCache = fullTypeBinaryRepresentationCache;
      }


      public byte[] GetWriterBuffer() {
         var result = writerBuffer;
         if (result == null) {
            return writerBuffer = new byte[8];
         }
         return result;
      }

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         dest.Write(fullTypeBinaryRepresentationCache.GetOrCompute(typeof(TimeSpan)));

         var buffer = GetWriterBuffer();
         fixed (byte* pBuffer = buffer) {
            *(long*)pBuffer = ((TimeSpan)subject).Ticks;
         }
         dest.Write(buffer);
      }

      public object ReadBody(BinaryReader reader) {
         return new TimeSpan(reader.ReadInt64());
      }
   }
}