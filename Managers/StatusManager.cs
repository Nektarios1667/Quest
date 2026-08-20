using Quest.Gui;

namespace Quest.Managers;

public enum StatusEffect : byte
{
    Speed,
    Slowness,
    Regeneration,
    Poison,
    Strength,
    Weakness,
    Protection,
    Vulnerability,
    Delerium,
    Lifesteal,
    Burning,
    Cravings,
    Fullness,
}

public static class StatusManager
{
    private static readonly Dictionary<StatusEffect, Notification> playerNotifs = new();

    public static void AddStatusEffect(IStatusEffectable entity, StatusEffect effect, float duration)
    {
        entity.StatusEffects[effect] = duration;

        if (entity is not PlayerManager playerManager) return;
        // Player
        if (playerNotifs.TryGetValue(effect, out var notif))
            notif.Duration = Math.Max(duration, notif.Duration);
        else
        {
            Notification newNotif = playerManager.StatusArea.AddNotification($"{TimeSpan.FromSeconds(duration):mm\\:ss} | {effect}", color: IsPositiveEffect(effect) ? Color.Lime : Color.Red, int.MaxValue);
            playerNotifs[effect] = newNotif;
        }
    }
    public static void ClearStatusEffect(StatusEffect effect, IStatusEffectable entity)
    {
        bool isPlayer = entity.UID == 0; // PlayerManager always has UID 0
        entity.StatusEffects.Remove(effect);

        if (isPlayer && playerNotifs.TryGetValue(effect, out var notif))
            notif.Duration = 0;
    }
    public static void ClearAllStatusEffects(GameManager gameManager, IStatusEffectable entity)
    {
        bool isPlayer = entity.UID == 0; // PlayerManager always has UID 0

        entity.StatusEffects.Clear();

        // Player
        if (isPlayer)
        {
            gameManager.GradingEffect?.Parameters["Saturation"].SetValue(1);
            foreach (var notif in playerNotifs.Values)
                notif.Duration = 0;
        }
    }
    public static bool HasStatusEffect(StatusEffect effect, IStatusEffectable entity)
    {
        return entity.StatusEffects.ContainsKey(effect);
    }
    public static float GetStatusEffectDuration(StatusEffect effect, IStatusEffectable entity)
    {
        return entity.StatusEffects.TryGetValue(effect, out float duration) ? duration : 0;
    }
    private static float GetEffectMult(StatusEffect effect, IStatusEffectable entity)
    {
        return effect switch
        {
            StatusEffect.Speed => HasStatusEffect(effect, entity) ? 2f : 1f,
            StatusEffect.Strength => HasStatusEffect(effect, entity) ? 1.5f : 1f,
            StatusEffect.Weakness => HasStatusEffect(effect, entity) ? 0.5f : 1f,
            StatusEffect.Slowness => HasStatusEffect(effect, entity) ? 0.5f : 1f,
            StatusEffect.Vulnerability => HasStatusEffect(effect, entity) ? 1.5f : 1f,
            StatusEffect.Protection => HasStatusEffect(effect, entity) ? 0.5f : 1f,
            StatusEffect.Lifesteal => HasStatusEffect(effect, entity) ? 0.3f : 0f,
            StatusEffect.Cravings => HasStatusEffect(effect, entity) ? 3.0f : 1f,
            StatusEffect.Fullness => HasStatusEffect(effect, entity) ? 0.67f : 1f,
            _ => 1f
        };
    }
    private static bool IsPositiveEffect(StatusEffect effect)
    {
        return effect switch
        {
            StatusEffect.Speed => true,
            StatusEffect.Strength => true,
            StatusEffect.Regeneration => true,
            StatusEffect.Protection => true,
            StatusEffect.Lifesteal => true,
            StatusEffect.Fullness => true,
            _ => false
        };
    }
    public static float GetSpeedMult(IStatusEffectable entity) => GetEffectMult(StatusEffect.Speed, entity) * GetEffectMult(StatusEffect.Slowness, entity);
    public static float GetDamageMult(IStatusEffectable entity) => GetEffectMult(StatusEffect.Strength, entity) * GetEffectMult(StatusEffect.Weakness, entity);
    public static float GetDefenseMult(IStatusEffectable entity) => GetEffectMult(StatusEffect.Protection, entity) * GetEffectMult(StatusEffect.Vulnerability, entity);
    public static int GetCravingsMult(IStatusEffectable entity) => (int)(GetEffectMult(StatusEffect.Cravings, entity) * GetEffectMult(StatusEffect.Fullness, entity));
    public static float GetLifestealMult(IStatusEffectable entity) => GetEffectMult(StatusEffect.Lifesteal, entity);
    public static void Update(GameManager gameManager, IStatusEffectable entity)
    {
        bool isPlayer = entity.UID == 0; // PlayerManager always has UID 0

        // Time
        var expiredEffects = new List<StatusEffect>();
        foreach (var kvp in entity.StatusEffects)
        {
            entity.StatusEffects[kvp.Key] -= GameManager.DeltaTime;

            // Player
            if (isPlayer)
            {
                playerNotifs[kvp.Key].Text = $"{TimeSpan.FromSeconds(entity.StatusEffects[kvp.Key]):mm\\:ss} | {kvp.Key}";
                if (entity.StatusEffects[kvp.Key] <= 0)
                {
                    expiredEffects.Add(kvp.Key);
                }
            }
        }


        // Reset visual effects
        if (isPlayer)
        {
            // Clear effects
            foreach (var effect in expiredEffects)
            {
                entity.StatusEffects.Remove(effect);
                playerNotifs[effect].Duration = 0;
            }
            // Clear visuals
            gameManager.GradingEffect?.Parameters["Tint"].SetValue(Vector3.One);
            gameManager.GradingEffect?.Parameters["Saturation"].SetValue(1f);
            gameManager.GradingEffect?.Parameters["Contrast"].SetValue(1f);
        }

        // Status effects
        if (HasStatusEffect(StatusEffect.Poison, entity) || HasStatusEffect(StatusEffect.Burning, entity))
        {
            if (TimerManager.IsCompleteOrMissing($"DOTTick_{entity.UID}"))
            {
                entity.Hurt(gameManager, 5);
                TimerManager.SetTimer($"DOTTick_{entity.UID}", 1, null);
            }
            if (isPlayer)
                gameManager.GradingEffect?.Parameters["Tint"].SetValue(new Vector3(.8f, 1, .8f));
        }
        if (HasStatusEffect(StatusEffect.Regeneration, entity) && TimerManager.IsCompleteOrMissing($"RegenerationTick_{entity.UID}"))
        {
            entity.Heal(gameManager, 5);
            TimerManager.SetTimer($"RegenerationTick_{entity.UID}", 1, null);
        }
        if (HasStatusEffect(StatusEffect.Delerium, entity) && isPlayer)
        {
            gameManager.GradingEffect?.Parameters["Saturation"].SetValue(0.1f + (0.9f / (0.4f * GetStatusEffectDuration(StatusEffect.Delerium, entity) + 1)));
            CameraManager.Camera += RandomManager.RandomUnitVec2() * Math.Clamp(0.1f * GetStatusEffectDuration(StatusEffect.Delerium, entity), 0, 4);
        }
    }
}
