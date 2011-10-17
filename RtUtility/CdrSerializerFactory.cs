using System;
using System.Reflection;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Util;
using RTC;
using omg.org.IOP;

namespace RtUtility
{
    public class CdrSerializerFactory
    {
        private dynamic _factory;

        public CdrSerializerFactory()
        {
            Assembly iiopChannelAsm = typeof(IiopChannel).Assembly;

            dynamic serializerFactory = AccessPrivateWrapper.FromType
                (iiopChannelAsm, "Ch.Elca.Iiop.Marshalling.SerializerFactory");

            dynamic codecFactory = AccessPrivateWrapper.FromType
                (iiopChannelAsm, "Ch.Elca.Iiop.Interception.CodecFactoryImpl", serializerFactory);

            var encoding = new Encoding(ENCODING_CDR_ENCAPS.ConstVal, 1, 2);

            var codec = codecFactory.create_codec(encoding);

            dynamic serializerFactoryConfig = AccessPrivateWrapper.FromType
                (iiopChannelAsm, "Ch.Elca.Iiop.Marshalling.SerializerFactoryConfig");

            var util = IiopUrlUtil.Create((Codec)codec);

            serializerFactory.Initalize(serializerFactoryConfig, util);

            _factory = serializerFactory;
        }

        public CdrSerializer<T> GetSerializer<T>(bool isLittleEndian = true)
        {
            var serializer = _factory.Create(typeof(T), AttributeExtCollection.EmptyCollection);
            return new CdrSerializer<T>(serializer, isLittleEndian);
        }
    }
}
