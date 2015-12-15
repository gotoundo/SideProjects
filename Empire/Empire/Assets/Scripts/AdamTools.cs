using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class AdamTools
{
    public static GameObject FindInChildren(this GameObject go, string name)
    {
        try
        {
            return (from x in go.GetComponentsInChildren<Transform>()
                    where x.gameObject.name == name
                    select x.gameObject).First();
        }
        catch { }

        return null;
    }
}

