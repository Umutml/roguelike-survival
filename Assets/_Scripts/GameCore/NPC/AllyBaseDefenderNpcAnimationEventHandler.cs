using _Scripts.GameCore.NPC;
using UnityEngine;

public class AllyBaseDefenderNpcAnimationEventHandler : MonoBehaviour
{
    [SerializeField] private AllyBaseDefenderAnimationModule _allyBaseDefenderAnimationModule;
    
    public void FireEvent()
    {
        _allyBaseDefenderAnimationModule.FireEvent();    
    }
    
    public void ReloadComplete()
    {
        _allyBaseDefenderAnimationModule.ReloadComplete();
    }
}
