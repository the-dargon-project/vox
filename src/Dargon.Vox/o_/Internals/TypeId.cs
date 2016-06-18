namespace Dargon.Vox.Internals {
   public enum TypeId : int {
      ByteArray            = -0x01,
      VarInt               = -0x02,
      String               = -0x03,
      Int8                 = -0x04,
      Int16                = -0x05,
      Int32                = -0x06,
      Guid                 = -0x08,
      Bool                 = -0x09,
      BoolTrue             = -0x0A,
      BoolFalse            = -0x0B,
      Type                 = -0x0C,
      DateTime             = -0x0D,
      Float                = -0x0E,
      Double               = -0x0F,

      Null                 = -0x20,
      Void                 = -0x21,

      Object               = -0x32,
      GenericTypeIdBegin   = -0x33,
      GenericTypeIdEnd     = -0x34,

      Array                = -0x40,
      Map                  = -0x41,
   }
}