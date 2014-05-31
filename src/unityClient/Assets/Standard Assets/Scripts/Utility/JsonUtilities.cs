using LitJson;
using UnityEngine;
using System;
using System.Collections;

public class JsonUtilities
{
    public static float ParseFloat(JsonData jsonData, string fieldName)
    {
        JsonData field = jsonData[fieldName];

        return field.IsInt ? (float)((int)field) : (float)((double)field);
    }

    public static long ParseLong(JsonData jsonData, string fieldName)
    {
        JsonData field = jsonData[fieldName];

        return field.IsLong ? (long)field : (long)((int)field);
    }

    public static int ParseInt(JsonData jsonData, string fieldName)
    {
        return (int)jsonData[fieldName];
    }

    public static T ParseEnum<T>(JsonData jsonData, string fieldName) where T : IConvertible
    {
        int int_value = (int)jsonData[fieldName];

        return (T)Enum.ToObject(typeof(T), int_value);
    }

    public static uint ParseUInt(JsonData jsonData, string fieldName)
    {
        return (uint)((int)jsonData[fieldName]);
    }

    public static bool ParseBool(JsonData jsonData, string fieldName)
    {
        return (bool)jsonData[fieldName];
    }

    public static string ParseString(JsonData jsonData, string fieldName)
    {
        return (string)jsonData[fieldName];
    }
}
