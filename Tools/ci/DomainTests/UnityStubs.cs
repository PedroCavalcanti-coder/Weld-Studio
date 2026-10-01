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

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject => Activator.CreateInstance<T>();
    }

    public class Component : Object { }
    public class Renderer : Component { }
    public class SkinnedMeshRenderer : Renderer { }
    public class Transform : Component { }
    public class Animator : Component { }
    public class Material : Object { }
    public class Sprite : Object { }
    public class Texture2D : Object { }
    public class AnimationClip : Object { }

    public class GameObject : Object
    {
        public T GetComponentInChildren<T>(bool includeInactive) => default;
    }

    public struct Color
    {
        public float r, g, b, a;

        public Color(float r, float g, float b, float a = 1f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static Color white => new Color(1f, 1f, 1f);
    }

    public struct Vector3
    {
        public float x, y, z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 zero => new Vector3(0f, 0f, 0f);
        public static Vector3 one => new Vector3(1f, 1f, 1f);
        public static Vector3 Scale(Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);
    }

    public struct Ray
    {
        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        public Vector3 origin { get; }
        public Vector3 direction { get; }
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
#if UNITY_EDITOR
        public virtual Object editorAsset => null;
#endif
    }

    [Serializable]
    public class AssetReferenceT<TObject> : AssetReference where TObject : Object
    {
#if UNITY_EDITOR
        public new TObject editorAsset => null;
#endif
    }

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

#if UNITY_EDITOR
namespace UnityEditor
{
    public static class AssetDatabase
    {
        public static bool TryGetGUIDAndLocalFileIdentifier(UnityEngine.Object obj, out string guid, out long localId)
        {
            guid = null;
            localId = 0;
            return false;
        }

        public static string GUIDToAssetPath(string guid) => string.Empty;
    }

    public static class EditorUtility
    {
        public static void SetDirty(UnityEngine.Object target) { }
    }
}
#endif
