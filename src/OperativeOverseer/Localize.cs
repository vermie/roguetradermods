using Kingmaker.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OperativeOverseer;

public static class Localize
{
    public static LocalizedString Create(string key)
    {
        return new LocalizedString()
        {
            Key = key,
        };
    }

    public static LocalizedString CreateShared(string key)
    {
        var shared = ScriptableObject.CreateInstance<SharedStringAsset>();
        shared.String = Create(key);

        return new LocalizedString()
        {
            Shared = shared
        };
    }
}
