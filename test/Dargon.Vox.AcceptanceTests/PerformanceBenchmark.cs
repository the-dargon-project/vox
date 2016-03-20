using Dargon.Commons;
using Dargon.Vox.InternalTestUtils;
using NMockito;
using ProtoBuf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dargon.Vox.Internals;
using Dargon.Vox.InternalTestUtils.Debugging;

namespace Dargon.Vox.AcceptanceTests {
   public class PerformanceBenchmark : NMockitoInstance {
      public void UserBenchmark() {
         TestTypeRegistrationHelpers.RegisterWithSerializer<User>(new UserSerializer());
         TestTypeRegistrationHelpers.RegisterWithSerializer<Address>(new AddressSerializer());

         var address = new Address {
            Line1 = CreatePlaceholder<string>(),
            Line2 = CreatePlaceholder<string>()
         };
         var user = new User {
            Id = CreatePlaceholder<int>(),
            Name = CreatePlaceholder<string>(),
            Address = address
         };
         RunBenchmark(user, 500000, 10);
      }

      private void RunBenchmark<T>(T subject, int serializationsPerTrial, int trials) {
         for (var trial = 0; trial < trials; trial++) {
//            RunBenchmarkHelper("protobuf", subject, serializationsPerTrial, Serializer.Serialize, Serializer.Deserialize<T>);
            RunBenchmarkHelper("     vox", subject, serializationsPerTrial, Serialize.To, Deserialize.From<T>);
         }
      }

      private void RunBenchmarkHelper<T>(string name, T subject, int serializations, Action<MemoryStream, T> serialize, Func<MemoryStream, T> deserialize) {
         // Warmup Pass
         GC.Collect();
         RunBenchmarkHelperPass(subject, serializations, serialize, deserialize);
         
         // Benchmark Pass
         var sw = new Stopwatch().With(x => x.Start());
         GC.Collect();
         var preCollectionCounts = Util.Generate(GC.MaxGeneration, GC.CollectionCount);
         var startTime = sw.ElapsedMilliseconds;
         var size = RunBenchmarkHelperPass(subject, serializations, serialize, deserialize);
         var endTime = sw.ElapsedMilliseconds;
         var postCollectionCounts = Util.Generate(GC.MaxGeneration, GC.CollectionCount);

         // Print Results:
         var collectionCountDeltas = preCollectionCounts.Zip(postCollectionCounts, (pre, post) => post - pre).ToArray();
         Console.WriteLine($"{name} completed in {endTime - startTime} millis taking {size} bytes, gc collects: {collectionCountDeltas.Join(", ")}");
      }

      private static int RunBenchmarkHelperPass<T>(T subject, int serializations, Action<MemoryStream, T> serialize, Func<MemoryStream, T> deserialize) {
         using (var ms = new MemoryStream()) {
            for (int i = 0; i < serializations; i++) {
               serialize(ms, subject);
            }
            ms.Position = 0;
            T[] result = new T[serializations];
            for (int i = 0; i < result.Length; i++) {
               result[i] = deserialize(ms);
            }
            return (int)ms.Length;
         }
      }

      [ProtoContract]
      public class User {
         [ProtoMember(1)] public int Id { get; set; }
         [ProtoMember(2)] public string Name { get; set; }
         [ProtoMember(3)] public Address Address { get; set; }
      }

      [ProtoContract]
      public class Address {
         [ProtoMember(1)] public string Line1 { get; set; }
         [ProtoMember(2)] public string Line2 { get; set; }
      }

      public class UserSerializer : ITypeSerializer<User> {
         public Type Type => typeof(User);

         public void Serialize(ISlotWriter writer, User source) {
            writer.WriteNumeric(0, source.Id);
            writer.WriteString(1, source.Name);
            writer.WriteObject(2, source.Address);
         }

         public void Deserialize(ISlotReader reader, User target) {
            target.Id = reader.ReadNumeric(0);
            target.Name = reader.ReadString(1);
            target.Address = reader.ReadObject<Address>(2);
         }
      }

      public class AddressSerializer : ITypeSerializer<Address> {
         public Type Type => typeof(Address);

         public void Serialize(ISlotWriter writer, Address source) {
            writer.WriteString(0, source.Line1);
            writer.WriteString(1, source.Line2);
         }

         public void Deserialize(ISlotReader reader, Address target) {
            target.Line1 = reader.ReadString(0);
            target.Line2 = reader.ReadString(1);
         }
      }
   }
}
