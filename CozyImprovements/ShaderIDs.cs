using System;
using UnityEngine;

namespace SpyciBot.LC.CozyImprovements;

public static class ShaderIDs
{
    public static readonly Lazy<int> EmissiveColor = new(() => Shader.PropertyToID("_EmissiveColor"));
    public static readonly Lazy<int> BaseColor = new(() => Shader.PropertyToID("_BaseColor"));
}