using System;
using Dargon.Vox.Internals;

namespace Dargon.Vox.Slots {
   public class SlotTypeMismatchException : Exception {
      public SlotTypeMismatchException(TypeId expected, TypeId actual) : base(GetMessage(expected, actual)) { }

      private static string GetMessage(TypeId expected, TypeId actual) {
         return $"Expected slot to contain {expected} but found {actual}.";
      }
   }
}