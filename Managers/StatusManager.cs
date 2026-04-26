using Quest.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}

public class StatusManager
{
    private readonly Dictionary<StatusEffect, float> _statusEffects = new();
    private readonly Dictionary<StatusEffect, Notification> _notifications = new();
    public Dictionary<StatusEffect, float> GetStatusEffects() => _statusEffects;
    public int GetStatusEffectsCount() => _statusEffects.Count;
    public void AddStatusEffect(PlayerManager player, StatusEffect effect, float duration)
    {
        _statusEffects[effect] = duration;
        if (_notifications.TryGetValue(effect, out var notif))
            notif.Duration = Math.Max(duration, notif.Duration);
        else
        {
            Notification newNotif = player.StatusArea.AddNotification($"{TimeSpan.FromSeconds(duration):mm\\:ss} | {effect}", color: IsPositiveEffect(effect) ? Color.Lime : Color.Red, int.MaxValue);
            _notifications[effect] = newNotif;
        }
    }
    public bool HasStatusEffect(StatusEffect effect)
    {
        return _statusEffects.ContainsKey(effect);
    }
    public float GetStatusEffectDuration(StatusEffect effect)
    {
        return _statusEffects.TryGetValue(effect, out float duration) ? duration : 0;
    }
    private float GetEffectMult(StatusEffect effect)
    {
        return effect switch
        {
            StatusEffect.Speed => HasStatusEffect(effect) ? 2f : 1f,
            StatusEffect.Strength => HasStatusEffect(effect) ? 1.5f : 1f,
            StatusEffect.Weakness => HasStatusEffect(effect) ? 0.5f : 1f,
            StatusEffect.Slowness => HasStatusEffect(effect) ? 0.5f : 1f,
            StatusEffect.Vulnerability => HasStatusEffect(effect) ? 1.5f : 1f,
            StatusEffect.Protection => HasStatusEffect(effect) ? 0.5f : 1f,
            StatusEffect.Lifesteal => HasStatusEffect(effect) ? 0.3f : 0f,
            _ => 1f
        };
    }
    private bool IsPositiveEffect(StatusEffect effect)
    {
        return effect switch
        {
            StatusEffect.Speed => true,
            StatusEffect.Strength => true,
            StatusEffect.Regeneration => true,
            StatusEffect.Protection => true,
            StatusEffect.Lifesteal => true,
            _ => false
        };
    }
    public float GetSpeedMult() => GetEffectMult(StatusEffect.Speed) * GetEffectMult(StatusEffect.Slowness);
    public float GetDamageMult() => GetEffectMult(StatusEffect.Strength) * GetEffectMult(StatusEffect.Weakness);
    public float GetDefenseMult() => GetEffectMult(StatusEffect.Protection) * GetEffectMult(StatusEffect.Vulnerability);
    public float GetLifestealMult() => GetEffectMult(StatusEffect.Lifesteal);
    public void Update(GameManager gameManager, PlayerManager player)
    {
        // Time
        var expiredEffects = new List<StatusEffect>();
        foreach (var kvp in _statusEffects)
        {
            _statusEffects[kvp.Key] -= GameManager.DeltaTime;
            _notifications[kvp.Key].Text = $"{TimeSpan.FromSeconds(_statusEffects[kvp.Key]):mm\\:ss} | {kvp.Key}";
            if (_statusEffects[kvp.Key] <= 0)
            {
                expiredEffects.Add(kvp.Key);
            }
        }

        // Clear
        foreach (var effect in expiredEffects)
        {
            _statusEffects.Remove(effect);
            _notifications[effect].Duration = 0;
        }

        // Reset visual effects
        gameManager.GradingEffect?.Parameters["Tint"].SetValue(Vector3.One);
        gameManager.GradingEffect?.Parameters["Saturation"].SetValue(1f);
        gameManager.GradingEffect?.Parameters["Contrast"].SetValue(1f);

        // Status effects
        if (HasStatusEffect(StatusEffect.Poison))
        {
            if (TimerManager.IsCompleteOrMissing("PlayerPoisonTick"))
            {
                player.Hurt(gameManager, 10);
                TimerManager.SetTimer("PlayerPoisonTick", 1, null);
            }
            gameManager.GradingEffect?.Parameters["Tint"].SetValue(new Vector3(.8f, 1, .8f));
        }
        if (HasStatusEffect(StatusEffect.Regeneration) && TimerManager.IsCompleteOrMissing("PlayerRegenerationTick"))
        {
            player.Heal(gameManager, 10);
            TimerManager.SetTimer("PlayerRegenerationTick", 1, null);
        }
        if (HasStatusEffect(StatusEffect.Delerium))
        {
            gameManager.GradingEffect?.Parameters["Saturation"].SetValue(0.1f + (0.9f / (0.4f * GetStatusEffectDuration(StatusEffect.Delerium) + 1)));
            CameraManager.Camera += RandomManager.RandomUnitVec2() * Math.Clamp(0.1f * GetStatusEffectDuration(StatusEffect.Delerium), 0, 4);
        }
    }
}
