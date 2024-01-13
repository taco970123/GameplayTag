using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Taco.Editor
{
    public static class BinaryHelper
    {
        public static byte[] ToBinary(this object obj)
        {
            using (MemoryStream mStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(mStream, obj);
                return mStream.ToArray();
            }
        }
        public static T Deserialize<T>(this byte[] bytes) where T : class
        {
            using (MemoryStream mStream = new MemoryStream(bytes))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(mStream) as T;
            }
        }
    }
}