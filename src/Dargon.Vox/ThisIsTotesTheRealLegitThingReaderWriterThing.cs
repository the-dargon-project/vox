﻿using System;
using System.Diagnostics;
using System.IO;
using Dargon.Commons.Exceptions;
using Dargon.Vox.Internals.TypePlaceholders;
using Dargon.Vox.Utilities;

namespace Dargon.Vox {
   public class ThisIsTotesTheRealLegitThingReaderWriterThing {
      private readonly ThingReaderWriterContainer thingReaderWriterContainer;
      private readonly TypeReader typeReader;

      public ThisIsTotesTheRealLegitThingReaderWriterThing(ThingReaderWriterContainer thingReaderWriterContainer, TypeReader typeReader) {
         this.thingReaderWriterContainer = thingReaderWriterContainer;
         this.typeReader = typeReader;
      }

      public object ReadThing(BinaryReader thingReader, Type hintType) {
         var type = typeReader.ReadType(thingReader.ReadByte);

         if (type == typeof(TBoolTrue) || type == typeof(TBoolFalse)) {
            // nothing
         } else if (type == typeof(Type)) {
            Trace.Assert(hintType == null || typeof(Type).IsAssignableFrom(hintType));
         } else if (hintType != null) {
            var simplifiedType = TypeSimplifier.SimplifyType(hintType);

            if (type != simplifiedType) {
               throw new InvalidStateException();
            }

            type = hintType;
         }

         return thingReaderWriterContainer.Get(type).ReadBody(thingReader);
      }

      public void WriteThing(SomeMemoryStreamWrapperThing dest, object subject) {
         var type = SubjectTypeGetterererer.GetSubjectType(subject);
         thingReaderWriterContainer.Get(type).WriteThing(dest, subject);
      }
   }
}