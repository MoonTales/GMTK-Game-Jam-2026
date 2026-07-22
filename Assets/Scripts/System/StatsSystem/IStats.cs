using UnityEngine;

/// <summary>
/// Interface for anything that has stats within the game
/// </summary>
public interface IStats
{
    
    
    // ————— Required Override Variables ————— //
    // Whether or not the stats of this entity should be saved to a save file
    bool BShouldSaveStats { get; } 
    
    // ————— Required Override functions ————— //
    // This is called on Awake() to initialize the stats of the entity 
    void InitializeStats();

    // This is called when an entity should "take damage". its up to the class to determine what this means
    void TakeDamage(float value);

}
