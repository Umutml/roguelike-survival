using GameCore.Spawner;
using UnityEngine;
using VContainer;

namespace GameCore.Misc
{
    public class MobSpawnerTrigger : MonoBehaviour
    {
        [SerializeField] private bool spawnerToggle;
        
        private MobManager _mobManager;
        private bool _isTriggeredOnce;

        [Inject]
        private void Construct(MobManager mobManager)
        {
            _mobManager = mobManager;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!_isTriggeredOnce && other.CompareTag("Player"))
            {
                _isTriggeredOnce = true;
                _mobManager.IsLocked = !spawnerToggle;
            }
        }
    }
}
