using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "Objective Object", menuName = "Objective/Objective Object")]
public class ObjectiveScriptable : ScriptableObject
{
   public int timeCondition;
   public int mobCountCondition;
}
