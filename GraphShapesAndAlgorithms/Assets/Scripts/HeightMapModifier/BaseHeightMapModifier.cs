using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHeightMapModifier : MonoBehaviour
{
    [SerializeField] [Range(0, 1)] protected float Strength = 1f;
}
