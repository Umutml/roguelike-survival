using System;

namespace Interfaces
{
    public interface IEnergyService
    {
        event Action<int> OnEnergyChanged;
        
        public int MaxEnergy { get; }
        
        public int CurrentEnergy { get; }
        
        bool ConsumeEnergy(int energy);
        
        void GiveEnergy(int energy);
        
        TimeSpan TimeLeftToNextEnergy { get; }
    }
}
