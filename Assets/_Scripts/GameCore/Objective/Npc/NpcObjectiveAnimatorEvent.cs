using _Scripts.GameCore.NPC;
using UnityEngine;

public class NpcObjectiveAnimatorEvent : MonoBehaviour
{
    [SerializeField] private NpcObjectiveAnimator npcObjectiveAnimator;
    
    public void FireEvent()
    {
        npcObjectiveAnimator.FireEvent();    
    }
    
    public void ReloadComplete()
    {
        npcObjectiveAnimator.ReloadComplete();
    }
}
