//Josip Medved <jmedved@jmedved.com>  http://www.jmedved.com

//2007-10-15: New version.
//2007-11-15: When setting is written, it's cache is invalidated in order to force re-reading from registry.
//2007-11-21: State is thrown out.
//2007-12-23: Added trace for configuration settings.
//            Fixed error that prevented cache from working.
//2007-12-28: Added reading from command-line.
//            App.config is case insensitive.
//            Trace is culture insensitive.
//2008-01-03: Fixed bug with cache invalidation.
//            Added checks for null key.
//            Added Resources.
//2008-04-11: Cleaned code to match FxCop 1.36 beta 2 (CompoundWordsShouldBeCasedCorrectly).
//2008-04-26: Fixed case sensitivity bug when reading command line (introduced with FxCop cleaning).
//2008-11-07: Inserted Args [001] class in order to perform proper command line parsing.
//2009-07-04: Compatibility with Mono 2.4.
//2010-10-31: Added option to skip registry writes (NoRegistryWrites).
//2011-08-26: Added Defaults property.
//2013-04-13: Writing null deletes the item and general refactoring.


using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;

namespace Medo.Configuration {

    /// <summary>
    /// Provides cached access to reading and writing settings.
    /// All settings are read in this order:
    ///   - Command line
    ///   - App.config
    ///   - registry (HKLM\Software\Company\Product)
    ///   - registry (HKCU\Software\Company\Product)
    /// Writing of settings is done in:
    ///   - registry (HKCU\Software\Company\Product)
    /// In case setting doesn't exist on reading, one is written with current value in:
    ///   - registry (HKCU\Software\Company\Product)
    /// Registry key contains company and (product|title|name).
    /// This class is thread-safe.
    /// </summary>
    public static class Settings {

        private static readonly object SyncRoot = new object(); //used for every access


        private static string _subkeyPath;
        /// <summary>
        /// Gets/sets subkey used for registry storage.
        /// </summary>
        public static String SubkeyPath {
            get {
                lock (SyncRoot) {
                    if (Settings._subkeyPath == null) {
                        Assembly assembly = Assembly.GetEntryAssembly();
                        if (assembly == null) { assembly = Assembly.GetExecutingAssembly(); }

                        string company = null;
                        object[] companyAttributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
                        if ((companyAttributes != null) && (companyAttributes.Length >= 1)) {
                            company = ((AssemblyCompanyAttribute)companyAttributes[companyAttributes.Length - 1]).Company;
                        }

                        string product = null;
                        object[] productAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
                        if ((productAttributes != null) && (productAttributes.Length >= 1)) {
                            product = ((AssemblyProductAttribute)productAttributes[productAttributes.Length - 1]).Product;
                        } else {
                            object[] titleAttributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
                            if ((titleAttributes != null) && (titleAttributes.Length >= 1)) {
                                product = ((AssemblyTitleAttribute)titleAttributes[titleAttributes.Length - 1]).Title;
                            } else {
                                product = assembly.GetName().Name;
                            }
                        }

                        string path = "Software";
                        if (!string.IsNullOrEmpty(company)) { path += "\\" + company; }
                        if (!string.IsNullOrEmpty(product)) { path += "\\" + product; }

                        _subkeyPath = path;
                    }
                    return _subkeyPath;
                }
            }
            set { lock (SyncRoot) { _subkeyPath = value; } }
        }

        /// <summary>
        /// Gets/sets whether settings should be written to registry.
        /// </summary>
        public static Boolean NoRegistryWrites { get; set; }

        /// <summary>
        /// Clears all cached data so on next access re-read of configuration data will occur.
        /// </summary>
        public static void ClearCachedData() {
            lock (SyncRoot) {
                Cache.Clear();
            }
        }


        #region String

        /// <summary>
        /// Retrieves the value associated with the specified key. If the key is not found in app.config, registry is checked (HKLM, then HKCU), if key is still not found returns the default value that you provide and creates entry in registry.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="defaultValue">The value to return if key does not exist.</param>
        /// <exception cref="ArgumentNullException">Key cannot be null.</exception>
        public static String Read(String key, String defaultValue) {
            if (key == null) { throw new ArgumentNullException("key", Resources.ExceptionKeyCannotBeNull); }

            lock (SyncRoot) {
                if (Cache.Contains(key)) { return Cache.Read(key); }

                string retValue = defaultValue;
                try {
                    if (_args.ContainsKey(key)) { //CommandLine
                        retValue = _args.GetValue(key);
                    } else if (AppConfig.ContainsKey(key)) {//AppConfig
                        retValue = AppConfig[key];
                    } else if (TryRegistryRead(key, Registry.LocalMachine, out retValue)) { //Registry (HKLM)
                    } else if (TryRegistryRead(key, Registry.CurrentUser, out retValue)) { //Registry (HKCU)
                    } else if ((Settings.Defaults != null) && (Settings.Defaults.ContainsKey(key))) { //Defaults
                        retValue = Settings.Defaults[key];
                    } else { //default
                        retValue = defaultValue;
                    }
                } finally {
                    Cache.Write(key, retValue);
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "V: Settings: {0} = '{1}'.", key, retValue));
                }

                return (string)retValue;
            }
        }

        /// <summary>
        /// Sets the value for specified key. If the specified key does not exist, it is created.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">The value to write. If value is null, it will be deleted.</param>
        /// <exception cref="ArgumentNullException">Key cannot be null.</exception>
        public static void Write(String key, String value) {
            if (key == null) { throw new ArgumentNullException("key", Resources.ExceptionKeyCannotBeNull); }

            lock (SyncRoot) {
                Cache.Write(key, value);
                if (Settings.NoRegistryWrites == false) {
                    try {
                        using (var rk = Registry.CurrentUser.CreateSubKey(Settings.SubkeyPath)) {
                            if (rk != null) {
                                if (value != null) {
                                    rk.SetValue(key, value, RegistryValueKind.String);
                                } else {
                                    rk.DeleteValue(key, false);
                                }
                            }
                        }
                    } catch (IOException) { //key is deleted. 
                    } catch (UnauthorizedAccessException) { } //key is write protected. 
                }
            }
        }

        #endregion


        #region Integer

        /// <summary>
        /// Retrieves the value associated with the specified key. If the key is not found in app.config, registry is checked (HKLM, then HKCU), if key is still not found returns the default value that you provide and creates entry in registry.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="defaultValue">The value to return if key does not exist.</param>
        /// <exception cref="System.FormatException">Input string was not in a correct format.</exception>
        /// <exception cref="ArgumentNullException">Key cannot be null.</exception>
        public static Int32 Read(String key, Int32 defaultValue) {
            if (key == null) { throw new ArgumentNullException("key", Resources.ExceptionKeyCannotBeNull); }

            lock (SyncRoot) {
                if (Cache.Contains(key)) { return GetInt32(Cache.Read(key), defaultValue); }

                int retValue = defaultValue;
                try {
                    if (_args.ContainsKey(key)) { //CommandLine
                        retValue = GetInt32(_args.GetValue(key), defaultValue);
                    } if (AppConfig.ContainsKey(key)) { //AppConfig
                        retValue = GetInt32(AppConfig[key], defaultValue);
                    } else if (TryRegistryRead(key, Registry.LocalMachine, out retValue)) { //Registry (HKLM)
                    } else if (TryRegistryRead(key, Registry.CurrentUser, out retValue)) { //Registry (HKCU)
                    } else if ((Settings.Defaults != null) && (Settings.Defaults.ContainsKey(key))) { //Defaults
                        retValue = GetInt32(Settings.Defaults[key], defaultValue);
                    } else { //default
                        retValue = defaultValue;
                    }
                } finally {
                    Cache.Write(key, retValue.ToString(CultureInfo.InvariantCulture));
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "V: Settings: {0} = '{1}'.", key, retValue));
                }

                return retValue;
            }
        }

        /// <summary>
        /// Sets the value for specified key. If the specified key does not exist, it is created.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentNullException">Key cannot be null.</exception>
        public static void Write(String key, Int32 value) {
            if (key == null) { throw new ArgumentNullException("key", Resources.ExceptionKeyCannotBeNull); }

            lock (SyncRoot) {
                Cache.Write(key, value.ToString(CultureInfo.InvariantCulture));
                if (Settings.NoRegistryWrites == false) {
                    try {
                        using (var rk = Registry.CurrentUser.CreateSubKey(Settings.SubkeyPath)) {
                            if (rk != null) {
                                rk.SetValue(key, value, RegistryValueKind.DWord);
                            }
                        }
                    } catch (IOException) { //key is deleted. 
                    } catch (UnauthorizedAccessException) { } //key is write protected.
                }
            }
        }

        #endregion


        #region Boolean

        /// <summary>
        /// Retrieves the value associated with the specified key. If the key is not found in app.config, registry is checked (HKLM, then HKCU), if key is still not found returns the default value that you provide and creates entry in registry.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="defaultValue">The value to return if key does not exist.</param>
        /// <exception cref="System.FormatException">Input string was not in a correct format.</exception>
        /// <exception cref="ArgumentNullException">Key cannot be null.</exception>
        public static Boolean Read(String key, Boolean defaultValue) {
            if (key == null) { throw new ArgumentNullException("key", Resources.ExceptionKeyCannotBeNull); }

            if (Cache.Contains(key)) { return GetBoolean(Cache.Read(key), defaultValue); }

            bool retValue = defaultValue;
            lock (SyncRoot) {
                try {
                    if (_args.ContainsKey(key)) { //CommandLine
                        retValue = GetBoolean(_args.GetValue(key), defaultValue);
                    } else if (AppConfig.ContainsKey(key)) { //AppConfig
                        retValue = GetBoolean(AppConfig[key], defaultValue);
                    } else if (TryRegistryRead(key, Registry.LocalMachine, out retValue)) { //Registry (HKLM)
                    } else if (TryRegistryRead(key, Registry.CurrentUser, out retValue)) { //Registry (HKCU)
                    } else if ((Settings.Defaults != null) && (Settings.Defaults.ContainsKey(key))) { //Defaults
                        retValue = GetBoolean(Settings.Defaults[key], defaultValue);
                    } else { //default
                        retValue = defaultValue;
                    }
                } finally {
                    Cache.Write(key, retValue.ToString(CultureInfo.InvariantCulture));
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "V: Settings: {0} = '{1}'", key, retValue));
                }

                return retValue;
            }
        }

        /// <summary>
        /// Sets the value for specified key. If the specified key does not exist, it is created.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">The value to write.</param>4
        /// <exception cref="ArgumentNullException">Key cannot be null.</exception>
        public static void Write(String key, Boolean value) {
            Write(key, value ? 1 : 0);
        }

        #endregion


        #region Double

        /// <summary>
        /// Retrieves the value associated with the specified key. If the key is not found in app.config, registry is checked (HKLM, then HKCU), if key is still not found returns the default value that you provide and creates entry in registry.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="defaultValue">The value to return if key does not exist.</param>
        /// <exception cref="ArgumentNullException">Key cannot be null.</exception>
        public static Double Read(String key, Double defaultValue) {
            if (key == null) { throw new ArgumentNullException("key", Resources.ExceptionKeyCannotBeNull); }
            return GetDouble(Read(key, defaultValue.ToString(CultureInfo.InvariantCulture)), defaultValue);
        }

        /// <summary>
        /// Sets the value for specified key. If the specified key does not exist, it is created.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">The value to write.</param>4
        /// <exception cref="ArgumentNullException">Key cannot be null.</exception>
        public static void Write(String key, Double value) {
            if (key == null) { throw new ArgumentNullException("key", Resources.ExceptionKeyCannotBeNull); }
            Settings.Write(key, value.ToString(CultureInfo.InvariantCulture));
        }

        #endregion


        #region Cache (private)

        private static class Cache {

            private static Dictionary<string, string> _cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            private static readonly object _cacheSyncRoot = new object();


            internal static void Clear() {
                lock (_cacheSyncRoot) {
                    _cache.Clear();
                }
            }

            internal static bool Contains(string key) {
                lock (_cacheSyncRoot) {
                    return _cache.ContainsKey(key);
                }
            }

            internal static string Read(string key) {
                lock (_cacheSyncRoot) {
                    if (_cache.ContainsKey(key)) {
                        return _cache[key];
                    }
                    return null;
                }
            }

            internal static void Write(string key, string value) {
                lock (_cacheSyncRoot) {
                    if (value != null) {
                        if (_cache.ContainsKey(key)) {
                            _cache[key] = value;
                        } else {
                            _cache.Add(key, value);
                        }
                    } else if (_cache.ContainsKey(key)) {
                        _cache.Remove(key);
                    }
                }
            }

        }

        #endregion


        #region AppConfig

        private static Dictionary<string, string> _appConfig;

        private static Dictionary<string, string> AppConfig {
            get {
                if (_appConfig == null) {
                    _appConfig = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < ConfigurationManager.AppSettings.Count; ++i) {
                        string currKey = ConfigurationManager.AppSettings.GetKey(i).ToUpperInvariant();
                        string[] currValues = ConfigurationManager.AppSettings.GetValues(i);
                        string currValue = string.Empty;
                        if (currValues.Length > 0) { currValue = currValues[currValues.Length - 1]; }
                        if (!string.IsNullOrEmpty(currKey)) {
                            if (_appConfig.ContainsKey(currKey)) {
                                _appConfig[currKey] = currValue;
                            } else {
                                _appConfig.Add(currKey, currValue);
                            }
                        }
                    }

                }
                return _appConfig;
            }
        }

        #endregion


        #region Args

        private static Args _args = Args.Current;

        private class Args {

            private static Args _current;
            /// <summary>
            /// Gets command-line arguments for current application.
            /// </summary>
            public static Args Current {
                get {
                    if (_current == null) {
                        string[] envArgs = Environment.GetCommandLineArgs();
                        _current = new Args(envArgs, 1, envArgs.Length - 1);
                    }
                    return _current;
                }
            }


            /// <summary>
            /// Creates new instance.
            /// </summary>
            /// <param name="array">Array of all arguments.</param>
            /// <param name="offset">Index of starting item.</param>
            /// <param name="count">Number of items.</param>
            public Args(string[] array, int offset, int count) {
                InitializeFromArray(array, offset, count, new string[] { "/", "--", "-" }, new char[] { ':', '=' });
            }


            private Dictionary<string, List<string>> _items;

            private void InitializeFromArray(string[] array, int offset, int count, string[] prefixes, char[] separators) {
                _items = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

                for (int i = 0; i < count; ++i) {
                    string curr = array[offset + i];
                    string key = null;
                    string value = null;

                    bool isDone = false;

                    //named
                    for (int j = 0; j < prefixes.Length; ++j) {
                        string currPrefix = prefixes[j];
                        if (curr.StartsWith(currPrefix, StringComparison.Ordinal)) {
                            int iSep = curr.IndexOfAny(separators);
                            if (iSep >= 0) {
                                key = curr.Substring(currPrefix.Length, iSep - currPrefix.Length);
                                value = curr.Remove(0, iSep + 1);
                            } else {
                                key = curr.Substring(currPrefix.Length, curr.Length - currPrefix.Length);
                                value = string.Empty;
                            }
                            isDone = true;
                            break;
                        }
                    }

                    //noname
                    if (!isDone) {
                        key = string.Empty;
                        value = curr;
                    } else {
                        key = key.ToUpperInvariant();
                    }

                    List<string> currList;
                    if (_items.ContainsKey(key)) {
                        currList = _items[key];
                    } else {
                        currList = new List<string>();
                        _items.Add(key, currList);
                    }
                    currList.Add(value);
                }
            }


            /// <summary>
            /// Return true if key exists in current list.
            /// </summary>
            /// <param name="key">Key.</param>
            public bool ContainsKey(string key) {
                if (key == null) {
                    key = string.Empty;
                } else {
                    key = key.ToUpperInvariant();
                }
                return _items.ContainsKey(key);
            }


            /// <summary>
            /// Returns single value connected to given key.
            /// If key is not found, null is returned.
            /// If multiple values exist, last one is returned.
            /// </summary>
            /// <param name="key">Key.</param>
            public string GetValue(string key) {
                if (key == null) {
                    key = string.Empty;
                } else {
                    key = key.ToUpperInvariant();
                }

                if (_items.ContainsKey(key)) {
                    return _items[key][_items[key].Count - 1];
                } else {
                    return null;
                }
            }


            private static class Helper {

                internal enum State {
                    Default = 0,
                    Quoted = 1
                }

            }

        }

        #endregion


        private static Dictionary<string, string> Defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Sets defaults to be used as last priority.
        /// It will also invalidate cache for that element so that load can happen.
        /// </summary>
        /// <param name="key">Setting key.</param>
        /// <param name="value">Setting value.</param>
        public static void SetDefaults(String key, String value) {
            Cache.Write(key, null);
            if (Settings.Defaults.ContainsKey(key)) {
                Settings.Defaults[key] = value;
            } else {
                Settings.Defaults.Add(key, value);
            }
        }

        /// <summary>
        /// Sets defaults to be used as last priority.
        /// </summary>
        /// <param name="key">Setting key.</param>
        /// <param name="value">Setting value.</param>
        public static void SetDefaults(String key, Int32 value) {
            SetDefaults(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Sets defaults to be used as last priority.
        /// </summary>
        /// <param name="key">Setting key.</param>
        /// <param name="value">Setting value.</param>
        public static void SetDefaults(String key, Boolean value) {
            SetDefaults(key, value ? 1 : 0);
        }

        /// <summary>
        /// Sets defaults to be used as last priority.
        /// </summary>
        /// <param name="key">Setting key.</param>
        /// <param name="value">Setting value.</param>
        public static void SetDefaults(String key, Double value) {
            SetDefaults(key, value.ToString(CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// Sets defaults to be used as last priority.
        /// </summary>
        /// <param name="defaults">Name/value collection of settings.</param>
        public static void SetDefaults(IDictionary<String, String> defaults) {
            if (defaults != null) {
                foreach (var item in defaults) {
                    SetDefaults(item.Key, item.Value);
                }
            }
        }


        private static bool TryRegistryRead(string key, RegistryKey root, out string value) {
            try {
                using (var rk = root.OpenSubKey(Settings.SubkeyPath, false)) {
                    if (rk != null) {
                        object regValue = rk.GetValue(key, null);
                        if (regValue != null) {
                            switch (Settings.IsRunningOnMono ? RegistryValueKind.String : rk.GetValueKind(key)) {
                                case RegistryValueKind.String:
                                case RegistryValueKind.ExpandString:
                                    value = regValue as string;
                                    return true;
                                case RegistryValueKind.MultiString:
                                    value = string.Join("\n", (regValue as string[]));
                                    return true;
                            }
                        }
                    }
                }
            } catch (SecurityException) { }

            value = default(string);
            return false;
        }

        private static bool TryRegistryRead(string key, RegistryKey root, out int value) {
            try {
                using (var rk = root.OpenSubKey(Settings.SubkeyPath, false)) {
                    if (rk != null) {
                        object regValue = rk.GetValue(key, null);
                        if (regValue != null) {
                            switch (Settings.IsRunningOnMono ? RegistryValueKind.String : rk.GetValueKind(key)) {
                                case RegistryValueKind.DWord:
                                    value = (int)regValue;
                                    return true;
                                case RegistryValueKind.String:
                                    value = GetInt32(String.Format(CultureInfo.InvariantCulture, "{0}", regValue), default(int));
                                    return true;
                            }
                        }
                    }
                }
            } catch (SecurityException) { }

            value = default(int);
            return false;
        }

        private static bool TryRegistryRead(string key, RegistryKey root, out bool value) {
            try {
                using (var rk = root.OpenSubKey(Settings.SubkeyPath, false)) {
                    if (rk != null) {
                        object regValue = rk.GetValue(key, null);
                        if (regValue != null) {
                            switch (Settings.IsRunningOnMono ? RegistryValueKind.String : rk.GetValueKind(key)) {
                                case RegistryValueKind.DWord:
                                    value = (int)regValue != 0;
                                    return true;
                                case RegistryValueKind.String:
                                    value = GetBoolean(String.Format(CultureInfo.InvariantCulture, "{0}", regValue), default(bool));
                                    return true;
                            }
                        }
                    }
                }
            } catch (SecurityException) { }

            value = default(bool);
            return false;
        }

        private static int GetInt32(string text, int defaultValue) {
            if (text == null) { return defaultValue; }

            int value;
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) {
                return value;
            } else {
                return defaultValue;
            }
        }

        private static bool GetBoolean(string text, bool defaultValue) {
            if (text == null) { return defaultValue; }

            bool valueBoolean;
            if (bool.TryParse(text, out valueBoolean)) {
                return valueBoolean;
            } else {
                int valueInteger;
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out valueInteger)) {
                    return (valueInteger != 0);
                } else {
                    return defaultValue;
                }
            }
        }

        private static double GetDouble(string text, double defaultValue) {
            if (text == null) { return defaultValue; }

            double value;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) {
                return value;
            } else {
                return defaultValue;
            }
        }


        private static bool IsRunningOnMono {
            get {
                return (Type.GetType("Mono.Runtime") != null);
            }
        }


        private static class Resources {
            internal const string ExceptionKeyCannotBeNull = "Key cannot be null.";
        }

    }
}
