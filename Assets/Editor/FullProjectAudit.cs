using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class FullProjectAudit
{
    private static readonly string TargetScene = "Assets/Scene/WallFinishingSystem/WallFinishingScene.unity";

    // Слова-маркеры для поиска следов компании в коде
    private static readonly string[] SuspiciousKeywords =
    {
        "Didgitech", "Copyright", "Created by", "Author:", "Company", "LabsInfo", "DataSaver", "QuestionSystem"
    };

    [MenuItem("Tools/Run Full Project Audit")]
    public static void RunFullAudit()
    {
        if (!File.Exists(TargetScene))
        {
            Debug.LogError($"Целевая сцена не найдена по пути: {TargetScene}");
            return;
        }

        // 1. Собираем все ассеты проекта (исключая Editor и служебные системные пакеты)
        var allAssets = AssetDatabase.GetAllAssetPaths()
            .Where(p => p.StartsWith("Assets/") && !AssetDatabase.IsValidFolder(p) && !p.Contains("/Editor/"))
            .OrderBy(p => p)
            .ToList();

        // 2. Собираем базовые зависимости сцены и ваших папок
        var usedAssets = new HashSet<string>(AssetDatabase.GetDependencies(TargetScene, true));
        usedAssets.Add(TargetScene);

        var myCoreAssets = allAssets
            .Where(p => p.StartsWith("Assets/Scene/") || p.StartsWith("Assets/Scripts/"))
            .ToArray();

        foreach (var dep in AssetDatabase.GetDependencies(myCoreAssets, true))
            usedAssets.Add(dep);

        // 3. Рекурсивно проверяем C#-код на зависимости между классами и вызовы Resources.Load
        var allCsFiles = allAssets.Where(p => p.EndsWith(".cs")).ToList();
        var loadedResources = new HashSet<string>();
        bool addedNew;

        do
        {
            addedNew = false;
            var currentUsedCs = usedAssets.Where(p => p.EndsWith(".cs")).ToList();

            foreach (var csPath in currentUsedCs)
            {
                if (!File.Exists(csPath)) continue;
                string code = File.ReadAllText(csPath);

                // Поиск Resources.Load("...")
                var resMatches = Regex.Matches(code, @"Resources\.Load(?:<[^>]+>)?\s*\(\s*""([^""]+)""");
                foreach (Match m in resMatches)
                {
                    string resName = m.Groups[1].Value;

                    loadedResources.Add(resName);

                    foreach (var asset in allAssets.Where(a => a.Contains("/Resources/")))
                    {
                        string fileNameNoExt = Path.GetFileNameWithoutExtension(asset);
                        if (asset.Contains(resName) || fileNameNoExt.Equals(resName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (usedAssets.Add(asset)) addedNew = true;
                        }
                    }
                }

                // Поиск связей между скриптами по имени класса/файла
                foreach (var otherCs in allCsFiles)
                {
                    if (usedAssets.Contains(otherCs)) continue;
                    string typeName = Path.GetFileNameWithoutExtension(otherCs);
                    if (Regex.IsMatch(code, $@"\b{Regex.Escape(typeName)}\b"))
                    {
                        usedAssets.Add(otherCs);
                        foreach (var subDep in AssetDatabase.GetDependencies(otherCs, true))
                            usedAssets.Add(subDep);
                        addedNew = true;
                    }
                }
            }
        } while (addedNew);

        // 4. Группируем все ассеты по категориям
        var categories = new Dictionary<string, List<string>>
        {
            { "1. СЦЕНЫ (.unity)", new List<string>() },
            { "2. ПРЕФАБЫ (.prefab)", new List<string>() },
            { "3. SCRIPTABLE OBJECTS (.asset)", new List<string>() },
            { "4. СКРИПТЫ C# (.cs)", new List<string>() },
            { "5. 3D-МОДЕЛИ (.fbx, .obj, .blend)", new List<string>() },
            { "6. МАТЕРИАЛЫ И ШЕЙДЕРЫ (.mat, .shader)", new List<string>() },
            { "7. ТЕКСТУРЫ И СПРАЙТЫ (.png, .jpg, .tga, .psd)", new List<string>() },
            { "8. АУДИО (.wav, .mp3, .ogg)", new List<string>() },
            { "9. ШРИФТЫ (.ttf, .otf, .fontsettings)", new List<string>() },
            { "10. АНИМАЦИИ И КОНТРОЛЛЕРЫ (.anim, .controller)", new List<string>() },
            { "11. ПРОЧИЕ ФАЙЛЫ", new List<string>() }
        };

        foreach (var path in allAssets)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            string cat = GetCategoryName(ext);
            categories[cat].Add(path);
        }

        // 5. Формируем итоговый отчет
        var report = new List<string>
        {
            "==========================================================================",
            $"ПОЛНЫЙ АУДИТ ПРОЕКТА (Дата: {DateTime.Now:yyyy-MM-dd HH:mm:ss})",
            $"Целевая сцена: {TargetScene}",
            $"Всего файлов в Assets (без Editor): {allAssets.Count}",
            $"Используется: {usedAssets.Count(p => p.StartsWith("Assets/"))} | Не используется: {allAssets.Count(p => !usedAssets.Contains(p))}",
            "==========================================================================\n"
        };

        // Блок А: Найденные Resources.Load
        report.Add(">>> ДИНАМИЧЕСКАЯ ЗАГРУЗКА ИЗ КОДА (Resources.Load) <<<");
        if (loadedResources.Count == 0) report.Add("  Не найдено.");
        else foreach (var r in loadedResources) report.Add($"  - \"{r}\"");
        report.Add("");

        // Блок Б: Проверка скриптов на следы компании и неймспейсы
        report.Add(">>> ПРОВЕРКА КОДА НА СЛЕДЫ КОМПАНИИ И NAMESPACE <<<");
        var suspiciousFound = false;
        foreach (var cs in categories["4. СКРИПТЫ C# (.cs)"])
        {
            if (cs.Contains("/Plugins/")) continue;
            string[] lines = File.ReadAllLines(cs);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("namespace "))
                {
                    report.Add($"  [NAMESPACE] {cs} (строка {i + 1}): {line}");
                    suspiciousFound = true;
                }
                foreach (var kw in SuspiciousKeywords)
                {
                    if (line.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        report.Add($"  [МАРКЕР '{kw}'] {cs} (строка {i + 1}): {line}");
                        suspiciousFound = true;
                    }
                }
            }
        }
        if (!suspiciousFound) report.Add("  Подозрительных маркеров компании и чужих namespace не найдено!");
        report.Add("");

        // Блок В: Детальный разбор всех категорий
        var unusedToDelete = new List<string>();
        foreach (var kvp in categories)
        {
            if (kvp.Value.Count == 0) continue;

            var usedInCat = kvp.Value.Where(p => usedAssets.Contains(p)).ToList();
            var unusedInCat = kvp.Value.Where(p => !usedAssets.Contains(p)).ToList();

            report.Add($"================ {kvp.Key} (Всего: {kvp.Value.Count} | Используется: {usedInCat.Count} | Не используется: {unusedInCat.Count}) ================");

            if (usedInCat.Count > 0)
            {
                report.Add("  [+] ИСПОЛЬЗУЕТСЯ:");
                foreach (var item in usedInCat)
                    report.Add($"      {item}");
            }

            if (unusedInCat.Count > 0)
            {
                report.Add("  [-] НЕ ИСПОЛЬЗУЕТСЯ:");
                foreach (var item in unusedInCat)
                {
                    report.Add($"      {item}");
                    // Плагины (DOTween, TMP) не добавляем в автоудаление во избежание поломки компиляции
                    if (!item.StartsWith("Assets/Plugins/"))
                        unusedToDelete.Add(item);
                }
            }
            report.Add("");
        }

                // Блок Г: Поиск пустых папок
        var emptyFolders = FindEmptyFolders("Assets");
        report.Add($">>> ПУСТЫЕ ПАПКИ В ASSETS ({emptyFolders.Count}) <<<");
        foreach (var folder in emptyFolders)
            report.Add($"  [ПУСТАЯ ПАПКА] {folder}");

        File.WriteAllLines("FULL_PROJECT_AUDIT.txt", report);
        File.WriteAllLines("SAFE_UNUSED_LIST.txt", unusedToDelete);

        Debug.Log($"Полный аудит завершен! Отчет сохранен в FULL_PROJECT_AUDIT.txt. Кандидатов на удаление: {unusedToDelete.Count}. Пустых папок: {emptyFolders.Count}.");
    }

    [MenuItem("Tools/Clean Safe Unused & Empty Folders")]
    public static void CleanSafeUnused()
    {
        int deletedFiles = 0;
        if (File.Exists("SAFE_UNUSED_LIST.txt"))
        {
            var paths = File.ReadAllLines("SAFE_UNUSED_LIST.txt");
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var p in paths)
                {
                    if (!string.IsNullOrWhiteSpace(p) && AssetDatabase.DeleteAsset(p))
                        deletedFiles++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        int deletedFolders = 0;
        var emptyFolders = FindEmptyFolders("Assets").OrderByDescending(f => f.Length).ToList();
        foreach (var folder in emptyFolders)
        {
            if (AssetDatabase.DeleteAsset(folder))
                deletedFolders++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"Очистка завершена! Удалено файлов: {deletedFiles}, пустых папок: {deletedFolders}.");
    }

    private static string GetCategoryName(string ext)
    {
        switch (ext)
        {
            case ".unity": return "1. СЦЕНЫ (.unity)";
            case ".prefab": return "2. ПРЕФАБЫ (.prefab)";
            case ".asset": return "3. SCRIPTABLE OBJECTS (.asset)";
            case ".cs": return "4. СКРИПТЫ C# (.cs)";
            case ".fbx":
            case ".obj":
            case ".blend":
            case ".dae": return "5. 3D-МОДЕЛИ (.fbx, .obj, .blend)";
            case ".mat":
            case ".shader":
            case ".cginc": return "6. МАТЕРИАЛЫ И ШЕЙДЕРЫ (.mat, .shader)";
            case ".png":
            case ".jpg":
            case ".jpeg":
            case ".tga":
            case ".psd":
            case ".tif": return "7. ТЕКСТУРЫ И СПРАЙТЫ (.png, .jpg, .tga, .psd)";
            case ".wav":
            case ".mp3":
            case ".ogg": return "8. АУДИО (.wav, .mp3, .ogg)";
            case ".ttf":
            case ".otf":
            case ".fontsettings": return "9. ШРИФТЫ (.ttf, .otf, .fontsettings)";
            case ".anim":
            case ".controller":
            case ".overridecontroller": return "10. АНИМАЦИИ И КОНТРОЛЛЕРЫ (.anim, .controller)";
            default: return "11. ПРОЧИЕ ФАЙЛЫ";
        }
    }

    private static List<string> FindEmptyFolders(string rootPath)
    {
        var result = new List<string>();
        foreach (var dir in Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories))
        {
            string normalized = dir.Replace("\\", "/");
            if (normalized.Contains("/Editor")) continue;

            var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".meta"))
                .ToArray();

            if (files.Length == 0)
                result.Add(normalized);
        }
        return result;
    }
}
