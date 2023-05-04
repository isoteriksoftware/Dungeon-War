using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtils
{
    public static bool ValidateCheckEmptyString(Object thisObj, string fieldName, string stringToCheck)
    {
        if (stringToCheck.Equals(string.Empty))
        {
            Debug.Log($"{fieldName} is empty and must contain a value in object {thisObj.name}");
            return true;
        }

        return false;
    }

    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectToCheck)
    {
        bool error = false;
        int count = 0;

        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null)
            {
                Debug.Log($"{fieldName} has null values in object {thisObject.name}");
                error = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0)
        {
            Debug.Log($"{fieldName} has no values in object {thisObject.name}");
            error = true;
        }

        return error;
    }
}
