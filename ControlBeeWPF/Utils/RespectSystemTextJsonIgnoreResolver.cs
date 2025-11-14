using System.Collections.Concurrent;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ControlBeeWPF.Utils;

// Written by GPT.
public class RespectSystemTextJsonIgnoreResolver : DefaultContractResolver
{
    private static readonly ConcurrentDictionary<Type, JsonContract> _contractCache = new();

    protected override JsonContract CreateContract(Type objectType)
    {
        return _contractCache.GetOrAdd(objectType, base.CreateContract);
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var prop = base.CreateProperty(member, memberSerialization);

        // Extremely fast ignore check (cached per-member)
        if (ShouldIgnore(member))
            prop.Ignored = true;

        return prop;
    }

    private static readonly ConcurrentDictionary<MemberInfo, bool> _ignoreCache = new();

    private static bool ShouldIgnore(MemberInfo member)
    {
        return _ignoreCache.GetOrAdd(member, static m =>
            m.IsDefined(typeof(System.Text.Json.Serialization.JsonIgnoreAttribute), inherit: false));
    }
}