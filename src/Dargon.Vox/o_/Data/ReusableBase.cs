using System.IO;

namespace Dargon.Vox.Data {
   public class ReusableBase<TTarget> : IRetargetable<TTarget> {
      protected TTarget _target;

      public void SetTarget(TTarget target) {
         _target = target;
      }
   }
}