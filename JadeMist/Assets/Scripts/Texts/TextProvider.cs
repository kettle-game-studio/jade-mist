using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Unity.Collections;
using System.Data.Common;

public class TextProvider : MonoBehaviour
{
    public TextAsset[] textFiles;

    Dictionary<string, string> allTexts;

    public string GetText(string id)
    {
        if (allTexts.TryGetValue(id, out var val))
        {
            return val;
        }

        Debug.LogError($"Unknown text id '{id}'");
        return "Undefined text";
    }

    void Start()
    {
        allTexts = new Dictionary<string, string>();
        foreach (var file in textFiles)
        {
            var str = file.text;
            var o = JsonConvert.DeserializeObject(str);
            if (o == null)
            {
                Debug.LogError($"Malformed Json '{file.name}'");
                continue;
            }

            var err = VisitObject(o, "");
            if (err != null)
            {
                Debug.LogError($"Error parsing file '{file.name}': {err}");

            }
        }
    }

    Exception VisitObject(object o, string prefix)
    {

        if (o is JObject jo)
        {
            foreach (var kv in jo)
            {
                var p = (prefix == "") ? kv.Key : $"{prefix}.{kv.Key}";
 
                VisitObject(kv.Value, p);
            }
            return null;
        }

        if (o is JToken jt)
        {
            allTexts[prefix] = jt.ToString();
            return null;
        }

        return new Exception($"Malformed Json, expected Object or Token, got {o.GetType()}'");
    }
}
