using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Seed : MonoBehaviour
{
    public string GameSeed = "default";
    public int CurrentSeed = 0;

    private void Awake() 
    {
        CurrentSeed = GameSeed.GetHashCode(); // Creates an integer seed based on the GameSeed string
        Random.InitState(CurrentSeed); // Initializes Random to use the seed, this means results using Random will be reproducable via a seed
    }
}
