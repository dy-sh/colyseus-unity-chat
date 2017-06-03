
public static class ChatUtils
{
    public static string PathToString(string[] path)
    {
        string fullPath = "";
        for (int i = 0; i < path.Length; i++)
        {
            fullPath += path[i];
            if (i != path.Length - 1)
                fullPath += ".";
        }
        return fullPath;
    }

    public static string ValueToString(object value)
    {
        // if (value is IndexedDictionary<string, object>)
        // {
        //     string val = "";
        //     var dic = (IndexedDictionary<string, object>)value;
        //     foreach (var key in dic.Keys)
        //     {
        //     }

        //     for (int i = 0; i < dic.Keys.Count; i++)
        //     {
        //         var key = dic.Keys[i];
        //         val += key + ":" + dic[key];
        //         if (i != dic.Keys.Count - 1)
        //             val += ", ";
        //     }

        //     return val;
        // }
        // else
        // {
        return value.ToString();
        // }
    }

}