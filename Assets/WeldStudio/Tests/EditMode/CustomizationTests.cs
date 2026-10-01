using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeldStudio.Core;

namespace WeldStudio.Tests
{
    public class CustomizationTests
    {
        private static readonly FakeEquipable Hair = new FakeEquipable("hair-bob", EquipmentSlot.Hair);
        private static readonly FakeEquipable OtherHair = new FakeEquipable("hair-long", EquipmentSlot.Hair);
        private static readonly FakeEquipable Shirt = new FakeEquipable("shirt", EquipmentSlot.Torso);

        private static readonly ColorRgba DarkBrown = new ColorRgba(0x2E, 0x1C, 0x12);
        private static readonly ColorRgba Blonde = new ColorRgba(0xE6, 0xC2, 0x8A);

        private CharacterModel model;
        private CommandHistory history;

        [SetUp]
        public void SetUp()
        {
            model = new CharacterModel(FakeEquipable.Rig);
            history = new CommandHistory();
        }

        // ------------------------------------------------------------ colours

        [Test]
        public void ColorRgba_ParsesAndFormatsHex()
        {
            Assert.AreEqual(new ColorRgba(0x2E, 0x1C, 0x12), ColorRgba.ParseHex("#2e1c12"));
            Assert.AreEqual(new ColorRgba(0xC0, 0x94, 0x38, 0x80), ColorRgba.ParseHex("C0943880"));
            Assert.AreEqual("#2E1C12", DarkBrown.ToHex());
            Assert.AreEqual("#C0943880", new ColorRgba(0xC0, 0x94, 0x38, 0x80).ToHex());
            Assert.IsFalse(ColorRgba.TryParseHex("#12345", out ColorRgba _));
            Assert.IsFalse(ColorRgba.TryParseHex("#GG0000", out ColorRgba _));
            Assert.IsFalse(ColorRgba.TryParseHex(null, out ColorRgba _));
        }

        [Test]
        public void HairWithRootTipAndStreakColours_IsStoredPerZone()
        {
            model.Equip(Hair);
            var changes = new List<string>();
            model.AppearanceChanged += (item, key) => changes.Add(key);

            Assert.IsTrue(model.SetItemColor("hair-bob", "root", DarkBrown));
            Assert.IsTrue(model.SetItemColor("hair-bob", "tip", Blonde));
            Assert.IsTrue(model.SetItemColor("hair-bob", "streak", Blonde));
            Assert.IsFalse(model.SetItemColor("hair-bob", "streak", Blonde));
            Assert.IsTrue(model.SetItemParameter("hair-bob", "length", 0.5f));

            ItemAppearance appearance = model.Equipped.Single().Appearance;
            Assert.IsTrue(appearance.TryGetColor("root", out ColorRgba root));
            Assert.AreEqual(DarkBrown, root);
            Assert.IsTrue(appearance.TryGetColor("tip", out ColorRgba tip));
            Assert.AreEqual(Blonde, tip);
            Assert.IsTrue(appearance.TryGetParameter("length", out float length));
            Assert.AreEqual(0.5f, length);
            CollectionAssert.AreEqual(new[] { "root", "tip", "streak", "length" }, changes);
        }

        [Test]
        public void ResettingAColour_ReturnsToDefaultAndRaisesEvent()
        {
            model.Equip(Shirt);
            model.SetItemColor("shirt", "collar", Blonde);
            string raised = null;
            model.AppearanceChanged += (item, key) => raised = key;

            Assert.IsTrue(model.SetItemColor("shirt", "collar", null));
            Assert.IsFalse(model.SetItemColor("shirt", "collar", null));

            Assert.IsTrue(model.Equipped.Single().Appearance.IsEmpty);
            Assert.AreEqual("collar", raised);
        }

        [Test]
        public void SetItemColor_OnItemNotWorn_ReturnsFalse()
        {
            Assert.IsFalse(model.SetItemColor("shirt", "primary", Blonde));
        }

        [Test]
        public void ItemParameter_NonFiniteValue_Throws()
        {
            model.Equip(Hair);

            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetItemParameter("hair-bob", "length", float.NaN));
        }

        [Test]
        public void ColourPickerDrag_IsOneUndoStep()
        {
            model.Equip(Shirt);
            history.Execute(new SetItemColorCommand(model, "shirt", "primary", new ColorRgba(10, 0, 0)));
            history.Execute(new SetItemColorCommand(model, "shirt", "primary", new ColorRgba(20, 0, 0)));
            history.Execute(new SetItemColorCommand(model, "shirt", "primary", Blonde));
            history.Seal();

            history.Undo();
            Assert.IsTrue(model.Equipped.Single().Appearance.IsEmpty);

            history.Redo();
            Assert.IsTrue(model.Equipped.Single().Appearance.TryGetColor("primary", out ColorRgba color));
            Assert.AreEqual(Blonde, color);
        }

        [Test]
        public void HairLengthDrag_IsOneUndoStep()
        {
            model.Equip(Hair);
            model.SetItemParameter("hair-bob", "length", 0.9f);
            history.Execute(new SetItemParameterCommand(model, "hair-bob", "length", 0.8f));
            history.Execute(new SetItemParameterCommand(model, "hair-bob", "length", 0.4f));

            history.Undo();

            Assert.IsTrue(model.Equipped.Single().Appearance.TryGetParameter("length", out float length));
            Assert.AreEqual(0.9f, length);
        }

        [Test]
        public void UndoReplacingHair_RestoresTheOldHairWithItsColours()
        {
            model.Equip(Hair);
            model.SetItemColor("hair-bob", "root", DarkBrown);
            model.SetItemParameter("hair-bob", "length", 0.4f);

            history.Execute(new EquipCommand(model, OtherHair));
            Assert.AreEqual("hair-long", model.Equipped.Single().ItemId);

            history.Undo();

            EquippedItem restored = model.Equipped.Single();
            Assert.AreEqual("hair-bob", restored.ItemId);
            Assert.IsTrue(restored.Appearance.TryGetColor("root", out ColorRgba root));
            Assert.AreEqual(DarkBrown, root);
        }

        [Test]
        public void UndoUnequip_RestoresColours()
        {
            model.Equip(Shirt);
            model.SetItemColor("shirt", "primary", Blonde);

            history.Execute(new UnequipCommand(model, "shirt"));
            history.Undo();

            Assert.IsFalse(model.Equipped.Single().Appearance.IsEmpty);
        }

        [Test]
        public void Preset_RoundTripsAppearance()
        {
            model.Equip(Hair);
            model.SetItemColor("hair-bob", "streak", new ColorRgba(0xC0, 0x94, 0x38, 0x80));
            model.SetItemParameter("hair-bob", "streakAmount", 0.3f);

            CharacterPreset preset = model.ToPreset();
            Assert.AreEqual("#C0943880", preset.Equipment.Single().Colors["streak"]);

            var restored = new CharacterModel(FakeEquipable.Rig);
            restored.ApplyPreset(preset, FakeEquipable.Resolver(Hair));

            Assert.AreEqual(model.Equipped.Single().Appearance, restored.Equipped.Single().Appearance);
        }

        [Test]
        public void ApplyPreset_InvalidColour_IsReportedAndSkipped()
        {
            var entry = new EquipmentEntry("shirt", null);
            entry.Colors["primary"] = "not-a-colour";
            entry.Colors["collar"] = "#FFFFFF";
            var preset = new CharacterPreset { RigId = FakeEquipable.Rig };
            preset.Equipment.Add(entry);

            PresetApplyReport report = model.ApplyPreset(preset, FakeEquipable.Resolver(Shirt));

            CollectionAssert.AreEqual(new[] { "shirt/primary" }, report.InvalidColors);
            ItemAppearance appearance = model.Equipped.Single().Appearance;
            Assert.IsFalse(appearance.TryGetColor("primary", out ColorRgba _));
            Assert.IsTrue(appearance.TryGetColor("collar", out ColorRgba _));
        }

        [Test]
        public void ApplyPreset_AppearanceOnlyChange_RaisesAppearanceChanged()
        {
            model.Equip(Shirt);
            CharacterPreset preset = model.ToPreset();
            preset.Equipment.Single().Colors["primary"] = "#FF0000";
            int equipmentEvents = 0;
            EquippedItem changed = null;
            model.EquipmentChanged += _ => equipmentEvents++;
            model.AppearanceChanged += (item, key) => changed = item;

            model.ApplyPreset(preset, FakeEquipable.Resolver(Shirt));

            Assert.AreEqual(0, equipmentEvents);
            Assert.IsNotNull(changed);
        }

        [Test]
        public void UnresolvedItems_KeepTheirAppearanceOnSave()
        {
            var entry = new EquipmentEntry("modpack-hat", null);
            entry.Colors["band"] = "#123456";
            var preset = new CharacterPreset { RigId = FakeEquipable.Rig };
            preset.Equipment.Add(entry);

            model.ApplyPreset(preset, FakeEquipable.Resolver());

            Assert.AreEqual("#123456", model.ToPreset().Equipment.Single().Colors["band"]);
        }

        // ------------------------------------------------------------ symmetric body editing

        [Test]
        public void SymmetricSliderDrag_SetsBothSidesAndUndoesInOneStep()
        {
            var both = new[] { "body.upperArm.l.length", "body.upperArm.r.length" };
            history.Execute(new SetModifierCommand(model, both, 0.1f));
            history.Execute(new SetModifierCommand(model, both, 0.2f));

            Assert.IsTrue(model.TryGetModifier("body.upperArm.r.length", out float right));
            Assert.AreEqual(0.2f, right);

            history.Undo();

            Assert.IsFalse(model.TryGetModifier("body.upperArm.l.length", out float _));
            Assert.IsFalse(model.TryGetModifier("body.upperArm.r.length", out float _));
            Assert.IsFalse(history.CanUndo);
        }

        [Test]
        public void SymmetricAndSingleSideCommands_DoNotMerge()
        {
            history.Execute(new SetModifierCommand(model, new[] { "a.l", "a.r" }, 0.5f));
            history.Execute(new SetModifierCommand(model, "a.l", 0.7f));

            history.Undo();

            Assert.IsTrue(model.TryGetModifier("a.l", out float left));
            Assert.AreEqual(0.5f, left);
        }

        // ------------------------------------------------------------ paint layers

        [Test]
        public void PaintLayers_AddUpdateMoveRemoveWithUndo()
        {
            var tattoo = new PaintLayer("tattoo", PaintLayer.BodyTarget, "Tattoo");
            var makeup = new PaintLayer("makeup", PaintLayer.BodyTarget, "Make-up", 0.5f, PaintBlendMode.Multiply);

            history.Execute(new AddPaintLayerCommand(model, tattoo));
            history.Execute(new AddPaintLayerCommand(model, makeup));
            history.Execute(new MovePaintLayerCommand(model, "makeup", 0));
            CollectionAssert.AreEqual(new[] { "makeup", "tattoo" }, model.PaintLayers.Select(l => l.Id));

            history.Execute(new UpdatePaintLayerCommand(model, tattoo.WithOpacity(0.4f)));
            history.Execute(new UpdatePaintLayerCommand(model, tattoo.WithOpacity(0.2f)));
            Assert.AreEqual(0.2f, model.PaintLayers[1].Opacity);
            history.Undo();
            Assert.AreEqual(1f, model.PaintLayers[1].Opacity);

            history.Execute(new RemovePaintLayerCommand(model, "makeup"));
            CollectionAssert.AreEqual(new[] { "tattoo" }, model.PaintLayers.Select(l => l.Id));
            history.Undo();
            CollectionAssert.AreEqual(new[] { "makeup", "tattoo" }, model.PaintLayers.Select(l => l.Id));

            history.Undo();
            CollectionAssert.AreEqual(new[] { "tattoo", "makeup" }, model.PaintLayers.Select(l => l.Id));
        }

        [Test]
        public void PaintLayer_ClampsOpacityAndRejectsDuplicatesAndTargetChange()
        {
            var layer = new PaintLayer("l1", "shirt", "Logo", 3f);
            Assert.AreEqual(1f, layer.Opacity);

            model.AddPaintLayer(layer);
            Assert.Throws<ArgumentException>(() => model.AddPaintLayer(new PaintLayer("l1", "shirt", "Copy")));
            Assert.Throws<ArgumentException>(() => model.UpdatePaintLayer(new PaintLayer("l1", PaintLayer.BodyTarget, "Logo")));
        }

        [Test]
        public void Preset_RoundTripsPaintLayers()
        {
            model.AddPaintLayer(new PaintLayer("l1", PaintLayer.BodyTarget, "Freckles", 0.6f, PaintBlendMode.Multiply));
            model.AddPaintLayer(new PaintLayer("l2", "shirt", "Logo", 1f, PaintBlendMode.Normal, visible: false));

            CharacterPreset preset = model.ToPreset();
            Assert.AreEqual("paint/l1.png", preset.PaintLayers[0].Image);

            var restored = new CharacterModel(FakeEquipable.Rig);
            string raised = "not raised";
            restored.PaintLayersChanged += id => raised = id;
            restored.ApplyPreset(preset, FakeEquipable.Resolver());

            Assert.IsNull(raised);
            Assert.AreEqual(2, restored.PaintLayers.Count);
            Assert.IsTrue(restored.PaintLayers[0].HasSameSettings(model.PaintLayers[0]));
            Assert.IsFalse(restored.PaintLayers[1].Visible);
        }

        [Test]
        public void ApplyPreset_DropsInvalidPaintLayers()
        {
            var preset = new CharacterPreset { RigId = FakeEquipable.Rig };
            preset.PaintLayers.Add(new PaintLayerEntry { Id = "ok", TargetId = PaintLayer.BodyTarget });
            preset.PaintLayers.Add(new PaintLayerEntry { Id = "no-target" });
            preset.PaintLayers.Add(new PaintLayerEntry { Id = "ok", TargetId = PaintLayer.BodyTarget });

            PresetApplyReport report = model.ApplyPreset(preset, FakeEquipable.Resolver());

            Assert.AreEqual("ok", model.PaintLayers.Single().Id);
            CollectionAssert.AreEqual(new[] { "no-target", "ok" }, report.InvalidPaintLayers);
        }
    }
}
