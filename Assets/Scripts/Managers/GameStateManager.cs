using System;
using System.Collections;
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
        
        
        
        // ————— Score Related Variable ————— //
        [SerializeField] private int currentScore = 0; public int GetCurrentScore() { return currentScore; } public void SetCurrentScore(int score) { currentScore = score; }
        
        
        // ————— Internal Variables ————— //
        private Types.GameState currentGameState = Types.GameState.MainMenu; public Types.GameState GetCurrentGameState() { return currentGameState; }
        private Types.GameState previousGameState = Types.GameState.MainMenu; public Types.GameState GetPreviousGameState() { return previousGameState; }
        private Coroutine _scoreCoroutine;
        // ————— External Variables ————— //
        public void Start()
        {
            // Initialize the game state
            currentGameState = defaultGameState;
            previousGameState = currentGameState;
            // for now, we will assume the game starts
            EventBroadcaster.Broadcast_GameStateChanged(currentGameState);
        }



        public void StartGameScore()
        {
            currentScore = 0;
    
            // Stop any existing coroutine first to avoid multiple overlapping loops
            if (_scoreCoroutine != null)
            {
                StopCoroutine(_scoreCoroutine);
            }
    
            _scoreCoroutine = StartCoroutine(StartGameScoreCoroutine());
        }
        
        public void StopGameScore()
        {
            if (_scoreCoroutine != null)
            {
                StopCoroutine(_scoreCoroutine);
                _scoreCoroutine = null;
            }
        }
        
        public void AddScore(int amount)
        {
            currentScore += amount;
        }
        
        private IEnumerator StartGameScoreCoroutine()
        {
            // Reuse the yield instruction to avoid garbage collector allocations
            var wait = new WaitForSeconds(1f);

            while (true)
            {
                yield return wait;
                currentScore += 5;
            }
        }
        
    }
}
