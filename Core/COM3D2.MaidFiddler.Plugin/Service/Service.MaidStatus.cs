using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MaidStatus;

namespace COM3D2.MaidFiddler.Plugin.Service
{
    public partial class Service
    {
        private Dictionary<string, MethodInfo> maidGetters;
        private Dictionary<string, MethodInfo> maidSetters;

        public Dictionary<string, string> GetMaidList()
        {
            return GameMain.Instance.CharacterMgr.GetStockMaidList().ToDictionary(m => m.status.guid, m => m.status.fullNameEnStyle);
        }

        public string[] GetMaidParameterList() => maidSetters.Keys.ToArray();

        public object GetMaidProperty(string maidId, string propertyName)
        {
            if (!maidGetters.TryGetValue(propertyName, out MethodInfo getter))
                throw new ArgumentException($"No such property: {propertyName}", nameof(propertyName));

            string id = maidId.ToLower(CultureInfo.InvariantCulture);

            Maid[] maids = GameMain.Instance.CharacterMgr.GetStockMaidList().Where(m => m.status.guid.ToLower(CultureInfo.InvariantCulture).StartsWith(id))
                                .ToArray();

            if (maids.Length == 0)
                throw new ArgumentException($"No such maid with ID: {maidId}", nameof(maidId));
            if (maids.Length > 1)
                throw new
                        ArgumentException($"Found multiple maids whose ID starts the same:\n\n{string.Join("\n", maids.Select(m => $"{m.status.fullNameEnStyle}; ID: {m.status.guid}").ToArray())}\nPlease give a more specific ID!");

            return getter.Invoke(maids[0].status, new object[0]);
        }

        public void SetMaidProperty(string maidId, string propertyName, object value)
        {
            if (!maidSetters.TryGetValue(propertyName, out MethodInfo setter))
                throw new ArgumentException($"No such property: {propertyName}", nameof(propertyName));
            Type paramType = setter.GetParameters()[0].ParameterType;

            object val;
            try
            {
                val = Convert.ChangeType(value, paramType);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Cannot convert value to {paramType.FullName}.", e);
            }

            string id = maidId.ToLower(CultureInfo.InvariantCulture);

            Maid[] maids = GameMain.Instance.CharacterMgr.GetStockMaidList().Where(m => m.status.guid.ToLower(CultureInfo.InvariantCulture).StartsWith(id))
                                .ToArray();

            if (maids.Length == 0)
                throw new ArgumentException($"No such maid with ID: {maidId}", nameof(maidId));
            if (maids.Length > 1)
                throw new
                        ArgumentException($"Found multiple maids whose ID starts the same:\n\n{string.Join("\n", maids.Select(m => $"{m.status.fullNameEnStyle}; ID: {m.status.guid}").ToArray())}\nPlease give a more specific ID!");

            setter.Invoke(maids[0].status, new[] {val});
        }

        private void InitMaidStatus()
        {
            maidSetters = new Dictionary<string, MethodInfo>();
            maidGetters = new Dictionary<string, MethodInfo>();

            PropertyInfo[] props = typeof(Status).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo propertyInfo in props)
            {
                MethodInfo get = propertyInfo.GetGetMethod();
                MethodInfo set = propertyInfo.GetSetMethod();

                if (get != null)
                    maidGetters.Add(propertyInfo.Name, get);

                if (set != null)
                    maidSetters.Add(propertyInfo.Name, set);
            }
        }
    }
}