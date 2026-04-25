using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Managers;

public enum StatusEffect
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
    public void AddStatusEffect(StatusEffect effect, float duration)
    {
        _statusEffects[effect] = duration;
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
            if (_statusEffects[kvp.Key] <= 0)
            {
                expiredEffects.Add(kvp.Key);
            }
        }

        // Clear
        foreach (var effect in expiredEffects)
            _statusEffects.Remove(effect);

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
        }
    }
}
