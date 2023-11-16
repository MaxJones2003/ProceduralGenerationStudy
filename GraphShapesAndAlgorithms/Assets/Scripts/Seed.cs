using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Seed : MonoBehaviour
{
    private static Seed instance;
    public static Seed Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new Seed();
            }
            return instance;
        }
    }
    [SerializeField] public string GameSeed = "default";
    [HideInInspector]public int CurrentSeed = 0;
    public int InitializeRandom(string seed)
    {
        //GameSeed = CreateRandomSeed(16);
        CurrentSeed = seed.GetHashCode(); // Creates an integer seed based on the GameSeed string
        Debug.Log("Generated Seed: " + GameSeed + " | " + CurrentSeed);
        Random.InitState(CurrentSeed); // Initializes Random to use the seed, this means results using Random will be reproducable via a seed
        
        return CurrentSeed;
    }

    public string CreateRandomSeed(int length)
    {
        string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_!?";
        string generated_string = "";

        for(int i = 0; i < length; i++)
            generated_string += characters[Random.Range(0, characters.Length)];

        return generated_string;
    }
}
