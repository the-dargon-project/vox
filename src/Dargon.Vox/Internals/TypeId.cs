namespace Dargon.Vox.Internals {
   public enum TypeId : int {
      ByteArray            = -0x01,
      VarInt               = -0x02,

      Null                 = -0x20,
      Void                 = -0x21,

      ObjectEnd            = -0x32,
      GenericTypeIdBegin   = -0x33,
      GenericTypeIdEnd     = -0x34
   }
}