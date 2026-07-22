using System;
using UnityEngine;
using Types = System.Types;

namespace Managers
{
    
    /// <summary>
    /// Manager class to handle the state of the game. This includes things like:
    /// tba.
    /// </summary>
    public class GameStateManager : Singleton<GameStateManager>
    {
        
        // ————— Game Defaults ————— //
        [SerializeField] private Types.GameState defaultGameState = Types.GameState.MainMenu;
        
        
        // ————— Internal Variables ————— //
        private Types.GameState currentGameState = Types.GameState.MainMenu; public Types.GameState GetCurrentGameState() { return currentGameState; }
        private Types.GameState previousGameState = Types.GameState.MainMenu; public Types.GameState GetPreviousGameState() { return previousGameState; }
        
        // ————— External Variables ————— //
        
        
        public void Start()
        {
            // Initialize the game state
            currentGameState = defaultGameState;
            previousGameState = currentGameState;
            // for now, we will assume the game starts
            EventBroadcaster.Broadcast_GameStateChanged(currentGameState);
        }
        
    }
}
