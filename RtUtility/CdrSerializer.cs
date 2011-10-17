using System;
using System.IO;
using System.Reflection;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Cdr;

namespace RtUtility
{
    public class CdrSerializer<TDataType>
    {
        private readonly dynamic _serializer;

        private readonly byte _isLittleEndian;

        public CdrSerializer(object serializer, bool isLittleEndian)
        {
            _serializer = new AccessPrivateWrapper(serializer);

            // _isLittleEndian = 0: ビッグエンディアン、 1: リトルエンディアン
            if (isLittleEndian)
            {
                _isLittleEndian = 1;
            }
            else
            {
                _isLittleEndian = 0;
            }
        }

        public void Serialize(TDataType data, Stream outputStream)
        {
            var stream = new CdrOutputStreamImpl(outputStream, _isLittleEndian);

            // これを設定しておかないと日本語の送受信ができない
            stream.WCharSet = (int)Ch.Elca.Iiop.Services.WCharSet.UTF16;

            _serializer.Serialize(data, stream);
        }

        public TDataType Deserialize(Stream inputStream)
        {
            var stream = new CdrInputStreamImpl(inputStream);
            stream.ConfigStream(_isLittleEndian, new GiopVersion(1, 2));

            // これを設定しておかないと日本語の送受信ができない
            stream.WCharSet = (int)Ch.Elca.Iiop.Services.WCharSet.UTF16;
            
            return (TDataType)_serializer.Deserialize(stream);
        }

    }
}
