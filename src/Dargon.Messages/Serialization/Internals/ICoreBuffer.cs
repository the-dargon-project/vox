using System.Collections.Generic;
using System.IO;
using Dargon.Messages.Serialization.Internals;

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

   public class CoreBuffer : ICoreBuffer {
      private readonly MemoryStream stream;

      public CoreBuffer(MemoryStream stream) {
         this.stream = stream;
      }

      public long Length => stream.Length;

      public byte ReadByte() {
         var b = stream.ReadByte();
         if (b == -1) {
            throw new EndOfStreamException();
         }
         return (byte)(b & 0xFF);
      }
      
      public byte[] ReadBytes(int length) {
         var buffer = new byte[length];
         int offset = 0;
         while (offset < buffer.Length) {
            offset += stream.Read(buffer, offset, length - offset);
         }
         return buffer;
      }

      public byte[] ReadRemaining() {
         var bytesRemaining = stream.Length - stream.Position;
         return ReadBytes((int)bytesRemaining);
      }

      public void WriteByte(byte val) {
         stream.WriteByte(val);
      }

      public void WriteBytes(byte[] val) {
         stream.Write(val, 0, val.Length);
      }

      public void WriteBytes(byte[] val, int offset, int length) {
         stream.Write(val, offset, length);
      }

      public void CopyTo(ICoreSerializerWriter writer) {
         writer.WriteBytes(stream.GetBuffer(), 0, (int)stream.Length);
      }
   }
}