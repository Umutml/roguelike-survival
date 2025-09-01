using System;
using GameCore.Health;
using UnityEngine;

public class ObjectiveBarricarde : ObjectiveDamageable
{
    public override void TakeDamage(DamageInfo damageInfo)
    {
        base.TakeDamage(damageInfo);
    }
    private void OnEnable()
    {
        Died += OnDied;
    }
    private void OnDisable()
    {
        Died -= OnDied;
    }
    private void OnDied(DamageSource damageSource)
    {
        gameObject.SetActive(false);
    }
}
