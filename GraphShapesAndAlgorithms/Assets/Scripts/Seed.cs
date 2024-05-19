using UnityEngine;

public static class Seed
{
    public static int InitializeRandom(string seed)
    {
        //GameSeed = CreateRandomSeed(16);
        int CurrentSeed = seed.GetHashCode(); // Creates an integer seed based on the GameSeed string
        Random.InitState(CurrentSeed); // Initializes Random to use the seed, this means results using Random will be reproducable via a seed
        
        return CurrentSeed;
    }
    public static int InitializeRandom(int length = 16)
    {
        int CurrentSeed = CreateRandomSeed(length).GetHashCode(); // Creates an integer seed based on the GameSeed string
        Random.InitState(CurrentSeed); // Initializes Random to use the seed, this means results using Random will be reproducable via a seed
        
        return CurrentSeed;
    }
    private const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_!?";
    public static string CreateRandomSeed(int length)
    {
        string generated_string = "";

        for(int i = 0; i < length; i++)
            generated_string += characters[Random.Range(0, characters.Length)];

        return generated_string;
    }
}
