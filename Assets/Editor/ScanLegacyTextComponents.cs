using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public static class ScanLegacyTextUsage
{
    static readonly string[] patterns = new string[]
    {
        @"\bGUIText\b",
        @"\bTextMesh\b",
        @"\bUnityEngine\.UI\.Text\b",
        @"using\s+UnityEngine\.UI\s*;",
        @"GetComponent\s*<\s*Text\s*>",
        @"GetComponent\s*<\s*TextMesh\s*>",
        @"GetComponent\s*<\s*GUIText\s*>",
        @"\bText\s+[a-zA-Z0-9_]+\s*;",
        @"\bTextMesh\s+[a-zA-Z0-9_]+\s*;",
        @"\bGUIText\s+[a-zA-Z0-9_]+\s*;"
    };

    [MenuItem("Project/Scan Scripts for Legacy Text")]
    public static void Scan()
    {
        string scriptsRoot = Path.Combine(Application.dataPath, "Scripts");
        if (!Directory.Exists(scriptsRoot))
        {
            Debug.LogWarning("Assets/Scripts/ does not exist!");
            return;
        }

        var csFiles = Directory.GetFiles(scriptsRoot, "*.cs", SearchOption.AllDirectories);
        var foundMatches = new List<string>();

        for (int fileIndex = 0; fileIndex < csFiles.Length; fileIndex++)
        {
            string file = csFiles[fileIndex];
            float progress = (float)fileIndex / csFiles.Length;
            bool canceled = EditorUtility.DisplayCancelableProgressBar(
                "Scanning for Legacy Text",
                $"Scanning file {fileIndex + 1} of {csFiles.Length}\n{file.Replace(Application.dataPath, "Assets")}",
                progress
            );
            if (canceled)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogWarning("Scan canceled.");
                return;
            }

            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                foreach (var pattern in patterns)
                {
                    if (Regex.IsMatch(lines[i], pattern))
                    {
                        string msg = $"[Legacy Text] {file.Replace(Application.dataPath, "Assets")} (Line {i + 1}): {lines[i].Trim()}";
                        Debug.Log(msg);
                        foundMatches.Add(msg);
                        break;
                    }
                }
            }
        }

        EditorUtility.ClearProgressBar();

        string txtPath = Path.Combine(Application.dataPath, "LegacyTextScan.txt");
        if (foundMatches.Count == 0)
        {
            Debug.Log("No legacy text usages found in Assets/Scripts/!");
            File.WriteAllText(txtPath, "No legacy text usages found in Assets/Scripts/!\n");
        }
        else
        {
            Debug.Log($"Found legacy text usages in {foundMatches.Count} places in Assets/Scripts/. See above for details.");
            File.WriteAllLines(txtPath, foundMatches);
        }

        AssetDatabase.Refresh(); // Show the new/updated .txt file in Project window
        Debug.Log($"Legacy text scan results written to: Assets/LegacyTextScan.txt");
    }
}
