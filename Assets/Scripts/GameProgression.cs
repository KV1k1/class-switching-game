using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    public List<ClassMasteryProgress> classMastery = new List<ClassMasteryProgress>();
}

[System.Serializable]
public class ClassMasteryProgress
{
    public string className;
    public int weaponTraitTier;
    public int skillTreeTier;
}

[System.Serializable]
public class ExpeditionRunLog
{
    public int runNumber;
    public int durationSeconds;
    public int enemiesKilled;
    public int experienceGained;
    public int memoryShardsGathered;
    public int memoryShardsBanked;
    public bool completed;

    public string ToDisplayString()
    {
        int minutes = durationSeconds / 60;
        int seconds = durationSeconds % 60;
        string result = "Duration: " + minutes.ToString("00") + ":" + seconds.ToString("00") +
            "\nEnemies killed: " + enemiesKilled +
            "\nXP gained: " + experienceGained +
            "\nMemory Shards gathered: " + memoryShardsGathered;
        if (completed) result += "\nMemory Shards banked: " + memoryShardsBanked;
        return result;
    }
}

public static class GameProgression
{
    public const int MaximumMasteryTier = 3;
    // Deliberately session-only for now: this survives scene loads but resets
    // when the application starts again.
    static SavedGameProgress sessionProgress = new SavedGameProgress();
    static readonly HashSet<string> classesUnlockedThisRun = new HashSet<string>();
    static int runMasteryResource;
    static int runSequence;
    static int activeRunNumber;
    static float runStartedAt;
    static int runEnemiesKilled;
    static int runExperienceGained;
    static ExpeditionRunLog lastRunLog;
    public static string LastRunLogFilePath { get; private set; }

    public static int RunMasteryResource => runMasteryResource;
    public static int MasteryResourceBalance => sessionProgress.masteryResourceBalance;

    public static void BeginRun()
    {
        runSequence = Mathf.Max(runSequence, sessionProgress.completedRuns) + 1;
        activeRunNumber = runSequence;
        classesUnlockedThisRun.Clear();
        runMasteryResource = 0;
        runStartedAt = Time.time;
        runEnemiesKilled = 0;
        runExperienceGained = 0;
        lastRunLog = null;
    }

    public static void AwardRunMasteryResource(int amount)
    {
        if (amount > 0 && lastRunLog == null) runMasteryResource += amount;
    }

    public static void RecordRunEnemyKilled()
    {
        if (lastRunLog == null) runEnemiesKilled++;
    }

    public static void RecordRunExperience(int amount)
    {
        if (amount > 0 && lastRunLog == null) runExperienceGained += amount;
    }

    public static ExpeditionRunLog FinishRunLog(bool completed)
    {
        if (lastRunLog != null) return lastRunLog;

        lastRunLog = new ExpeditionRunLog
        {
            runNumber = activeRunNumber,
            durationSeconds = Mathf.Max(0, Mathf.FloorToInt(Time.time - runStartedAt)),
            enemiesKilled = runEnemiesKilled,
            experienceGained = runExperienceGained,
            memoryShardsGathered = runMasteryResource,
            completed = completed
        };

        AppendRunLog(lastRunLog);
        return lastRunLog;
    }

    static void AppendRunLog(ExpeditionRunLog runLog)
    {
        string projectRoot = Application.isEditor && Directory.GetParent(Application.dataPath) != null
            ? Directory.GetParent(Application.dataPath).FullName
            : Application.persistentDataPath;
        LastRunLogFilePath = Path.Combine(projectRoot, "ExpeditionLogs.txt");

        string outcome = runLog.completed ? "COMPLETED" : "FAILED";
        int minutes = runLog.durationSeconds / 60;
        int seconds = runLog.durationSeconds % 60;
        string entry = "=== Run " + runLog.runNumber + " | " + outcome + " | " +
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ===" + Environment.NewLine +
            "Duration: " + minutes.ToString("00") + ":" + seconds.ToString("00") + Environment.NewLine +
            "Enemies killed: " + runLog.enemiesKilled + Environment.NewLine +
            "XP gained: " + runLog.experienceGained + Environment.NewLine +
            "Memory Shards gathered: " + runLog.memoryShardsGathered + Environment.NewLine +
            Environment.NewLine;

        try
        {
            Directory.CreateDirectory(projectRoot);
            File.AppendAllText(LastRunLogFilePath, entry, new UTF8Encoding(false));
        }
        catch (Exception exception)
        {
            if (GameUI.Instance != null)
                GameUI.Instance.ShowNotification("Could not save ExpeditionLogs.txt: " + exception.Message);
        }
    }

    public static bool TrySpendMasteryResource(int amount)
    {
        if (amount <= 0 || sessionProgress.masteryResourceBalance < amount) return false;
        sessionProgress.masteryResourceBalance -= amount;
        return true;
    }

    public static int GetWeaponTraitTier(string className)
    {
        ClassMasteryProgress mastery = FindClassMastery(className, false);
        return mastery != null ? mastery.weaponTraitTier : 0;
    }

    public static int GetSkillTreeTier(string className)
    {
        ClassMasteryProgress mastery = FindClassMastery(className, false);
        return mastery != null ? mastery.skillTreeTier : 0;
    }

    public static int GetWeaponTraitCost(string className)
    {
        return GetUpgradeCost(GetWeaponTraitTier(className), new int[] { 10, 15, 20 });
    }

    public static int GetSkillTreeCost(string className)
    {
        return GetUpgradeCost(GetSkillTreeTier(className), new int[] { 15, 25, 35 });
    }

    public static bool TryUpgradeWeaponTrait(string className)
    {
        int tier = GetWeaponTraitTier(className);
        int cost = GetWeaponTraitCost(className);
        if (string.IsNullOrEmpty(className) || !IsClassUnlocked(className) || cost < 0 || !TrySpendMasteryResource(cost))
            return false;

        FindClassMastery(className, true).weaponTraitTier = tier + 1;
        return true;
    }

    public static bool TryUpgradeSkillTree(string className)
    {
        int tier = GetSkillTreeTier(className);
        int cost = GetSkillTreeCost(className);
        if (string.IsNullOrEmpty(className) || !IsClassUnlocked(className) || cost < 0 || !TrySpendMasteryResource(cost))
            return false;

        FindClassMastery(className, true).skillTreeTier = tier + 1;
        return true;
    }

    public static float GetClassCooldownMultiplier(string className)
    {
        return Mathf.Max(0.7f, 1f - GetWeaponTraitTier(className) * 0.06f);
    }

    public static float GetClassSkillDamageMultiplier(string className)
    {
        return 1f + GetSkillTreeTier(className) * 0.1f;
    }

    static int GetUpgradeCost(int tier, int[] costs)
    {
        return tier < 0 || tier >= costs.Length ? -1 : costs[tier];
    }

    static bool IsClassUnlocked(string className)
    {
        foreach (string unlockedName in GetUnlockedClassNames())
            if (unlockedName == className) return true;
        return false;
    }

    static ClassMasteryProgress FindClassMastery(string className, bool create)
    {
        if (string.IsNullOrEmpty(className)) return null;
        if (sessionProgress.classMastery == null)
            sessionProgress.classMastery = new List<ClassMasteryProgress>();

        foreach (ClassMasteryProgress mastery in sessionProgress.classMastery)
            if (mastery != null && mastery.className == className) return mastery;

        if (!create) return null;
        ClassMasteryProgress created = new ClassMasteryProgress { className = className };
        sessionProgress.classMastery.Add(created);
        return created;
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
