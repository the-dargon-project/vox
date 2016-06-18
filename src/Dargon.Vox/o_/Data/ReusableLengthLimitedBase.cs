using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dargon.Vox.Data {
   public class ReusableLengthLimitedBase<TTarget> : IRetargetable<TTarget>, ILengthLimitedAdapter {
      protected TTarget _target;
      private long _bytesRemaining;

      public void SetTarget(TTarget target) {
         _target = target;
      }

      public long BytesRemaining => _bytesRemaining;

      public void SetBytesRemaining(long bytesRemaining) {
         _bytesRemaining = bytesRemaining;
      }

      protected void Advance(int count) {
         _bytesRemaining -= count;
         if (_bytesRemaining < 0) {
            throw new EndOfStreamException();
         }
      }
   }
}
