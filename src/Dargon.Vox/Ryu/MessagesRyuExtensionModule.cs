using System;
using System.Linq;
using System.Reflection;
using Dargon.Commons;
using Dargon.Commons.Collections;
using Dargon.Ryu;
using Dargon.Ryu.Extensibility;
using Dargon.Ryu.Modules;
using Dargon.Vox.Internals;

namespace Dargon.Vox.Ryu {
   public class MessagesRyuExtensionModule : RyuModule, IRyuExtensionModule {
      public void Loaded(IRyuExtensionArguments args) { }

      public void PreConstruction(IRyuExtensionArguments args) {

      }

      public void PostConstruction(IRyuExtensionArguments args) {
         var ryu = args.Container;

         // Create auto-serializers
         var autoSerializableTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.ExportedTypes)
                                              .Where(x => x.GetAttributeOrNull<AutoSerializableAttribute>() != null)
                                              .ToArray();
         var autoTypeSerializers = AutoTypeSerializerFactory.CreateMany(autoSerializableTypes);

         // Find type serializers registered via ryu:
         var ryuTypeSerializers = ryu.Find<ITypeSerializer>();

         foreach (var typeSerializer in autoTypeSerializers.Values.Concat(ryuTypeSerializers)) {
            typeSerializer.VisitGeneric(
               typeof(ITypeSerializer<>), 
               this, nameof(PostConstruction_RegisterTypeSerializer),
               typeSerializer);
         }
      }

      private void PostConstruction_RegisterTypeSerializer<T>(ITypeSerializer<T> typeSerializer) {
         TypeSerializerRegistry<T>.Register(typeSerializer);
      }

      public void PreInitialization(IRyuExtensionArguments args) {

      }

      public void PostInitialization(IRyuExtensionArguments args) {

      }
   }

   public class A {

   }

   public class B {
      public B() {

      }
   }
}
