
using UnityEngine;

public class Validations
{
    public static void IsNull(object data, string message) {
        if (data == null) { 
            Debug.LogError($"{message} is null");
        }
    }
}
