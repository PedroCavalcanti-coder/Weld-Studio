using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        private PackagePresetRepository repository;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "WeldStudioTests", Guid.NewGuid().ToString("N"));
            repository = new PackagePresetRepository(directory);
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
            var hair = new EquipmentEntry("3f2a9c0d8e7b4a6f9c1d2e3f4a5b6c7d", null);
            hair.Colors["root"] = "#2E1C12";
            hair.Colors["streak"] = "#C0943880";
            hair.Parameters["length"] = 0.6f;
            preset.Equipment.Add(hair);
            preset.Equipment.Add(new EquipmentEntry("9b8a7c6d5e4f40312a1b2c3d4e5f6a7b", "navy"));
            preset.Modifiers["body.weight"] = 0.35f;
            preset.Modifiers["Face.MouthSmileLeft"] = 0.8f;
            preset.PaintLayers.Add(new PaintLayerEntry
            {
                Id = "a1b2c3", TargetId = PaintLayer.BodyTarget, Name = "Tattoo", Opacity = 0.75f,
                BlendMode = PaintBlendMode.Multiply, Image = PaintLayerEntry.ImageNameFor("a1b2c3"),
            });
            return preset;
        }

        private static PresetPackage SamplePackage()
        {
            var package = new PresetPackage(SamplePreset());
            package.SetAttachment(PaintLayerEntry.ImageNameFor("a1b2c3"), new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3 });
            return package;
        }

        // Async test methods require a newer NUnit than Unity ships, so tests block on the task instead.
        private static T Run<T>(Task<T> task) => task.GetAwaiter().GetResult();
        private static void Run(Task task) => task.GetAwaiter().GetResult();

        private string WriteZip(Action<ZipArchive> fill)
        {
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "crafted" + PackagePresetRepository.Extension);
            using (var stream = File.Create(path))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create)) fill(archive);
            return path;
        }

        private static void AddText(ZipArchive archive, string name, string text)
        {
            using (var writer = new StreamWriter(archive.CreateEntry(name).Open())) writer.Write(text);
        }

        [Test]
        public void SaveThenLoad_RoundTripsPresetAndAttachments()
        {
            string path = repository.GetDefaultPath("Ana");

            Run(repository.SaveAsync(SamplePackage(), path));
            PresetPackage loaded = Run(repository.LoadAsync(path));
            CharacterPreset preset = loaded.Preset;

            Assert.AreEqual(CharacterPreset.CurrentSchemaVersion, preset.SchemaVersion);
            Assert.AreEqual("0.1.0", preset.CreatedWith);
            Assert.AreEqual("Ana", preset.Metadata.Name);
            Assert.AreEqual(new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc), preset.Metadata.CreatedAt);
            Assert.AreEqual("#2E1C12", preset.Equipment[0].Colors["root"]);
            Assert.AreEqual(0.6f, preset.Equipment[0].Parameters["length"]);
            Assert.AreEqual("navy", preset.Equipment[1].VariantId);
            Assert.AreEqual(0.35f, preset.Modifiers["body.weight"]);
            Assert.AreEqual(PaintBlendMode.Multiply, preset.PaintLayers[0].BlendMode);
            Assert.AreEqual(0.75f, preset.PaintLayers[0].Opacity);
            Assert.IsTrue(loaded.TryGetAttachment("paint/a1b2c3.png", out byte[] png));
            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3 }, png);
        }

        [Test]
        public void Serialize_UsesCamelCaseEnumNamesAndVerbatimIds()
        {
            JObject json = JObject.Parse(PresetJson.Serialize(SamplePreset()));

            Assert.AreEqual(1, (int)json["schemaVersion"]);
            Assert.IsNotNull(json["equipment"][0]["itemId"]);
            Assert.IsNotNull(json["modifiers"]["Face.MouthSmileLeft"]);
            Assert.AreEqual("Multiply", (string)json["paintLayers"][0]["blendMode"]);
        }

        [Test]
        public void Save_OverwritesExistingFileAndLeavesNoTemporaryFile()
        {
            string path = repository.GetDefaultPath("Ana");
            Run(repository.SaveAsync(SamplePackage(), path));

            PresetPackage changed = SamplePackage();
            changed.Preset.Metadata.Name = "Ana 2";
            Run(repository.SaveAsync(changed, path));

            Assert.AreEqual("Ana 2", Run(repository.LoadAsync(path)).Preset.Metadata.Name);
            CollectionAssert.AreEqual(new[] { path }, Directory.GetFiles(directory));
        }

        [Test]
        public void GetDefaultPath_ReplacesInvalidFileNameCharacters()
        {
            string path = repository.GetDefaultPath("a/b\\c");

            Assert.AreEqual(directory, Path.GetDirectoryName(path));
            Assert.AreEqual("a_b_c.weld", Path.GetFileName(path));
        }

        [Test]
        public void Package_RejectsAttachmentNamesThatEscapeThePackage()
        {
            var package = new PresetPackage(new CharacterPreset());

            Assert.Throws<ArgumentException>(() => package.SetAttachment("../evil.png", new byte[1]));
            Assert.Throws<ArgumentException>(() => package.SetAttachment("/etc/passwd", new byte[1]));
            Assert.Throws<ArgumentException>(() => package.SetAttachment("paint\\x.png", new byte[1]));
            Assert.DoesNotThrow(() => package.SetAttachment("paint/layer-1.png", new byte[1]));
        }

        [Test]
        public void Load_CraftedArchiveWithTraversalName_IsRejected()
        {
            string path = WriteZip(archive =>
            {
                AddText(archive, PackagePresetRepository.PresetEntryName, "{ \"schemaVersion\": 1 }");
                AddText(archive, PackagePresetRepository.AttachmentFolder + "../../evil.png", "x");
            });

            Assert.Throws<PresetFormatException>(() => Run(repository.LoadAsync(path)));
        }

        [Test]
        public void Load_ArchiveWithoutPresetJson_IsRejected()
        {
            string path = WriteZip(archive => AddText(archive, "readme.txt", "hello"));

            Assert.Throws<PresetFormatException>(() => Run(repository.LoadAsync(path)));
        }

        [Test]
        public void Load_FileThatIsNotAnArchive_IsRejected()
        {
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "plain.weld");
            File.WriteAllText(path, "{ \"schemaVersion\": 1 }");

            Assert.Throws<PresetFormatException>(() => Run(repository.LoadAsync(path)));
        }

        [Test]
        public void Load_IgnoresUnknownEntriesOutsideAttachments()
        {
            string path = WriteZip(archive =>
            {
                AddText(archive, PackagePresetRepository.PresetEntryName, "{ \"schemaVersion\": 1 }");
                AddText(archive, "thumbnail.txt", "future feature");
            });

            PresetPackage package = Run(repository.LoadAsync(path));

            Assert.AreEqual(0, package.Attachments.Count);
        }

        [Test]
        public void Deserialize_DropsInvalidEntriesAndFillsMissingCollections()
        {
            CharacterPreset preset = PresetJson.Deserialize(
                "{ \"schemaVersion\": 1, \"equipment\": [ { \"itemId\": \"\" }, null, { \"itemId\": \"hat\", \"colors\": null } ], \"modifiers\": null, \"paintLayers\": [ null ] }");

            Assert.AreEqual("hat", preset.Equipment[0].ItemId);
            Assert.AreEqual(1, preset.Equipment.Count);
            Assert.IsNotNull(preset.Equipment[0].Colors);
            Assert.IsNotNull(preset.Equipment[0].Parameters);
            Assert.IsNotNull(preset.Modifiers);
            Assert.IsNotNull(preset.Metadata);
            Assert.IsEmpty(preset.PaintLayers);
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
            Assert.Catch<IOException>(() => Run(repository.LoadAsync(Path.Combine(directory, "missing.weld"))));
        }
    }
}
