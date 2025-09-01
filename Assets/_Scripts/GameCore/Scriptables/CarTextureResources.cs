using UnityEngine;
using System.Collections.Generic;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "CarTextureResources", menuName = "Scriptables/CarTextureResources", order = 1)]
    public class CarTextureResources : ScriptableObject
    {
        [SerializeField] private List<Texture> carTextureList = new();
        
        public List<Texture> CarTextureList => carTextureList;
    }
}

