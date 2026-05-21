using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class GameTypeBinder : ISerializationBinder
{
    private readonly Dictionary<string, Type> _typeCache = new();
    private readonly List<Assembly> _gameAssemblies;
    private readonly DefaultSerializationBinder _defaultBinder = new();

    public GameTypeBinder()
    {
        _gameAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName!.Contains("TextGame"))
            .ToList();
    }

    public Type BindToType(string assemblyName, string typeName)
    {
        var key = $"{typeName}, {assemblyName}";
        if (_typeCache.TryGetValue(key, out var cachedType))
            return cachedType;

        Type? type = null;
        foreach (var assembly in _gameAssemblies)
        {
            type = assembly.GetType(typeName);
            if (type != null) break;
        }

        if (type == null)
        {
            type = _defaultBinder.BindToType(assemblyName, typeName);
        }

        _typeCache[key] = type;
        return type;
    }

    public void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        if (_gameAssemblies.Contains(serializedType.Assembly))
        {
            assemblyName = serializedType.Assembly.FullName!;
            typeName = serializedType.FullName!;
        }
        else
        {
            _defaultBinder.BindToName(serializedType, out assemblyName, out typeName);
        }
    }
}