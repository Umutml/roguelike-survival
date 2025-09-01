using System;
using System.Globalization;
using _Scripts.Utilities;
using _Utilities;
using Interfaces;
using Managers.Scriptables;
using UnityEngine;

namespace Managers
{
    public class EnergyManager : MonoBehaviour, IEnergyService
    {
        #region Serializable Fields

        [SerializeField] private EnergySettings settings;

        #endregion

        #region Fields

        private int _currentEnergy;
        private DateTime _lastEnergyTickTime;
        
        public event Action<int> EnergyGiven; // Define the EnergyGiven event

        #endregion

        #region Properties

        protected string EnergyValue
        {
            get => PlayerPrefs.GetString("EV", String.Empty); //EV: energy value
            set => PlayerPrefs.SetString("EV", value);
        }

        protected string SavedTime
        {
            get => PlayerPrefs.GetString("OT", String.Empty); //OT: last check time
            set => PlayerPrefs.SetString("OT", value);
        }

        public DateTime TimeToNextEnergy => _lastEnergyTickTime.AddSeconds(settings.EnergyRecoveryTimeSeconds);

        #endregion

        #region Unity Methods

        private void Start()
        {
            LoadAndCalculateEnergy();
        }

        private void Update()
        {
            if (CurrentEnergy < MaxEnergy && UnbiasedTime.Instance.Now() >= TimeToNextEnergy)
            {
                GiveEnergy(1);
                _lastEnergyTickTime = UnbiasedTime.Instance.Now();
                SaveCurrentState();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveCurrentState();
            }
            else
            {
                LoadAndCalculateEnergy();
            }
        }

        private void OnApplicationQuit()
        {
            SaveCurrentState();
        }

        #endregion

        #region Private Methods

        private void LoadAndCalculateEnergy()
        {
            if (SavedTime == String.Empty || EnergyValue == String.Empty)
            {
                Initialize();
                return;
            }

            var lastCheckTime = DateTime.Parse(DecryptString(SavedTime), null, DateTimeStyles.RoundtripKind);
            CurrentEnergy = int.Parse(DecryptString(EnergyValue));

            var currentTime = UnbiasedTime.Instance.Now();
            var timeDifference = currentTime - lastCheckTime;

            int energyToAdd = (int) (timeDifference.TotalSeconds / settings.EnergyRecoveryTimeSeconds);

            energyToAdd = Mathf.Min(energyToAdd, MaxEnergy - CurrentEnergy);

            if (energyToAdd > 0)
            {
                GiveEnergy(energyToAdd);
                var remainingSeconds = timeDifference.TotalSeconds % settings.EnergyRecoveryTimeSeconds;
                _lastEnergyTickTime = currentTime.AddSeconds(-remainingSeconds);
            }
            else
            {
                _lastEnergyTickTime = lastCheckTime;
            }

            SaveCurrentState();
        }

        private string EncryptString(string stringToEncrypt)
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
            return EncryptionHelper.Encrypt(stringToEncrypt);
#else
            return stringToEncrypt;
#endif
        }

        private string DecryptString(string stringToDecrypt)
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
            return EncryptionHelper.Decrypt(stringToDecrypt);
#else
            return stringToDecrypt;
#endif
        }

        private void Initialize()
        {
            CurrentEnergy = MaxEnergy;
            _lastEnergyTickTime = UnbiasedTime.Instance.Now();
            SaveCurrentState();
        }

        private void SaveCurrentState()
        {
            var currentTime = UnbiasedTime.Instance.Now().ToString("o");
            SavedTime = EncryptString(currentTime);
            SaveEnergyValue();
        }

        private void SaveEnergyValue()
        {
            EnergyValue = EncryptString(CurrentEnergy.ToString());
        }

        #endregion

        #region IEnergyService Members

        public event Action<int> OnEnergyChanged;
        public int MaxEnergy => settings.MaxEnergy;

        public int CurrentEnergy
        {
            get => _currentEnergy;
            set
            {
                _currentEnergy = value;
                OnEnergyChanged?.Invoke(_currentEnergy);
                SaveEnergyValue();
                LoggerNS.Log($"Current energy: {_currentEnergy}");
            }
        }

        public bool ConsumeEnergy(int energy)
        {
            energy = Math.Abs(energy);
            if (_currentEnergy >= energy)
            {
                CurrentEnergy -= energy;
                SaveCurrentState();
                return true;
            }

            return false;
        }

        public void GiveEnergy(int energy)
        {
            CurrentEnergy += energy;
            EnergyGiven?.Invoke(energy); // Invoke the EnergyGiven event
        }

        public TimeSpan TimeLeftToNextEnergy => TimeToNextEnergy - UnbiasedTime.Instance.Now();

        #endregion
    }
}
