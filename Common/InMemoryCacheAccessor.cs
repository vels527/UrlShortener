using System.Collections.Generic;

namespace Common
{
    public class InMemoryCacheAccessor : ICacheAccessor
    {
        /// <summary>
        /// Holds In Memory cache data
        /// </summary>
        public Dictionary<string, string> TableStorage { get; set; }

        /// <summary>
        /// Constructor: Create Dictionary
        /// </summary>
        public InMemoryCacheAccessor()
        {
            if (this.TableStorage == null)
            {
                this.TableStorage = new Dictionary<string, string>();
            }
        }

        public void Set(string key, string value)
        {
            if (this.TableStorage.ContainsKey(key))
                this.TableStorage[key] = value;
            else
                this.TableStorage.Add(key, value);
        }

        public string Get(string key)
        {
            return this.TableStorage.ContainsKey(key) ? this.TableStorage[key] : null;
        }

        public void Clear(string key)
        {
            if (this.TableStorage.ContainsKey(key))
                this.TableStorage.Remove(key);
        }
    }
}
