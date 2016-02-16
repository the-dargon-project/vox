using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dargon.Messages.Internals;

namespace Dargon.Messages {
   public class Program {
      public static void Main() {
         var boundsTest = new CoreSerializerAcceptanceTests();
         foreach (var n in Enumerable.Range(-100, 200))
            Console.WriteLine(n + ": " + boundsTest.ComputeSize(n));
      }
   }
}
