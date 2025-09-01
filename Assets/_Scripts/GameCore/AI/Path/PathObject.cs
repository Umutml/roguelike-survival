using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PatrolRoute
{
    public List<Transform> points;
}

public class PatrolManager : MonoBehaviour
{
    public List<PatrolRoute> patrolRoutes;
}
