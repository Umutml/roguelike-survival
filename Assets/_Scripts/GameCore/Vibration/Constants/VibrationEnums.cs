namespace _Scripts.GameCore.Vibration.Constants
{
    public struct VibrationEnums
    {
        public enum VibrationReleaseType
        {
            Both,
            Internal,
            Live
        }


        public enum VibrationEventType
        {
            HitZombie,
            HitPlayer,
            HitCar,
            Drift,
            Refill,
            ButtonUI,
            Npc,
        }
    }
}
