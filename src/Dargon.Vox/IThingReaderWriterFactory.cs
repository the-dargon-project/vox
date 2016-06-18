using Dargon.Commons.Collections;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Dargon.Commons;
using SCG = System.Collections.Generic;

namespace Dargon.Vox {
   public class ThingReaderWriterFactory {
      private readonly FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache;
      private ThingReaderWriterContainer thingReaderWriterContainer;
      private ThisIsTotesTheRealLegitThingReaderWriterThing thisIsTotesTheRealLegitThingReaderWriterThing;

      public ThingReaderWriterFactory(FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache) {
         this.fullTypeBinaryRepresentationCache = fullTypeBinaryRepresentationCache;
      }

      public void SetContainer(ThingReaderWriterContainer thingReaderWriterContainer) {
         this.thingReaderWriterContainer = thingReaderWriterContainer;
      }

      public void SetThisIsTotesTheRealLegitThingReaderWriterThing(ThisIsTotesTheRealLegitThingReaderWriterThing thisIsTotesTheRealLegitThingReaderWriterThing) {
         this.thisIsTotesTheRealLegitThingReaderWriterThing = thisIsTotesTheRealLegitThingReaderWriterThing;
      }

      public IThingReaderWriter Create(Type arg) {
         if (arg.IsArray) {
            return new VectorlikeReaderWriter(fullTypeBinaryRepresentationCache, thisIsTotesTheRealLegitThingReaderWriterThing, arg);
         } else if (typeof(IEnumerable).IsAssignableFrom(arg)) {
            var enumerableType = EnumerableUtilities.GetGenericIEnumerableInterfaceType(arg);
            var elementType = enumerableType.GetGenericArguments()[0];
            if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(SCG.KeyValuePair<,>)) {
               throw new NotImplementedException();
            } else {
               return new VectorlikeReaderWriter(fullTypeBinaryRepresentationCache, thisIsTotesTheRealLegitThingReaderWriterThing, arg);
            }
         } else  {
            throw new NotImplementedException();
         }
      }
   }

   public static class EnumerableUtilities {
      private static readonly ConcurrentDictionary<Type, Type> typeSimplifyingMapThingNamingStuffIsHard = new ConcurrentDictionary<Type, Type>();

      public static Type GetGenericIEnumerableInterfaceType(Type genericType) {
         return genericType.GetInterfaces()
                           .FirstOrDefault(i => i.Name.Contains(nameof(IEnumerable)) && i.IsGenericType);
      }

      public static Type SimplifyType(Type type) {
         if (!type.IsGenericType && !type.IsArray) {
            return type;
         }
         return typeSimplifyingMapThingNamingStuffIsHard.GetOrAdd(type, SimplifyCollectionType);
      }

      private static Type SimplifyCollectionType(Type genericType) {
         var enumerableInterfaceType = GetGenericIEnumerableInterfaceType(genericType);
         if (enumerableInterfaceType == null) {
            return genericType;
         }
         var elementType = SimplifyCollectionType(enumerableInterfaceType.GetGenericArguments()[0]);
         if (elementType.IsGenericType &&
            elementType.GetGenericTypeDefinition() == typeof(SCG.KeyValuePair<,>)) {
            var kvpGenericArgs = elementType.GetGenericArguments();
            return typeof(SCG.Dictionary<,>).MakeGenericType(kvpGenericArgs);
         }
         return elementType.MakeArrayType();
      }
   }

   public static class ArrayToVectorlikeConverterThing {
      private static object[] ShallowCloneArrayToType(object[] elements, Type destinationArrayElementType) {
         var result = (object[])Array.CreateInstance(destinationArrayElementType, elements.Length);
         for (var i = 0; i < elements.Length; i++) {
            result[i] = RecursiveCollectionTypeConversionHelper(elements[i], destinationArrayElementType);
         }
         return result;
      }

      private static object RecursiveCollectionTypeConversionHelper(object subject, Type destinationType) {
         if (!typeof(IEnumerable).IsAssignableFrom(destinationType)) {
            return subject;
         }

         if (destinationType == subject.GetType()) {
            return subject;
         }
         if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition() == typeof(SCG.IReadOnlyDictionary<,>)) {
            destinationType = typeof(SCG.Dictionary<,>).MakeGenericType(destinationType.GetGenericArguments());
         }
         if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition() == typeof(IReadOnlySet<>)) {
            destinationType = typeof(HashSet<>).MakeGenericType(destinationType.GetGenericArguments());
         }

         var subjectElements = (object[])subject;

         if (destinationType.IsArray) {
            return ShallowCloneArrayToType(subjectElements, destinationType.GetElementType());
         } else {
            var destinationEnumerableType = EnumerableUtilities.GetGenericIEnumerableInterfaceType(destinationType);
            var destinationElementType = destinationEnumerableType.GetGenericArguments()[0];
            var constructor = destinationType.GetConstructor(new[] { destinationEnumerableType });
            if (constructor != null) {
               var shallowClonedCastedElements = ShallowCloneArrayToType(subjectElements, destinationElementType);
               return constructor.Invoke(new object[] { shallowClonedCastedElements });
            }
            var type = destinationType;
            var instance = Activator.CreateInstance(destinationType);
            var add = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                          .FirstOrDefault(m => m.Name.Contains("Add") && m.GetParameters().Length == 1) ?? type.GetMethod("Enqueue");
            foreach (var element in subjectElements) {
               add.Invoke(instance, new object[] { element });
            }
            return instance;
         }
      }

      public static object DoIt(object[] elements, Type collectionEnumerableType, Type returnedCollectionType) {
         return RecursiveCollectionTypeConversionHelper(elements, returnedCollectionType);
      }
   }

   public class VectorlikeReaderWriter : IThingReaderWriter {
      private readonly FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache;
      private readonly ThisIsTotesTheRealLegitThingReaderWriterThing thingReaderWriterDispatcherThing;
      private readonly Type vectorlikeCollectionType;
      private readonly Type arraySimplifiedCollectionType;
      private readonly Type elementType;
      private readonly Type enumerableType;

      public VectorlikeReaderWriter(FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache, ThisIsTotesTheRealLegitThingReaderWriterThing thingReaderWriterDispatcherThing, Type vectorlikeCollectionType) {
         this.fullTypeBinaryRepresentationCache = fullTypeBinaryRepresentationCache;
         this.thingReaderWriterDispatcherThing = thingReaderWriterDispatcherThing;
         this.vectorlikeCollectionType = vectorlikeCollectionType;
         this.arraySimplifiedCollectionType = EnumerableUtilities.SimplifyType(vectorlikeCollectionType);
         this.elementType = arraySimplifiedCollectionType.GetElementType();
         this.enumerableType = typeof(SCG.IEnumerable<>).MakeGenericType(elementType);
      }

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         dest.Write(fullTypeBinaryRepresentationCache.GetOrCompute(arraySimplifiedCollectionType));

         using (dest.ReserveLength())
         using (var countReservation = dest.ReserveCount()) {
            int count = 0;
            foreach (var x in (SCG.IEnumerable<object>)subject) {
               count++;
               thingReaderWriterDispatcherThing.WriteThing(dest, x);
            }
            countReservation.SetValue(count);
         }
      }

      public object ReadBody(BinaryReader reader) {
         var dataLength = VarIntSerializer.ReadVariableInt(reader.ReadByte);
         using (var ms = new MemoryStream(reader.ReadBytes(dataLength)))
         using (var dataReader = new BinaryReader(ms)) {
            var elementCount = VarIntSerializer.ReadVariableInt(dataReader.ReadByte);
            var elements = (object[])Array.CreateInstance(elementType, elementCount);
            for (var i = 0; i < elementCount; i++) {
               elements[i] = thingReaderWriterDispatcherThing.ReadThing(dataReader, null);
            }
            return ArrayToVectorlikeConverterThing.DoIt(elements, enumerableType, vectorlikeCollectionType);
         }
      }
   }
}