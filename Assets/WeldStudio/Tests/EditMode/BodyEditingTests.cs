using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using WeldStudio.Core;
using WeldStudio.Core.Data;

namespace WeldStudio.Tests
{
    public class BodyEditingTests
    {
        /// <summary>Creates a ScriptableObject and sets its private serialized fields, as the inspector would.</summary>
        private static T CreateData<T>(Dictionary<string, object> fields) where T : ScriptableObject
        {
            T instance = ScriptableObject.CreateInstance<T>();
            foreach (KeyValuePair<string, object> field in fields)
            {
                FieldInfo info = null;
                for (Type type = typeof(T); info == null && type != null; type = type.BaseType)
                    info = type.GetField(field.Key, BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(info, $"Field {field.Key} not found on {typeof(T).Name}");
                info.SetValue(instance, field.Value);
            }
            return instance;
        }

        private static BoneTransformModifierDefinition BoneModifier(BoneOperation operation, BoneAxis axis = BoneAxis.Y, float min = -0.5f) =>
            CreateData<BoneTransformModifierDefinition>(new Dictionary<string, object>
            {
                { "id", "body.upperArm.l.test" }, { "displayName", "Test" }, { "minValue", min }, { "maxValue", 0.5f },
                { "bones", new[] { "upperarm_l" } }, { "operation", operation }, { "axis", axis },
            });

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, 1e-5f);
            Assert.AreEqual(expected.y, actual.y, 1e-5f);
            Assert.AreEqual(expected.z, actual.z, 1e-5f);
        }

        [Test]
        public void Length_StretchesOnlyAlongTheBoneAxis()
        {
            BoneAdjustment adjustment = BoneModifier(BoneOperation.Length).ToAdjustment(0.2f);

            AssertVector(new Vector3(1f, 1.2f, 1f), adjustment.Scale);
            AssertVector(Vector3.zero, adjustment.Offset);
        }

        [Test]
        public void Thickness_ScalesThePerpendicularAxes()
        {
            BoneAdjustment adjustment = BoneModifier(BoneOperation.Thickness).ToAdjustment(-0.25f);

            AssertVector(new Vector3(0.75f, 1f, 0.75f), adjustment.Scale);
        }

        [Test]
        public void UniformScaleAndOffset_ProduceExpectedAdjustments()
        {
            AssertVector(new Vector3(1.1f, 1.1f, 1.1f), BoneModifier(BoneOperation.UniformScale).ToAdjustment(0.1f).Scale);

            BoneAdjustment move = BoneModifier(BoneOperation.Offset, BoneAxis.X).ToAdjustment(0.03f);
            AssertVector(Vector3.one, move.Scale);
            AssertVector(new Vector3(0.03f, 0f, 0f), move.Offset);
        }

        [Test]
        public void BoneModifier_RejectsScaleRangesThatCollapseTheMesh()
        {
            var errors = new List<string>();
            BoneModifier(BoneOperation.Length, min: -1f).CollectValidationErrors(errors);

            Assert.IsTrue(errors.Exists(e => e.Contains("greater than -1")));
        }

        [Test]
        public void Stack_CombinesContributionsFromDifferentModifiers()
        {
            var stack = new BoneAdjustmentStack();
            stack.Set("upperarm_l", "length", new BoneAdjustment(new Vector3(1f, 1.2f, 1f), Vector3.zero));
            stack.Set("upperarm_l", "thickness", new BoneAdjustment(new Vector3(0.9f, 1f, 0.9f), Vector3.zero));
            stack.Set("upperarm_l", "move", new BoneAdjustment(Vector3.one, new Vector3(0f, 0f, 0.01f)));

            BoneAdjustment combined = stack.GetCombined("upperarm_l");

            AssertVector(new Vector3(0.9f, 1.2f, 0.9f), combined.Scale);
            AssertVector(new Vector3(0f, 0f, 0.01f), combined.Offset);
        }

        [Test]
        public void Stack_SettingAgainReplacesAndRemovingRestoresIdentity()
        {
            var stack = new BoneAdjustmentStack();
            stack.Set("head", "size", new BoneAdjustment(Vector3.one * 1.5f, Vector3.zero));
            stack.Set("head", "size", new BoneAdjustment(Vector3.one * 1.1f, Vector3.zero));
            AssertVector(Vector3.one * 1.1f, stack.GetCombined("head").Scale);

            CollectionAssert.AreEqual(new[] { "head" }, stack.RemoveSource("size"));

            AssertVector(Vector3.one, stack.GetCombined("head").Scale);
            CollectionAssert.IsEmpty(stack.AdjustedBones);
        }

        [Test]
        public void BlendShapeModifier_DrivesPositiveOrNegativeShape()
        {
            var nose = CreateData<BlendShapeModifierDefinition>(new Dictionary<string, object>
            {
                { "id", "face.nose.width" }, { "displayName", "Nose width" }, { "minValue", -1f }, { "maxValue", 1f },
                { "positiveShape", "nose_wide" }, { "negativeShape", "nose_narrow" },
            });

            nose.Weights(0.5f, out float positive, out float negative);
            Assert.AreEqual(50f, positive, 1e-4f);
            Assert.AreEqual(0f, negative);

            nose.Weights(-0.25f, out positive, out negative);
            Assert.AreEqual(0f, positive);
            Assert.AreEqual(25f, negative, 1e-4f);
        }

        [Test]
        public void HairStandardOptions_AreValidAndMapToTheHairShader()
        {
            var zones = HairItemData.StandardColorZones();
            CollectionAssert.AreEquivalent(new[] { HairItemData.RootColor, HairItemData.TipColor, HairItemData.StreakColor },
                Array.ConvertAll(zones, z => z.Id));

            foreach (ItemParameter parameter in HairItemData.StandardParameters())
            {
                Assert.Less(parameter.MinValue, parameter.MaxValue, parameter.Id);
                Assert.AreEqual(parameter.DefaultValue, parameter.Clamp(parameter.DefaultValue), parameter.Id);
                Assert.IsFalse(string.IsNullOrEmpty(parameter.PropertyName), parameter.Id);
            }
        }

        [Test]
        public void ItemParameter_MapsValueRangeToBlendShapeWeight()
        {
            var volume = new ItemParameter("volume", "Volume", 0f, 2f, 0f, ParameterTarget.BlendShape, "hair_volume");

            Assert.AreEqual(50f, volume.ToBlendShapeWeight(1f), 1e-4f);
            Assert.AreEqual(100f, volume.ToBlendShapeWeight(5f), 1e-4f);
            Assert.AreEqual(0f, volume.Clamp(float.NaN));
        }
    }
}
