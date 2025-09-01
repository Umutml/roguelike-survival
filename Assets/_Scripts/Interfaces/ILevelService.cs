using System;

namespace Interfaces
{
    public interface ILevelService
    {
        event Action WaveLevelFailed;
        event Action<int> WaveLevelChanged;
        event Action<float, float> WaveLevelSliderChanged;
    }
}
