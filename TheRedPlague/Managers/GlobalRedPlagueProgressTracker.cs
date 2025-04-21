using UnityEngine;

namespace TheRedPlague.Managers;

// Used for the Main Menu infection progress
public static class GlobalRedPlagueProgressTracker
{
    private const string PlayerPrefKey = "RedPlagueGlobalProgress";
    
    public static void OnProgressAchieved(ProgressStatus progress)
    {
        if (PlayerPrefs.HasKey(PlayerPrefKey))
        {
            int oldNumber = PlayerPrefs.GetInt(PlayerPrefKey);
            int finalNumber = Mathf.Max((int)progress, oldNumber);
            PlayerPrefs.SetInt(PlayerPrefKey, finalNumber);
        }
        else
        {
            PlayerPrefs.SetInt(PlayerPrefKey, (int)progress);
        }
    }

    public static void ForgetCurrentProgress()
    {
        PlayerPrefs.DeleteKey(PlayerPrefKey);
    }

    // Returns the number of the largest number or 0
    public static int GetCurrentProgressValue()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefKey))
            return 0;
        return PlayerPrefs.GetInt(PlayerPrefKey);
    }

    public enum ProgressStatus
    {
        NewSave,
        DomeConstructed,
        InitialChecklistMissionsCompleted
    }
}