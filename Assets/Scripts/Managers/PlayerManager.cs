using System;
using UnityEngine;
using Types = System.Types;

namespace Managers
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        //————— Internal Variables —————//
        [Header("Base Stats")]
        [SerializeField] private float baseVitality = 10f;
        [SerializeField] private float baseStrength = 10f;
        [SerializeField] private float baseEndurance = 10f;
        [SerializeField] private float baseIntelligence = 10f;
        [SerializeField] private float baseWillpower = 10f;
        [SerializeField] private float baseWisdom = 10f;
        //————— Internal Variables —————//
        private GameObject player;
        private void Start()
        {
            
            //————— Get access to the player —————//
            player = GameObject.FindGameObjectWithTag("Player");
            if (!player){DebugUtils.LogError("No player found within the game");}

            StatInitialization();
        }

        private void OnDeath()
        {
            // play death anim, drop loot, etc.
            DebugUtils.Log("Player has died!");
        }
        
        //————— Initialization Functions —————//
        private void StatInitialization()
        {
            var stats = StatsSystem.Instance;
            //——— Vitality Stats
            stats.Register(player, PrimaryStatType.Vitality, baseVitality, OnVitalityChanged);
            stats.RegisterSubStat(player, PrimaryStatType.Vitality, SubStatType.MaxHealth,   v => v * 10f);
            stats.RegisterSubStat(player, PrimaryStatType.Vitality, SubStatType.CurrentHealth, stats.GetStat(player, SubStatType.MaxHealth));
            stats.RegisterSubStat(player, PrimaryStatType.Vitality, SubStatType.HealthRegen, v => v * 0.25f);
            
            //——— Strength Stats
            stats.Register(player, PrimaryStatType.Strength, baseStrength, OnStrengthChanged);
            stats.RegisterSubStat(player, PrimaryStatType.Strength, SubStatType.MaxCarryWeight, s => s * 5f);
            
            //——— Endurance Stats
            stats.Register(player, PrimaryStatType.Endurance, baseEndurance, OnEnduranceChanged);
            stats.RegisterSubStat(player, PrimaryStatType.Endurance, SubStatType.Speed, e => 300 + (e * 10f));
            stats.RegisterSubStat(player, PrimaryStatType.Endurance, SubStatType.MaxStamina, e => e * 10f);
            stats.RegisterSubStat(player, PrimaryStatType.Endurance, SubStatType.CurrentStamina, stats.GetStat(player, SubStatType.MaxStamina));
            stats.RegisterSubStat(player, PrimaryStatType.Endurance, SubStatType.StaminaRegen, e => e * 0.5f);
            
            //stats.DebugPrintStatsForEntity(player);
        }
        
        private void OnVitalityChanged(PrimaryStatType stat, float oldValue, float newValue)
        {
            Debug.Log($"{stat} changed: {oldValue} -> {newValue}");
        }
        
        private void OnStrengthChanged(PrimaryStatType stat, float oldValue, float newValue)
        {
            Debug.Log($"{stat} changed: {oldValue} -> {newValue}");
        }
        
        private void OnEnduranceChanged(PrimaryStatType stat, float oldValue, float newValue)
        {
            Debug.Log($"{stat} changed: {oldValue} -> {newValue}");
        }
        
        
        //————— Public API functions —————//
        // Get the player game object
        public GameObject GetPlayer() { return player; }
        // Get the distance to the player from a given position
        public float GetDistanceToPlayer(Vector3 position)
        {
            if (player){ return Vector3.Distance(position, player.transform.position);}
            DebugUtils.LogError("No player found within the game");
            return float.MaxValue;
        }
        
        
        protected override void OnGameStateChanged(Types.GameState newState)
        {
            
        }
    }
}
