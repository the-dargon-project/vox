# An Introduction to Vox
Vox is an open-source binary serializer released under the BSD license specifically built for the .NET ecosystem.

## Some Highlights
You can manually serialize your POCOs:
```
public class RemoteException : Exception, ISerializableType {
   private static readonly FieldInfo kMessageField = typeof(RemoteException).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance);

   public RemoteException() { }
   private RemoteException(string message) : base(message) { }
   ...
   public void Serialize(IBodyWriter writer) => writer.Write(Message);
   public void Deserialize(IBodyReader reader) => kMessageField.SetValue(this, reader.Read<string>());
   }
}
```
Even outside their definitions:
```
public class Vector3TypeSerializer : ITypeSerializer<Vector3> {
   public void Serialize(IBodyWriter writer, Vector3 source) {
      writer.Write(source.X);
      writer.Write(source.Y);
      writer.Write(source.Z);
   }

   public void Deserialize(IBodyReader reader, Vector3 target) {
      target.X = reader.Read<float>();
      target.Y = reader.Read<float>();
      target.Z = reader.Read<float>();
   }
}
```
Or you can depend on runtime serializer bytecode generation!
```
[AutoSerializable]
public class RmiRequestDto {
   public Guid InvocationId { get; set; }
   public Guid ServiceId { get; set; }
   public string MethodName { get; set; }
   public Type[] MethodGenericArguments { get; set; }
   public object[] MethodArguments { get; set; }
}
```

## Documentation
All serializable types must be assigned unique TypeIDs.
```
public class CourierVoxTypes : VoxTypes {
   public CourierVoxTypes() : base(0) {
      // Courier Core (starts at 1 - note can't use 0 as that's TNull in Vox).
      Register<MessageDto>(1);

      // Udp
      var udpBaseId = 10;
      Register<PacketDto>(udpBaseId + 0);
      Register<AcknowledgementDto>(udpBaseId + 1);
      ...
   }
}
```
The VoxTypes subclasses are detected and loaded at Ryu's IoC PostConstruction phase.

## Future Plans
### Tighter language / Dargon framework coupling
Ideally you'd be able to send around magical things like expression trees (useful for querying) and courier service references.

### Versioned Serialization and Migrations
The current plan is as follows, though this is up in the air:
* Code contains uniquely IDed classes e.g. RequestDtoV1, RequestDtoV2.
* Versioned classes may implement IMigratable<>. E.g. V1 : IMigratable<V2>.
* Codebase depends on RequestDto which extends e.g. RequestDtoV2, serializer handles typing differences or Vox patches bytecode.
