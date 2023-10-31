using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FluidFramework.Context
{
    /// <summary>
    /// Singleton class that stores global information.
    /// </summary>
    public class ClientContext
    {
        private static ClientContext _instance;
        private static Guid _sessionId;

        /// <summary>
        /// The properties stored in context.
        /// </summary>
        public Properties Properties { get; set; }

        /// <summary>
        /// Specifies that the context is stored in session.
        /// </summary>
        public static bool IsSessionContext = false;

        /// <summary>
        /// Static storage manager class.
        /// </summary>
        public static IContextStorageManager StorageManager = null;

        private readonly Dictionary<string, object> _globals = new Dictionary<string, object>();

        /// <summary>
        /// Static constructor
        /// </summary>
        static ClientContext()
        {
            _sessionId = Guid.NewGuid();
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private ClientContext()
        {
            Properties = new Properties();
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static ClientContext Instance
        {
            get
            {
                if (StorageManager != null)
                {
                    ClientContext context = StorageManager.GetInstance();
                    if (context == null)
                    {
                        context = new ClientContext();
                        StorageManager.SetInstance(context);
                    }

                    return context;
                }
                
                if (IsSessionContext)
                {
                    string sessionId = _sessionId.ToString();
                    ClientContext context = HttpContext.Current.Session[sessionId] as ClientContext;
                    if (context == null)
                    {
                        context = new ClientContext();
                        HttpContext.Current.Session[sessionId] = context;
                    }
                    return context;
                }

                return _instance ?? (_instance = new ClientContext());
            }
        }

        /// <summary>
        /// Adds or updates an object in the global dictionary.
        /// </summary>
        public void Set(string name, object data)
        {
            if (_globals.ContainsKey(name))
            {
                _globals[name] = data;
            }
            else
            {
                _globals.Add(name, data);
            }
        }

        /// <summary>
        /// Gets an object from the global dictionary.
        /// </summary>
        public dynamic Get(string name)
        {
            if (_globals.ContainsKey(name))
            {
                return _globals[name];
            }
            return null;
        }

        /// <summary>
        /// Removes an object from the global dictionary.
        /// </summary>
        public void Remove(string name)
        {
            if (_globals.ContainsKey(name))
            {
                _globals.Remove(name);
            }
        }

        /// <summary>
        /// Removes from the global dictionary all the objects whose names start with the given prefix.
        /// </summary>
        public void RemoveByPrefix(string prefix)
        {
            List<string> keys = _globals.Where(g => g.Key.StartsWith(prefix)).Select(g => g.Key).ToList();
            foreach (string key in keys)
            {
                _globals.Remove(key);
            }
        }

        /// <summary>
        /// Removes from the global dictionary all the objects whose names end with the given suffix.
        /// </summary>
        public void RemoveBySuffix(string suffix)
        {
            List<string> keys = _globals.Where(g => g.Key.EndsWith(suffix)).Select(g => g.Key).ToList();
            foreach (string key in keys)
            {
                _globals.Remove(key);
            }
        }

        /// <summary>
        /// Clears the global dictionary.
        /// </summary>
        public void Clear()
        {
            _globals.Clear();
        }
    }
}
