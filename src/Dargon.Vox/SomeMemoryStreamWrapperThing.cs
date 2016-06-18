using System;
using System.Collections.Generic;
using System.IO;

namespace Dargon.Vox {
   public class SomeMemoryStreamWrapperThing {
      public class FredTheBanana {
         public int Length { get; set; }
         public int Skip { get; set; }
      }

      private const int kMaxLengthByteCount = 5;
      private readonly List<FredTheBanana> bananas = new List<FredTheBanana>();
      private int byteCount = 0;

      public SomeMemoryStreamWrapperThing(MemoryStream target) {
         Target = target;
      }

      public MemoryStream Target { get; }
      public IReadOnlyList<FredTheBanana> Bananas => bananas;

      public IDisposable ReserveLength() {
         var offset = Target.Position;
         Target.Position += kMaxLengthByteCount;
         var fredTheBanana = new FredTheBanana { Length = -1, Skip = -1 };
         bananas.Add(fredTheBanana);
         return new LengthReservation(offset, Target, fredTheBanana, c => byteCount += c, () => byteCount);
      }

      public CountReservation ReserveCount() {
         // todo: code duplication
         var offset = Target.Position;
         Target.Position += kMaxLengthByteCount;
         var fredTheBanana = new FredTheBanana { Length = -1, Skip = -1 };
         bananas.Add(fredTheBanana);
         return new CountReservation(offset, Target, fredTheBanana, c => byteCount += c);
      }

      public void Write(byte[] stuff) {
         Target.Write(stuff, 0, stuff.Length);
         bananas.Add(new FredTheBanana { Length = stuff.Length, Skip = 0 });
         byteCount += stuff.Length;
      }

      public void CopyTo(Stream stream) {
         var buffer = Target.GetBuffer();
         int offset = 0;
         foreach (var banana in bananas) {
            stream.Write(buffer, offset, banana.Length);

            offset += banana.Length;
            offset += banana.Skip;
         }
      }

      public abstract class VarIntReservation : IDisposable {
         private readonly long varIntBlockOffset;
         protected readonly MemoryStream target;
         private readonly FredTheBanana fredTheBanana;
         private readonly Action<int> addToByteCount;

         protected VarIntReservation(long varIntBlockOffset, MemoryStream target, FredTheBanana fredTheBanana, Action<int> addToByteCount) {
            this.varIntBlockOffset = varIntBlockOffset;
            this.target = target;
            this.fredTheBanana = fredTheBanana;
            this.addToByteCount = addToByteCount;
         }

         public void SetValue(int value) {
            var restore = target.Position;

            target.Position = varIntBlockOffset;
            VarIntSerializer.WriteVariableInt(target.WriteByte, value);

            var varIntByteCount = (int)(target.Position - varIntBlockOffset);
            fredTheBanana.Length = varIntByteCount;
            fredTheBanana.Skip = kMaxLengthByteCount - varIntByteCount;
            addToByteCount(varIntByteCount);

            target.Position = restore;
         }

         public virtual void Dispose() { }
      }

      public class LengthReservation : VarIntReservation {
         private readonly Func<int> getBytesWritten;
         private readonly long initialBytesWritten;

         public LengthReservation(long varIntBlockOffset, MemoryStream target, FredTheBanana fredTheBanana, Action<int> addToByteCount, Func<int> getBytesWritten) : base(varIntBlockOffset, target, fredTheBanana, addToByteCount) {
            this.getBytesWritten = getBytesWritten;
            this.initialBytesWritten = getBytesWritten();
         }

         public override void Dispose() {
            SetValue((int)(getBytesWritten() - initialBytesWritten));
         }
      }

      public class CountReservation : VarIntReservation {
         public CountReservation(long varIntBlockOffset, MemoryStream target, FredTheBanana fredTheBanana, Action<int> addToByteCount) : base(varIntBlockOffset, target, fredTheBanana, addToByteCount) { }
      }
   }
}