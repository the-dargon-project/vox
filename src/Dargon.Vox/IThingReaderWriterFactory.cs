using Dargon.Commons.Collections;
using Dargon.Vox.Utilities;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
         if (arg.IsArray || typeof(IEnumerable).IsAssignableFrom(arg)) {
            return new CollectionReaderWriter(fullTypeBinaryRepresentationCache, thisIsTotesTheRealLegitThingReaderWriterThing, arg);
         } else if (arg.IsGenericType && arg.GetGenericTypeDefinition() == typeof(SCG.KeyValuePair<,>)) {
            return new KeyValuePairReaderWriter(fullTypeBinaryRepresentationCache, thisIsTotesTheRealLegitThingReaderWriterThing, arg);
         } else {
            throw new NotImplementedException();
         }
      }
   }

   public class KeyValuePairReaderWriter : IThingReaderWriter {
      private readonly FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache;
      private readonly ThisIsTotesTheRealLegitThingReaderWriterThing thisIsTotesTheRealLegitThingReaderWriterThing;
      private readonly Type userKvpType;
      private readonly Type userKeyType;
      private readonly Type userValueType;
      private readonly Type simplifiedKvpType;

      public KeyValuePairReaderWriter(FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache, ThisIsTotesTheRealLegitThingReaderWriterThing thisIsTotesTheRealLegitThingReaderWriterThing, Type userKvpType) {
         this.fullTypeBinaryRepresentationCache = fullTypeBinaryRepresentationCache;
         this.thisIsTotesTheRealLegitThingReaderWriterThing = thisIsTotesTheRealLegitThingReaderWriterThing;

         Trace.Assert(userKvpType.IsGenericType && userKvpType.GetGenericTypeDefinition() == typeof(SCG.KeyValuePair<,>));
         this.userKvpType = userKvpType;
         var genericArguments = userKvpType.GetGenericArguments();
         this.userKeyType = genericArguments[0];
         this.userValueType = genericArguments[1];
         this.simplifiedKvpType = EnumerableUtilities.SimplifyType(userKvpType);
      }

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         dest.Write(fullTypeBinaryRepresentationCache.GetOrCompute(simplifiedKvpType));

         using (dest.ReserveLength()) {
            dynamic s = subject;
            thisIsTotesTheRealLegitThingReaderWriterThing.WriteThing(dest, s.Key);
            thisIsTotesTheRealLegitThingReaderWriterThing.WriteThing(dest, s.Value);
         }
      }

      public object ReadBody(BinaryReader reader) {
         var length = VarIntSerializer.ReadVariableInt(reader.ReadByte);
         using (var ms = new MemoryStream(reader.ReadBytes(length)))
         using (var msReader = new BinaryReader(ms)) {
            var key = thisIsTotesTheRealLegitThingReaderWriterThing.ReadThing(msReader, userKeyType);
            var value = thisIsTotesTheRealLegitThingReaderWriterThing.ReadThing(msReader, userValueType);
            return Activator.CreateInstance(userKvpType, key, value);
         }
      }
   }

   public static class InternalRepresentationToHintTypeConverter {
      private delegate IEnumerable CastFunc(IEnumerable x);

      private static IGenericFlyweightFactory<CastFunc> castFuncs
         = GenericFlyweightFactory.ForMethod<CastFunc>(
            typeof(Enumerable),
            nameof(Enumerable.Cast));

      private static object[] ShallowCloneArrayToType(object[] elements, Type destinationArrayElementType) {
         var result = (object[])Array.CreateInstance(destinationArrayElementType, elements.Length);
         for (var i = 0; i < elements.Length; i++) {
            result[i] = ConvertCollectionToHintType(elements[i], destinationArrayElementType);
         }
         return result;
      }

      private static IEnumerable ShallowCloneDictionaryToType(IEnumerable dictionary, Type destinationKvpType) {
         Trace.Assert(destinationKvpType.GetGenericTypeDefinition() == typeof(SCG.KeyValuePair<,>));

         var genericArgs = destinationKvpType.GetGenericArguments();
         var keyType = genericArgs[0];
         var valueType = genericArgs[1];

         var listType = typeof(SCG.List<>).MakeGenericType(destinationKvpType);
         var result = Activator.CreateInstance(listType);
         var add = listType.GetMethods().First(m => m.Name == "Add" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == destinationKvpType);
         var enumerator = dictionary.GetEnumerator();
         while (enumerator.MoveNext()) {
            dynamic kvp = enumerator.Current;
            var key = ConvertCollectionToHintType(kvp.Key, keyType);
            var value = ConvertCollectionToHintType(kvp.Value, valueType);
            var castedIntermediateKvp = Activator.CreateInstance(destinationKvpType, key, value);
            add.Invoke(result, new object [] { castedIntermediateKvp });
         }
         return (IEnumerable)result;
      }

      public static object ConvertCollectionToHintType(object collection, Type hintType) {
         // fast fail on non-collections, since we aren't converting them.
         if (!typeof(IEnumerable).IsAssignableFrom(hintType)) {
            return collection;
         }

         // TODO: This will fail on nulls right now
         if (hintType.IsInstanceOfType(collection)) {
            return collection;
         }

         // If hintType is IROD/IROS, up that to the Dictionary default, since we can't 
         // construct an interface.
         // TODO: All sets are IROS, all dicts are IROD, so is this an impossible code path?
         if (hintType.IsGenericType && hintType.GetGenericTypeDefinition() == typeof(SCG.IReadOnlyDictionary<,>)) {
            hintType = typeof(SCG.Dictionary<,>).MakeGenericType(hintType.GetGenericArguments());
         }
         if (hintType.IsGenericType && hintType.GetGenericTypeDefinition() == typeof(IReadOnlySet<>)) {
            hintType = typeof(HashSet<>).MakeGenericType(hintType.GetGenericArguments());
         }

         var enumerableType = EnumerableUtilities.GetGenericIEnumerableInterfaceTypeOrNull(hintType);
         var elementType = EnumerableUtilities.GetEnumerableElementType(enumerableType);

         if (EnumerableUtilities.IsDictionaryLikeType(hintType)) {
            // e.g. KeyValuePair<TKey, TValue>[]
            var subjectKvpVectorLike = (IEnumerable)collection;
            var shallowClonedCastedElements = ShallowCloneDictionaryToType(subjectKvpVectorLike, elementType);

            // invoke .ctor(IEnumerable<KVP<TKey, TVal>>)
            var constructor = hintType.GetConstructor(new[] { enumerableType });
            if (constructor != null) {
               return constructor.Invoke(new object[] { shallowClonedCastedElements });
            }

            // invoke .ctor(), then Add/Enqueue of collection type
            var instance = Activator.CreateInstance(hintType);
            var add = hintType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                              .FirstOrDefault(m => m.Name.Contains("Add") && m.GetParameters().Length == 2);
            
            foreach (var kvpObject in shallowClonedCastedElements) {
               dynamic kvp = kvpObject;
               add.Invoke(instance, new object[] { kvp.Key, kvp.Value });
            }
            return instance;
         } else {
            // Vector-like case
            var subjectElements = (object[])collection;

            if (hintType.IsArray) {
               return ShallowCloneArrayToType(subjectElements, hintType.GetElementType());
            } else {
               // Invoke .ctor(IEnumerable<TElement>)
               var constructor = hintType.GetConstructor(new[] { enumerableType });
               if (constructor != null) {
                  var shallowClonedCastedElements = ShallowCloneArrayToType(subjectElements, elementType);
                  return constructor.Invoke(new object[] { shallowClonedCastedElements });
               }

               // Invoke .ctor(), then Add/Enqueue(TElement)
               var instance = Activator.CreateInstance(hintType);
               var add = hintType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                 .FirstOrDefault(m => m.Name.Contains("Add") && m.GetParameters().Length == 1) ?? hintType.GetMethod("Enqueue");
               foreach (var element in subjectElements) {
                  add.Invoke(instance, new object[] { element });
               }
               return instance;
            }
         }
      }
   }

   public class CollectionReaderWriter : IThingReaderWriter {
      private readonly FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache;
      private readonly ThisIsTotesTheRealLegitThingReaderWriterThing thingReaderWriterDispatcherThing;
      private readonly Type userCollectionType;
      private readonly Type simplifiedCollectionType;
      private readonly Type elementType;

      public CollectionReaderWriter(FullTypeBinaryRepresentationCache fullTypeBinaryRepresentationCache, ThisIsTotesTheRealLegitThingReaderWriterThing thingReaderWriterDispatcherThing, Type userCollectionType) {
         this.fullTypeBinaryRepresentationCache = fullTypeBinaryRepresentationCache;
         this.thingReaderWriterDispatcherThing = thingReaderWriterDispatcherThing;
         this.userCollectionType = userCollectionType;
         this.simplifiedCollectionType = EnumerableUtilities.SimplifyType(userCollectionType);
         this.elementType = EnumerableUtilities.GetEnumerableElementType(simplifiedCollectionType);
      }

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         dest.Write(fullTypeBinaryRepresentationCache.GetOrCompute(simplifiedCollectionType));

         using (dest.ReserveLength())
         using (var countReservation = dest.ReserveCount()) {
            int count = 0;
            foreach (var x in (IEnumerable)subject) {
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
            var elements = Array.CreateInstance(elementType, elementCount);
            for (var i = 0; i < elements.Length; i++) {
               var thing = thingReaderWriterDispatcherThing.ReadThing(dataReader, null);
               elements.SetValue(thing, i);
            }
            return InternalRepresentationToHintTypeConverter.ConvertCollectionToHintType(elements, userCollectionType);
         }
      }
   }
}