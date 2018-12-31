using System.Reflection;

namespace COM3D2.MaidFiddler.Core.Utils
{
    public static class ResourceUtil
    {
        public static byte[] GetResourceBytes(this Assembly self, string resourceName)
        {
            string name = $"{self.GetName().Name}.{resourceName.Replace(" ", "_").Replace("\\", ".").Replace("/", ".")}";
            using (var stream = self.GetManifestResourceStream(name))
            {
                if (stream == null)
                    return null;

                var result = new byte[stream.Length];
                stream.Read(result, 0, result.Length);
                return result;
            }
        }
    }
}