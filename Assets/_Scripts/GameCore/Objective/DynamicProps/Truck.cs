using System;
using Cathei.LinqGen;
using DG.Tweening;
using GameCore.AI;
using GameCore.Health;
using GameCore.Spawner;
using UnityEngine;
using static ObjectiveStructure;

public class Truck : ObjectiveDamageable
{
    [SerializeField] private Animation _animation;
    [SerializeField] private NpcObjectiveSpeechBubble npcSpeechBuble;
    [SerializeField] private Transform hitPoint,truckParent;
    private Vector3 _previousPosition;
    private MobManager _mobManager;
    private float speed;

    private void Start()
    {
        _mobManager = FindObjectOfType<MobManager>();
    }

    private void Update()
    {
        if (Time.deltaTime == 0)
            return;
        if(speed>0.1f)
            DetectAndHandleNearbyZombies();
        if (Time.frameCount % 30 != 0)
            return;
        speed = (Vector3.Distance(_previousPosition, transform.position) / Time.deltaTime) * 0.005f;
        _animation["Truck"].speed = Mathf.Clamp(speed, 0, 5);
        _previousPosition = transform.position;
    }
    private void DetectAndHandleNearbyZombies()
    {
        if (!_mobManager) return;
        foreach (var mob in _mobManager.ActiveMobs.Gen().Where(IsMobValid))
        {
            if (IsWithinRadius(mob.Position))
            {
                TakeDamage(new DamageInfo(0.25f));
                mob.TakeDamageFromVehicle(hitPoint.position,speed * 25f);
            }
        }
    }
    private bool IsWithinRadius(Vector3 position) => Vector3.Distance(position, hitPoint.position) < 3;
    private bool IsMobValid(IDamageable mob) => mob is {IsDead: false};
    public void StartAnimation()
    {
        _animation.Play();
        Health = maxHealth;
        transform.tag = "Truck";
    }
    public void StopAnimation()
    {
        mobHealthManager.gameObject.SetActive(false);
        truckParent.DOKill();
        transform.tag = "Untagged";
        _animation.Stop();
    }
    public override void TakeDamage(DamageInfo damageInfo)
    {
        base.TakeDamage(damageInfo);
    }
    public void ShowDialog(FunctionParameter objectFunction)
    {
        if (Health < 10)
            return;
        npcSpeechBuble?.ExecuteShowSpeechBubble(objectFunction.GetParameter<string>());
    }

}
