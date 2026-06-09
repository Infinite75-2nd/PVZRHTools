using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(RhythmGame.RhythmGameManager))]
public static class RhythmGameAutoPlayPatch
{
    private const float AutoRhythmLateWindow = 0.12f;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(RhythmGame.RhythmGameManager.Update))]
    public static void PostUpdate(RhythmGame.RhythmGameManager __instance)
    {
        if (!AutoRhythmGame || __instance == null) return;
        if (!__instance.isPlaying || __instance.isPaused) return;

        try
        {
            float now = __instance.CurrentTime;
            var tracks = __instance.tracks;
            if (tracks == null) return;

            foreach (var track in tracks)
            {
                if (track == null) continue;
                var notes = track.GetActiveNotes();
                if (notes == null || notes.Count == 0) continue;

                // 仅在到达判定点（targetTime）后触发，不提前按。
                RhythmGame.FallingNote? targetNote = null;
                float bestDelay = float.MaxValue;

                foreach (var note in notes)
                {
                    if (note == null || note.hasAutoPlayed) continue;
                    if (!note.IsClickable()) continue;

                    float delay = now - note.targetTime;
                    // 到判定线后才按，且限制在可接受晚判范围内。
                    if (delay < 0f || delay > AutoRhythmLateWindow) continue;

                    if (delay < bestDelay)
                    {
                        bestDelay = delay;
                        targetNote = note;
                    }
                }

                if (targetNote == null) continue;

                if (targetNote.noteType == RhythmGame.NoteType.Hold)
                {
                    targetNote.OnHoldStart();
                }
                else
                {
                    targetNote.OnClick();
                }
            }
        }
        catch
        {
            // 静默失败，避免影响正常局内流程。
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(RhythmGame.RhythmGameManager.IsHoldKeyPressed))]
    public static void PostIsHoldKeyPressed(RhythmGame.RhythmGameManager __instance, int trackIndex, ref bool __result)
    {
        if (!AutoRhythmGame || __instance == null) return;
        if (!__instance.isPlaying || __instance.isPaused) return;
        if (trackIndex < 0 || trackIndex >= 4) return;

        // 自动音游开启时，视为对应轨道按键（S/D/J/K）持续按下，保证长按音符稳定结算。
        __result = true;
    }
}