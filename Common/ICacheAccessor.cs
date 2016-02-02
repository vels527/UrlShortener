using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface ICacheAccessor
    {
        void Set(string key, string value);
        string Get(string key);
        void Clear(string key);
    }

}
