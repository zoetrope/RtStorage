using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RtUtility.Mock;

namespace RtStorage.Models
{
    public class RepositoryFactory
    {
        static public IRecordDescriptionRepository CreateRepository()
        {
#if USE_MOCK
            var proxy = new MockProxy<MockRecordDescriptionRepository>(new MockRecordDescriptionRepository());
            return proxy.GetTransparentProxy();
#else
            return new RecordDescriptionRepository();
#endif
        }

    }
}
