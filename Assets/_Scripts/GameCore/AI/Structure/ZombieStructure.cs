using UnityEngine;

public class ZombieStructure
{
    public enum MobLOD
    {
        Low,
        High
    }
    public enum WalkMode { Walk, Crawl };
    public enum ZombieState
    {
        Idle,
        Run,
        Attack,
        Dead,
        Crashed
    }
}
