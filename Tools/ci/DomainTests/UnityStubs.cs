// Minimal stand-ins for the Unity and Addressables APIs referenced by WeldStudio.Core, so the domain
// (CharacterModel, commands, presets) can be compiled and tested with plain .NET in CI, without a Unity
// license. Only signatures are reproduced; no behaviour beyond what the domain tests need.
//
// When Core starts using a new Unity type or member, add the smallest possible stub here.
// The real EditMode tests inside Unity remain the source of truth.

using System;

namespace UnityEngine
{
    public class Object
    {
        public string name { get; set; }
    }

    public class ScriptableObject : Object { }
    public class Component : Object { }
    public class Renderer : Component { }
    public class SkinnedMeshRenderer : Renderer { }
    public class Transform : Component { }
    public class Animator : Component { }
    public class Material : Object { }
    public class Sprite : Object { }
    public class Texture2D : Object { }

    public class GameObject : Object
    {
        public T GetComponentInChildren<T>(bool includeInactive) => default;
    }

    public struct Color
    {
        public static Color white => default;
    }

    [AttributeUsage(AttributeTargets.Field)] public sealed class SerializeField : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string header) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TooltipAttribute : Attribute
    {
        public TooltipAttribute(string tooltip) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TextAreaAttribute : Attribute
    {
        public TextAreaAttribute(int minLines, int maxLines) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CreateAssetMenuAttribute : Attribute
    {
        public string fileName { get; set; }
        public string menuName { get; set; }
        public int order { get; set; }
    }
}

namespace UnityEngine.AddressableAssets
{
    [Serializable]
    public class AssetReference
    {
        public virtual bool RuntimeKeyIsValid() => false;
    }

    [Serializable]
    public class AssetReferenceT<TObject> : AssetReference where TObject : Object { }

    [Serializable]
    public class AssetReferenceGameObject : AssetReferenceT<GameObject>
    {
        public AssetReferenceGameObject(string guid) { }
    }

    [Serializable]
    public class AssetReferenceSprite : AssetReferenceT<Sprite>
    {
        public AssetReferenceSprite(string guid) { }
    }

    [Serializable]
    public class AssetReferenceTexture2D : AssetReferenceT<Texture2D>
    {
        public AssetReferenceTexture2D(string guid) { }
    }
}
