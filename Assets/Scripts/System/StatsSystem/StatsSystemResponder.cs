//————————————————————————————————————————————————————————————————
// The following code is written and maintained by MoonTales Studio,
// under the creative direction of Cohen Calvert. 
// You are not allowed to use, alter, modify, or re-distribute this
// code without explicit permission from MoonTales Studio.
//————————————————————————————————————————————————————————————————

//—————— Includes ——————//

using System;
using UnityEngine;
//——————————————————————//

/// <summary>
/// This is the global class that is designed to respond and handle all events related to the stats system.
/// </summary>

public class StatsSystemResponder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    
        StatsSystem.OnPrimaryStatChanged += HandlePrimaryStatChanged;
        StatsSystem.OnSubStatChanged += HandleSecondaryStatChanged;
    }

    // we can setup "rules" here for some stat connections, for example, never allowing current health to exceed max health, or something like that.
    private void HandlePrimaryStatChanged(GameObject entity, PrimaryStatType stattype, float oldvalue, float newvalue)
    {
        // This function will know whenever ANY primary stat in the entire game changes
        DebugUtils.Log($"Primary stat changed! Entity: {entity.name}, Stat: {stattype}, Old Value: {oldvalue}, New Value: {newvalue}", "green");
    }
    
    private void HandleSecondaryStatChanged(GameObject entity, SubStatType stattype, float oldvalue, float newvalue)
    {
        // This function will know whenever ANY secondary stat in the entire game changes
        DebugUtils.Log($"Secondary stat changed! Entity: {entity.name}, Stat: {stattype}, Old Value: {oldvalue}, New Value: {newvalue}", "green");
    }
}

