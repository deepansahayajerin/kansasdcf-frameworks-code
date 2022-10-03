using MDSY.Framework.Buffer.Common;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Buffer.Unity;
using Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Lifetime;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the interface ITypeRegistrationModule.
    /// </summary>
    public sealed class TypeRegistrator : ITypeRegistrationModule
    {
        /// <summary>
        /// This loads the Registrations Types into <paramref name="container"/>.
        /// </summary>
        /// <param name="container">Place where all registrations of Type are loaded into.</param>
        public void LoadRegistrationsInto(IUnityContainer container)
        {
            container
                // services
                .RegisterType<IFactoryService, FactoryService>(new ContainerControlledLifetimeManager())
                .RegisterType<IArrayUtilitiesService, ArrayUtilitiesService>(new ContainerControlledLifetimeManager())
                .RegisterType<ILanguageService, LanguageService>(new ContainerControlledLifetimeManager())
                .RegisterType<IRecordCollectionService, RecordCollectionService>(new ContainerControlledLifetimeManager())
                .RegisterType<IBufferAddressCollectionService, BufferAddressCollectionService>(new ContainerControlledLifetimeManager())
                .RegisterType<IRecordBufferCollectionService, RecordBufferCollectionService>(new ContainerControlledLifetimeManager())
                .RegisterType<ILoggingService, TextLoggingService>(new ContainerControlledLifetimeManager())
                .RegisterType<IIndexBaseServices, IndexBaseServices>(new ContainerControlledLifetimeManager())
                .RegisterType<IDirectiveServices, DirectiveServices>(new ContainerControlledLifetimeManager())

                .RegisterType<IBufferObjectFactory, ObjectFactory>(new ContainerControlledLifetimeManager())

                .RegisterType<IBufferAddress, BufferAddress>()

                .RegisterType<IRecord, Record>()

                .RegisterType<IField, Field>()
                .RegisterType<IField, RedefineField>(Constants.TypeMappingRegistrationNames.Redefine)
                .RegisterType<IFieldInitializer, Field>()
                .RegisterType<IFieldInitializer, RedefineField>(Constants.TypeMappingRegistrationNames.Redefine)

                .RegisterType<IGroup, Group>()
                .RegisterType<IGroup, RedefineGroup>(Constants.TypeMappingRegistrationNames.Redefine)
                .RegisterType<IGroupInitializer, Group>()
                .RegisterType<IGroupInitializer, RedefineGroup>(Constants.TypeMappingRegistrationNames.Redefine)

                .RegisterType<IFieldArray, FieldArray>()
                .RegisterType<IFieldArrayInitializer, FieldArray>()
                .RegisterType<IGroupArray, GroupArray>()
                .RegisterType<IGroupArrayInitializer, GroupArray>()

                .RegisterType(typeof(IArrayElementAccessor<>),
                    typeof(ArrayElementAccessor<>),
                    Constants.TypeMappingRegistrationNames.ZeroBasedIdx)
                .RegisterType(typeof(IArrayElementAccessor<>),
                    typeof(ArrayElementOneBasedAccessor<>),
                    Constants.TypeMappingRegistrationNames.OneBasedIdx)

                .RegisterType<IDataBuffer, DataBufferByteArray>()
                .RegisterType<IDataBuffer, DataBufferByteList>(Constants.TypeMappingRegistrationNames.InitialDataBuffer)
                .RegisterType<IDataBuffer, DataBufferRedirectionPipeline>(Constants.TypeMappingRegistrationNames.PipelineDataBuffer)
                .RegisterType<IDataBufferArrayInitializer, DataBufferByteArray>()

                .RegisterType<IFieldValueSerializer, FieldValueSerializer>()
            ;
        }
    }
}
