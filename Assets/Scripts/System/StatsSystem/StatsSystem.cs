//————————————————————————————————————————————————————————————————
// The following code is written and maintained by MoonTales Studio,
// under the creative direction of Cohen Calvert. 
// You are not allowed to use, alter, modify, or re-distribute this
// code without explicit permission from MoonTales Studio.
//————————————————————————————————————————————————————————————————

//—————— Includes ——————//
using System;
using System.Collections.Generic;
using UnityEngine;
//——————————————————————//

namespace System
{
    /// <summary>
    /// Singleton system that manages all gameplay stats.
    /// Structure:
    ///   Character (Level and Experience)
    ///   - PrimaryStat
    ///     - SubStat (calculated from its parent PrimaryStat)
    ///
    /// Supports per-entity stat tracking via a GameObject key.
    /// SubStat -> PrimaryStat relationships are global (shared across all entities).
    /// </summary>
    public class StatsSystem : Singleton<StatsSystem>, ISaveSystemInterface<StatsSystem.SaveData>
    {
        
        
        //————— Save System Variables —————//
        public struct SaveData
        {
            public Dictionary<SubStatType, PrimaryStatType> SubStatLookup;
            public Dictionary<GameObject, Dictionary<PrimaryStatType, PrimaryStat>> EntityPrimaryStats;
        }
        
        //————— Internal Variables —————//
        // SubStatType -> PrimaryStatType is global; the relationship is the same for every entity.
        // This is making the assumption all initializations follow the same relationship
        private Dictionary<SubStatType, PrimaryStatType> subStatLookup = new Dictionary<SubStatType, PrimaryStatType>();
        // Per-entity stat storage: entity → (PrimaryStatType → PrimaryStat)
        private Dictionary<GameObject, Dictionary<PrimaryStatType, PrimaryStat>> entityStats = new Dictionary<GameObject, Dictionary<PrimaryStatType, PrimaryStat>>();

        
        //————— External Variables —————//
        // Template function for the delegate
        public delegate void StatChangedHandler<T>(GameObject entity, T statType, float oldValue, float newValue) where T : Enum;

        // overload functions for both PrimaryStatType and SubStatType
        public static event StatChangedHandler<PrimaryStatType> OnPrimaryStatChanged;
        public static event StatChangedHandler<SubStatType>     OnSubStatChanged;
        
        // Overloaded broadcasts
        public void Broadcast_StatChanged(GameObject entity, PrimaryStatType stat, float oldVal, float newVal) => OnPrimaryStatChanged?.Invoke(entity, stat, oldVal, newVal);
        public void Broadcast_StatChanged(GameObject entity, SubStatType stat, float oldVal, float newVal) => OnSubStatChanged?.Invoke(entity, stat, oldVal, newVal);
        
        
        
        //————— Core Overrides —————//
        protected override void Awake()
        {
            base.Awake();
            gameObject.AddComponent<StatsSystemResponder>();
        }
        
        //————— Public API —————//
        
        /// <summary>
        /// Registers a PrimaryStat for the given entity.
        /// </summary>
        public void Register(GameObject entity, PrimaryStatType type, float baseValue, Action<PrimaryStatType, float, float> callback = null)
        {
            Dictionary<PrimaryStatType, PrimaryStat> stats = GetOrCreateEntityStats(entity);

            // Ensure we dont already have this stat registered for this entity
            if (stats.ContainsKey(type)) { DebugUtils.LogWarning($"'{entity.name}' already has stat '{type}' registered."); return; }

            // Create the new primary stat
            PrimaryStat stat = new PrimaryStat(type, baseValue);
            
            // Subscribe to the callback if provided
            if (callback != null) { stat.OnStatChanged += callback; }
    
            // Add the stat to the entity's stat dictionary
            stats[type] = stat;
        }

        /// <summary>
        /// Registers a SubStat under a PrimaryStat for the given entity.
        /// The SubStatType → PrimaryStatType relationship is recorded globally.
        /// </summary>
        public void RegisterSubStat(GameObject entity, PrimaryStatType primaryType, SubStatType subType, Func<float, float> calculationFunc, Action<SubStatType, float, float> callback = null)
        {
            PrimaryStat stat = GetPrimaryStat(entity, primaryType);
            if (stat == null)
            {
                UnityEngine.Debug.LogError($"[StatsSystem] '{entity.name}': Cannot register SubStat '{subType}', PrimaryStat '{primaryType}' not found.");
                return;
            }

            stat.AddSubStat(subType, calculationFunc, callback);

            if (!subStatLookup.ContainsKey(subType))
                subStatLookup[subType] = primaryType;
        }

        public void RegisterSubStat(GameObject entity, PrimaryStatType primaryType, SubStatType subType, float initialValue, Action<SubStatType, float, float> callback = null)
        {
            PrimaryStat stat = GetPrimaryStat(entity, primaryType);
            if (stat == null)
            {
                UnityEngine.Debug.LogError($"[StatsSystem] '{entity.name}': Cannot register SubStat '{subType}', PrimaryStat '{primaryType}' not found.");
                return;
            }

            stat.AddSubStat(subType, _ => initialValue, callback);

            if (!subStatLookup.ContainsKey(subType))
                subStatLookup[subType] = primaryType;
        }
        

        /// <summary>
        /// Returns the base value of a PrimaryStat for the given entity.
        /// </summary>
        public float GetStat(GameObject entity, PrimaryStatType type)
        {
            return GetPrimaryStat(entity, type)?.BaseValue ?? 0f;
        }

        /// <summary>
        /// Returns the computed value of a SubStat for the given entity.
        /// </summary>
        public float GetStat(GameObject entity, SubStatType subType)
        {
            if (!subStatLookup.TryGetValue(subType, out PrimaryStatType primaryType))
            {
                UnityEngine.Debug.LogError($"[StatsSystem] SubStat '{subType}' has no registered parent.");
                return 0f;
            }

            return GetSubStatValue(entity, primaryType, subType);
        }

        public void UpdateStat(GameObject entity, PrimaryStatType type, float amount)
        {
            PrimaryStat stat = GetPrimaryStat(entity, type);
            if (stat == null)
            {
                UnityEngine.Debug.LogError($"[StatsSystem] '{entity.name}': Cannot update PrimaryStat '{type}'.");
                return;
            }

            stat.AddToStatValue(entity, amount);
        }

        public void UpdateStat(GameObject entity, SubStatType subType, float amount)
        {
            if (!subStatLookup.TryGetValue(subType, out PrimaryStatType primaryType))
            {
                UnityEngine.Debug.LogError($"[StatsSystem] SubStat '{subType}' has no registered parent.");
                return;
            }

            PrimaryStat stat = GetPrimaryStat(entity, primaryType);
            if (stat == null){ return;}

            stat.UpdateSubStatValue(entity, subType, amount);
        }
        
        // We will also need functions for directly setting stat values, in the case of loading from a save
        public void SetStat(GameObject entity, PrimaryStatType type, float newValue)
        {
            PrimaryStat stat = GetPrimaryStat(entity, type);
            if (stat == null)
            {
                UnityEngine.Debug.LogError($"[StatsSystem] '{entity.name}': Cannot set PrimaryStat '{type}'.");
                return;
            }

            float delta = newValue - stat.BaseValue;
            stat.AddToStatValue(entity, delta);
        }
        
        public void SetStat(GameObject entity, SubStatType subType, float newValue)
        {
            if (!subStatLookup.TryGetValue(subType, out PrimaryStatType primaryType))
            {
                UnityEngine.Debug.LogError($"[StatsSystem] SubStat '{subType}' has no registered parent.");
                return;
            }

            PrimaryStat stat = GetPrimaryStat(entity, primaryType);
            if (stat == null){ return;}

            float currentValue = stat.GetSubStatValue(subType);
            float delta = newValue - currentValue;
            stat.UpdateSubStatValue(entity, subType, delta);
        }

        /// <summary>
        /// Removes all stats for a specific entity (e.g. on death or despawn).
        /// </summary>
        public void ResetStats(GameObject entity)
        {
            if (!entityStats.ContainsKey(entity))
            {
                UnityEngine.Debug.LogWarning($"[StatsSystem] '{entity.name}' has no registered stats to reset.");
                return;
            }

            DebugUtils.Log($"[StatsSystem] Resetting stats for '{entity.name}'.");
            entityStats.Remove(entity);
        }

        /// <summary>
        /// Clears all stats for all entities, and the global SubStat lookup.
        /// </summary>
        public void ResetAllStats()
        {
            entityStats.Clear();
            subStatLookup.Clear();
        }

        /// <summary>
        /// Debug prints all stats for all entities to the console.
        /// </summary>
        public void DebugPrintStats()
        {
            foreach (var entityEntry in entityStats)
            {
                UnityEngine.Debug.Log($"[StatsSystem] Entity: {entityEntry.Key.name}");
                foreach (var statEntry in entityEntry.Value)
                {
                    UnityEngine.Debug.Log($"  {statEntry.Key}: {statEntry.Value.BaseValue}");
                    foreach (var subStatEntry in statEntry.Value.GetType().GetField("_subStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(statEntry.Value) as Dictionary<SubStatType, SubStat>)
                    {
                        UnityEngine.Debug.Log($"    {subStatEntry.Key}: {subStatEntry.Value.GetValue()}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Debug prints all stats for a specific entity to the console.
        /// </summary>
        /// <param name="entity"></param>
        public void DebugPrintStatsForEntity(GameObject entity)
        {
            if (!entityStats.TryGetValue(entity, out Dictionary<PrimaryStatType, PrimaryStat> stats))
            {
                UnityEngine.Debug.LogWarning($"[StatsSystem] '{entity.name}' has no registered stats to print.");
                return;
            }

            UnityEngine.Debug.Log($"[StatsSystem] Stats for Entity: {entity.name}");
            foreach (var statEntry in stats)
            {
                UnityEngine.Debug.Log($"  {statEntry.Key}: {statEntry.Value.BaseValue}");
                foreach (var subStatEntry in statEntry.Value.GetType().GetField("_subStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(statEntry.Value) as Dictionary<SubStatType, SubStat>)
                {
                    UnityEngine.Debug.Log($"    {subStatEntry.Key}: {subStatEntry.Value.GetValue()}");
                }
            }
        }

        //————— Internal Helpers —————//

        /// <summary>
        /// Returns the entity's stat dictionary, creating it if it doesn't exist yet.
        /// </summary>
        private Dictionary<PrimaryStatType, PrimaryStat> GetOrCreateEntityStats(GameObject entity)
        {
            if (!entityStats.TryGetValue(entity, out Dictionary<PrimaryStatType, PrimaryStat> stats))
            {
                stats = new Dictionary<PrimaryStatType, PrimaryStat>();
                entityStats[entity] = stats;
            }

            return stats;
        }

        /// <summary>
        /// Retrieves the PrimaryStat for an entity, or null if not registered.
        /// </summary>
        private PrimaryStat GetPrimaryStat(GameObject entity, PrimaryStatType type)
        {
            if (!entityStats.TryGetValue(entity, out Dictionary<PrimaryStatType, PrimaryStat> stats))
            {
                UnityEngine.Debug.LogError($"[StatsSystem] Entity '{entity.name}' has no registered stats.");
                return null;
            }

            stats.TryGetValue(type, out PrimaryStat stat);
            return stat;
        }

        /// <summary>
        /// Gets the computed value of a SubStat for a given entity.
        /// </summary>
        private float GetSubStatValue(GameObject entity, PrimaryStatType primaryType, SubStatType subType)
        {
            PrimaryStat stat = GetPrimaryStat(entity, primaryType);
            if (stat == null)
            {
                UnityEngine.Debug.LogError($"[StatsSystem] '{entity.name}': PrimaryStat '{primaryType}' not found.");
                return 0f;
            }

            return stat.GetSubStatValue(subType);
        }

        //————— Save System Interface —————//
        public string SaveId => "StatsSystem";
        public SaveData OnSave()
        {
            return new SaveData
            {
                SubStatLookup = subStatLookup,
                EntityPrimaryStats = entityStats
            };
        }

        public void OnLoad(SaveData data)
        {
            subStatLookup = data.SubStatLookup;
            entityStats = data.EntityPrimaryStats;
        }
    }
    

    // ————— Enums, PrimaryStat, SubStat unchanged below —————


    public enum PrimaryStatType
    {
        Vitality,
        Strength,
        Endurance,
        Intelligence,
        Willpower,
        Wisdom,
    }

    public enum SubStatType
    {
        // Vitality SubStats
        MaxHealth,
        CurrentHealth,
        HealthRegen,
        // Strength SubStats
        MaxCarryWeight,
        // Endurance SubStats
        Speed,
        MaxStamina,
        CurrentStamina,
        StaminaRegen,   
        // Intelligence SubStats
        // (none yet)
        // Willpower SubStats
        MentalFortitude,
        // Wisdom SubStats
        Perception,
    }


    public class PrimaryStat
    {
        private float _baseValue;
        private Dictionary<SubStatType, SubStat> _subStats = new Dictionary<SubStatType, SubStat>();
        
        public event Action<PrimaryStatType, float, float> OnStatChanged;
        public PrimaryStatType StatType { get; private set; }



        public PrimaryStat(PrimaryStatType type, float baseValue)
        {
            StatType = type;
            _baseValue = baseValue;
        }

        public float BaseValue
        {
            get => _baseValue;
            set { _baseValue = value; }
        }

        public void AddSubStat(SubStatType type, Func<float, float> calculationFunc, Action<SubStatType, float, float> callback = null)
        {
            if (_subStats.ContainsKey(type))
            { 
                DebugUtils.LogWarning($"[PrimaryStat] SubStat '{type}' already exists for PrimaryStat '{StatType}'. Skipping registration.");
                return;
            }

            _subStats[type] = new SubStat(this, type, calculationFunc, callback);
        }

        public float GetSubStatValue(SubStatType type)
        {
            if (!_subStats.TryGetValue(type, out SubStat subStat))
            {
                DebugUtils.LogError($"[PrimaryStat] SubStat '{type}' not found.");
                return 0f;
            }

            return subStat.GetValue();
        }

        public void AddToStatValue(GameObject entity, float newValue)
        {
            float oldValue = _baseValue;
            _baseValue  += newValue;
            OnStatChanged?.Invoke(StatType, oldValue, _baseValue);
            StatsSystem.Instance.Broadcast_StatChanged(entity, StatType, oldValue, _baseValue);

        }
        
        public void UpdateSubStatValue(GameObject entity, SubStatType type, float amount)
        {
            if (!_subStats.TryGetValue(type, out SubStat subStat))
            {
                DebugUtils.LogError($"[PrimaryStat] Cannot update SubStat '{type}' for entity '{entity.name}'.");
                return;
            }

            subStat.AddToValue(entity, amount);
        }
    }


    public class SubStat
    {
        private PrimaryStat _primaryStat;
        private Func<float, float> _calculationFunc;
        private float _currentValue;
        
        public SubStatType StatType { get; private set; }
        public event Action<SubStatType, float, float> OnStatChanged;

        public SubStat(PrimaryStat primaryStat, SubStatType type, Func<float, float> calculationFunc, Action<SubStatType, float, float> callback = null)
        {
            StatType = type;
            _primaryStat = primaryStat;
            _calculationFunc = calculationFunc;
            _currentValue = calculationFunc(primaryStat.BaseValue);
            if (callback != null) {OnStatChanged += callback; }
                
        }

        /// <summary>
        /// Recalculates the current value from the parent PrimaryStat.
        /// Call this when the parent's base value changes (e.g. a Vitality upgrade).
        /// </summary>
        public void Recalculate()
        {
            _currentValue = _calculationFunc(_primaryStat.BaseValue);
        }
        
        public float GetValue() => _currentValue;
        public void AddToValue(GameObject entity, float amount)
        {
            float oldValue = _currentValue;
            _currentValue += amount;
            OnStatChanged?.Invoke(StatType, oldValue, _currentValue);
            StatsSystem.Instance.Broadcast_StatChanged(entity, StatType, oldValue, _currentValue);
        }
    }
}


public static class StatsInitializer
{
    public static void InitializeDestructible(GameObject entity, float health,  Action onDeath = null)
    {
        StatsSystem.Instance.Register(entity, PrimaryStatType.Vitality, 1f);
        StatsSystem.Instance.RegisterSubStat(entity, PrimaryStatType.Vitality, SubStatType.MaxHealth, health);
        StatsSystem.Instance.RegisterSubStat(entity, PrimaryStatType.Vitality, SubStatType.CurrentHealth, StatsSystem.Instance.GetStat(entity, SubStatType.MaxHealth),
            (stat, oldValue, newValue) =>
            {
                if (newValue <= 0f)
                {
                    onDeath?.Invoke();
                    // Now we can clear up these stats, since we most likely wont access them again
                    //we want to hook up the reset to be whenever this object gets destroyed, not rn
                    StatsSystem.Instance.ResetStats(entity);
                }
            });    
    }
    

    public static void InitializePlayer(GameObject entity, float baseVitality)
    {
        StatsSystem.Instance.Register(entity, PrimaryStatType.Vitality, baseVitality);
        StatsSystem.Instance.RegisterSubStat(entity, PrimaryStatType.Vitality, SubStatType.MaxHealth,     v => v * 2f);
        StatsSystem.Instance.RegisterSubStat(entity, PrimaryStatType.Vitality, SubStatType.HealthRegen,   v => v * 0.5f);
        StatsSystem.Instance.RegisterSubStat(entity, PrimaryStatType.Vitality, SubStatType.CurrentHealth, StatsSystem.Instance.GetStat(entity, SubStatType.MaxHealth));
    }
}