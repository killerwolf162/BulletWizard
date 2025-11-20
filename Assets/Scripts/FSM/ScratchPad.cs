using System.Collections.Generic;

public class ScratchPad
{
    //A Dictionary that can hold any datatype like an int, string or object with a string as its key value
    public Dictionary<string, object> data = new Dictionary<string, object>();

    //A Method that returns the data value if the correct key is given, otherwise it returns the default value of the type
    //so if its an int it returns '0' if its a string it returns 'null'
    public T Get<T>(string key)
    {
        if (data.ContainsKey(key))
        {
            return (T)data[key];
        }
        return default(T);
    }

    //A Method that simply sets the data value and adds to the Dictionary
    public void Set<T>(string key, T value)
    {
        data[key] = value;
    }
}
