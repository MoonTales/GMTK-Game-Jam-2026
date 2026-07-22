

//—————— Includes ——————//

//——————————————————————//

namespace System
{
    /// <summary>
    /// a static class used to hold all of the types used throughout the project:
    /// including but not limted to: scruts, enums, and other type definitions.
    /// </summary>
    public static class Types
    {
        /* —————————————————————— System Types —————————————————————— */
        [Serializable]
        public enum GameState
        {
            MainMenu, // Used for when we are in / at the main menu
            Gameplay, // Used for standard gameplay
            Paused, // Used for when the game is paused
            GameOver, // Used to mark when the player has lost the game
            Victory, // Used to mark when the player has won the game
            Cutscene, // Used to mark when the player is within a cutscene
            Initializing, // Used to mark when the player, world, or game is initializing

        }
    }
}