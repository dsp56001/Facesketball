using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonoGameLibrary.Network.Serializer
{
    public static class Serializer
    {
        //Turns an object that is marked as Serializable into a Byte array which
        //is necessary to write across the network.
        public static Byte[] SerializeObject(object obj)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        //Turn a Byte array into an object.  Necessary for reading in Serializable
        //objects on the recieving end of the network
        public static object DeserializeByteArray(Byte[] array)
        {
            return Serializer.DeserializeByteArray(array, 0);
        }

        //This overload allows the caller to specify a point in the array to being
        //the deserializing.  Useful if you have one Byte array with multiple
        //objects of a known size.  
        public static object DeserializeByteArray(Byte[] array, long position)
        {
            MemoryStream ms = new MemoryStream(array);
            BinaryFormatter bf = new BinaryFormatter();

            ms.Position = position;

            return bf.Deserialize(ms);
        }
    }
}
