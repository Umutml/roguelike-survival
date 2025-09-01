using System.Collections.Generic;
using UnityEngine;

public class CarModelParts : MonoBehaviour
{
    [SerializeField] private List<GameObject> carParts;
    [SerializeField] private ParticleSystem upgradeParticle;

    
    public void SetCarPartsActive(int level)
    {
        for (var i = 0; i < level; i++)
        {
            carParts[i].SetActive(true);
            upgradeParticle.Play();
        }
    }
}
