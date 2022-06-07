using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Teams.EmbeddedChat.Helpers;

public static class SerializationHelper
{
    public static T Deserialize<T>(string json) where T : new()
    {
        if (string.IsNullOrEmpty(json))
            return new T();

        return JsonConvert.DeserializeObject<T>(json);
    }

    public static IEnumerable<T> DeserializeList<T>(string json) where T : new()
    {
        if (string.IsNullOrEmpty(json))
            return new List<T>();

        return JsonConvert.DeserializeObject<List<T>>(json);
    }

}
