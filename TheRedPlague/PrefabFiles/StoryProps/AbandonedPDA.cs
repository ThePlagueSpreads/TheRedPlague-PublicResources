using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Story;

namespace TheRedPlague.PrefabFiles.StoryProps;

public class AbandonedPda
{
    private PrefabInfo Info { get; }
    private string DatabankId { get; }

    public AbandonedPda(string classId, string databankId)
    {
        Info = PrefabInfo.WithTechType(classId);
        DatabankId = databankId;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(new CloneTemplate(Info, "02dbd99a-a279-4678-9be7-a21202862cb7")
        {
            ModifyPrefab = obj =>
            {
                obj.GetComponent<StoryHandTarget>().goal = new StoryGoal(DatabankId, Story.GoalType.Encyclopedia, 0);
            }
        });
        prefab.Register();
    }
}