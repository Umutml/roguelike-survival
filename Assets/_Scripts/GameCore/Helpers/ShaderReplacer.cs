using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameCore.Helpers
{
    [Serializable]
    public class ShaderPropertyOverride
    {
        #region Serializable Fields

        public string propertyName;
        public ShaderPropertyType propertyType;

        public Color colorValue;
        public Vector4 vectorValue;
        public float floatValue;
        public Texture textureValue;

        #endregion

        #region Public Methods

        public object GetValue()
        {
            switch (propertyType)
            {
                case ShaderPropertyType.Color: return colorValue;
                case ShaderPropertyType.Vector: return vectorValue;
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range: return floatValue;
                case ShaderPropertyType.Texture: return textureValue;
                default: return null;
            }
        }

        #endregion
    }

    public class ShaderReplacer : MonoBehaviour
    {
        #region Serializable Fields

        [SerializeField] private Shader replacementShader;
        [SerializeField] private List<ShaderPropertyOverride> propertyOverrides = new List<ShaderPropertyOverride>();
        [SerializeField] private bool replaceOnStart = false;

        #endregion

        #region Fields

        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        private Dictionary<Renderer, MaterialPropertyBlock> propertyBlocks = new Dictionary<Renderer, MaterialPropertyBlock>();
        private bool _isReplaced;

        #endregion

        #region Unity Methods
        
        private void Start()
        {
            if (replaceOnStart)
            {
                ReplaceShaders();
            }
        }

        private void OnDestroy()
        {
            RevertShaders();
        }

        #endregion

        #region Public Methods

        public void ReplaceShaders()
        {
            if (_isReplaced) return;
            if (replacementShader == null)
            {
                Debug.LogWarning("No replacement shader assigned!", this);
                return;
            }

            // Get both MeshRenderers and SkinnedMeshRenderers
            var meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
            var skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);

            if (meshRenderers.Length == 0 && skinnedMeshRenderers.Length == 0)
            {
                Debug.LogWarning("No Renderers found on this GameObject or its children!", this);
                return;
            }

            // Process MeshRenderers
            foreach (var renderer in meshRenderers)
            {
                ProcessRenderer(renderer);
            }

            // Process SkinnedMeshRenderers
            foreach (var renderer in skinnedMeshRenderers)
            {
                ProcessRenderer(renderer);
            }

            _isReplaced = true;
        }

        public void RevertShaders()
        {
            if (!_isReplaced) return;
            foreach (var kvp in originalMaterials)
            {
                Renderer renderer = kvp.Key;
                if (renderer != null)
                {
                    renderer.sharedMaterials = kvp.Value;

                    if (propertyBlocks.TryGetValue(renderer, out MaterialPropertyBlock propertyBlock))
                    {
                        propertyBlock.Clear();
                        renderer.SetPropertyBlock(propertyBlock);
                    }
                }
            }

            originalMaterials.Clear();
            propertyBlocks.Clear();
            _isReplaced = false;
        }

        #endregion

        #region Private Methods

        private void ProcessRenderer(Renderer renderer)
        {
            if (!originalMaterials.ContainsKey(renderer))
            {
                originalMaterials[renderer] = renderer.sharedMaterials;
            }

            Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                Material originalMaterial = renderer.sharedMaterials[i];
                Material newMaterial = new Material(originalMaterial);
                newMaterial.shader = replacementShader;
                newMaterials[i] = newMaterial;
            }

            renderer.sharedMaterials = newMaterials;

            MaterialPropertyBlock propertyBlock = null;
            if (!propertyBlocks.TryGetValue(renderer, out propertyBlock))
            {
                propertyBlock = new MaterialPropertyBlock();
                propertyBlocks[renderer] = propertyBlock;
            }

            ApplyPropertyOverrides(propertyBlock);
            renderer.SetPropertyBlock(propertyBlock);
        }

        private void CopyMaterialProperties(Material material, MaterialPropertyBlock propertyBlock)
        {
            Shader shader = material.shader;
            int propertyCount = shader.GetPropertyCount();

            for (int propIdx = 0; propIdx < propertyCount; propIdx++)
            {
                string propertyName = shader.GetPropertyName(propIdx);
                ShaderPropertyType propertyType = shader.GetPropertyType(propIdx);

                switch (propertyType)
                {
                    case ShaderPropertyType.Color:
                        propertyBlock.SetColor(propertyName, material.GetColor(propertyName));
                        break;
                    case ShaderPropertyType.Vector:
                        propertyBlock.SetVector(propertyName, material.GetVector(propertyName));
                        break;
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        propertyBlock.SetFloat(propertyName, material.GetFloat(propertyName));
                        break;
                    case ShaderPropertyType.Texture:
                        propertyBlock.SetTexture(propertyName, material.GetTexture(propertyName));
                        break;
                }
            }
        }

        private void ApplyPropertyOverrides(MaterialPropertyBlock propertyBlock)
        {
            foreach (var property in propertyOverrides)
            {
                if (string.IsNullOrEmpty(property.propertyName)) continue;

                switch (property.propertyType)
                {
                    case ShaderPropertyType.Color:
                        propertyBlock.SetColor(property.propertyName, property.colorValue);
                        break;
                    case ShaderPropertyType.Vector:
                        propertyBlock.SetVector(property.propertyName, property.vectorValue);
                        break;
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        propertyBlock.SetFloat(property.propertyName, property.floatValue);
                        break;
                    case ShaderPropertyType.Texture:
                        if (property.textureValue != null)
                        {
                            propertyBlock.SetTexture(property.propertyName, property.textureValue);
                        }

                        break;
                }
            }
        }

        #endregion
    }
}
