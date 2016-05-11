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
   public class VoxRyuExtensionModule : RyuModule, IRyuExtensionModule {
      public void Loaded(IRyuExtensionArguments args) { }

      public void PreConstruction(IRyuExtensionArguments args) {

      }

      public void PostConstruction(IRyuExtensionArguments args) {
         var ryu = args.Container;

         // Create auto-serializers
         var types = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).SelectMany(x => x.ExportedTypes).ToList();

         // instantiate all VoxTypes classes, which registers typeIds
         types.Where(t => t != typeof(VoxTypes) && typeof(VoxTypes).IsAssignableFrom(t)).ForEach(t => { Activator.CreateInstance(t); });

         var autoSerializableTypes = types.Where(t => t.GetAttributeOrNull<AutoSerializableAttribute>() != null).ToArray();
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
