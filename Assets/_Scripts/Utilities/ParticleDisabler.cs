using UnityEngine;

public class ParticleDisabler : MonoBehaviour
{
    [SerializeField]private ParticleSystem particleSystem;
    private void Update()
    {
        if (!particleSystem.IsAlive())
        {
            gameObject.SetActive(false);
        }
    }
}
