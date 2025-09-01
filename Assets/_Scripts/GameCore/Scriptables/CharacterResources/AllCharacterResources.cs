using System.Collections.Generic;
using UnityEngine;


namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "AllCharacterResources", menuName = "ScriptableObjects/AllCharacterResources", order = 1)]
    public class AllCharacterResources : ScriptableObject
    {
        [SerializeField] private List<CharacterResources> characterResourcesList = new();

        public List<CharacterResources> CharacterResourcesList => characterResourcesList;

        public CharacterResources GetCharacter(Character character)
        {
            return characterResourcesList.Find(characterResources => characterResources.Character.Equals(character));
        }

        public CharacterResources GetCharacter(string modelAddressableKey)
        {
            return characterResourcesList.Find(characterResources => characterResources.CharacterModelAddressableKey.Equals(modelAddressableKey));
        }

    }
}


public enum Character
{
    Blaster,
    Hattori,
    Henry,
}

