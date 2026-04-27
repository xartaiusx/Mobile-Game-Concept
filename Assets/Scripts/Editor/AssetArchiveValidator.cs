#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public static class AssetArchiveValidator
    {
        public const string ArchiveRoot = "Assets/ThirdParty/_Archive";
        public const string ManifestPath = ArchiveRoot + "/ARCHIVE_MANIFEST.md";
        public const string ReadmePath = ArchiveRoot + "/README.md";

        private static readonly string[] RequiredFolders =
        {
            ArchiveRoot,
            ArchiveRoot + "/KayKit",
            ArchiveRoot + "/Kenney"
        };

        private static readonly string[] ActiveReferenceRoots =
        {
            "Assets/Scenes/VerticalSlice.unity",
            "Assets/Prefabs",
            "Assets/Art/Prefabs",
            "Assets/Art/Animation/Controllers",
            "Assets/ScriptableObjects"
        };

        private static readonly string[] JunkFileNames =
        {
            ".DS_Store",
            "thumbs.db"
        };

        private static readonly string[] ExternalVaultExtensions =
        {
            ".glb",
            ".gltf",
            ".mtl",
            ".obj",
            ".tmx",
            ".tsx"
        };

        private static readonly string[] ExternalVaultFolderFragments =
        {
            "/Tiled/",
            "/Tilemap/"
        };

        private const long ArchiveWarningBytes = 25L * 1024L * 1024L;
        private const long ArchiveFailureBytes = 100L * 1024L * 1024L;

        [MenuItem("Game/Visuals/Validate Asset Archive")]
        public static bool ValidateAssetArchiveMenu()
        {
            return ValidateAssetArchive(true);
        }

        public static bool ValidateAssetArchive(bool logResult)
        {
            ArchiveValidationReport report = CreateReport();
            if (report.failures.Count == 0)
            {
                if (logResult && report.warnings.Count > 0)
                    Debug.LogWarning("Asset archive validation warnings:\n" + string.Join("\n", report.warnings));
                if (logResult)
                    Debug.Log("Asset archive validation passed. " + report.Summary);
                return true;
            }

            if (logResult)
                Debug.LogError("Asset archive validation failed:\n" + string.Join("\n", report.failures));
            return false;
        }

        public static ArchiveValidationReport CreateReport()
        {
            var report = new ArchiveValidationReport();
            for (int i = 0; i < RequiredFolders.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(RequiredFolders[i]))
                    report.failures.Add("Missing archive folder: " + RequiredFolders[i]);
            }

            if (!File.Exists(ReadmePath))
                report.failures.Add("Missing archive README: " + ReadmePath);
            if (!File.Exists(ManifestPath))
                report.failures.Add("Missing archive manifest: " + ManifestPath);
            else if (new FileInfo(ManifestPath).Length == 0)
                report.failures.Add("Archive manifest is empty: " + ManifestPath);

            if (!Directory.Exists(ArchiveRoot))
                return report;

            string[] archiveFiles = Directory.GetFiles(ArchiveRoot, "*", SearchOption.AllDirectories);
            Array.Sort(archiveFiles, StringComparer.Ordinal);
            report.fileCount = archiveFiles.Length;
            var archiveGuids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < archiveFiles.Length; i++)
            {
                string path = ToAssetPath(archiveFiles[i]);
                string fileName = Path.GetFileName(path);
                report.totalBytes += new FileInfo(archiveFiles[i]).Length;

                if (IsJunkFile(fileName) || path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    report.failures.Add("Archive contains junk or packaged source file: " + path);

                if (ShouldLiveInExternalVault(path))
                    report.warnings.Add("Archive contains external-vault candidate: " + path);

                if (!path.EndsWith(".meta", StringComparison.Ordinal))
                {
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    if (!string.IsNullOrEmpty(guid))
                        archiveGuids.Add(guid);
                }
            }

            if (report.totalBytes > ArchiveFailureBytes)
            {
                report.failures.Add("Archive size is excessive: " + ArchiveValidationReport.FormatBytes(report.totalBytes));
            }
            else if (report.totalBytes > ArchiveWarningBytes)
            {
                report.warnings.Add("Archive size is large: " + ArchiveValidationReport.FormatBytes(report.totalBytes));
            }

            ValidateArchivedPrefabs(report);
            ValidateNoActiveArchiveReferences(archiveGuids, report);
            return report;
        }

        private static void ValidateArchivedPrefabs(ArchiveValidationReport report)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { ArchiveRoot });
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    continue;

                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab) > 0)
                    report.failures.Add(path + " has missing scripts.");

                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
                for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    Material[] materials = renderers[rendererIndex].sharedMaterials;
                    for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                    {
                        if (materials[materialIndex] == null)
                            report.failures.Add(path + " has a missing material on " + renderers[rendererIndex].name + ".");
                    }
                }
            }
        }

        private static void ValidateNoActiveArchiveReferences(HashSet<string> archiveGuids, ArchiveValidationReport report)
        {
            if (archiveGuids.Count == 0)
                return;

            List<string> activeFiles = CollectActiveSerializedFiles();
            for (int i = 0; i < activeFiles.Count; i++)
            {
                string path = activeFiles[i];
                string text = File.ReadAllText(path);
                if (text.Contains(ArchiveRoot, StringComparison.Ordinal))
                    report.failures.Add("Active file contains literal archive path reference: " + path);

                foreach (string guid in archiveGuids)
                {
                    if (text.Contains(guid, StringComparison.OrdinalIgnoreCase))
                    {
                        report.failures.Add("Active file references archived asset GUID " + guid + ": " + path);
                        break;
                    }
                }
            }
        }

        private static List<string> CollectActiveSerializedFiles()
        {
            var files = new List<string>();
            for (int i = 0; i < ActiveReferenceRoots.Length; i++)
            {
                string root = ActiveReferenceRoots[i];
                if (File.Exists(root))
                {
                    files.Add(root);
                    continue;
                }

                if (!Directory.Exists(root))
                    continue;

                string[] found = Directory.GetFiles(root, "*", SearchOption.AllDirectories);
                Array.Sort(found, StringComparer.Ordinal);
                for (int j = 0; j < found.Length; j++)
                {
                    string path = ToAssetPath(found[j]);
                    if (path.Contains("/_Archive/", StringComparison.Ordinal) || path.EndsWith(".meta", StringComparison.Ordinal))
                        continue;
                    if (IsSerializedUnityFile(path))
                        files.Add(path);
                }
            }

            return files;
        }

        private static bool IsSerializedUnityFile(string path)
        {
            return path.EndsWith(".unity", StringComparison.Ordinal)
                || path.EndsWith(".prefab", StringComparison.Ordinal)
                || path.EndsWith(".asset", StringComparison.Ordinal)
                || path.EndsWith(".controller", StringComparison.Ordinal)
                || path.EndsWith(".overrideController", StringComparison.Ordinal)
                || path.EndsWith(".mat", StringComparison.Ordinal);
        }

        private static bool IsJunkFile(string fileName)
        {
            for (int i = 0; i < JunkFileNames.Length; i++)
            {
                if (string.Equals(fileName, JunkFileNames[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool ShouldLiveInExternalVault(string path)
        {
            if (path.EndsWith(".meta", StringComparison.Ordinal))
                return false;

            string extension = Path.GetExtension(path);
            for (int i = 0; i < ExternalVaultExtensions.Length; i++)
            {
                if (string.Equals(extension, ExternalVaultExtensions[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            for (int i = 0; i < ExternalVaultFolderFragments.Length; i++)
            {
                if (path.Contains(ExternalVaultFolderFragments[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string ToAssetPath(string path)
        {
            return path.Replace('\\', '/');
        }
    }

    public sealed class ArchiveValidationReport
    {
        public readonly List<string> failures = new List<string>();
        public readonly List<string> warnings = new List<string>();
        public int fileCount;
        public long totalBytes;

        public string Summary => "files=" + fileCount + ", size=" + FormatBytes(totalBytes) + ", warnings=" + warnings.Count + ".";

        public static string FormatBytes(long bytes)
        {
            if (bytes >= 1024L * 1024L)
                return (bytes / (1024f * 1024f)).ToString("0.0") + " MB";
            if (bytes >= 1024L)
                return (bytes / 1024f).ToString("0.0") + " KB";
            return bytes + " B";
        }
    }
}
#endif
