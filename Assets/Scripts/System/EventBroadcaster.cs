//————————————————————————————————————————————————————————————————
// The following code is written and maintained by MoonTales Studio,
// under the creative direction of Cohen Calvert. 
// You are not allowed to use, alter, modify, or re-distribute this
// code without explicit permission from MoonTales Studio.
//————————————————————————————————————————————————————————————————

//—————— Includes ——————//
using UnityEngine;
//——————————————————————//

namespace System
{
    /// <summary>
    /// A static utility class for broadcasting and handling activity-related events.
    /// Provides functionality for notifying subscribers when an activity starts or completes.
    /// Subscribers can listen for these events and respond accordingly.
    /// 
    /// Template for how to set up a new event
    /// 
    ///  public delegate void EventNameHandler(ParameterType parameter);
    ///  public static event EventNameHandler EventName;
    ///  public static void Broadcast_EventName(ParameterType parameter) { EventName?.Invoke(parameter); }
    /// 
    /// 
    ///  in the class that is going to SUBSCRIBE to the event, do the following:
    ///  public void start(){
    ///     EventBroadcaster.EventName += YourClassMethod;
    ///  }
    /// 
    ///  in the class that is going to BROADCAST the event, do the following:
    ///  EventBroadcaster.Broadcast_EventName(parameter);
    /// 
    ///
    /// Created by: MoonTales
    /// </summary>
    public static class EventBroadcaster
    {

        /* Template for how to setup a new event
         *
         * public delegate void EventNameHandler(ParameterType parameter);
         * public static event EventNameHandler EventName;
         * public static void Broadcast_EventName(ParameterType parameter) { EventName?.Invoke(parameter); }
         *
         *
         * in the class that is going to subscribe to the event, do the following:
         * public void start(){
         *    EventBroadcaster.EventName += YourClassMethod;
         * }
         *
         * in the class that is going to broadcast the event, do the following:
         * EventBroadcaster.Broadcast_EventName(parameter);
         */

        // These are just for show, they will be removed soon

        
        /// <summary>
        /// Global Project level broadcasts.
        ///
        /// These are broadcast intended to be used as major gameplay markings, which
        /// are automatically connected to each and ever class which inherits from the
        /// EventSubscriber Base.
        ///
        ///
        /// OnGameStarted -> Called when the game originally starts (not the main menu. actual gameplay)
        /// 
        /// OnGameInitialized -> Called after the game has been initialized
        ///     Controlled by a game manager of some sort. should be called after all
        ///     Systems have been "Started" such as stats, generation, player setup, etc.
        ///
        /// OnGameRestarted -> Called when the game is restarted (after a game over)
        ///
        /// OnGameStateChanged -> Called when the game state changes, passes the new game state as a parameter
        ///     Such as from "initialization" -> "gameplay" or "gameplay" -> "game over"
        /// 
        /// </summary>
        
        
        public delegate void GameStartedHandler();
        public static event GameStartedHandler OnGameStarted;
        public static void Broadcast_GameStarted() { OnGameStarted?.Invoke();}

        public delegate void GameInitializedHandler();
        public static event GameInitializedHandler OnGameInitialized;
        public static void Broadcast_GameInitialized() {OnGameInitialized?.Invoke();}
        
        public delegate void GameRestartedHandler();
        public static event GameRestartedHandler OnGameRestarted;
        public static void Broadcast_GameRestarted() {OnGameRestarted?.Invoke();}

        public delegate void GameStateChangedHandler(Types.GameState newState);
        public static event GameStateChangedHandler OnGameStateChanged;
        public static void Broadcast_GameStateChanged(Types.GameState newState) { OnGameStateChanged?.Invoke(newState); }
        //---------------------------------------------------------------------------------//\
    }
}
