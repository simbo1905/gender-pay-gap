using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Xml.Serialization;
using Extensions;

[Serializable]
public class StyleCollection
{
    public StyleCollection()
    {
    }
    public StyleCollection(char separator)
    {
        Separator = separator;
    }
    public StyleCollection(string value, string namePrefix = null, char separator = ';'):this(separator)
    {
        Value = value;
        NamePrefix = namePrefix;
    }

    readonly string NamePrefix;
    public readonly char Separator=';';

    [field: NonSerialized]
    CssStyleCollection _collection;
    CssStyleCollection collection
    {
        get
        {
            if (_collection == null)
            {
                _collection = new System.Web.UI.HtmlControls.HtmlGenericControl().Style;
                _collection.Value = Value;
            }
            return _collection;
        }
    }

    public string this[string key]
    {
        get
        {
            key = key.Trim();
            if (!string.IsNullOrWhiteSpace(NamePrefix) && !ContainsKey(key))
            {
                if (key.StartsWithI(NamePrefix))
                    key = key.Substring(NamePrefix.Length);
                else
                    key = NamePrefix + key;
            }

            var i = IndexOf(key);
            if (i>-1) return collection[Keys[i]].TrimI();
            return null;
        }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                Remove(key);
            else
                Add(key,value);
        }
    }

    string _Value = "";
    [XmlElement]
    public string Value
    {
        get
        {
            return _Value == null ? null : _Value.Replace(';',Separator);
        }
        set
        {
            collection.Clear();
            if (!string.IsNullOrWhiteSpace(value))
            {
                var args = value.SplitI(Separator.ToString());
                foreach (var arg in args)
                {
                    if (string.IsNullOrWhiteSpace(arg))continue;
                    string name = null;
                    string val = null;
                    var i = arg.IndexOf(':');
                    if (i > -1)
                    {
                        val=arg.Substring(i + 1);
                        name = arg.Substring(0, i);
                    }
                    else
                    {
                        name = arg;
                        val = "true";
                    }

                    if (string.IsNullOrWhiteSpace(name))new HandledException("Missing parameter name");
                    if (string.IsNullOrWhiteSpace(val))new HandledException("Missing parameter value");
                    collection[name.Trim()] = val.Trim();
                }
            }
            ;
            _Value = collection.Value == null ? null : collection.Value.TrimEnd(Separator,' ');
        }
    }

    public List<string> Keys
    {
        get
        {
            return collection.Keys.ToList<string>();
        }
    }

    public int Count
    {
        get
        {
            return collection.Count;
        }
    }

    public void Add(string key,string value)
    {
        key = key.Trim();
        Remove(key);
        collection.Add(key,value);

        _Value = collection.Value;
    }

    public void Remove(string key)
    {
        key = key.Trim();
        if (!string.IsNullOrWhiteSpace(NamePrefix) && !ContainsKey(key))
        {
            if (key.StartsWithI(NamePrefix))
                key=key.Substring(NamePrefix.Length);
            else
                key=NamePrefix + key;
        }

        var keys = Keys;
        foreach (string k in keys)
        {
            if (k.Trim().EqualsI(key)) collection.Remove(key);
        }

        _Value = collection.Value;
    }

    public void Clear()
    {
        collection.Clear();
        _Value = collection.Value;
    }

    public bool ContainsKey(string key)
    {
        return IndexOf(key)>-1;
    }

    public int IndexOf(string key)
    {
        key = key.Trim();
        var keys = Keys;
        for (var k=0;k<keys.Count;k++)
        {
            if (keys[k].Trim().EqualsI(key)) return k;
        }

        return -1;
    }

    public bool Equals(StyleCollection collection)
    {
        if (collection == null || collection.Count!=Count) return false;
        for (var i = 0; i < Keys.Count;i++)
        {
            if (!Keys[i].EqualsI(collection.Keys[i])) return false;
            if (!this[Keys[i]].EqualsI(collection[Keys[i]])) return false;
        }
        return true;
    }
}
