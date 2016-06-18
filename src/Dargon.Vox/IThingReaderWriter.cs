using System.IO;

namespace Dargon.Vox {
   public interface IThingReaderWriter {
      void WriteThing(SomeMemoryStreamWrapperThing dest, object subject);
      object ReadBody(BinaryReader reader);
   }
}