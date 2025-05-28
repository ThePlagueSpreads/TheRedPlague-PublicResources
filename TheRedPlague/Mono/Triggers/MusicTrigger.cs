using TheRedPlague.Mono.Systems;

namespace TheRedPlague.Mono.Triggers;

public class MusicTrigger : PlayerTrigger
{
    public FMODAsset music;
    public float musicDuration;

    public void SetUp(FMODAsset music, float musicDuration)
    {
        this.music = music;
        this.musicDuration = musicDuration;
    }
    
    protected override void OnTriggerActivated()
    {
        TrpEventMusicPlayer.PlayMusic(music, musicDuration);
    }
}