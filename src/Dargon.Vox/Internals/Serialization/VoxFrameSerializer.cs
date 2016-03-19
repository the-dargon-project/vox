using Dargon.Vox.Data;
using Dargon.Vox.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Dargon.Vox.Internals.Serialization {
   public static class VoxFrameSerializer {
      private delegate void NonpolymorphicSerializeFunction(object subject, Stream output, SerializerOptions options);

      private static readonly GenericFlyweightFactory<NonpolymorphicSerializeFunction> serializeFactory
         = GenericFlyweightFactory.ForMethod<NonpolymorphicSerializeFunction>(
            typeof(VoxFrameSerializer), nameof(SerializeNonpolymorphic));

      [ThreadStatic] private static ReusableStreamToForwardDataWriterAdapter writer;

      public static void Serialize(object subject, Stream output, SerializerOptions options) {
         serializeFactory.Get(subject.GetType())(subject, output, options);
      }

      public static void SerializeNonpolymorphic<T>(object subject, Stream output, SerializerOptions options) {
         writer = writer ?? new ReusableStreamToForwardDataWriterAdapter();
         writer.SetTarget(output);
         VoxFrameSerializer<T>.SerializeNonpolymorphic((T)subject, writer, options);
      }
   }

   public static class VoxFrameSerializer<T> {
      [ThreadStatic] private static TwoPassFrameWriter<T> twoPassFrameWriter; 

      public static void SerializeNonpolymorphic(T subject, IForwardDataWriter writer, SerializerOptions options) {
         twoPassFrameWriter = twoPassFrameWriter ?? new TwoPassFrameWriter<T>();
         twoPassFrameWriter.RunPasses(subject, writer);
      }
   }

   public class TwoPassFrameWriter<T> : ISlotWriter {
      private readonly Stack<int> objectLengthStack = new Stack<int>();
      private readonly Queue<int> stringLengthQueue = new Queue<int>(); 
      private readonly FakeForwardDataWriter fakeWriter = new FakeForwardDataWriter();
      private bool isDryPass;
      private IForwardDataWriter output;
      private byte[] stringBuffer = new byte[16];

      public void RunPasses(T subject, IForwardDataWriter writer) {
         PrepareDryPass();
         WriteObject<T>(0, subject);
         PrepareRealPass(writer);
         WriteObject<T>(0, subject);
         Trace.Assert(objectLengthStack.Count == 0);
      }

      private void PrepareDryPass() {
         objectLengthStack.Clear();
         isDryPass = true;
         output = fakeWriter;
         fakeWriter.Position = 0;
      }

      private void PrepareRealPass(IForwardDataWriter writer) {
         isDryPass = false;
         output = writer;

         // write frame header
         const int kReservedSize = sizeof(uint);
         writer.WriteVariableInt(fakeWriter.Position + kReservedSize);
         writer.WriteByte(0);
         writer.WriteByte(0);
         writer.WriteByte(0);
         writer.WriteByte(0);
      }

      public void WriteObject<U>(int slot, U subject) {
         if (typeof(U) == typeof(byte[])) {
            WriteBytes(slot, (byte[])(object)subject);
         } else if (typeof(U) == typeof(string)) {
            WriteString(slot, (string)(object)subject);
         } else {
            if (subject == null) {
               WriteNull(slot);
            } else {
               // Custom object writer pushes to length queue on its own
               WriteCustomObject(slot, subject);
            }
         }
      }

      public void WriteCustomObject<U>(int slot, U subject) {
         if (isDryPass) {
            int savedWriterPosition = fakeWriter.Position;
            TypeSerializerRegistry<U>.Serializer.Serialize(this, subject);
            var length = fakeWriter.Position - savedWriterPosition;
            objectLengthStack.Push(length);
            fakeWriter.Position += FullTypeToBinaryRepresentationCache<U>.Serialization.Length;
            fakeWriter.Position += length.ComputeVariableIntLength();
         } else {
            output.WriteBytes(FullTypeToBinaryRepresentationCache<U>.Serialization);
            output.WriteVariableInt(objectLengthStack.Pop());
            TypeSerializerRegistry<U>.Serializer.Serialize(this, subject);
         }
      }

      public void WriteBytes(int slot, byte[] bytes) {
         if (bytes == null) {
            WriteNull(slot);
         } else {
            WriteBytes(slot, bytes, 0, bytes.Length);
         }
      }

      public void WriteBytes(int slot, byte[] bytes, int offset, int length) {
         if (isDryPass) {
            fakeWriter.Position += TypeId.ByteArray.ComputeTypeIdLength() + length.ComputeVariableIntLength() + length;
         } else {
            output.WriteTypeId(TypeId.ByteArray);
            output.WriteVariableInt(length);
            output.WriteBytes(bytes, offset, length);
         }
      }

      public unsafe void WriteString(int slot, string str) {
         if (str == null) {
            WriteNull(slot);
         } else if (isDryPass) {
            var bodyByteCount = Encoding.UTF8.GetByteCount(str);
            stringLengthQueue.Enqueue(bodyByteCount);
            fakeWriter.Position += TypeId.String.ComputeTypeIdLength() + bodyByteCount.ComputeVariableIntLength() + bodyByteCount;
         } else {
            var bodyByteCount = stringLengthQueue.Dequeue();
            EnsureSize(ref stringBuffer, bodyByteCount);
            Encoding.UTF8.GetBytes(str, 0, str.Length, stringBuffer, 0);
            output.WriteTypeId(TypeId.String);
            output.WriteVariableInt(bodyByteCount);
            output.WriteBytes(stringBuffer, 0, bodyByteCount);
         }
      }

      public void WriteNull(int slot) {
         if (isDryPass) {
            fakeWriter.Position += TypeId.Null.ComputeTypeIdLength();
         } else {
            output.WriteTypeId(TypeId.Null);
         }
      }

      private void EnsureSize(ref byte[] buffer, int length) {
         if (buffer.Length < length) {
            buffer = new byte[length];
         }
      }

      public class FakeForwardDataWriter : IForwardDataWriter {
         public int Position { get; set; }

         public void WriteByte(byte val) => Position++;
         public void WriteBytes(byte[] val) => WriteBytes(val, 0, val.Length);
         public void WriteBytes(byte[] val, int offset, int length) => Position += length;
      }
   }
}
