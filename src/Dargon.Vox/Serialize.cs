using Dargon.Vox.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Dargon.Vox.Internals.TypePlaceholders;

namespace Dargon.Vox {
   public class VoxFactory {
      public VoxSerializer Create() {
         var typeRegistry = new TypeRegistry();
         typeRegistry.RegisterReservedTypeId(typeof(int), (int)TypeId.Int32);
         typeRegistry.RegisterReservedTypeId(typeof(string), (int)TypeId.String);
         typeRegistry.RegisterReservedTypeId(typeof(bool), (int)TypeId.Bool);
         typeRegistry.RegisterReservedTypeId(typeof(TBoolTrue), (int)TypeId.BoolTrue);
         typeRegistry.RegisterReservedTypeId(typeof(TBoolFalse), (int)TypeId.BoolFalse);
         typeRegistry.RegisterReservedTypeId(typeof(Type), (int)TypeId.Type);
         typeRegistry.RegisterReservedTypeId(typeof(object), (int)TypeId.Object, () => new object());
         typeRegistry.RegisterReservedTypeId(typeof(Array), (int)TypeId.Array);
         typeRegistry.RegisterReservedTypeId(typeof(Dictionary<,>), (int)TypeId.Map);
         typeRegistry.RegisterReservedTypeId(typeof(KeyValuePair<,>), (int)TypeId.KeyValuePair);

         var fullTypeBinaryRepresentationCache = new FullTypeBinaryRepresentationCache(typeRegistry);
         var typeReader = new TypeReader(typeRegistry);
         var thingReaderWriterFactory = new ThingReaderWriterFactory(fullTypeBinaryRepresentationCache);
         var thingReaderWriterContainer = new ThingReaderWriterContainer(thingReaderWriterFactory);
         thingReaderWriterContainer.AddOrThrow(typeof(string), new StringThingReaderWriter(fullTypeBinaryRepresentationCache));
         thingReaderWriterContainer.AddOrThrow(typeof(bool), new BoolThingReaderWriter());
         thingReaderWriterContainer.AddOrThrow(typeof(TBoolTrue), new BoolTrueThingReaderWriter(fullTypeBinaryRepresentationCache));
         thingReaderWriterContainer.AddOrThrow(typeof(TBoolFalse), new BoolFalseThingReaderWriter(fullTypeBinaryRepresentationCache));
         thingReaderWriterContainer.AddOrThrow(typeof(Type), new TypeThingReaderWriter(fullTypeBinaryRepresentationCache, typeReader));
         thingReaderWriterFactory.SetContainer(thingReaderWriterContainer);

         var thisIsTotesTheRealLegitThingReaderWriterThing = new ThisIsTotesTheRealLegitThingReaderWriterThing(thingReaderWriterContainer, typeReader);
         var frameReader = new FrameReader(thisIsTotesTheRealLegitThingReaderWriterThing);
         var frameWriter = new FrameWriter(thisIsTotesTheRealLegitThingReaderWriterThing);
         thingReaderWriterFactory.SetThisIsTotesTheRealLegitThingReaderWriterThing(thisIsTotesTheRealLegitThingReaderWriterThing);

         return new VoxSerializer(typeRegistry, frameReader, frameWriter);
      }
   }

   public class ThingReaderWriterContainer {
      private readonly CopyOnAddDictionary<Type, IThingReaderWriter> storage = new CopyOnAddDictionary<Type, IThingReaderWriter>();
      private readonly ThingReaderWriterFactory thingReaderWriterFactory;

      public ThingReaderWriterContainer(ThingReaderWriterFactory thingReaderWriterFactory) {
         this.thingReaderWriterFactory = thingReaderWriterFactory;
      }

      public void AddOrThrow(Type type, IThingReaderWriter thingReaderWriter) => storage.AddOrThrow(type, thingReaderWriter);

      public IThingReaderWriter Get(Type type) => storage.GetOrAdd(type, thingReaderWriterFactory.Create);
   }

   public class VoxSerializer {
      private readonly TypeRegistry typeRegistry;
      private readonly FrameReader frameReader;
      private readonly FrameWriter frameWriter;

      public VoxSerializer(TypeRegistry typeRegistry, FrameReader frameReader, FrameWriter frameWriter) {
         this.typeRegistry = typeRegistry;
         this.frameReader = frameReader;
         this.frameWriter = frameWriter;
      }

      public void ImportTypes(VoxTypes voxTypes) {
         typeRegistry.ImportTypes(voxTypes);
      }

      public void Serialize(Stream dest, object subject) {
         var scratch = new MemoryStream();
         var target = new SomeMemoryStreamWrapperThing(scratch);
         frameWriter.WriteFrame(target, subject);
         target.CopyTo(dest);
      }

      public object Deserialize(Stream dest, Type hintType = null) {
         return frameReader.ReadFrame(dest, hintType);
      }
   }

   public static class Globals {
      static Globals() {
      }

      public static FrameWriter FrameWriter { get; }
      public static FrameReader FrameReader { get; }
   }

   public static class Serialize {
      public static void To(Stream dest, object subject) {
         var scratch = new MemoryStream();
         var target = new SomeMemoryStreamWrapperThing(scratch);
         Globals.FrameWriter.WriteFrame(target, subject);
         target.CopyTo(dest);
      }
   }

   public static class Deserialize {
      public static object From(Stream dest) {
         return Globals.FrameReader.ReadFrame(dest, null);
      }
   }
}
