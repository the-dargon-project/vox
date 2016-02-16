using System;

namespace Dargon.Messages.Internals {
   public class SlotTypeMismatchException : Exception {
      public SlotTypeMismatchException(TypeId expected, TypeId actual) : base(GetMessage(expected, actual)) { }

      private static string GetMessage(TypeId expected, TypeId actual) {
         return $"Expected slot to contain {expected} but found {actual}.";
      }
   }
}