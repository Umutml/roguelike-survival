using Cysharp.Threading.Tasks;

namespace Interfaces
{
    public interface IMobSpawnService
    {
        void SpawnMobs();

        void ClearActiveMobs();
        
        void SpawnRandomWithCount(int count);
        
        bool EnableDebugMode { get; set; }
        
        bool FreeRoamEnabled { get; set; }
        
        bool RingSystemEnabled { get; set; }
    }
}
