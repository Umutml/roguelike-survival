using System;
using GameCore.Health;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

public class ObjectiveDamageable :MonoBehaviour, IDamageable
{
    [SerializeField] private AssetReferenceGameObject hitParticle, deathParticle;
    [SerializeField] protected MobHealthManager mobHealthManager;
    [SerializeField] private GameObject[] damageTypes;
    public string SpecificDamageType { get; }
    public BoxCollider Bounds { get; set; }
    [SerializeField] private BoxCollider bounds;
    public float maxHealth = 100;
    public float Health { get;set; }
    public Vector3 Position { get; }
    public Vector3 ForcePosition { get; }
    public float ForcePower { get; }
    public Transform RandomTransform { get; }
    public Transform Transform { get; set; }
    public bool IsDead { get; set; }
    public bool IsNotDamageable { get; set; }

    private void Awake()
    {
        Bounds = bounds;
        Health = maxHealth;
        Transform = transform;
        mobHealthManager?.SetHealthText(Health, maxHealth);
        mobHealthManager?.gameObject.SetActive(true);
    }

    public virtual void TakeDamage(DamageInfo damageInfo)
    {
        Health -= damageInfo.Amount;
        var particleRandomPosition = transform.position;
        if(Health <= 0)
        {
            CreateDestroyParticle(particleRandomPosition);
            IsDead = true;
            Died?.Invoke(damageInfo.Source);
        }
        else
        {
            if(hitParticle != null && hitParticle.IsValid())
                CreateHitParticle(particleRandomPosition);
        }
        mobHealthManager?.OnHealthChanged(Health / maxHealth);
        mobHealthManager?.SetHealthText(Health, maxHealth);
        UpdateDamageType();
    }
    private void UpdateDamageType()
    {
        if (damageTypes.Length == 0) return;
        var stageCount = damageTypes.Length;
        var healthPerStage = maxHealth / stageCount;
        for (var i = 0; i < stageCount; i++) 
            damageTypes[i].SetActive(Health > i * healthPerStage && Health <= (i + 1) * healthPerStage);
    }

    public void TakeDOT(DamageInfo damageInfo, float duration)
    {
        
    }

    public void TakeDamageFromVehicle(Vector3 carPosition, float collisionForce)
    {
    }
    public async void CreateHitParticle(Vector3 targetPosition)
    {
        if(string.IsNullOrEmpty(hitParticle.AssetGUID)) return;
        var particle = await ObjectManager.GetObject(hitParticle, targetPosition, Quaternion.identity);
    }
    public async void CreateDestroyParticle(Vector3 targetPosition)
    {
        if(string.IsNullOrEmpty(deathParticle.AssetGUID)) return;
        await ObjectManager.GetObject(deathParticle, targetPosition, Quaternion.identity);
    }

    public event Action<DamageSource> Died;
    public void OnLoseFocus()
    {
        
    }
}
