using System.Collections;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;
using VContainer;

public class EngineSound : MonoBehaviour
{
    [Header("Car Engine Sounds")]
    [SerializeField] private AudioSource carStartingSound;
    [SerializeField] private AudioSource carIdleSound;
    [SerializeField] private AudioSource carRunningSound;
    [SerializeField] private AudioSource carDriftingSound;
    [SerializeField] private AudioSource carReverseSound;
    [SerializeField] private bool isEngineRunning = false;

    [SerializeField] private GameObject audioSourcesParent;

    
    
    private CarController carController;
    private CarMovementController carMovementController;
    private IObjectResolver _resolver;
    private AudioManager _audioManager;
    private bool isSoundsOn = false;
    private bool isEngineInit = false;
    
    // Audio settings
    private float minVolume = 0.1f;
    private float maxVolume = 0.3f;
    private const float THROTTLE_THRESHOLD = 0.5f;
    
    // Gear system settings
    private const int TOTAL_GEARS = 5;
    private const float GEAR_SHIFT_DURATION = 0.35f;
    private const float BASE_GEAR_DURATION = 3.0f;
    private const float GEAR_DURATION_INCREMENT = 0.5f;
    private const float PITCH_DROPOFF = 0.1f;
    private const float GEAR_SHIFT_ENGINE_SOUND_FADEOUT_DURATION = 0.1f;
    
    private int currentGear = 1;
    private float gearTimer = 0f;
    private float gearShiftTimer = 0f;
    private bool isGearShifting = false;
    private bool isMaxGearReached = false;

    private void Awake()
    {
        carController = GetComponent<CarController>();
        carController.EngineStatusChanged += EngineStatusChanged;
        carMovementController = carController.CarMovementController;
    }

    public void Init()
    {
        _resolver = carController.Resolver;
        _audioManager = _resolver.Resolve<AudioManager>();
        _audioManager.OnSoundChanged += OnSoundChanged;
        isSoundsOn = _audioManager.IsSoundsOn;
        isEngineInit = true;
    }

    private void OnDisable()
    {
        carController.EngineStatusChanged -= EngineStatusChanged;
    }

    void Update()
    {
        UpdateEngineSounds();
    }
    
    private float GetGearDuration(int gear)
    {
        return BASE_GEAR_DURATION + (gear - 1) * GEAR_DURATION_INCREMENT;
    }
    
    private float GetStartingPitch(int gear)
    {
        return Mathf.Max(0.5f, -(gear - 1) * PITCH_DROPOFF);
    }
    
    private float CalculateGearPitch()
    {
        float gearDuration = GetGearDuration(currentGear);
        float startPitch = GetStartingPitch(currentGear);
        return Mathf.Lerp(startPitch, 1f, gearTimer / gearDuration);
    }
    
    private void OnSoundChanged(bool newSoundStatus)
    {
        isSoundsOn = newSoundStatus;
        if (isEngineRunning)
        {
            audioSourcesParent.SetActive(isSoundsOn);
        }
    }

    private void EngineStatusChanged(bool isEngineOn)
    {
        isEngineRunning = isEngineOn;
        if (isEngineOn)
        {
            audioSourcesParent.SetActive(isSoundsOn);
            StartCoroutine(StartEngine());
        }
        else
        {
            audioSourcesParent.SetActive(false);
            carIdleSound.volume = 0;
            carRunningSound.volume = 0;
            carReverseSound.volume = 0;
            ResetGearSystem();
        }
    }
    
    private void ResetGearSystem()
    {
        currentGear = 1;
        gearTimer = 0f;
        gearShiftTimer = 0f;
        isGearShifting = false;
        isMaxGearReached = false;
        carRunningSound.pitch = 0.5f;
    }

    private IEnumerator StartEngine()
    {
        carStartingSound.Play();
        yield return new WaitForSeconds(0.6f);
        isEngineRunning = true;
        carIdleSound.Play();
        ResetGearSystem();
    }

    private void UpdateEngineSounds()
    {
        if (!isEngineInit || !isSoundsOn || !isEngineRunning)
            return;
        
        float carSpeed = carMovementController.GetSpeed();
        float speedRatio = carMovementController.GetSpeedRatio();
        float throttleMagnitude = carMovementController.GetThrottleMagnitude();
        bool isGoingReverse = carMovementController.IsGoingReverse();
        
        if (carMovementController.IsMoveBlocked)
        {
            if (!carReverseSound.isPlaying)
            {
                StopAllSoundsExcept(carReverseSound);
                carReverseSound.Play();
            }
            ResetGearSystem();
            return;
        }

        if (isGoingReverse)
        {
            if (!carReverseSound.isPlaying)
            {
                StopAllSoundsExcept(carReverseSound);
                carReverseSound.Play();
            }
            ResetGearSystem();
            return;
        }
        else
        {
            carReverseSound.Stop();
        }

        // Handle low speed idle transition
        if (carSpeed < 0.2f)
        {
            StopAllSoundsExcept(carIdleSound);
            if (!carIdleSound.isPlaying)
            {
                carIdleSound.Play();
            }
            ResetGearSystem();
            return;
        }
        
        if (carMovementController.IsDrive)
        {
            // Handle drifting sound
            if (carMovementController.IsDrifting && carSpeed > 5f)
            {
                if (!carDriftingSound.isPlaying)
                {
                    carDriftingSound.Play();
                }
            }
            else
            {
                carDriftingSound.Stop();
            }
            
            // Handle running sound and gear system
            if (!carRunningSound.isPlaying && !isGearShifting)
            {
                StopAllSoundsExcept(carRunningSound);
                carRunningSound.Play();
            }
            
            if (Mathf.Abs(throttleMagnitude) > THROTTLE_THRESHOLD)
            {
                if (!isGearShifting && !isMaxGearReached)
                {
                    float gearDuration = GetGearDuration(currentGear);
                    gearTimer = Mathf.Min(gearTimer + Time.deltaTime, gearDuration);
                    
                    // Check if we've reached max pitch for current gear
                    if (gearTimer >= gearDuration)
                    {
                        if (currentGear < TOTAL_GEARS)
                        {
                            isGearShifting = true;
                            gearShiftTimer = 0f;
                            CarGearshiftEngineFadeout();
                            carIdleSound.Play();
                        }
                        else
                        {
                            isMaxGearReached = true;
                        }
                    }
                }
                else if (isGearShifting)
                {
                    gearShiftTimer += Time.deltaTime;
                    if (gearShiftTimer >= GEAR_SHIFT_DURATION)
                    {
                        isGearShifting = false;
                        currentGear++;
                        gearTimer = 0f;
                        carIdleSound.Stop();
                        carRunningSound.volume = 1f;
                    }
                }
                
                if (!isGearShifting)
                {
                    carRunningSound.pitch = isMaxGearReached ? 1f : CalculateGearPitch();
                    carRunningSound.volume = Mathf.Lerp(minVolume, maxVolume, speedRatio);
                }
            }
            else if (Mathf.Abs(throttleMagnitude) < 0.1f)
            {
                StopAllSoundsExcept(carIdleSound);
                if (!carIdleSound.isPlaying)
                {
                    carIdleSound.Play();
                }
                ResetGearSystem();
            }
        }
        else
        {
            StopAllSoundsExcept(carIdleSound);
            if (!carIdleSound.isPlaying)
            {
                carIdleSound.Play();
            }
            ResetGearSystem();
        }
    }
    
private async UniTask CarGearshiftEngineFadeout()
{
    float startVolume = carRunningSound.volume;
    float elapsedTime = 0f;

    while (elapsedTime < GEAR_SHIFT_ENGINE_SOUND_FADEOUT_DURATION)
    {
        elapsedTime += Time.deltaTime;
        carRunningSound.volume = Mathf.Lerp(startVolume, 0.01f, elapsedTime / GEAR_SHIFT_ENGINE_SOUND_FADEOUT_DURATION);
        await UniTask.Yield();
    }

    carRunningSound.volume = 0f;
}

    private void StopAllSoundsExcept(AudioSource exception)
    {
        if (carIdleSound != exception) carIdleSound.Stop();
        if (carRunningSound != exception) carRunningSound.Stop();
        if (carDriftingSound != exception) carDriftingSound.Stop();
        if (carReverseSound != exception) carReverseSound.Stop();
    }
}
