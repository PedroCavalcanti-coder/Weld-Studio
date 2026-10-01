using System.Linq;
using NUnit.Framework;
using WeldStudio.Core;

namespace WeldStudio.Tests
{
    public class CommandHistoryTests
    {
        private static readonly FakeEquipable TShirt = new FakeEquipable("tshirt", EquipmentSlot.Torso, EquipmentLayer.Base, FakeEquipable.Rig, "white", "navy", "red");
        private static readonly FakeEquipable Trousers = new FakeEquipable("trousers", EquipmentSlot.Legs);
        private static readonly FakeEquipable Dress = new FakeEquipable("dress", EquipmentSlot.Torso | EquipmentSlot.Legs);

        private CharacterModel model;
        private CommandHistory history;

        [SetUp]
        public void SetUp()
        {
            model = new CharacterModel(FakeEquipable.Rig);
            history = new CommandHistory();
        }

        [Test]
        public void UndoEquip_RestoresReplacedItemsWithTheirVariants()
        {
            history.Execute(new EquipCommand(model, TShirt, "navy"));
            history.Execute(new EquipCommand(model, Trousers));
            history.Execute(new EquipCommand(model, Dress));

            Assert.IsTrue(history.Undo());

            CollectionAssert.AreEquivalent(new[] { "tshirt (navy)", "trousers" }, model.Equipped.Select(i => i.ToString()));
        }

        [Test]
        public void Redo_ReappliesUndoneCommand()
        {
            history.Execute(new EquipCommand(model, TShirt));
            history.Execute(new EquipCommand(model, Dress));
            history.Undo();

            Assert.IsTrue(history.Redo());

            Assert.AreEqual("dress", model.Equipped.Single().ItemId);
            Assert.IsFalse(history.CanRedo);
        }

        [Test]
        public void UndoUnequip_EquipsItemAgain()
        {
            history.Execute(new EquipCommand(model, TShirt, "red"));
            history.Execute(new UnequipCommand(model, "tshirt"));

            history.Undo();

            Assert.AreEqual("tshirt (red)", model.Equipped.Single().ToString());
        }

        [Test]
        public void SliderDrag_IsOneUndoStep_UntilSealed()
        {
            model.SetModifier("body.weight", 0.1f);

            history.Execute(new SetModifierCommand(model, "body.weight", 0.2f));
            history.Execute(new SetModifierCommand(model, "body.weight", 0.3f));
            history.Execute(new SetModifierCommand(model, "body.weight", 0.4f));
            history.Seal();
            history.Execute(new SetModifierCommand(model, "body.weight", 0.9f));

            history.Undo();
            Assert.IsTrue(model.TryGetModifier("body.weight", out float afterFirstUndo));
            Assert.AreEqual(0.4f, afterFirstUndo);

            history.Undo();
            Assert.IsTrue(model.TryGetModifier("body.weight", out float afterSecondUndo));
            Assert.AreEqual(0.1f, afterSecondUndo);
            Assert.IsFalse(history.CanUndo);

            history.Redo();
            Assert.IsTrue(model.TryGetModifier("body.weight", out float afterRedo));
            Assert.AreEqual(0.4f, afterRedo);
        }

        [Test]
        public void UndoModifier_WithNoPreviousValue_ResetsIt()
        {
            history.Execute(new SetModifierCommand(model, "face.jawOpen", 0.5f));

            history.Undo();

            Assert.IsFalse(model.TryGetModifier("face.jawOpen", out float _));
        }

        [Test]
        public void DifferentModifiers_DoNotMerge()
        {
            history.Execute(new SetModifierCommand(model, "body.weight", 0.5f));
            history.Execute(new SetModifierCommand(model, "body.height", 0.5f));

            history.Undo();

            Assert.IsTrue(history.CanUndo);
            Assert.IsTrue(model.TryGetModifier("body.weight", out float _));
        }

        [Test]
        public void SwatchClicks_MergeAndRedoTheLastVariant()
        {
            history.Execute(new EquipCommand(model, TShirt));
            history.Seal();
            history.Execute(new SetVariantCommand(model, "tshirt", "navy"));
            history.Execute(new SetVariantCommand(model, "tshirt", "red"));

            history.Undo();
            Assert.AreEqual("white", model.Equipped.Single().VariantId);

            history.Redo();
            Assert.AreEqual("red", model.Equipped.Single().VariantId);
        }

        [Test]
        public void NewCommand_ClearsRedo()
        {
            history.Execute(new EquipCommand(model, TShirt));
            history.Undo();

            history.Execute(new EquipCommand(model, Trousers));

            Assert.IsFalse(history.CanRedo);
        }

        [Test]
        public void Capacity_DropsOldestCommands()
        {
            var small = new CommandHistory(capacity: 2);
            small.Execute(new SetModifierCommand(model, "a", 1f));
            small.Execute(new SetModifierCommand(model, "b", 1f));
            small.Execute(new SetModifierCommand(model, "c", 1f));

            Assert.IsTrue(small.Undo());
            Assert.IsTrue(small.Undo());
            Assert.IsFalse(small.Undo());
            Assert.IsTrue(model.TryGetModifier("a", out float _));
        }

        [Test]
        public void UndoApplyPreset_RestoresPreviousCharacter()
        {
            model.Equip(TShirt, "navy");
            model.SetModifier("body.weight", 0.3f);
            var preset = new CharacterPreset { RigId = FakeEquipable.Rig };
            preset.Equipment.Add(new EquipmentEntry("dress", null));

            history.Execute(new ApplyPresetCommand(model, preset, FakeEquipable.Resolver(TShirt, Dress)));
            Assert.AreEqual("dress", model.Equipped.Single().ItemId);

            history.Undo();

            Assert.AreEqual("tshirt (navy)", model.Equipped.Single().ToString());
            Assert.IsTrue(model.TryGetModifier("body.weight", out float weight));
            Assert.AreEqual(0.3f, weight);
        }

        [Test]
        public void Labels_DescribeNextUndoAndRedo()
        {
            history.Execute(new EquipCommand(model, TShirt, label: "Equip T-Shirt"));

            Assert.AreEqual("Equip T-Shirt", history.UndoLabel);
            history.Undo();
            Assert.AreEqual("Equip T-Shirt", history.RedoLabel);
            Assert.IsNull(history.UndoLabel);
        }
    }
}
