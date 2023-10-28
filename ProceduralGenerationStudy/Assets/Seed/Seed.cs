using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Seed : MonoBehaviour
{
    private static Seed _instance;
    public static Seed Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Seed>();
                if (_instance == null)
                {
                    GameObject seedObject = new GameObject("Seed");
                    _instance = seedObject.AddComponent<Seed>();
                }
            }
            return _instance;
        }
    }
    public string GameSeed = "default";
    public int CurrentSeed = 0;

    public void InitializeSeed() 
    {
        CurrentSeed = GameSeed.GetHashCode(); // Creates an integer seed based on the GameSeed string
        Random.InitState(CurrentSeed); // Initializes Random to use the seed, this means results using Random will be reproducable via a seed
    }

    public void RandomizeSeed()
    {
        GameSeed = CreateRandomSeed(16);
    }

    private string CreateRandomSeed(int length)
    {
        string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_!?";
        string generated_string = "";

        for(int i = 0; i < length; i++)
            generated_string += characters[Random.Range(0, length)];

        return generated_string;
    }
}
