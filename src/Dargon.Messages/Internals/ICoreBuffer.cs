using System.Collections.Generic;
using System.IO;

namespace Dargon.Messages.Internals {
   public interface ICoreSerializerReader {
      long Length { get; }

      byte ReadByte();
      byte[] ReadBytes(int length);
      byte[] ReadRemaining();
   }

   public interface ICoreSerializerWriter {
      void WriteByte(byte val);
      void WriteBytes(byte[] val);
      void WriteBytes(byte[] val, int offset, int length);
   }

   public interface ICoreBuffer : ICoreSerializerReader, ICoreSerializerWriter {
      void CopyTo(ICoreSerializerWriter writer);
   }

   public unsafe class CoreBuffer : ICoreBuffer {
      private readonly MemoryStream dataStream;

      public CoreBuffer(MemoryStream dataStream) {
         this.dataStream = dataStream;
      }

      public long Length => dataStream.Length;

      public byte ReadByte() {
         var b = dataStream.ReadByte();
         if (b == -1) {
            throw new EndOfStreamException();
         }
         return (byte)(b & 0xFF);
      }
      
      public byte[] ReadBytes(int length) {
         var buffer = new byte[length];
         int offset = 0;
         while (offset < buffer.Length) {
            offset += dataStream.Read(buffer, offset, length - offset);
         }
         return buffer;
      }

      public byte[] ReadRemaining() {
         var bytesRemaining = dataStream.Length - dataStream.Position;
         return ReadBytes((int)bytesRemaining);
      }

      public void WriteByte(byte val) {
         dataStream.WriteByte(val);
      }

      public void WriteBytes(byte[] val) {
         dataStream.Write(val, 0, val.Length);
      }

      public void WriteBytes(byte[] val, int offset, int length) {
         dataStream.Write(val, offset, length);
      }

      public void CopyTo(ICoreSerializerWriter writer) {
         writer.WriteBytes(dataStream.GetBuffer(), 0, (int)dataStream.Length);
      }
   }
}