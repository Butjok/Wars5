using UnityEngine;
using UnityEngine.Assertions;

public static class ResourceExtensions {
    public static T LoadAs<T>(this string name)where T:UnityEngine.Object {
        var result = Resources.Load<T>(name);
        Assert.IsTrue(result);
        return result;
    }
}