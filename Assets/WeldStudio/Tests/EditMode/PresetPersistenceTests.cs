using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WeldStudio.Core;
using WeldStudio.Persistence;

namespace WeldStudio.Tests
{
    public class PresetPersistenceTests
    {
        private string directory;
        private JsonPresetRepository repository;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "WeldStudioTests", Guid.NewGuid().ToString("N"));
            repository = new JsonPresetRepository(directory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }

        private static CharacterPreset SamplePreset()
        {
            var preset = new CharacterPreset
            {
                CreatedWith = "0.1.0",
                RigId = "makehuman.game_engine",
                Metadata = new PresetMetadata { Name = "Ana", CreatedAt = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc) },
            };
            preset.Equipment.Add(new EquipmentEntry("3f2a9c0d8e7b4a6f9c1d2e3f4a5b6c7d", "navy"));
            preset.Equipment.Add(new EquipmentEntry("9b8a7c6d5e4f40312a1b2c3d4e5f6a7b", null));
            preset.Modifiers["body.weight"] = 0.35f;
            preset.Modifiers["Face.MouthSmileLeft"] = 0.8f;
            return preset;
        }

        // Async test methods require a newer NUnit than Unity ships, so tests block on the task instead.
        private static T Run<T>(Task<T> task) => task.GetAwaiter().GetResult();
        private static void Run(Task task) => task.GetAwaiter().GetResult();

        [Test]
        public void SaveThenLoad_RoundTripsEveryField()
        {
            string path = repository.GetDefaultPath("Ana");

            Run(repository.SaveAsync(SamplePreset(), path));
            CharacterPreset loaded = Run(repository.LoadAsync(path));

            Assert.AreEqual(CharacterPreset.CurrentSchemaVersion, loaded.SchemaVersion);
            Assert.AreEqual("0.1.0", loaded.CreatedWith);
            Assert.AreEqual("makehuman.game_engine", loaded.RigId);
            Assert.AreEqual("Ana", loaded.Metadata.Name);
            Assert.AreEqual(new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc), loaded.Metadata.CreatedAt);
            Assert.AreEqual(2, loaded.Equipment.Count);
            Assert.AreEqual("navy", loaded.Equipment[0].VariantId);
            Assert.IsNull(loaded.Equipment[1].VariantId);
            Assert.AreEqual(0.35f, loaded.Modifiers["body.weight"]);
        }

        [Test]
        public void Serialize_UsesCamelCaseButKeepsModifierIdsVerbatim()
        {
            JObject json = JObject.Parse(PresetJson.Serialize(SamplePreset()));

            Assert.AreEqual(1, (int)json["schemaVersion"]);
            Assert.IsNotNull(json["equipment"][0]["itemId"]);
            Assert.IsNotNull(json["modifiers"]["Face.MouthSmileLeft"]);
        }

        [Test]
        public void Save_OverwritesExistingFileAndLeavesNoTemporaryFile()
        {
            string path = repository.GetDefaultPath("Ana");
            Run(repository.SaveAsync(SamplePreset(), path));

            CharacterPreset changed = SamplePreset();
            changed.Metadata.Name = "Ana 2";
            Run(repository.SaveAsync(changed, path));

            Assert.AreEqual("Ana 2", Run(repository.LoadAsync(path)).Metadata.Name);
            CollectionAssert.AreEqual(new[] { path }, Directory.GetFiles(directory));
        }

        [Test]
        public void GetDefaultPath_ReplacesInvalidFileNameCharacters()
        {
            string path = repository.GetDefaultPath("a/b");

            Assert.AreEqual(directory, Path.GetDirectoryName(path));
            StringAssert.EndsWith(JsonPresetRepository.Extension, path);
            StringAssert.DoesNotContain("/", Path.GetFileName(path));
        }

        [Test]
        public void Deserialize_DropsInvalidEntriesAndFillsMissingCollections()
        {
            CharacterPreset preset = PresetJson.Deserialize(
                "{ \"schemaVersion\": 1, \"equipment\": [ { \"itemId\": \"\" }, null, { \"itemId\": \"hat\" } ], \"modifiers\": null }");

            Assert.AreEqual("hat", preset.Equipment[0].ItemId);
            Assert.AreEqual(1, preset.Equipment.Count);
            Assert.IsNotNull(preset.Modifiers);
            Assert.IsNotNull(preset.Metadata);
        }

        [Test]
        public void Deserialize_InvalidJson_ThrowsPresetFormatException()
        {
            Assert.Throws<PresetFormatException>(() => PresetJson.Deserialize("{ not json"));
            Assert.Throws<PresetFormatException>(() => PresetJson.Deserialize(""));
            Assert.Throws<PresetFormatException>(() => PresetJson.Deserialize("{ \"equipment\": [] }"));
        }

        [Test]
        public void Deserialize_NewerSchemaVersion_ThrowsPresetFormatException()
        {
            var exception = Assert.Throws<PresetFormatException>(() => PresetJson.Deserialize("{ \"schemaVersion\": 999 }"));
            StringAssert.Contains("999", exception.Message);
        }

        [Test]
        public void Migrator_AppliesEveryStepInOrder()
        {
            var steps = new Dictionary<int, Action<JObject>>
            {
                { 1, doc => doc["fromV1"] = true },
                { 2, doc => doc["fromV2"] = (bool)doc["fromV1"] },
            };
            var migrator = new PresetMigrator(3, steps);
            var document = new JObject { ["schemaVersion"] = 1 };

            int original = migrator.Migrate(document);

            Assert.AreEqual(1, original);
            Assert.AreEqual(3, (int)document["schemaVersion"]);
            Assert.IsTrue((bool)document["fromV2"]);
        }

        [Test]
        public void Migrator_MissingStep_Throws()
        {
            var migrator = new PresetMigrator(2, new Dictionary<int, Action<JObject>>());

            Assert.Throws<InvalidOperationException>(() => migrator.Migrate(new JObject { ["schemaVersion"] = 1 }));
        }

        [Test]
        public void Load_MissingFile_ThrowsIOException()
        {
            // FileNotFoundException or DirectoryNotFoundException depending on what is missing; both are IOExceptions.
            Assert.Catch<IOException>(() => Run(repository.LoadAsync(Path.Combine(directory, "missing.weld.json"))));
        }
    }
}
