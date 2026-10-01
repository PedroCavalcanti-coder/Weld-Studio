using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeldStudio.Core;

namespace WeldStudio.Tests
{
    public class CharacterModelTests
    {
        private static readonly FakeEquipable TShirt = new FakeEquipable("tshirt", EquipmentSlot.Torso, EquipmentLayer.Base, FakeEquipable.Rig, "white", "navy");
        private static readonly FakeEquipable Jacket = new FakeEquipable("jacket", EquipmentSlot.Torso, EquipmentLayer.Outer);
        private static readonly FakeEquipable Trousers = new FakeEquipable("trousers", EquipmentSlot.Legs);
        private static readonly FakeEquipable Dress = new FakeEquipable("dress", EquipmentSlot.Torso | EquipmentSlot.Legs);
        private static readonly FakeEquipable Belt = new FakeEquipable("belt", EquipmentSlot.Waist, EquipmentLayer.Accessory);
        private static readonly FakeEquipable OtherRigShirt = new FakeEquipable("alien-shirt", EquipmentSlot.Torso, EquipmentLayer.Base, "other.rig");

        private CharacterModel model;

        [SetUp]
        public void SetUp() => model = new CharacterModel(FakeEquipable.Rig);

        [Test]
        public void Equip_DifferentLayersOnSameSlot_Coexist()
        {
            model.Equip(TShirt);
            model.Equip(Jacket);

            CollectionAssert.AreEquivalent(new[] { "tshirt", "jacket" }, model.Equipped.Select(i => i.ItemId));
        }

        [Test]
        public void Equip_MultiSlotItem_ReplacesEveryConflictingItemOnItsLayer()
        {
            model.Equip(TShirt);
            model.Equip(Trousers);
            model.Equip(Belt);

            EquipmentChange change = model.Equip(Dress);

            CollectionAssert.AreEquivalent(new[] { "tshirt", "trousers" }, change.Removed.Select(i => i.ItemId));
            CollectionAssert.AreEquivalent(new[] { "belt", "dress" }, model.Equipped.Select(i => i.ItemId));
        }

        [Test]
        public void Equip_RaisesEquipmentChangedOnce()
        {
            var changes = new List<EquipmentChange>();
            model.EquipmentChanged += changes.Add;

            model.Equip(TShirt);

            Assert.AreEqual(1, changes.Count);
            Assert.AreEqual("tshirt", changes[0].Added.Single().ItemId);
            Assert.IsEmpty(changes[0].Removed);
        }

        [Test]
        public void Equip_AlreadyEquipped_IsNoOp()
        {
            model.Equip(TShirt);
            int raised = 0;
            model.EquipmentChanged += _ => raised++;

            EquipmentChange change = model.Equip(TShirt);

            Assert.IsTrue(change.IsEmpty);
            Assert.AreEqual(0, raised);
        }

        [Test]
        public void Equip_UsesDefaultVariantWhenNoneGiven()
        {
            model.Equip(TShirt);

            Assert.AreEqual("white", model.Equipped.Single().VariantId);
        }

        [Test]
        public void Equip_UnknownVariant_Throws()
        {
            Assert.Throws<ArgumentException>(() => model.Equip(TShirt, "plaid"));
            Assert.IsEmpty(model.Equipped);
        }

        [Test]
        public void Equip_ItemForAnotherRig_Throws()
        {
            Assert.Throws<ArgumentException>(() => model.Equip(OtherRigShirt));
        }

        [Test]
        public void Unequip_RemovesItemAndRaisesEvent()
        {
            model.Equip(TShirt);
            EquipmentChange raised = null;
            model.EquipmentChanged += change => raised = change;

            Assert.IsTrue(model.Unequip("tshirt"));
            Assert.IsFalse(model.Unequip("tshirt"));
            Assert.IsEmpty(model.Equipped);
            Assert.AreEqual("tshirt", raised.Removed.Single().ItemId);
        }

        [Test]
        public void SetVariant_ChangesVariantWithoutEquipmentEvent()
        {
            model.Equip(TShirt);
            int equipmentEvents = 0;
            EquippedItem changed = null;
            model.EquipmentChanged += _ => equipmentEvents++;
            model.VariantChanged += item => changed = item;

            Assert.IsTrue(model.SetVariant("tshirt", "navy"));
            Assert.IsFalse(model.SetVariant("tshirt", "navy"));

            Assert.AreEqual("navy", changed.VariantId);
            Assert.AreEqual(0, equipmentEvents);
        }

        [Test]
        public void SetModifier_RaisesOnlyWhenValueChanges()
        {
            var raised = new List<string>();
            model.ModifierChanged += raised.Add;

            model.SetModifier("body.weight", 0.5f);
            model.SetModifier("body.weight", 0.5f);
            model.SetModifier("body.weight", 0.7f);

            Assert.AreEqual(2, raised.Count);
            Assert.IsTrue(model.TryGetModifier("body.weight", out float value));
            Assert.AreEqual(0.7f, value);
        }

        [Test]
        public void SetModifier_NonFiniteValue_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => model.SetModifier("body.weight", float.NaN));
        }

        [Test]
        public void ResetModifier_RemovesExplicitValue()
        {
            model.SetModifier("face.jawOpen", 0.3f);

            Assert.IsTrue(model.ResetModifier("face.jawOpen"));
            Assert.IsFalse(model.TryGetModifier("face.jawOpen", out float _));
            Assert.IsFalse(model.ResetModifier("face.jawOpen"));
        }

        [Test]
        public void ToPresetThenApplyPreset_RoundTripsState()
        {
            model.Equip(TShirt, "navy");
            model.Equip(Trousers);
            model.SetModifier("body.weight", 0.25f);
            CharacterPreset preset = model.ToPreset(new PresetMetadata { Name = "Test" }, "0.1.0");

            var restored = new CharacterModel(FakeEquipable.Rig);
            PresetApplyReport report = restored.ApplyPreset(preset, FakeEquipable.Resolver(TShirt, Trousers));

            Assert.IsFalse(report.HasIssues);
            CollectionAssert.AreEqual(new[] { "tshirt (navy)", "trousers" }, restored.Equipped.Select(i => i.ToString()));
            Assert.IsTrue(restored.TryGetModifier("body.weight", out float weight));
            Assert.AreEqual(0.25f, weight);
        }

        [Test]
        public void ApplyPreset_MissingItems_AreReportedAndPreservedOnSave()
        {
            var preset = new CharacterPreset { RigId = FakeEquipable.Rig };
            preset.Equipment.Add(new EquipmentEntry("tshirt", null));
            preset.Equipment.Add(new EquipmentEntry("modpack-hat", "red"));

            PresetApplyReport report = model.ApplyPreset(preset, FakeEquipable.Resolver(TShirt));

            CollectionAssert.AreEqual(new[] { "modpack-hat" }, report.MissingItemIds);
            Assert.AreEqual("tshirt", model.Equipped.Single().ItemId);
            EquipmentEntry kept = model.ToPreset().Equipment.Single(e => e.ItemId == "modpack-hat");
            Assert.AreEqual("red", kept.VariantId);
        }

        [Test]
        public void ApplyPreset_UnknownVariant_FallsBackToDefaultAndReports()
        {
            var preset = new CharacterPreset { RigId = FakeEquipable.Rig };
            preset.Equipment.Add(new EquipmentEntry("tshirt", "plaid"));

            PresetApplyReport report = model.ApplyPreset(preset, FakeEquipable.Resolver(TShirt));

            Assert.AreEqual("plaid", report.MissingVariants.Single().VariantId);
            Assert.AreEqual("white", model.Equipped.Single().VariantId);
        }

        [Test]
        public void ApplyPreset_ForAnotherRig_KeepsEverythingUnresolved()
        {
            var preset = new CharacterPreset { RigId = "other.rig" };
            preset.Equipment.Add(new EquipmentEntry("tshirt", null));

            PresetApplyReport report = model.ApplyPreset(preset, FakeEquipable.Resolver(TShirt));

            Assert.IsTrue(report.RigMismatch);
            Assert.IsEmpty(model.Equipped);
            Assert.AreEqual("tshirt", model.UnresolvedEquipment.Single().ItemId);
        }

        [Test]
        public void ApplyPreset_RaisesDiffEventsOnly()
        {
            model.Equip(TShirt);
            model.Equip(Belt);
            model.SetModifier("body.weight", 0.5f);
            model.SetModifier("body.height", 0.1f);

            var preset = new CharacterPreset { RigId = FakeEquipable.Rig };
            preset.Equipment.Add(new EquipmentEntry("tshirt", "navy"));
            preset.Equipment.Add(new EquipmentEntry("trousers", null));
            preset.Modifiers["body.weight"] = 0.5f;
            preset.Modifiers["face.jawOpen"] = 0.2f;

            var equipmentChanges = new List<EquipmentChange>();
            var variantChanges = new List<EquippedItem>();
            var modifierChanges = new List<string>();
            int applied = 0;
            model.EquipmentChanged += equipmentChanges.Add;
            model.VariantChanged += variantChanges.Add;
            model.ModifierChanged += modifierChanges.Add;
            model.PresetApplied += () => applied++;

            model.ApplyPreset(preset, FakeEquipable.Resolver(TShirt, Trousers, Belt));

            Assert.AreEqual(1, equipmentChanges.Count);
            CollectionAssert.AreEquivalent(new[] { "trousers" }, equipmentChanges[0].Added.Select(i => i.ItemId));
            CollectionAssert.AreEquivalent(new[] { "belt" }, equipmentChanges[0].Removed.Select(i => i.ItemId));
            Assert.AreEqual("navy", variantChanges.Single().VariantId);
            CollectionAssert.AreEquivalent(new[] { "body.height", "face.jawOpen" }, modifierChanges);
            Assert.AreEqual(1, applied);
        }

        [Test]
        public void ApplyPreset_ConflictingEntries_LaterEntryWins()
        {
            var preset = new CharacterPreset { RigId = FakeEquipable.Rig };
            preset.Equipment.Add(new EquipmentEntry("tshirt", null));
            preset.Equipment.Add(new EquipmentEntry("dress", null));

            model.ApplyPreset(preset, FakeEquipable.Resolver(TShirt, Dress));

            Assert.AreEqual("dress", model.Equipped.Single().ItemId);
        }
    }
}
