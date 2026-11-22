using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedGameProgress
{
    public int completedRuns;
    public int bestRunLevel;
    public int masteryResourceBalance;
    public bool archivistQuestAccepted;
    public bool firstBossDefeated;
    public bool archivistRecruited;
    public string[] unlockedClassNames = new string[0];
    public string[] equippedClassNames = new string[0];
    public string[] metNpcNames = new string[0];
}

public static class GameProgression
{
    // Deliberately session-only for now: this survives scene loads but resets
    // when the application starts again.
    static SavedGameProgress sessionProgress = new SavedGameProgress();
    static readonly HashSet<string> classesUnlockedThisRun = new HashSet<string>();
    static int runMasteryResource;

    public static int RunMasteryResource => runMasteryResource;
    public static int MasteryResourceBalance => sessionProgress.masteryResourceBalance;

    public static void BeginRun()
    {
        classesUnlockedThisRun.Clear();
        runMasteryResource = 0;
    }

    public static void AwardRunMasteryResource(int amount)
    {
        if (amount > 0) runMasteryResource += amount;
    }

    public static bool TrySpendMasteryResource(int amount)
    {
        if (amount <= 0 || sessionProgress.masteryResourceBalance < amount) return false;
        sessionProgress.masteryResourceBalance -= amount;
        return true;
    }

    public static void RegisterClassAcquired(ClassData classData)
    {
        if (classData != null && !string.IsNullOrEmpty(classData.className))
            classesUnlockedThisRun.Add(classData.className);
    }

    public static bool ArchivistQuestAccepted => sessionProgress.archivistQuestAccepted;
    public static bool FirstBossDefeated => sessionProgress.firstBossDefeated;
    public static bool ArchivistRecruited => sessionProgress.archivistRecruited;

    public static void AcceptArchivistQuest()
    {
        sessionProgress.archivistQuestAccepted = true;
        RegisterNpcMet("The Archivist");
    }

    public static void MarkFirstBossDefeated()
    {
        sessionProgress.firstBossDefeated = true;
    }

    public static bool RecruitArchivistAfterCompletedExpedition()
    {
        if (!sessionProgress.archivistQuestAccepted || !sessionProgress.firstBossDefeated ||
            sessionProgress.archivistRecruited)
            return false;

        sessionProgress.archivistRecruited = true;
        return true;
    }

    public static bool RegisterNpcMet(string npcName)
    {
        if (string.IsNullOrEmpty(npcName)) return false;

        SavedGameProgress progress = sessionProgress;
        HashSet<string> met = new HashSet<string>(progress.metNpcNames ?? new string[0]);
        if (!met.Add(npcName)) return false;

        progress.metNpcNames = new List<string>(met).ToArray();
        return true;
    }

    public static string[] GetMetNpcNames()
    {
        return Load().metNpcNames ?? new string[0];
    }

    public static SavedGameProgress Load()
    {
        return sessionProgress;
    }

    public static int SaveCompletedRun(ClassData[] equippedClasses, int runLevel)
    {
        SavedGameProgress progress = sessionProgress;
        progress.completedRuns++;
        progress.bestRunLevel = Mathf.Max(progress.bestRunLevel, runLevel);
        progress.equippedClassNames = GetClassNames(equippedClasses);

        HashSet<string> unlocked = new HashSet<string>(progress.unlockedClassNames ?? new string[0]);
        unlocked.Add("Warrior");
        unlocked.Add("Gunner");
        foreach (string className in progress.equippedClassNames)
            unlocked.Add(className);
        foreach (string className in classesUnlockedThisRun)
            unlocked.Add(className);

        progress.unlockedClassNames = new List<string>(unlocked).ToArray();
        int bankedResource = runMasteryResource;
        progress.masteryResourceBalance += bankedResource;
        runMasteryResource = 0;
        return bankedResource;
    }

    public static string[] GetEquippedClassNames()
    {
        return Load().equippedClassNames ?? new string[0];
    }

    public static string[] GetUnlockedClassNames()
    {
        SavedGameProgress progress = Load();
        HashSet<string> names = new HashSet<string>(progress.unlockedClassNames ?? new string[0]);
        names.Add("Warrior");
        names.Add("Gunner");
        return new List<string>(names).ToArray();
    }

    public static ClassData[] RestoreEquippedClasses(ClassData[] defaults, int slotLimit)
    {
        string[] savedNames = GetEquippedClassNames();
        if (savedNames == null || savedNames.Length == 0) return defaults;

        Dictionary<string, ClassData> catalog = new Dictionary<string, ClassData>();
        AddToCatalog(catalog, defaults);
        AddToCatalog(catalog, Resources.FindObjectsOfTypeAll<ClassData>());

        List<ClassData> restored = new List<ClassData>();
        foreach (string savedName in savedNames)
        {
            if (restored.Count >= slotLimit) break;
            if (!string.IsNullOrEmpty(savedName) && catalog.TryGetValue(savedName, out ClassData classData) &&
                !restored.Contains(classData))
                restored.Add(classData);
        }

        foreach (ClassData defaultClass in defaults)
        {
            if (restored.Count >= slotLimit) break;
            if (defaultClass != null && !restored.Contains(defaultClass))
                restored.Add(defaultClass);
        }

        return restored.Count > 0 ? restored.ToArray() : defaults;
    }

    static void AddToCatalog(Dictionary<string, ClassData> catalog, ClassData[] classes)
    {
        if (classes == null) return;
        foreach (ClassData classData in classes)
        {
            if (classData != null && !string.IsNullOrEmpty(classData.className))
                catalog[classData.className] = classData;
        }
    }

    static string[] GetClassNames(ClassData[] classes)
    {
        List<string> names = new List<string>();
        if (classes != null)
        {
            foreach (ClassData classData in classes)
            {
                if (classData != null && !string.IsNullOrEmpty(classData.className) && !names.Contains(classData.className))
                    names.Add(classData.className);
            }
        }
        return names.ToArray();
    }
}
