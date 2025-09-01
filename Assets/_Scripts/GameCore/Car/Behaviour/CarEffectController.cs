using System.Collections.Generic;
using GameCore.Car;
using GameCore.Scriptables;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using GameCore.Helpers;
using Random = UnityEngine.Random;


public class CarEffectController : MonoBehaviour
{
    #region Serializable Fields

    [SerializeField] private ShaderReplacer highlightShaderReplacer;
    [SerializeField] private CarTextureResources carTextureResources;
    [SerializeField] private Renderer[] carRenderers;
    [SerializeField] private GameObject[] doorCircles;
    [SerializeField] private ParticleSystem[] exhaustParticles;
    [SerializeField] private ParticleSystem fireParticle;
    [SerializeField] private ParticleSystem hitParticle;
    [SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private GameObject outlineArea;
    [SerializeField] private Transform carBody;
    [SerializeField] private bool randomTexture;
    [SerializeField] private ParticleSystem[] doorParticles;

    [SerializeField] private Color insideColor;
    [SerializeField] private Color outsideColor;

    #endregion


    #region Fields

    private MaterialPropertyBlock _materialPropertyBlock;
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    #endregion


    #region Unity Methods

    private void Start()
    {
        AssignRandomCarTexture();

        if (highlightShaderReplacer)
            highlightShaderReplacer.ReplaceShaders();
    }

    #endregion


    #region Public Methods

    public void ApplyBodyTilt(CarInputHandler input, Car car, Vector3 moveForce)
    {
        var liftAmount = Mathf.Lerp(0, car.LiftAmount, moveForce.magnitude / car.MaxSpeed);
        var liftRotation = Quaternion.Euler(-liftAmount, carBody.localRotation.y, carBody.localRotation.z);

        var tiltAmount = Mathf.Lerp(0, car.TiltAmount, Mathf.Abs(input.SteerInput));
        var tiltDirection = input.SteerInput > 0 ? -1f : 1f;
        var tiltRotation =
            Quaternion.Euler(carBody.localRotation.x, carBody.localRotation.y, tiltAmount * tiltDirection);

        if (input.MoveInput == 0 && moveForce.magnitude > 3.5f)
        {
            liftRotation = Quaternion.Euler(10f, carBody.localRotation.y, carBody.localRotation.z);
        }
        else if (input.MoveInput == 0 && moveForce.magnitude <= 3.5f)
        {
            carBody.localRotation = Quaternion.Slerp(carBody.localRotation, Quaternion.identity, Time.deltaTime * 2f);
            return;
        }

        carBody.localRotation =
            Quaternion.Slerp(carBody.localRotation, liftRotation * tiltRotation, Time.deltaTime * 5f);
    }


    public void CloseCarEffects()
    {
        foreach (var door in doorCircles)
        {
            door.gameObject.SetActive(false);
        }

        foreach (var exhaustParticle in exhaustParticles)
        {
            exhaustParticle.gameObject.SetActive(false);
        }
    }


    public void WheelEffects(List<Wheel> wheels, bool isDrifting)
    {
        foreach (var wheel in wheels)
        {
            wheel.TrailObject.emitting = isDrifting;
            wheel.DustParticle.Emit(isDrifting ? 1 : 0);
        }
    }


    public void SetEnableDoorCircles(bool isActive)
    {
        foreach (var circle in doorCircles)
        {
            circle.SetActive(isActive);
        }
    }


    public void SetColorDoorParticle(bool isInside)
    {
        foreach (var particle in doorParticles)
        {
            particle.startColor = isInside ? insideColor : outsideColor;
        }
    }


    public void PlayHitParticle(Vector3 position)
    {
        hitParticle.transform.position = position;
        if (hitParticle.isPlaying) return;
        hitParticle.Play();
    }


    public void AnimatedWheels(List<Wheel> wheels, float steerInput, float moveInput)
    {
        foreach (var wheel in wheels)
        {
            wheel.UpdateWheelPositionAndRotation(steerInput, moveInput);
        }
    }


    public void SetFireParticles(bool isActive)
    {
        if (isActive)
        {
            fireParticle.Play();
        }
        else
        {
            fireParticle.Stop();
        }
    }


    public void SetExhaustParticles(bool isActive)
    {
        foreach (var particle in exhaustParticles)
        {
            if (isActive)
            {
                particle.Play();
            }
            else
            {
                particle.Stop();
            }
        }
    }


    public void SetCarStatuEffects(bool isDead, bool isEnterCar)
    {
        SetExhaustParticles(!isDead && isEnterCar);
        SetOutlineArea(!isDead && !isEnterCar);
        SetEnableDoorCircles(!isDead && !isEnterCar);
    }


    public IEnumerator ExplosionCar()
    {
        yield return new WaitForSeconds(1.25f);
        explosionParticle.Play();
        transform.DOLocalJump(new Vector3(transform.position.x, transform.position.y, transform.position.z), 2, 1, 1.3f)
            .OnComplete(() => Destroy(gameObject, 5f));
    }

    #endregion


    #region Private Methods

    private void AssignRandomCarTexture()
    {
        if (randomTexture)
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
            _materialPropertyBlock.SetTexture(BaseMap,
                carTextureResources.CarTextureList[Random.Range(0, carTextureResources.CarTextureList.Count)]);

            foreach (var renderer in carRenderers)
            {
                renderer.SetPropertyBlock(_materialPropertyBlock);
            }
        }
    }


    private void SetOutlineArea(bool isActive)
    {
        //outlineArea.SetActive(isActive);
    }

    #endregion
}
