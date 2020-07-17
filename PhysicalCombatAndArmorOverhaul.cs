// Project:         PhysicalCombatAndArmorOverhaul mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    3/17/2020, 6:30 PM
// Last Edit:		4/3/2020, 10:40 PM
// Version:			1.10
// Special Thanks:  Hazelnut and Ralzar
// Modifier:		

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace PhysicalCombatAndArmorOverhaul
{
    public class PhysicalCombatAndArmorOverhaul : MonoBehaviour
    {
        static Mod mod;
		public static bool archeryModuleCheck { get; set; }
        public static bool critStrikeModuleCheck { get; set; }
		public static bool armorHitFormulaModuleCheck { get; set; }
		public static bool shieldBlockSuccess { get; set; }
        public static bool ralzarRacesRedoneModifiers = false;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject("PhysicalCombatAndArmorOverhaul");
            go.AddComponent<PhysicalCombatAndArmorOverhaul>();
        }

        void Awake()
        {	
            ModSettings settings = mod.GetSettings();
            Mod roleplayRealism = ModManager.Instance.GetMod("RoleplayRealism");
            Mod meanerMonsters = ModManager.Instance.GetMod("Meaner Monsters");
            Mod racesRedone = ModManager.Instance.GetMod("Races Redone");
            bool equipmentDamageEnhanced = settings.GetBool("Modules", "equipmentDamageEnhanced");
			bool fixedStrengthDamageModifier = settings.GetBool("Modules", "fixedStrengthDamageModifier");
			bool armorHitFormulaRedone = settings.GetBool("Modules", "armorHitFormulaRedone");
			bool criticalStrikesIncreaseDamage = settings.GetBool("Modules", "criticalStrikesIncreaseDamage");
            bool rolePlayRealismArcheryModule = false;
            bool ralzarMeanerMonstersEdit = false;

            if (roleplayRealism != null)
            {
                ModSettings rolePlayRealismSettings = roleplayRealism.GetSettings();
                rolePlayRealismArcheryModule = rolePlayRealismSettings.GetBool("Modules", "advancedArchery");
            }
            if (meanerMonsters != null)
            {
                ralzarMeanerMonstersEdit = true;
            }
            if (racesRedone != null)
            {
                ModSettings ralzarRacesRedoneSettings = racesRedone.GetSettings();
                ralzarRacesRedoneModifiers = (ralzarRacesRedoneSettings.GetInt("Features", "Rules") == 1);
            }

            InitMod(equipmentDamageEnhanced, fixedStrengthDamageModifier, armorHitFormulaRedone, criticalStrikesIncreaseDamage, rolePlayRealismArcheryModule, ralzarMeanerMonstersEdit);

            mod.IsReady = true;
        }
		
		#region InitMod and Settings
		
		private static void InitMod(bool equipmentDamageEnhanced, bool fixedStrengthDamageModifier, bool armorHitFormulaRedone, bool criticalStrikesIncreaseDamage, bool rolePlayRealismArcheryModule, bool ralzarMeanerMonstersEdit)
        {
            Debug.Log("Begin mod init: PhysicalCombatAndArmorOverhaul");

            if (equipmentDamageEnhanced)
            {
                FormulaHelper.RegisterOverride(mod, "DamageEquipment", (Func<DaggerfallEntity, DaggerfallEntity, int, DaggerfallUnityItem, int, bool>)DamageEquipment);
				
				Debug.Log("PhysicalCombatAndArmorOverhaul: Enhanced Equipment Damage Module Active");
            }
			else
				Debug.Log("PhysicalCombatAndArmorOverhaul: Enhanced Equipment Damage Module Disabled");
			
			if (fixedStrengthDamageModifier)
			{
				FormulaHelper.RegisterOverride(mod, "DamageModifier", (Func<int, int>)DamageModifier);
				
				Debug.Log("PhysicalCombatAndArmorOverhaul: Fixed Strength Damage Modifier Module Active");
			}
			else
				Debug.Log("PhysicalCombatAndArmorOverhaul: Fixed Strength Damage Modifier Module Disabled");
			
			if (armorHitFormulaRedone)
			{
				FormulaHelper.RegisterOverride(mod, "CalculateAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int>)CalculateAttackDamage);
				FormulaHelper.RegisterOverride(mod, "CalculateSwingModifiers", (Func<FPSWeapon, ToHitAndDamageMods>)CalculateSwingModifiers);
				FormulaHelper.RegisterOverride(mod, "CalculateProficiencyModifiers", (Func<DaggerfallEntity, DaggerfallUnityItem, ToHitAndDamageMods>)CalculateProficiencyModifiers);
				FormulaHelper.RegisterOverride(mod, "CalculateRacialModifiers", (Func<DaggerfallEntity, DaggerfallUnityItem, PlayerEntity, ToHitAndDamageMods>)CalculateRacialModifiers);
				FormulaHelper.RegisterOverride(mod, "CalculateWeaponToHit", (Func<DaggerfallUnityItem, int>)CalculateWeaponToHit);
				FormulaHelper.RegisterOverride(mod, "CalculateArmorToHit", (Func<DaggerfallEntity, int, int>)CalculateArmorToHit);
				FormulaHelper.RegisterOverride(mod, "CalculateAdrenalineRushToHit", (Func<DaggerfallEntity, DaggerfallEntity, int>)CalculateAdrenalineRushToHit);
				FormulaHelper.RegisterOverride(mod, "CalculateStatDiffsToHit", (Func<DaggerfallEntity, DaggerfallEntity, int>)CalculateStatDiffsToHit);
				FormulaHelper.RegisterOverride(mod, "CalculateSkillsToHit", (Func<DaggerfallEntity, DaggerfallEntity, int>)CalculateSkillsToHit);
				FormulaHelper.RegisterOverride(mod, "CalculateAdjustmentsToHit", (Func<DaggerfallEntity, DaggerfallEntity, int>)CalculateAdjustmentsToHit);
				FormulaHelper.RegisterOverride(mod, "CalculateWeaponAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int>)CalculateWeaponAttackDamage);
				FormulaHelper.RegisterOverride(mod, "CalculateSuccessfulHit", (Func<DaggerfallEntity, DaggerfallEntity, int, int, bool>)CalculateSuccessfulHit);
				
				// Overridden Due To FormulaHelper.cs Private Access Modifiers, otherwise would not be included here.
				FormulaHelper.RegisterOverride(mod, "CalculateStruckBodyPart", (Func<int>)CalculateStruckBodyPart);
				FormulaHelper.RegisterOverride(mod, "CalculateBackstabChance", (Func<PlayerEntity, DaggerfallEntity, int, int>)CalculateBackstabChance);
				FormulaHelper.RegisterOverride(mod, "CalculateBackstabDamage", (Func<int, int, int>)CalculateBackstabDamage);
				FormulaHelper.RegisterOverride(mod, "GetBonusOrPenaltyByEnemyType", (Func<DaggerfallEntity, EnemyEntity, int>)GetBonusOrPenaltyByEnemyType);
				
				Debug.Log("PhysicalCombatAndArmorOverhaul: Armor Hit Formula Redone Module Active");
				
				armorHitFormulaModuleCheck = true;
			}
			else
			{
				Debug.Log("PhysicalCombatAndArmorOverhaul: Armor Hit Formula Redone Module Disabled");
				
				armorHitFormulaModuleCheck = false;
			}
			
			if (criticalStrikesIncreaseDamage)
			{
				if (armorHitFormulaRedone)
				{
					Debug.Log("PhysicalCombatAndArmorOverhaul: Critical Strikes Increase Damage Module Active");
					
					critStrikeModuleCheck = true;
				}
				else
				{
					critStrikeModuleCheck = false;
					Debug.Log("PhysicalCombatAndArmorOverhaul: Critical Strikes Increase Damage Module Is Dependent On Armor Hit Formula Redone To Function, So By Default is Disabled");
				}
			}
			else
			{
				critStrikeModuleCheck = false;
				Debug.Log("PhysicalCombatAndArmorOverhaul: Critical Strikes Increase Damage Module Disabled");
			}
			
			if (rolePlayRealismArcheryModule)
			{
				FormulaHelper.RegisterOverride(mod, "AdjustWeaponHitChanceMod", (Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int>)AdjustWeaponHitChanceMod);
				FormulaHelper.RegisterOverride(mod, "AdjustWeaponAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int>)AdjustWeaponAttackDamage);
				
				Debug.Log("PhysicalCombatAndArmorOverhaul: Roleplay Realism's Archery Module Active");
				
				archeryModuleCheck = true;
			}
			else
			{
				archeryModuleCheck = false;
				Debug.Log("PhysicalCombatAndArmorOverhaul: Roleplay Realism's Archery Module Disabled");
			}
			
			if (ralzarMeanerMonstersEdit)
			{
				Debug.Log("PhysicalCombatAndArmorOverhaul: Ralzar's Meaner Monsters Edited Module Active");
				Debug.Log("PhysicalCombatAndArmorOverhaul: Initializing Enemy Stat Values, Ralzar's Meaner Monsters Edited Module...");
				
				//-------------------- Animals -------------------------------------
				
				// Rat
				EnemyBasics.Enemies[0].MinDamage = 1;
				EnemyBasics.Enemies[0].MaxDamage = 3;
				EnemyBasics.Enemies[0].MinHealth = 15;
				EnemyBasics.Enemies[0].MaxHealth = 30;
				EnemyBasics.Enemies[0].Level = 1;
				EnemyBasics.Enemies[0].ArmorValue = 6;
				// Estimated Avoidance Modifier (Lower means harder to hit): [(ArmorValue * 5) - (Enemy_dodge_skill/2)]: 30 - (35/2) = 12.5
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: 56.5 [(Player_Hit_Mod_Total) + (Player comp. Monster Hit Mod) - (Monster_Avoid)]: (49) + (-2) + (-10.75) = 36.25%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: 107.5 [(Player_Hit_Mod_Total) + (Player comp. Monster Hit Mod) - (Monster_Avoid)]: (113) + (0) + (-10.75) = 102.25%
				// Lvl.20 Player Example: [Breton, Daedric Wep, 85 wep skill, 100 agi, 60 luck, 70 crit skill]: 141.5 [(Player_Hit_Mod_Total) + (Player comp. Monster Hit Mod) - (Monster_Avoid)]: (113) + (0) + (-10.75) = 102.25%
				
				// Giant Bat
				EnemyBasics.Enemies[3].MinDamage = 1;
				EnemyBasics.Enemies[3].MaxDamage = 4;
				EnemyBasics.Enemies[3].MinHealth = 5;
				EnemyBasics.Enemies[3].MaxHealth = 12;
				EnemyBasics.Enemies[3].Level = 2;
				EnemyBasics.Enemies[3].ArmorValue = 2;
				// Estimated Avoidance Modifier (Lower means harder to hit): 15 - (40/2) = -10
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-1) + (-17) = 31%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (1) + (-17) = 97%

				// Grizzly Bear
				EnemyBasics.Enemies[4].MinDamage = 4;
				EnemyBasics.Enemies[4].MaxDamage = 8;
				EnemyBasics.Enemies[4].MinDamage2 = 6;
				EnemyBasics.Enemies[4].MaxDamage2 = 10;
				EnemyBasics.Enemies[4].MinDamage3 = 8;
				EnemyBasics.Enemies[4].MaxDamage3 = 12;
				EnemyBasics.Enemies[4].MinHealth = 55;
				EnemyBasics.Enemies[4].MaxHealth = 110;
				EnemyBasics.Enemies[4].Level = 4;
				EnemyBasics.Enemies[4].ArmorValue = 8;
				// Estimated Avoidance Modifier (Lower means harder to hit): 40 - (50/2) = 15
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-1) + (-13.5) = 34.5%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (1) + (-13.5) = 100.5%

				// Sabertooth Tiger		
				EnemyBasics.Enemies[5].MinDamage = 6;
				EnemyBasics.Enemies[5].MaxDamage = 12;
				EnemyBasics.Enemies[5].MinDamage2 = 8;
				EnemyBasics.Enemies[5].MaxDamage2 = 14;
				EnemyBasics.Enemies[5].MinDamage3 = 10;
				EnemyBasics.Enemies[5].MaxDamage3 = 20;
				EnemyBasics.Enemies[5].MinHealth = 35;
				EnemyBasics.Enemies[5].MaxHealth = 60;
				EnemyBasics.Enemies[5].Level = 4;
				EnemyBasics.Enemies[5].ArmorValue = 5;
				// Estimated Avoidance Modifier (Lower means harder to hit): 25 - (50/2) = 0
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-18.5) = 28.5%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-18.5) = 94.5%

				// Spider		
				EnemyBasics.Enemies[6].MinDamage = 3;
				EnemyBasics.Enemies[6].MaxDamage = 9;
				EnemyBasics.Enemies[6].MinHealth = 12;
				EnemyBasics.Enemies[6].MaxHealth = 28;
				EnemyBasics.Enemies[6].Level = 2;
				EnemyBasics.Enemies[6].ArmorValue = 4;
				// Estimated Avoidance Modifier (Lower means harder to hit): 20 - (40/2) = 0
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-16) = 31%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-16) = 97%
				
				// Slaughterfish
				EnemyBasics.Enemies[11].MinDamage = 3;
				EnemyBasics.Enemies[11].MaxDamage = 12;
				EnemyBasics.Enemies[11].MinHealth = 20;
				EnemyBasics.Enemies[11].MaxHealth = 50;
				EnemyBasics.Enemies[11].Level = 7;
				EnemyBasics.Enemies[11].ArmorValue = 5;
				// Estimated Avoidance Modifier (Lower means harder to hit): 30 - (65/2) = -7.5
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-20.25) = 26.75%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-20.25) = 92.75%

				// Giant Scorpion		
				EnemyBasics.Enemies[20].MinDamage = 7;
				EnemyBasics.Enemies[20].MaxDamage = 16;
				EnemyBasics.Enemies[20].MinHealth = 22;
				EnemyBasics.Enemies[20].MaxHealth = 48;
				EnemyBasics.Enemies[20].Level = 4;
				EnemyBasics.Enemies[20].ArmorValue = 5;
				// Estimated Avoidance Modifier (Lower means harder to hit): 25 - (50/2) = 0
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-19.5) = 27.5%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-19.5) = 93.5%
				
				//-------------------- Animals -------------------------------------
				
				//-------------------- Fantastical Creatures -------------------------------------
				
				// Imp
				EnemyBasics.Enemies[1].MinDamage = 2;
				EnemyBasics.Enemies[1].MaxDamage = 13;
				EnemyBasics.Enemies[1].MinHealth = 10;
				EnemyBasics.Enemies[1].MaxHealth = 18;
				EnemyBasics.Enemies[1].Level = 2;
				EnemyBasics.Enemies[1].ArmorValue = 3;
				// Estimated Avoidance Modifier (Lower means harder to hit): 15 - (40/2) = -5
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-17) = 31%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-17) = 97%
				
				// Spriggan
				EnemyBasics.Enemies[2].MinDamage = 1;
				EnemyBasics.Enemies[2].MaxDamage = 8;
				EnemyBasics.Enemies[2].MinDamage2 = 1;
				EnemyBasics.Enemies[2].MaxDamage2 = 8;
				EnemyBasics.Enemies[2].MinDamage3 = 1;
				EnemyBasics.Enemies[2].MaxDamage3 = 10;
				EnemyBasics.Enemies[2].MinHealth = 18;
				EnemyBasics.Enemies[2].MaxHealth = 36;
				EnemyBasics.Enemies[2].Level = 3;
				EnemyBasics.Enemies[2].ArmorValue = -2;
				// Estimated Avoidance Modifier (Lower means harder to hit): -10 - (45/2) = -32.5
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (1) + (-25.25) = 24.75%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (3) + (-25.25) = 90.75%
				
				// Centaur
				EnemyBasics.Enemies[8].MinDamage = 10;
				EnemyBasics.Enemies[8].MaxDamage = 22;
				EnemyBasics.Enemies[8].MinHealth = 30;
				EnemyBasics.Enemies[8].MaxHealth = 65;
				EnemyBasics.Enemies[8].Level = 5;
				EnemyBasics.Enemies[8].ArmorValue = 7;
				// Estimated Avoidance Modifier (Lower means harder to hit): 35 - (45/2) = 12.5
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-15.25) = 31.75%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-15.25) = 97.75%
				
				// Nymph
				EnemyBasics.Enemies[10].MinDamage = 1;
				EnemyBasics.Enemies[10].MaxDamage = 7;
				EnemyBasics.Enemies[10].MinHealth = 20;
				EnemyBasics.Enemies[10].MaxHealth = 45;
				EnemyBasics.Enemies[10].Level = 6;
				EnemyBasics.Enemies[10].ArmorValue = 0;
				// Estimated Avoidance Modifier (Lower means harder to hit): 0 - (60/2) = -30
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-25) = 22%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-25) = 88%
				
				// Harpy
				EnemyBasics.Enemies[13].MinDamage = 7;
				EnemyBasics.Enemies[13].MaxDamage = 12;
				EnemyBasics.Enemies[13].MinHealth = 25;
				EnemyBasics.Enemies[13].MaxHealth = 60;
				EnemyBasics.Enemies[13].Level = 8;
				EnemyBasics.Enemies[13].ArmorValue = 3;
				// Estimated Avoidance Modifier (Lower means harder to hit): 15 - (70/2) = -20
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-25.5) = 21.5%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-25.5) = 87.5%
				
				// Giant		
				EnemyBasics.Enemies[16].MinDamage = 10;
				EnemyBasics.Enemies[16].MaxDamage = 35;
				EnemyBasics.Enemies[16].MinHealth = 70;
				EnemyBasics.Enemies[16].MaxHealth = 110;
				EnemyBasics.Enemies[16].Level = 10;
				EnemyBasics.Enemies[16].ArmorValue = 10;
				// Estimated Avoidance Modifier (Lower means harder to hit): 50 - (80/2) = 10
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-1) + (-22) = 26%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (1) + (-22) = 92%
				
				// Gargoyle
				EnemyBasics.Enemies[22].MinDamage = 15;
				EnemyBasics.Enemies[22].MaxDamage = 40;
				EnemyBasics.Enemies[22].MinHealth = 50;
				EnemyBasics.Enemies[22].MaxHealth = 100;
				EnemyBasics.Enemies[22].Level = 14;
				EnemyBasics.Enemies[22].ArmorValue = 2;
				// Estimated Avoidance Modifier (Lower means harder to hit): 10 - (100/2) = -40
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-35) = 12%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-35) = 78%
				
				// Dragonling
				EnemyBasics.Enemies[34].MinDamage = 15;
				EnemyBasics.Enemies[34].MaxDamage = 25;
				EnemyBasics.Enemies[34].MinHealth = 35;
				EnemyBasics.Enemies[34].MaxHealth = 70;
				EnemyBasics.Enemies[34].Level = 16;
				EnemyBasics.Enemies[34].ArmorValue = 5;
				// Estimated Avoidance Modifier (Lower means harder to hit): 25 - (100/2) = -25
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-29) = 17%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-29) = 83%
				
				// Dragonling [Quest Specific Dragonling? Has 500,000 Soul Point Value]
				EnemyBasics.Enemies[40].MinDamage = 15;
				EnemyBasics.Enemies[40].MaxDamage = 25;
				EnemyBasics.Enemies[40].MinHealth = 35;
				EnemyBasics.Enemies[40].MaxHealth = 70;
				EnemyBasics.Enemies[40].Level = 16;
				EnemyBasics.Enemies[40].ArmorValue = 5;
				// Estimated Avoidance Modifier (Lower means harder to hit): 25 - (100/2) = -25
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-29) = 17%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-29) = 83%
				
				// Dreugh
				EnemyBasics.Enemies[41].MinDamage = 10;
				EnemyBasics.Enemies[41].MaxDamage = 18;
				EnemyBasics.Enemies[41].MinHealth = 40;
				EnemyBasics.Enemies[41].MaxHealth = 80;
				EnemyBasics.Enemies[41].Level = 16;
				EnemyBasics.Enemies[41].ArmorValue = 4;
				// Estimated Avoidance Modifier (Lower means harder to hit): 20 - (100/2) = -30
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-29) = 17%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-29) = 83%
				
				// Lamia
				EnemyBasics.Enemies[42].MinDamage = 6;
				EnemyBasics.Enemies[42].MaxDamage = 12;
				EnemyBasics.Enemies[42].MinHealth = 30;
				EnemyBasics.Enemies[42].MaxHealth = 65;
				EnemyBasics.Enemies[42].Level = 16;
				EnemyBasics.Enemies[42].ArmorValue = 3;
				// Estimated Avoidance Modifier (Lower means harder to hit): 15 - (100/2) = -35
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-29) = 17%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-29) = 83%
				
				//-------------------- Fantastical Creatures -------------------------------------
				
				//-------------------- Human Enemies -------------------------------------
				
				// WIP WIP WIP WIP WIP
				
				//-------------------- Human Enemies -------------------------------------
				
				//-------------------- Orcs -------------------------------------
				
				// Orc		
				EnemyBasics.Enemies[7].MinDamage = 8;
				EnemyBasics.Enemies[7].MaxDamage = 20;
				EnemyBasics.Enemies[7].MinHealth = 40;
				EnemyBasics.Enemies[7].MaxHealth = 70;
				EnemyBasics.Enemies[7].Level = 6;
				EnemyBasics.Enemies[7].ArmorValue = 7;
				// Estimated Avoidance Modifier (Lower means harder to hit): 35 - (60/2) = 5
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (1) + (-19) = 31%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (3) + (-19) = 97%
				
				// Orc Sergeant		
				EnemyBasics.Enemies[12].MinDamage = 15;
				EnemyBasics.Enemies[12].MaxDamage = 30;
				EnemyBasics.Enemies[12].MinHealth = 50;
				EnemyBasics.Enemies[12].MaxHealth = 85;
				EnemyBasics.Enemies[12].Level = 9;
				EnemyBasics.Enemies[12].ArmorValue = 4;
				// Estimated Avoidance Modifier (Lower means harder to hit): 20 - (75/2) = -17.5
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (1) + (-24.75) = 25.25%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (3) + (-24.75) = 91.25%

				// Orc Shaman		
				EnemyBasics.Enemies[21].MinDamage = 10;
				EnemyBasics.Enemies[21].MaxDamage = 22;
				EnemyBasics.Enemies[21].MinHealth = 45;
				EnemyBasics.Enemies[21].MaxHealth = 80;
				EnemyBasics.Enemies[21].Level = 15;
				EnemyBasics.Enemies[21].ArmorValue = 3;
				// Estimated Avoidance Modifier (Lower means harder to hit): 15 - (100/2) = -35
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (1) + (-29) = 21%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (3) + (-29) = 87%

				// Orc Warlord		
				EnemyBasics.Enemies[24].MinDamage = 25;
				EnemyBasics.Enemies[24].MaxDamage = 45;
				EnemyBasics.Enemies[24].MinHealth = 70;
				EnemyBasics.Enemies[24].MaxHealth = 110;
				EnemyBasics.Enemies[24].Level = 19;
				EnemyBasics.Enemies[24].ArmorValue = 0;
				// Estimated Avoidance Modifier (Lower means harder to hit): 0 - (100/2) = -50
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (1) + (-37) = 13%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (3) + (-37) = 79%
				
				//-------------------- Orcs -------------------------------------
				
				//-------------------- Lycanthropes -------------------------------------
				
				// Werewolf		
				EnemyBasics.Enemies[9].MinDamage = 6;
				EnemyBasics.Enemies[9].MaxDamage = 10;
				EnemyBasics.Enemies[9].MinDamage2 = 8;
				EnemyBasics.Enemies[9].MaxDamage2 = 12;
				EnemyBasics.Enemies[9].MinDamage3 = 10;
				EnemyBasics.Enemies[9].MaxDamage3 = 20;
				EnemyBasics.Enemies[9].MinHealth = 35;
				EnemyBasics.Enemies[9].MaxHealth = 60;
				EnemyBasics.Enemies[9].Level = 8;
				EnemyBasics.Enemies[9].ArmorValue = 2;
				// Estimated Avoidance Modifier (Lower means harder to hit): 10 - (70/2) = -25
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-25.5) = 21.5%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-25.5) = 87.5%

				// Wereboar		
				EnemyBasics.Enemies[14].MinDamage = 5;
				EnemyBasics.Enemies[14].MaxDamage = 8;
				EnemyBasics.Enemies[14].MinDamage2 = 8;
				EnemyBasics.Enemies[14].MaxDamage2 = 10;
				EnemyBasics.Enemies[14].MinDamage3 = 10;
				EnemyBasics.Enemies[14].MaxDamage3 = 22;
				EnemyBasics.Enemies[14].MinHealth = 55;
				EnemyBasics.Enemies[14].MaxHealth = 90;
				EnemyBasics.Enemies[14].Level = 8;
				EnemyBasics.Enemies[14].ArmorValue = 4;
				// Estimated Avoidance Modifier (Lower means harder to hit): 20 - (70/2) = -15
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-1) + (-22.5) = 25.5%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (1) + (-22.5) = 91.5%
				
				//-------------------- Lycanthropes -------------------------------------
				
				//-------------------- Atronachs -------------------------------------
				
				// Fire Atronach		
				EnemyBasics.Enemies[35].MinDamage = 20;
				EnemyBasics.Enemies[35].MaxDamage = 30;
				EnemyBasics.Enemies[35].MinHealth = 40;
				EnemyBasics.Enemies[35].MaxHealth = 65;
				EnemyBasics.Enemies[35].Level = 16;
				EnemyBasics.Enemies[35].ArmorValue = 2;
				// Estimated Avoidance Modifier (Lower means harder to hit): 10 - (100/2) = -40
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-33) = 13%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-33) = 79%

				// Iron Atronach		
				EnemyBasics.Enemies[36].MinDamage = 30;
				EnemyBasics.Enemies[36].MaxDamage = 55;
				EnemyBasics.Enemies[36].MinHealth = 80;
				EnemyBasics.Enemies[36].MaxHealth = 150;
				EnemyBasics.Enemies[36].Level = 21;
				EnemyBasics.Enemies[36].ArmorValue = 4;
				// Estimated Avoidance Modifier (Lower means harder to hit): 20 - (100/2) = -30
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-32) = 14%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-32) = 80%

				// Flesh Atronach		
				EnemyBasics.Enemies[37].MinDamage = 7;
				EnemyBasics.Enemies[37].MaxDamage = 15;
				EnemyBasics.Enemies[37].MinHealth = 130;
				EnemyBasics.Enemies[37].MaxHealth = 260;
				EnemyBasics.Enemies[37].Level = 16;
				EnemyBasics.Enemies[37].ArmorValue = 8;
				// Estimated Avoidance Modifier (Lower means harder to hit): 40 - (100/2) = -10
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3.5) + (-26) = 19.5%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1.5) + (-26) = 85.5%

				// Ice Atronach		
				EnemyBasics.Enemies[38].MinDamage = 15;
				EnemyBasics.Enemies[38].MaxDamage = 25;
				EnemyBasics.Enemies[38].MinHealth = 60;
				EnemyBasics.Enemies[38].MaxHealth = 110;
				EnemyBasics.Enemies[38].Level = 16;
				EnemyBasics.Enemies[38].ArmorValue = 6;
				// Estimated Avoidance Modifier (Lower means harder to hit): 30 - (100/2) = -20
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-4) + (-32) = 13%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-2) + (-32) = 79%
				
				//-------------------- Atronachs -------------------------------------
				
				//-------------------- Undead -------------------------------------
				
				// Skeletal Warrior
				EnemyBasics.Enemies[15].MinDamage = 8;
				EnemyBasics.Enemies[15].MaxDamage = 16;
				EnemyBasics.Enemies[15].MinHealth = 25;
				EnemyBasics.Enemies[15].MaxHealth = 75;
				EnemyBasics.Enemies[15].Level = 9;
				EnemyBasics.Enemies[15].ArmorValue = 4;
				// Estimated Avoidance Modifier (Lower means harder to hit): 20 - (75/2) = -17.5
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-26.75) = 20.25%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-26.75) = 86.25%
				
				// Zombie		
				EnemyBasics.Enemies[17].MinDamage = 4;
				EnemyBasics.Enemies[17].MaxDamage = 9;
				EnemyBasics.Enemies[17].MinHealth = 70;
				EnemyBasics.Enemies[17].MaxHealth = 120;
				EnemyBasics.Enemies[17].Level = 5;
				EnemyBasics.Enemies[17].ArmorValue = 8;
				// Estimated Avoidance Modifier (Lower means harder to hit): 40 - (55/2) = 12.5
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-16.75) = 30.25%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-16.75) = 96.25%
				
				// Ghost
				EnemyBasics.Enemies[18].MinDamage = 10;
				EnemyBasics.Enemies[18].MaxDamage = 25;
				EnemyBasics.Enemies[18].MinHealth = 20;
				EnemyBasics.Enemies[18].MaxHealth = 55;
				EnemyBasics.Enemies[18].Level = 11;
				EnemyBasics.Enemies[18].ArmorValue = 0;
				// Estimated Avoidance Modifier (Lower means harder to hit): 0 - (85/2) = -42.5
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-31.25) = 15.75%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-31.25) = 81.75%

				// Mummy		
				EnemyBasics.Enemies[19].MinDamage = 7;
				EnemyBasics.Enemies[19].MaxDamage = 23;
				EnemyBasics.Enemies[19].MinHealth = 60;
				EnemyBasics.Enemies[19].MaxHealth = 135;
				EnemyBasics.Enemies[19].Level = 15;
				EnemyBasics.Enemies[19].ArmorValue = 0;
				// Estimated Avoidance Modifier (Lower means harder to hit): 0 - (100/2) = -50
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-40) = 7%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-40) = 73%
				
				// Wraith
				EnemyBasics.Enemies[23].MinDamage = 15;
				EnemyBasics.Enemies[23].MaxDamage = 35;
				EnemyBasics.Enemies[23].MinHealth = 35;
				EnemyBasics.Enemies[23].MaxHealth = 65;
				EnemyBasics.Enemies[23].Level = 15;
				EnemyBasics.Enemies[23].ArmorValue = 0;
				// Estimated Avoidance Modifier (Lower means harder to hit): 0 - (100/2) = -50
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-35) = 12%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-35) = 78%
				
				
				// Vampire
				EnemyBasics.Enemies[28].MinDamage = 20;
				EnemyBasics.Enemies[28].MaxDamage = 40;
				EnemyBasics.Enemies[28].MinHealth = 65;
				EnemyBasics.Enemies[28].MaxHealth = 100;
				EnemyBasics.Enemies[28].Level = 19;
				EnemyBasics.Enemies[28].ArmorValue = -2;
				// Estimated Avoidance Modifier (Lower means harder to hit): -10 - (100/2) = -60
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-4) + (-37) = 8%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-2) + (-37) = 74%

				// Vampire Ancient		
				EnemyBasics.Enemies[30].MinDamage = 30;
				EnemyBasics.Enemies[30].MaxDamage = 50;
				EnemyBasics.Enemies[30].MinHealth = 95;
				EnemyBasics.Enemies[30].MaxHealth = 150;
				EnemyBasics.Enemies[30].Level = 20;
				EnemyBasics.Enemies[30].ArmorValue = -7;
				// Estimated Avoidance Modifier (Lower means harder to hit): -35 - (100/2) = -85
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-41) = 5%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-41) = 71%
				
				// Lich		
				EnemyBasics.Enemies[32].MinDamage = 35;
				EnemyBasics.Enemies[32].MaxDamage = 60;
				EnemyBasics.Enemies[32].MinHealth = 80;
				EnemyBasics.Enemies[32].MaxHealth = 135;
				EnemyBasics.Enemies[32].Level = 20;
				EnemyBasics.Enemies[32].ArmorValue = -5;
				// Estimated Avoidance Modifier (Lower means harder to hit): -25 - (100/2) = -75
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-42) = 4%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-42) = 70%

				// Ancient Lich		
				EnemyBasics.Enemies[33].MinDamage = 40;
				EnemyBasics.Enemies[33].MaxDamage = 70;
				EnemyBasics.Enemies[33].MinHealth = 105;
				EnemyBasics.Enemies[33].MaxHealth = 185;
				EnemyBasics.Enemies[33].Level = 21;
				EnemyBasics.Enemies[33].ArmorValue = -8;
				// Estimated Avoidance Modifier (Lower means harder to hit): -40 - (100/2) = -90
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-45) = 1%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-45) = 67%
				
				//-------------------- Undead -------------------------------------
				
				//-------------------- Daedra -------------------------------------

				// Frost Daedra
				EnemyBasics.Enemies[25].MinDamage = 30;
				EnemyBasics.Enemies[25].MaxDamage = 55;
				EnemyBasics.Enemies[25].MinHealth = 85;
				EnemyBasics.Enemies[25].MaxHealth = 155;
				EnemyBasics.Enemies[25].Level = 17;
				EnemyBasics.Enemies[25].ArmorValue = -5;
				// Estimated Avoidance Modifier (Lower means harder to hit): -25 - (100/2) = -75
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-40) = 7%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-40) = 73%
				
				// Fire Daedra
				EnemyBasics.Enemies[26].MinDamage = 60;
				EnemyBasics.Enemies[26].MaxDamage = 85;
				EnemyBasics.Enemies[26].MinHealth = 55;
				EnemyBasics.Enemies[26].MaxHealth = 95;
				EnemyBasics.Enemies[26].Level = 17;
				EnemyBasics.Enemies[26].ArmorValue = 0;
				// Estimated Avoidance Modifier (Lower means harder to hit): 0 - (100/2) = -40
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-4) + (-34) = 11%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-2) + (-34) = 77%
				
				// Daedroth
				EnemyBasics.Enemies[27].MinDamage = 35;
				EnemyBasics.Enemies[27].MaxDamage = 75;
				EnemyBasics.Enemies[27].MinHealth = 65;
				EnemyBasics.Enemies[27].MaxHealth = 95;
				EnemyBasics.Enemies[27].Level = 18;
				EnemyBasics.Enemies[27].ArmorValue = 4;
				// Estimated Avoidance Modifier (Lower means harder to hit): 20 - (100/2) = -30
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-4) + (-34) = 11%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-2) + (-34) = 77%
				
				// Daedra Seducer
				EnemyBasics.Enemies[29].MinDamage = 35;
				EnemyBasics.Enemies[29].MaxDamage = 75;
				EnemyBasics.Enemies[29].MinHealth = 70;
				EnemyBasics.Enemies[29].MaxHealth = 100;
				EnemyBasics.Enemies[29].Level = 19;
				EnemyBasics.Enemies[29].ArmorValue = -8;
				// Estimated Avoidance Modifier (Lower means harder to hit): -40 - (100/2) = -90
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-4) + (-34) = 11%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-2) + (-34) = 77%
				
				// Daedra Lord		
				EnemyBasics.Enemies[31].MinDamage = 55;
				EnemyBasics.Enemies[31].MaxDamage = 110;
				EnemyBasics.Enemies[31].MinHealth = 175;
				EnemyBasics.Enemies[31].MaxHealth = 275;
				EnemyBasics.Enemies[31].Level = 21;
				EnemyBasics.Enemies[31].ArmorValue = -12;
				// Estimated Avoidance Modifier (Lower means harder to hit): -60 - (100/2) = -110
				// Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-4) + (-48) = -3%
				// Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-2) + (-48) = 63%
				
				//-------------------- Daedra -------------------------------------
				
				Debug.Log("PhysicalCombatAndArmorOverhaul: initialization Complete For Enemy Stat Values, Ralzar's Meaner Monsters Edited Module");
			}
			else
				Debug.Log("PhysicalCombatAndArmorOverhaul: Ralzar's Meaner Monsters Edited Module Disabled");

            if (ralzarRacesRedoneModifiers)
                Debug.Log("PhysicalCombatAndArmorOverhaul: Ralzar's Races Redone CalculateRacialModifiers Override Active");
            else
                Debug.Log("PhysicalCombatAndArmorOverhaul: Ralzar's Races Redone CalculateRacialModifiers Override Disabled");

            Debug.Log("Finished mod init: PhysicalCombatAndArmorOverhaul");
        }
		
		#endregion
		
		#region Overridden Base Methods
		
		public static int DamageModifier(int strength) // Fixes the inconsistency with Unity and Classic, changes the 5 to a 10 divider.
        {
            return (int)Mathf.Floor((float)(strength - 50) / 10f);
        }
		
		private static int CalculateAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, int enemyAnimStateRecord, int weaponAnimTime, DaggerfallUnityItem weapon)
		{
			if (attacker == null || target == null)
                return 0;

            int damageModifiers = 0;
            int damage = 0;
            int chanceToHitMod = 0;
            int backstabChance = 0;
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            short skillID = 0;
			bool unarmedAttack = false;
			bool weaponAttack = false;
			bool bluntWep = false;
			bool monsterArmorCheck = false;
			bool critSuccess = false;
			float critDamMulti = 1f;
			int critHitAddi = 0;
			
			EnemyEntity AITarget = null;
            AITarget = target as EnemyEntity;

            // Choose whether weapon-wielding enemies use their weapons or weaponless attacks.
            // In classic, weapon-wielding enemies use the damage values of their weapons
            // instead of their weaponless values.
            // For some enemies this gives lower damage than similar-tier monsters
            // and the weaponless values seems more appropriate, so here
            // enemies will choose to use their weaponless attack if it is more damaging.
            EnemyEntity AIAttacker = attacker as EnemyEntity;
            if (AIAttacker != null && weapon != null)
            {
                int weaponAverage = (weapon.GetBaseDamageMin() + weapon.GetBaseDamageMax()) / 2;
                int noWeaponAverage = (AIAttacker.MobileEnemy.MinDamage + AIAttacker.MobileEnemy.MaxDamage) / 2;

                if (noWeaponAverage > weaponAverage)
                {
                    // Use hand-to-hand
                    weapon = null;
                }
            }

            if (weapon != null)
            {
                // If the attacker is using a weapon, check if the material is high enough to damage the target
                if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                {
                    if (attacker == player)
                    {
                        DaggerfallUI.Instance.PopupMessage(HardStrings.materialIneffective);
                    }
                    return 0;
                }
                // Get weapon skill used
                skillID = weapon.GetWeaponSkillIDAsShort();
            }
            else
            {
                skillID = (short)DFCareer.Skills.HandToHand;
            }

			if (attacker == player)
			{
				int playerWeaponSkill = attacker.Skills.GetLiveSkillValue(skillID);
				playerWeaponSkill = (int)Mathf.Ceil(playerWeaponSkill*1.5f); // Makes it so player weapon skill has 150% of the effect it normally would on hit chance. So now instead of 50 weapon skill adding +50 to the end, 50 will now add +75.
				chanceToHitMod = playerWeaponSkill;
			}
			else
				chanceToHitMod = attacker.Skills.GetLiveSkillValue(skillID);

			if (critStrikeModuleCheck) // Applies the 'Critical Strikes Increase Damage' module if it is enabled in the settings.
			{
				if (attacker == player) // Crit modifiers, if true, for the player.
				{
					critSuccess = CriticalStrikeHandler(attacker); // Rolls for if the attacker is sucessful with a critical strike, if yes, critSuccess is set to 'true'.
					
					if (critSuccess)
					{
						critDamMulti = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 5);
						//Debug.LogFormat("1. critDamMulti From PLAYER Skills = {0}", critDamMulti);
						critHitAddi = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 4);
						//Debug.LogFormat("2. critHitAddi From PLAYER Skills = {0}", critHitAddi);
						
						critDamMulti = (critDamMulti*.05f) + 1;
						//Debug.LogFormat("3. Final critDamMulti From PLAYER Skills = {0}", critDamMulti);
						
						chanceToHitMod += critHitAddi; // Adds the critical success value to the 'chanceToHitMod'.
					}
				}
				else // Crit modifiers, if true, for monsters/enemies.
				{
					critSuccess = CriticalStrikeHandler(attacker); // Rolls for if the attacker is sucessful with a critical strike, if yes, critSuccess is set to 'true'.
					
					if (critSuccess)
					{
						critDamMulti = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 5);
						//Debug.LogFormat("1. critDamMulti From MONSTER Skills = {0}", critDamMulti);
						critHitAddi = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 10);
						//Debug.LogFormat("2. critHitAddi From MONSTER Skills = {0}", critHitAddi);
						
						critDamMulti = (critDamMulti*.025f) + 1;
						//Debug.LogFormat("3. Final critDamMulti From MONSTER Skills = {0}", critDamMulti);
						
						chanceToHitMod += critHitAddi; // Adds the critical success value to the 'chanceToHitMod'.
					}
				}
			}
			
            if (attacker == player)
            {
				// Apply swing modifiers
				ToHitAndDamageMods swingMods = CalculateSwingModifiers(GameManager.Instance.WeaponManager.ScreenWeapon);
				damageModifiers += swingMods.damageMod;
				chanceToHitMod += swingMods.toHitMod;
				
				// Apply proficiency modifiers
				ToHitAndDamageMods proficiencyMods = CalculateProficiencyModifiers(attacker, weapon);
				damageModifiers += proficiencyMods.damageMod;
				chanceToHitMod += proficiencyMods.toHitMod;

				// Apply racial bonuses
				ToHitAndDamageMods racialMods = CalculateRacialModifiers(attacker, weapon, player);
				damageModifiers += racialMods.damageMod;
				chanceToHitMod += racialMods.toHitMod;

				backstabChance = CalculateBackstabChance(player, null, enemyAnimStateRecord);
				chanceToHitMod += backstabChance;
            }

            // Choose struck body part
            int struckBodyPart = CalculateStruckBodyPart();

            // Get damage for weaponless attacks
            if (skillID == (short)DFCareer.Skills.HandToHand)
            {
				unarmedAttack = true; // Check for later if weapon is NOT being used.
				
                if (attacker == player || (AIAttacker != null && AIAttacker.EntityType == EntityTypes.EnemyClass))
                {
                    if (CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                    {
                        damage = FormulaHelper.CalculateHandToHandAttackDamage(attacker, target, damageModifiers, attacker == player);
						
                        damage = CalculateBackstabDamage(damage, backstabChance);
                    }
                }
                else if (AIAttacker != null) // attacker is a monster
                {
                    // Handle multiple attacks by AI
					int minBaseDamage = 0;
                    int maxBaseDamage = 0;
                    int attackNumber = 0;
                    while (attackNumber < 3) // Classic supports up to 5 attacks but no monster has more than 3
                    {
                        if (attackNumber == 0)
                        {
                            minBaseDamage = AIAttacker.MobileEnemy.MinDamage;
                            maxBaseDamage = AIAttacker.MobileEnemy.MaxDamage;
                        }
                        else if (attackNumber == 1)
                        {
                            minBaseDamage = AIAttacker.MobileEnemy.MinDamage2;
                            maxBaseDamage = AIAttacker.MobileEnemy.MaxDamage2;
                        }
                        else if (attackNumber == 2)
                        {
                            minBaseDamage = AIAttacker.MobileEnemy.MinDamage3;
                            maxBaseDamage = AIAttacker.MobileEnemy.MaxDamage3;
                        }

                        int reflexesChance = 50 - (10 * ((int)player.Reflexes - 2));

                        if (DFRandom.rand() % 100 < reflexesChance && minBaseDamage > 0 && CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                        {
                            int hitDamage = UnityEngine.Random.Range(minBaseDamage, maxBaseDamage + 1);
                            // Apply special monster attack effects
                            if (hitDamage > 0)
                                FormulaHelper.OnMonsterHit(AIAttacker, target, hitDamage);

                            damage += hitDamage;
                        }
                        ++attackNumber;
                    }
                }
            }
            // Handle weapon attacks
            else if (weapon != null)
            {
				weaponAttack = true; // Check for later on if weapon is being used.
				
                // Apply weapon material modifier.
				chanceToHitMod += CalculateWeaponToHit(weapon);
				
				// Mod hook for adjusting final hit chance mod. (is a no-op in DFU)
				if (archeryModuleCheck)
					chanceToHitMod = AdjustWeaponHitChanceMod(attacker, target, chanceToHitMod, weaponAnimTime, weapon);

                if (CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                {	
                    damage = CalculateWeaponAttackDamage(attacker, target, damageModifiers, weaponAnimTime, weapon);

                    damage = CalculateBackstabDamage(damage, backstabChance);
                }

                // Handle poisoned weapons
                if (damage > 0 && weapon.poisonType != Poisons.None)
                {
                    FormulaHelper.InflictPoison(target, weapon.poisonType, false);
                    weapon.poisonType = Poisons.None;
                }
            }

            damage = Mathf.Max(0, damage); // I think this is just here to keep damage from outputting a negative value.
			
			//Debug.LogFormat("4. Here is damage value before crit modifier is applied = {0}", damage);
			
			if (critSuccess) // Since the critSuccess variable only ever becomes true inside when the module is active, this is always false when that module is disabled.
			{
				damage = (int)Mathf.Round(damage*critDamMulti); // Multiplies 'Final' damage values, before reductions, with the critical damage multiplier.
				//Debug.LogFormat("5. Here is damage value AFTER crit modifier is applied = {0}", damage);
			}
			
			int targetEndur = target.Stats.LiveEndurance - 50;
			int targetStren = target.Stats.LiveStrength - 50; // Every point of these does something, positive and negative between 50.
			int targetWillp = target.Stats.LiveWillpower - 50;
			
			float naturalDamResist = (targetEndur*.002f);
			naturalDamResist += (targetStren*.001f);
			naturalDamResist += (targetWillp*.001f);
			
			Mathf.Clamp(naturalDamResist, -0.2f, 0.2f); // This is to keep other mods that allow over 100 attribute points from allowing damage reduction values to go over 20%. May actually remove this cap for monsters, possibly, since some of the higher level ones have over 100 attribute points.
			//Debug.LogFormat("Natural Damage Resist = {0}", naturalDamResist);
			
			DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand); // Checks if character is using a shield or not.
			bool shieldStrongSpot = false;
			shieldBlockSuccess = false;
			if (shield != null)
			{
				BodyParts[] protectedBodyParts = shield.GetShieldProtectedBodyParts();

				for (int i = 0; (i < protectedBodyParts.Length) && !shieldStrongSpot; i++)
				{
					if (protectedBodyParts[i] == (BodyParts)struckBodyPart)
						shieldStrongSpot = true;
				}
				shieldBlockSuccess = ShieldBlockChanceCalculation(target, shieldStrongSpot, shield);
				
				if (shieldBlockSuccess)
					shieldBlockSuccess = CompareShieldToUnderArmor(target, struckBodyPart, naturalDamResist);
			}
			
			DamageEquipment(attacker, target, damage, weapon, struckBodyPart); // Might alter this later so that equipment damage is only calculated with the amount that was reduced, not the whole initial amount, will see.
			
			if(((target != player) && (AITarget.EntityType == EntityTypes.EnemyMonster)))
			{
				monsterArmorCheck = ArmorStruckVerification(target, struckBodyPart); // Check for if a monster has a piece of armor/shield hit by an attack, returns true if so.
				
				if(!monsterArmorCheck)
				{
					//Debug.Log("------------------------------------------------------------------------------------------");
					//Debug.LogFormat("Here is damage value before Monster 'Natural' Damage reduction is applied = {0}", damage);

					damage = PercentageReductionCalculationForMonsters(attacker, target, damage, bluntWep, naturalDamResist);
					
					//Debug.LogFormat("Here is damage value after Monster 'Natural' Damage reduction = {0}", damage);
					//Debug.Log("------------------------------------------------------------------------------------------");
				}
				else
				{
					if (unarmedAttack)
					{
						//Debug.Log("------------------------------------------------------------------------------------------");
						//Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);

						damage = CalculateArmorDamageReductionWithUnarmed(attacker, target, damage, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against unarmed.
						
						//Debug.LogFormat("Here is damage value after armor reduction = {0}", damage);
						//Debug.Log("------------------------------------------------------------------------------------------");
					}
					else if (weaponAttack)
					{
						//Debug.Log("------------------------------------------------------------------------------------------");
						//Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);

						damage = CalculateArmorDamageReductionWithWeapon(attacker, target, damage, weapon, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against weapons.
						
						//Debug.LogFormat("Here is damage value after armor reduction = {0}", damage);
						//Debug.Log("------------------------------------------------------------------------------------------");
					}
				}
			}
			else
			{
				if (unarmedAttack)
				{
                    Debug.Log("------------------------------------------------------------------------------------------");
					Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);
                    int damBefore = damage;

					damage = CalculateArmorDamageReductionWithUnarmed(attacker, target, damage, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against unarmed.

					int damAfter = damage;
					Debug.LogFormat("Here is damage value after armor reduction = {0}", damage);
					if (damBefore > 0)
					{
						int damReduPercent = ((100 * damAfter / damBefore) - 100) * -1;
						Debug.LogFormat("Here is damage reduction percent = {0}%", damReduPercent);
					}
                    Debug.Log("------------------------------------------------------------------------------------------");
                }
				else if (weaponAttack)
				{
                    Debug.Log("------------------------------------------------------------------------------------------");
                    Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);
                    int damBefore = damage;

                    damage = CalculateArmorDamageReductionWithWeapon(attacker, target, damage, weapon, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against weapons.

					int damAfter = damage;
                    Debug.LogFormat("Here is damage value after armor reduction = {0}", damage);
					if (damBefore > 0)
					{
						int damReduPercent = ((100 * damAfter / damBefore) - 100) * -1;
						Debug.LogFormat("Here is damage reduction percent = {0}%", damReduPercent);
					}
                    Debug.Log("------------------------------------------------------------------------------------------");
                }
			}

            // Apply Ring of Namira effect
            if (target == player)
            {
                DaggerfallUnityItem[] equippedItems = target.ItemEquipTable.EquipTable;
                DaggerfallUnityItem item = null;
                if (equippedItems.Length != 0)
                {
                    if (IsRingOfNamira(equippedItems[(int)EquipSlots.Ring0]) || IsRingOfNamira(equippedItems[(int)EquipSlots.Ring1]))
                    {
                        IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(RingOfNamiraEffect.EffectKey);
                        effectTemplate.EnchantmentPayloadCallback(EnchantmentPayloadFlags.None,
                            targetEntity: AIAttacker.EntityBehaviour,
                            sourceItem: item,
                            sourceDamage: damage);
                    }
                }
            }

            //Debug.LogFormat("Damage {0} applied, animTime={1}  ({2})", damage, weaponAnimTime, GameManager.Instance.WeaponManager.ScreenWeapon.WeaponState);

            return damage;
		}
		
		private static ToHitAndDamageMods CalculateSwingModifiers(FPSWeapon onscreenWeapon)
		{
			ToHitAndDamageMods mods = new ToHitAndDamageMods();
            if (onscreenWeapon != null)
            {
                // The Daggerfall manual groups diagonal slashes to the left and right as if they are the same, but they are different.
                // Classic does not apply swing modifiers to unarmed attacks.
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeUp)
                {
                    mods.damageMod = -4;
                    mods.toHitMod = 10;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDownRight)
                {
                    mods.damageMod = -2;
                    mods.toHitMod = 5;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDownLeft)
                {
                    mods.damageMod = 2;
                    mods.toHitMod = -5;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDown)
                {
                    mods.damageMod = 4;
                    mods.toHitMod = -10;
                }
            }
            return mods;
		}
		
		public static ToHitAndDamageMods CalculateProficiencyModifiers(DaggerfallEntity attacker, DaggerfallUnityItem weapon)
		{
			ToHitAndDamageMods mods = new ToHitAndDamageMods();
            if (weapon != null)
            {
                // Apply weapon proficiency
                if (((int)attacker.Career.ExpertProficiencies & weapon.GetWeaponSkillUsed()) != 0)
                {
                    mods.damageMod = (attacker.Level / 2) + 1; // Buffed from /3 to /2
                    mods.toHitMod = attacker.Level;
                }
            }
            // Apply hand-to-hand proficiency. Hand-to-hand proficiency is not applied in classic.
            else if (((int)attacker.Career.ExpertProficiencies & (int)DFCareer.ProficiencyFlags.HandToHand) != 0)
            {
                mods.damageMod = (attacker.Level / 2) + 1; // Buffed from /3 to /2
                mods.toHitMod = attacker.Level;
            }
            return mods;
		}
		
		public static ToHitAndDamageMods CalculateRacialModifiers(DaggerfallEntity attacker, DaggerfallUnityItem weapon, PlayerEntity player)
		{
			ToHitAndDamageMods mods = new ToHitAndDamageMods();
            if (ralzarRacesRedoneModifiers)
            {
                mods.damageMod = RacesRedone.RacesRedone.RacesRedone_DamageMod(attacker, weapon, player);
                mods.toHitMod = RacesRedone.RacesRedone.RacesRedone_ToHitMod(attacker, weapon, player);
            }
            else
            {
                if (weapon != null)
                {
                    if (player.RaceTemplate.ID == (int)Races.DarkElf)
                    {
                        mods.damageMod = attacker.Level / 2; // Buffed Racial Mod from /4 to /2
                        mods.toHitMod = attacker.Level / 2; // Buffed Racial Mod from /4 to /2
                    }
                    else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.Archery)
                    {
                        if (player.RaceTemplate.ID == (int)Races.WoodElf)
                        {
                            mods.damageMod = attacker.Level / 3;
                            mods.toHitMod = attacker.Level / 1; // Buffed Racial Mod from /3 to /1
                        }
                    }
                    else if (player.RaceTemplate.ID == (int)Races.Redguard)
                    {
                        mods.damageMod = attacker.Level / 3;
                        mods.toHitMod = attacker.Level / 1; // Buffed Racial Mod from /3 to /1
                    }
                }
            }
            return mods;
		}
		
		/// <summary>Struct for return values of formula that affect damage and to-hit chance.</summary>
        public struct ToHitAndDamageMods
        {
            public int damageMod;
            public int toHitMod;
        }
		
		public static int CalculateWeaponToHit(DaggerfallUnityItem weapon)
		{
			return weapon.GetWeaponMaterialModifier() * 2 + 2;
			
		}
		
		private static int CalculateWeaponAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, int damageModifier, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            int damage = UnityEngine.Random.Range(weapon.GetBaseDamageMin(), weapon.GetBaseDamageMax() + 1) + damageModifier;
			
            EnemyEntity AITarget = null;
            if (target != GameManager.Instance.PlayerEntity)
            {
                AITarget = target as EnemyEntity;
                if (AITarget.CareerIndex == (int)MonsterCareers.SkeletalWarrior)
                {
                    // Apply edged-weapon damage modifier for Skeletal Warrior
                    if ((weapon.flags & 0x10) == 0)
                        damage /= 2;

                    // Apply silver weapon damage modifier for Skeletal Warrior
                    // Arena applies a silver weapon damage bonus for undead enemies, which is probably where this comes from.
                    if (weapon.NativeMaterialValue == (int)WeaponMaterialTypes.Silver)
                        damage *= 2;
                }
				// Has most of the "obvious" enemies take extra damage from silver weapons, most of the lower level undead, as well as werebeasts.
				else if (AITarget.CareerIndex == (int)MonsterCareers.Werewolf || AITarget.CareerIndex == (int)MonsterCareers.Ghost || AITarget.CareerIndex == (int)MonsterCareers.Wraith || AITarget.CareerIndex == (int)MonsterCareers.Vampire || AITarget.CareerIndex == (int)MonsterCareers.Mummy || AITarget.CareerIndex == (int)MonsterCareers.Wereboar)
				{
					if (weapon.NativeMaterialValue == (int)WeaponMaterialTypes.Silver)
                        damage *= 2;
				}
            }
            // TODO: Apply strength bonus from Mace of Molag Bal

            // Apply strength modifier
			if (ItemEquipTable.GetItemHands(weapon) == ItemHands.Both && weapon.TemplateIndex != (int)Weapons.Short_Bow && weapon.TemplateIndex != (int)Weapons.Long_Bow)
				damage += (DamageModifier(attacker.Stats.LiveStrength)) * 2; // Multiplying by 2, so that two-handed weapons gets double the damage mod from Strength, except bows.
			else
				damage += DamageModifier(attacker.Stats.LiveStrength);

            // Apply material modifier.
            // The in-game display in Daggerfall of weapon damages with material modifiers is incorrect. The material modifier is half of what the display suggests.
            damage += weapon.GetWeaponMaterialModifier();
            if (damage < 1)
                damage = 0;

            damage += GetBonusOrPenaltyByEnemyType(attacker, AITarget);
			
			// Mod hook for adjusting final damage. (is a no-op in DFU)
			if (archeryModuleCheck)
				damage = AdjustWeaponAttackDamage(attacker, target, damage, weaponAnimTime, weapon);

            return damage;
        }
		
		/// Calculates whether an attack on a target is successful or not.
		private static bool CalculateSuccessfulHit(DaggerfallEntity attacker, DaggerfallEntity target, int chanceToHitMod, int struckBodyPart)
		{
			PlayerEntity player = GameManager.Instance.PlayerEntity;
			
			if (attacker == null || target == null)
                return false;

            int chanceToHit = chanceToHitMod;
			//Debug.LogFormat("Starting chanceToHitMod = {0}", chanceToHit);
			
			// Get armor value for struck body part
            chanceToHit += CalculateArmorToHit(target, struckBodyPart);
			
			// Apply adrenaline rush modifiers.
            chanceToHit += CalculateAdrenalineRushToHit(attacker, target);

            // Apply enchantment modifier. 
            chanceToHit += attacker.ChanceToHitModifier;
			//Debug.LogFormat("Attacker Chance To Hit Mod 'Enchantment' = {0}", attacker.ChanceToHitModifier); // No idea what this does, always seeing 0.
			
			// Apply stat differential modifiers. (default: luck and agility)
			chanceToHit += CalculateStatDiffsToHit(attacker, target);
			
			// Apply skill modifiers. (default: dodge and crit strike)
            chanceToHit += CalculateSkillsToHit(attacker, target);
			//Debug.LogFormat("After Dodge = {0}", chanceToHitMod);
			
			// Apply monster modifier and biography adjustments.
            chanceToHit += CalculateAdjustmentsToHit(attacker, target);
			//Debug.LogFormat("Final chanceToHitMod = {0}", chanceToHitMod);

            Mathf.Clamp(chanceToHit, 3, 97);

            return Dice100.SuccessRoll(chanceToHit);
        }
		
		public static int CalculateArmorToHit(DaggerfallEntity target, int struckBodyPart)
		{
			EnemyEntity AITarget = null;
            AITarget = target as EnemyEntity;
			PlayerEntity player = GameManager.Instance.PlayerEntity;
			int armorValue = 0;
			
			// Get armor value for struck body part. This value is multiplied by 5 times in the "EnemyEntity.cs" script, makes a big difference.
            if (struckBodyPart <= target.ArmorValues.Length)
            {
                armorValue = target.ArmorValues[struckBodyPart];
            }
			
			// Sets the armorValue so that armor does not have any effect on the hit chance, it just defaults to the "naked" amount for the player and humanoid enemies, other monsters still have their normal AC score factored in.
			if (target == player)
				armorValue = 100 - target.IncreasedArmorValueModifier - target.DecreasedArmorValueModifier;
			else if (AITarget.EntityType == EntityTypes.EnemyClass)
				armorValue = 60;

            return armorValue;
		}
		
		public static int CalculateAdrenalineRushToHit(DaggerfallEntity attacker, DaggerfallEntity target)
		{
			const int adrenalineRushModifier = 8; //Buffed base adrenalineRushModifier by 3
            const int improvedAdrenalineRushModifier = 12; //Buffed improvedAdrenalineRushModifier by 4

            int chanceToHitMod = 0;
            if (attacker.Career.AdrenalineRush && attacker.CurrentHealth < (attacker.MaxHealth / 6)) //Made adrenaline rush effect come into effect earlier, I.E. at higher health percent. From /8 to /6
            {
                chanceToHitMod += (attacker.ImprovedAdrenalineRush) ? improvedAdrenalineRushModifier : adrenalineRushModifier;
            }

            if (target.Career.AdrenalineRush && target.CurrentHealth < (target.MaxHealth / 6)) //Made adrenaline rush effect come into effect earlier, I.E. at higher health percent. From /8 to /6
            {
                chanceToHitMod -= (target.ImprovedAdrenalineRush) ? improvedAdrenalineRushModifier : adrenalineRushModifier;
            }
            return chanceToHitMod;
		}
		
		public static int CalculateStatDiffsToHit(DaggerfallEntity attacker, DaggerfallEntity target)
		{
			int chanceToHitMod = 0;

			// Apply luck modifier.
            chanceToHitMod += ((attacker.Stats.LiveLuck - target.Stats.LiveLuck) / 10);
			//Debug.LogFormat("After Luck = {0}", chanceToHitMod);

            // Apply agility modifier.
            chanceToHitMod += ((attacker.Stats.LiveAgility - target.Stats.LiveAgility) / 4); //Made Agility have twice as much effect on final hit chance.
			//Debug.LogFormat("After Agility = {0}", chanceToHitMod);
			
			// Possibly make the Speed Stat a small factor as well, seems like it would make sense.
			chanceToHitMod += ((attacker.Stats.LiveSpeed - target.Stats.LiveSpeed) / 8);
			//Debug.LogFormat("After Speed = {0}", chanceToHitMod);
			
			// When I think about it, I might want to get some of the other stats into this formula as well, to help casters somewhat, as well as explain it like a more intelligent character notices patterns in enemy movement and uses to to get in more hits, maybe even strength, the character strikes with such force that they pierce through armor easier.

			// Apply flat Luck factor for the target's chance of being hit. Higher luck above 50 means enemies will miss you more, and below 50 will mean they hit you more often.
			chanceToHitMod -= (int)Mathf.Round((float)(target.Stats.LiveLuck - 50) / 10); // With this, at most Luck will effect chances by either -5 or +5.
			
			return chanceToHitMod;
		}
		
		public static int CalculateSkillsToHit(DaggerfallEntity attacker, DaggerfallEntity target)
		{
			PlayerEntity player = GameManager.Instance.PlayerEntity;
			int chanceToHitMod = 0;

            // Apply dodging modifier.
            // This modifier is bugged in classic and the attacker's dodging skill is used rather than the target's.
            // DF Chronicles says the dodging calculation is (dodging / 10), but it actually seems to be (dodging / 4).
            // Apply dodging modifier.
            chanceToHitMod -= (target.Skills.GetLiveSkillValue(DFCareer.Skills.Dodging) / 2); // Changing 4 to a 2, so 100 dodge will give -50 to hit chance, very powerful.
			
			if (!critStrikeModuleCheck) // Applies the mostly unmodified version of critical strike if the module in settings for the redone version is disabled.
			{
				// Apply critical strike modifier. For Player only.
				if (attacker == player)
				{
					if (Dice100.SuccessRoll(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 3)) // Now for player, at 100 crit skill, their rolls will be 33%
					{
						chanceToHitMod += (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 3); // Now for player, at 100 crit skill, "crit" success will give 33 more hit-mod
						//DaggerfallUI.Instance.PopupMessage("A Devastating Strike!"); // Turned off seems pretty annoying, even with the lower crit roll chance, for now at least.
					}
					//Debug.LogFormat("After Crit, Player = {0}", chanceToHitMod); // May consider eventually moving/adding the Critical Strike equation to the damage modifier formula, likely not in 1.0 of this mod, but maybe later on.
				}
				// Apply critical strike modifier. For Enemies.
				else
				{
					if (Dice100.SuccessRoll(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike))) // For monsters, works like normal. 100% chance at 100 skill so lvl 14 and >
					{
						chanceToHitMod += (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 10); // For monsters, works like normal. "crit" success will give 10 more hit-mod
					}
					//Debug.LogFormat("After Crit, Monster = {0}", chanceToHitMod);
				}
			}
			return chanceToHitMod;
		}
		
		private static int CalculateAdjustmentsToHit(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            EnemyEntity AITarget = target as EnemyEntity;

            int chanceToHitMod = 0;

            // Apply hit mod from character biography. This gives -5 to player chances to not be hit if they say they have trouble "Fighting and Parrying"
            if (target == player)
            {
                chanceToHitMod -= player.BiographyAvoidHitMod;
            }

            // Apply monster modifier.
            if ((target != player) && (AITarget.EntityType == EntityTypes.EnemyMonster))
            {
                chanceToHitMod += 50; // Changed from 40 to 50, +10, in since i'm going to make dodging have double the effect, as well as nerf weapon material hit mod more.
            }

            // DF Chronicles says -60 is applied at the end, but it actually seems to be -50.
            chanceToHitMod -= 50;

            return chanceToHitMod;
			
			
        }
		
		/// Allocate any equipment damage from a strike, and reduce item condition.
		private static bool DamageEquipment(DaggerfallEntity attacker, DaggerfallEntity target, int damage, DaggerfallUnityItem weapon, int struckBodyPart)
        {
			int atkStrength = attacker.Stats.LiveStrength;
			int tarMatMod = 0;
			int matDifference = 0;
			bool bluntWep = false;
			bool shtbladeWep = false;
			bool missileWep = false;
			int wepEqualize = 1;
			int wepWeight = 1;
			float wepDamResist = 1f;
			float armorDamResist = 1f;
			
			if (!armorHitFormulaModuleCheck) // Uses the regular shield formula if the "armorHitFormula" Module is disabled in settings, but the equipment damage module is still active.
			{
				DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand); // Checks if character is using a shield or not.
				shieldBlockSuccess = false;
				if (shield != null)
				{
					BodyParts[] protectedBodyParts = shield.GetShieldProtectedBodyParts();

					for (int i = 0; (i < protectedBodyParts.Length) && !shieldBlockSuccess; i++)
					{
						if (protectedBodyParts[i] == (BodyParts)struckBodyPart)
							shieldBlockSuccess = true;
					}
				}
			}
			
            // If damage was done by a weapon, damage the weapon and armor of the hit body part.
            if (weapon != null && damage > 0)
			{
				int atkMatMod = weapon.GetWeaponMaterialModifier() + 2;
				int wepDam = damage;
				wepEqualize = EqualizeMaterialConditions(weapon);
				wepDam *= wepEqualize;
		
				if (weapon.GetWeaponSkillIDAsShort() == 32) // Checks if the weapon being used is in the Blunt Weapon category, then sets a bool value to true.
				{
					wepDam += (atkStrength / 10);
					wepDamResist = (wepEqualize*.20f) + 1;
					wepDam = (int)Mathf.Ceil(wepDam/wepDamResist);
					bluntWep = true;
					wepWeight = (int)Mathf.Ceil(weapon.EffectiveUnitWeightInKg());
					
					ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
				}
				else if (weapon.GetWeaponSkillIDAsShort() == 28) // Checks if the weapon being used is in the Short Blade category, then sets a bool value to true.
				{
					if (weapon.TemplateIndex == (int)Weapons.Dagger || weapon.TemplateIndex == (int)Weapons.Tanto)
					{
						wepDam += (atkStrength / 30);
						wepDamResist = (wepEqualize*.90f) + 1;
						wepDam = (int)Mathf.Ceil(wepDam/wepDamResist);
						shtbladeWep = true;
					}
					else
					{
						wepDam += (atkStrength / 30);
						wepDamResist = (wepEqualize*.30f) + 1;
						wepDam = (int)Mathf.Ceil(wepDam/wepDamResist);
						shtbladeWep = true;
					}
					
					ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
				}
				else if (weapon.GetWeaponSkillIDAsShort() == 33) // Checks if the weapon being used is in the Missile Weapon category, then sets a bool value to true.
				{
					missileWep = true;
					
					ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
				}
				else // If all other weapons categories have not been found, it defaults to this, which currently includes long blades and axes.
				{
					wepDam += (atkStrength / 10);
					wepDamResist = (wepEqualize*.20f) + 1;
					wepDam = (int)Mathf.Ceil(wepDam/wepDamResist);
					
					ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
				}

				if (attacker == GameManager.Instance.PlayerEntity)
					WarningMessagePlayerEquipmentCondition(weapon);

				if (shieldBlockSuccess)
				{
					DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
					int shieldEqualize = EqualizeMaterialConditions(shield);
					damage *= shieldEqualize;
					tarMatMod = ArmorMaterialModifierFinder(shield);
					matDifference = tarMatMod - atkMatMod;
					damage = MaterialDifferenceDamageCalculation(shield, matDifference, atkStrength, damage, bluntWep, wepWeight, shieldBlockSuccess);
					
					ApplyConditionDamageThroughWeaponDamage(shield, target, damage, bluntWep, shtbladeWep, missileWep, wepEqualize);
					
					if (target == GameManager.Instance.PlayerEntity)
						WarningMessagePlayerEquipmentCondition(shield);
				}
				else
				{
					EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
					DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
					if (armor != null)
					{
						int armorEqualize = EqualizeMaterialConditions(armor);
						damage *= armorEqualize;
						tarMatMod = ArmorMaterialModifierFinder(armor);
						matDifference = tarMatMod - atkMatMod;
						damage = MaterialDifferenceDamageCalculation(armor, matDifference, atkStrength, damage, bluntWep, wepWeight, shieldBlockSuccess);
						
						ApplyConditionDamageThroughWeaponDamage(armor, target, damage, bluntWep, shtbladeWep, missileWep, wepEqualize);
						
						if (target == GameManager.Instance.PlayerEntity)
							WarningMessagePlayerEquipmentCondition(armor);
					}
				}
				return false;
			}
			else if (weapon == null && damage > 0) // Handles Unarmed attacks.
			{
				if (shieldBlockSuccess)
				{
					DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
					int shieldEqualize = EqualizeMaterialConditions(shield);
					damage *= shieldEqualize;
					tarMatMod = ArmorMaterialModifierFinder(shield);
					atkStrength /= 5;
					armorDamResist = (tarMatMod*.40f) + 1;
					damage = (int)Mathf.Ceil((damage + atkStrength)/armorDamResist);
					
					ApplyConditionDamageThroughUnarmedDamage(shield, target, damage);
					
					if (target == GameManager.Instance.PlayerEntity)
						WarningMessagePlayerEquipmentCondition(shield);
				}
				else
				{
					EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
					DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
					if (armor != null)
					{
						int armorEqualize = EqualizeMaterialConditions(armor);
						damage *= armorEqualize;
						tarMatMod = ArmorMaterialModifierFinder(armor);
						atkStrength /= 5;
						armorDamResist = (tarMatMod*.20f) + 1;
						damage = (int)Mathf.Ceil((damage + atkStrength)/armorDamResist);
						
						ApplyConditionDamageThroughUnarmedDamage(armor, target, damage);
						
						if (target == GameManager.Instance.PlayerEntity)				
							WarningMessagePlayerEquipmentCondition(armor);
					}
				}
				return false;
			}
			return false;
        }
		
		private static int AdjustWeaponHitChanceMod(DaggerfallEntity attacker, DaggerfallEntity target, int hitChanceMod, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            if (weaponAnimTime > 0 && (weapon.TemplateIndex == (int)Weapons.Short_Bow || weapon.TemplateIndex == (int)Weapons.Long_Bow))
            {
                int adjustedHitChanceMod = hitChanceMod;
                if (weaponAnimTime < 200)
                    adjustedHitChanceMod -= 40;
                else if (weaponAnimTime < 500)
                    adjustedHitChanceMod -= 10;
                else if (weaponAnimTime < 1000)
                    adjustedHitChanceMod = hitChanceMod;
                else if (weaponAnimTime < 2000)
                    adjustedHitChanceMod += 10;
                else if (weaponAnimTime > 5000)
                    adjustedHitChanceMod -= 10;
                else if (weaponAnimTime > 8000)
                    adjustedHitChanceMod -= 20;

                //Debug.LogFormat("Adjusted Weapon HitChanceMod for bow drawing from {0} to {1} (t={2}ms)", hitChanceMod, adjustedHitChanceMod, weaponAnimTime);
                return adjustedHitChanceMod;
            }

            return hitChanceMod;
        }
		
		private static int AdjustWeaponAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, int damage, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            if (weaponAnimTime > 0 && (weapon.TemplateIndex == (int)Weapons.Short_Bow || weapon.TemplateIndex == (int)Weapons.Long_Bow))
            {
                double adjustedDamage = damage;
                if (weaponAnimTime < 800)
                    adjustedDamage *= (double)weaponAnimTime / 800;
                else if (weaponAnimTime < 5000)
                    adjustedDamage = damage;
                else if (weaponAnimTime < 6000)
                    adjustedDamage *= 0.85;
                else if (weaponAnimTime < 8000)
                    adjustedDamage *= 0.75;
                else if (weaponAnimTime < 9000)
                    adjustedDamage *= 0.5;
                else if (weaponAnimTime >= 9000)
                    adjustedDamage *= 0.25;

                //Debug.LogFormat("Adjusted Weapon Damage for bow drawing from {0} to {1} (t={2}ms)", damage, (int)adjustedDamage, weaponAnimTime);
                return (int)adjustedDamage;
            }

            return damage;
        }
		
		#endregion
		
		#region Mod Specific Methods
		
		private static int CalculateArmorDamageReductionWithWeapon(DaggerfallEntity attacker, DaggerfallEntity target, int damage, DaggerfallUnityItem weapon, int struckBodyPart, float naturalDamResist)
		{
			int atkStrength = attacker.Stats.LiveStrength;
			int armorMaterial = 0;
			bool bluntWep = false;
			int wepWeight = 1;
			
			
			if (weapon.GetWeaponSkillIDAsShort() == 32) // Checks if the weapon being used is in the Blunt Weapon category, then sets a bool value to true.
			{
				bluntWep = true;
				wepWeight = (int)Mathf.Ceil(weapon.EffectiveUnitWeightInKg());
			}
			
			//int atkMatMod = weapon.GetWeaponMaterialModifier() + 2; // Probably use with more 'realistic' version of this mod.

			if (shieldBlockSuccess)
			{
				DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
				armorMaterial = ArmorMaterialIdentifier(shield);
				
				damage = PercentageReductionCalculationWithWeapon(shield, armorMaterial, atkStrength, damage, bluntWep, wepWeight, shieldBlockSuccess, naturalDamResist);
			}
			else
			{
				EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
				DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
				if (armor != null)
				{
					armorMaterial = ArmorMaterialIdentifier(armor);

					damage = PercentageReductionCalculationWithWeapon(armor, armorMaterial, atkStrength, damage, bluntWep, wepWeight, shieldBlockSuccess, naturalDamResist);
				}
				else // If the body part struck in 'naked' IE has no armor or shield protecting it.
				{
					damage = (int)Mathf.Round(damage*(1f - naturalDamResist));
					//Debug.LogFormat("Here is damage value after 'Natural' Damage reduction = {0}", damage);
				}
			}
			return damage;
		}
			// Once I make it easier to add onto damage reduction values, I should take weapon weight into consideration and have it so heavier weapons do more damage resistance penetration, this would somewhat help in balancing out weapons that are just clearly better than others. Like the broadsword having less damage and weighing more than the longsword, that does not really make much sense to me, the broadsword does look a lot cooler at least.
			// Possibly try and make it so getting hit in different parts of the body can do different things like bonus damage for the attack or something around that.
			// If I do make it so the player hits monsters more often, I will want to also make it so knock-back is tweeked heavily so that enemies can't just be super easily stun-locked by any weapon.
			// Will probably do some tests with and without my damage reduction mod, and try and see what the "Time Till Death" is for both. Maybe my mod will make death quicker, maybe it will make it slower compared to non-modded daggerfall.
			// Something else I would like to add onto this mod eventually. Make it so, that when an attack is reduced from any amount that did not start as zero, to zero after damage reduction, make a sound that is different than the normal "miss" "woosh" sound, make one that sounds like something clanging off a metal surface or something of the like. To give an indication that armor/shield has completely negated an attacked.
			// As this mod develops, I will definitely want/have to convert the "percentage" reduction amounts to an array or something, so I would more readily be able to add/subtract from these values on the fly, for situations like adding other variables into play that would chance the damage resistance amount on a per-attack basis.
		
		private static int CalculateArmorDamageReductionWithUnarmed(DaggerfallEntity attacker, DaggerfallEntity target, int damage, int struckBodyPart, float naturalDamResist)
		{
			int atkStrength = attacker.Stats.LiveStrength;
			int armorMaterial = 0;

			if (shieldBlockSuccess)
			{
				DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
				armorMaterial = ArmorMaterialIdentifier(shield);
				
				damage = PercentageReductionCalculationWithUnarmed(shield, armorMaterial, atkStrength, damage, shieldBlockSuccess, naturalDamResist);
			}
			else
			{
				EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
				DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
				if (armor != null)
				{
					armorMaterial = ArmorMaterialIdentifier(armor);
					
					damage = PercentageReductionCalculationWithUnarmed(armor, armorMaterial, atkStrength, damage, shieldBlockSuccess, naturalDamResist);
				}
				else // If the body part struck in 'naked' IE has no armor or shield protecting it.
				{
					damage = (int)Mathf.Round(damage*(1f - naturalDamResist));
					//Debug.LogFormat("Here is damage value after 'Natural' Damage reduction = {0}", damage);
				}
			}
			return damage;
		}
		
		/// Does most of the calculations determining how much an attack gets reduced by a piece of armor, if no weapon is being used to attack with.
		private static int PercentageReductionCalculationWithUnarmed(DaggerfallUnityItem item, int armorMaterial, int atkStrength, int damage, bool shieldCheck, float naturalDamResist)
		{
			bool unarmedCheck = true;
			bool bluntWep = false;
			int wepWeight = 1;
			
			
			if (shieldCheck) // This part is a bit more difficult than I expected, I think i'll just have to make the shields a flat % reduction, regardless of material and what is being worn under the shield. Base this on the type of shield being used, also make blunt do more against shields than other types. Later on, i'll want to improve this to consider what is under the shield, as well as other factors, just do this for the time being.
			{
				damage = ShieldDamageReductionCalculation(item, armorMaterial, atkStrength, damage, bluntWep, wepWeight, unarmedCheck, naturalDamResist);
				
				return damage;
			}
			else // Possibly add later on enemies/monsters that can more readily penetrate through damage reduction from armor and such, for now though, just do with this for now.
			{
                switch (armorMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (.90f - naturalDamResist));
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (.84f - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (.80f - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (.72f - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (.66f - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (.58f - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (.52f - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (.46f - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (.36f - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (.30f - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (1f - naturalDamResist)); // May have to change this? Don't think so though.
                }
			}
		}
		
		/// Does most of the calculations determining how much an attack gets reduced by a piece of armor, if a weapon is being used to attack with.
		private static int PercentageReductionCalculationWithWeapon(DaggerfallUnityItem item, int armorMaterial, int atkStrength, int damage, bool bluntWep, int wepWeight, bool shieldCheck, float naturalDamResist)
		{
			bool unarmedCheck = false;
			
			if (shieldCheck) // This part is a bit more difficult than I expected, I think i'll just have to make the shields a flat % reduction, regardless of material and what is being worn under the shield. Base this on the type of shield being used, also make blunt do more against shields than other types. Later on, i'll want to improve this to consider what is under the shield, as well as other factors, just do this for the time being.
			{
				damage = ShieldDamageReductionCalculation(item, armorMaterial, atkStrength, damage, bluntWep, wepWeight, unarmedCheck, naturalDamResist);
				
				return damage;
			}
			else
			{
				if (bluntWep)
				{
                    switch (armorMaterial)
                    {
                        case 1: // leather
                            return (int)Mathf.Round(damage * (.72f - naturalDamResist)); // Made it so blunt weapons do 4% more damage to plate armors (most of them) compared to other weapons.
                        case 2: // chains 1 and 2
                            return (int)Mathf.Round(damage * (.88f - naturalDamResist));
                        case 3: // iron
                            return (int)Mathf.Round(damage * (.92f - naturalDamResist));
                        case 4: // steel and silver
                            return (int)Mathf.Round(damage * (.84f - naturalDamResist));
                        case 5: // elven
                            return (int)Mathf.Round(damage * (.80f - naturalDamResist));
                        case 6: // dwarven
                            return (int)Mathf.Round(damage * (.72f - naturalDamResist));
                        case 7: // mithril and adamantium
                            return (int)Mathf.Round(damage * (.64f - naturalDamResist));
                        case 8: // ebony
                            return (int)Mathf.Round(damage * (.56f - naturalDamResist));
                        case 9: // orcish
                            return (int)Mathf.Round(damage * (.48f - naturalDamResist));
                        case 10: // daedric
                            return (int)Mathf.Round(damage * (.40f - naturalDamResist));
                        default:
                            return (int)Mathf.Round(damage * (1f - naturalDamResist));
                    }
                }
				else
				{
                    switch (armorMaterial)
                    {
                        case 1: // leather
                            return (int)Mathf.Round(damage * (.88f - naturalDamResist));
                        case 2: // chains 1 and 2
                            return (int)Mathf.Round(damage * (.72f - naturalDamResist));
                        case 3: // iron
                            return (int)Mathf.Round(damage * (.86f - naturalDamResist));
                        case 4: // steel and silver
                            return (int)Mathf.Round(damage * (.78f - naturalDamResist));
                        case 5: // elven
                            return (int)Mathf.Round(damage * (.74f - naturalDamResist));
                        case 6: // dwarven
                            return (int)Mathf.Round(damage * (.66f - naturalDamResist));
                        case 7: // mithril and adamantium
                            return (int)Mathf.Round(damage * (.58f - naturalDamResist));
                        case 8: // ebony
                            return (int)Mathf.Round(damage * (.50f - naturalDamResist));
                        case 9: // orcish
                            return (int)Mathf.Round(damage * (.42f - naturalDamResist));
                        case 10: // daedric
                            return (int)Mathf.Round(damage * (.34f - naturalDamResist));
                        default:
                            return (int)Mathf.Round(damage * (1f - naturalDamResist));
                    }
                }
			}
		}
		
		private static int ShieldDamageReductionCalculation(DaggerfallUnityItem shield, int shieldMaterial, int atkStrength, int damage, bool bluntWep, int wepWeight, bool unarmedCheck, float naturalDamResist)
		{
			// So I want to expand on this more later on, but for right now, I think i'm just going to go with something fairly basic. That being that shields, no matter what the type will have a large damage reduction, but this reduction will increase with the material type of the shield. Like I said, want to a lot more with this, but have to think more about it and learn more about actually doing it before I wrack my head on it.
			if (unarmedCheck)
			{
                switch (shieldMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (.59f - naturalDamResist));
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (.55f - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (.49f - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (.45f - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (.43f - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (.39f - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (.35f - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (.33f - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (.29f - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (.25f - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                }
            }
			else if (bluntWep)
			{
                switch (shieldMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (.74f - naturalDamResist)); // Blunt does slightly more damage to shields, overall.
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (.70f - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (.64f - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (.58f - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (.54f - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (.50f - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (.46f - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (.42f - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (.38f - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (.34f - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                }
            }
			else
			{
                switch (shieldMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (.70f - naturalDamResist)); // You get a very good damage reduction from any type of shield, it gets slightly better as the material of the shield gets better.
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (.66f - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (.60f - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (.54f - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (.50f - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (.46f - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (.42f - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (.38f - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (.34f - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (.30f - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                }
            }
		}
		
		// This will be the main section of where various groups of enemies will have their 'Natural' damage reduction calculated.
		private static int PercentageReductionCalculationForMonsters(DaggerfallEntity attacker, DaggerfallEntity target, int damage, bool bluntWep, float naturalDamResist)
		{
			EnemyEntity AITarget = null;
            AITarget = target as EnemyEntity;

            switch (AITarget.GetEnemyGroup())
            {
                case DFCareer.EnemyGroups.Animals:
                    return (int)Mathf.Round(damage * (1f - naturalDamResist));
                case DFCareer.EnemyGroups.Undead: // Did not know that, humanoids don't effect "human" enemies, but humanoid like monsters like orcs, etc.
                    if (AITarget.CareerIndex == (int)MonsterCareers.SkeletalWarrior) // Logic here being that the skeleton warrior clearly has a shield.
                        return (int)Mathf.Round(damage * (.80f - naturalDamResist));
                    else
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                case DFCareer.EnemyGroups.Humanoid:
                    if (AITarget.CareerIndex == (int)MonsterCareers.Gargoyle) // Logic here being that the gargoyle is made of some pretty hard material. Have blunt do bonus damage to these.
                    {
                        if (bluntWep)
                            return (int)Mathf.Round(damage * (1.21f - naturalDamResist));
                        else
                            return (int)Mathf.Round(damage * (.71f - naturalDamResist));
                    }
                    else if (AITarget.CareerIndex == (int)MonsterCareers.Spriggan) // Logic here being that the spriggan has bark skin. Take more from non-blunt.
                    {
                        if (bluntWep)
                            return (int)Mathf.Round(damage * (.66f - naturalDamResist));
                        else
                            return (int)Mathf.Round(damage * (1.26f - naturalDamResist));
                    }
                    else
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                case DFCareer.EnemyGroups.Daedra:
                    if (AITarget.CareerIndex == (int)MonsterCareers.Daedroth) // Logic here being that the daedroth is wearing some armor, as well as has a scaly exterior skin.
                        return (int)Mathf.Round(damage * (.95f - naturalDamResist));
                    else if (AITarget.CareerIndex == (int)MonsterCareers.FireDaedra) // Logic here being that the fire daedra is wearing some armor.
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                    else if (AITarget.CareerIndex == (int)MonsterCareers.FrostDaedra) // Logic here being that the frost daedra is wearing some armor, also colder, thus harder, increase damage from blunt.
                    {
                        if (bluntWep)
                            return (int)Mathf.Round(damage * (1.04f - naturalDamResist));
                        else
                            return (int)Mathf.Round(damage * (.84f - naturalDamResist));
                    }
                    else if (AITarget.CareerIndex == (int)MonsterCareers.DaedraLord) // Logic here being that the daedra lord is wearing some armor.
                        return (int)Mathf.Round(damage * (.95f - naturalDamResist));
                    else
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                case DFCareer.EnemyGroups.None:
                    if (AITarget.CareerIndex == (int)MonsterCareers.IceAtronach) // Logic here being that the ice atronach is made of fairly hard ice, increased damage from blunt.
                    {
                        if (bluntWep)
                            return (int)Mathf.Round(damage * (1.4f - naturalDamResist));
                        else
                            return (int)Mathf.Round(damage * (1f - naturalDamResist));
                    }
                    else if (AITarget.CareerIndex == (int)MonsterCareers.IronAtronach) // Logic here being that the iron atronach is a construct of pure metal, pretty tough. Less damage from blunt.
                    {
                        if (bluntWep)
                            return (int)Mathf.Round(damage * (.60f - naturalDamResist));
                        else
                            return (int)Mathf.Round(damage * (.90f - naturalDamResist));
                    }
                    else
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                default:
                    return (int)Mathf.Round(damage * (1f - naturalDamResist));
            }
		}
		
		/// Does most of the calculations determining how much a material/piece of equipment should be taking damage from something hitting it.
		private static int MaterialDifferenceDamageCalculation(DaggerfallUnityItem item, int matDifference, int atkStrength, int damage, bool bluntWep, int wepWeight, bool shieldCheck)
		{
			int itemMat = item.NativeMaterialValue;

			if (bluntWep) // Personally, I think the higher tier materials should have higher weight modifiers than most of the lower tier stuff, that's another idea for another mod though.
			{
				if (shieldCheck)
					damage *= 2;
				
				if (itemMat == (int)ArmorMaterialTypes.Leather)
					damage /= 2;
				else if (itemMat == (int)ArmorMaterialTypes.Chain || itemMat == (int)ArmorMaterialTypes.Chain2)
					damage *= 2;
				
				if (wepWeight >= 7 && wepWeight <= 9) // Later on, possibly add some settings into this specific mod, just for more easy modification for users, as well as practice for myself.
				{
					atkStrength /= 5; // 1-3 Kg Staves, 3-6 Kg Maces, 4-9 Kg Flails, 4-9 Kg Warhammers (I think Warhammers are bugged here, should be higher, not equal to flails)
					damage = (damage * 3) + atkStrength;
					damage /= 3;
					return damage;
				}
				else if (wepWeight >= 4 && wepWeight <= 6) // The Extra Weight negative enchantment just multiplies the current weight by 4, feather weight sets everything to 0.25 Kg.
				{
					atkStrength /= 5;
					damage = (damage * 2) + atkStrength;
					damage /= 3;
					return damage;
				}
				else if (wepWeight >= 10 && wepWeight <= 12) // This will matter if the Warhammer weight "bug" is ever fixed.
				{
					atkStrength /= 5;
					damage = (damage * 4) + atkStrength;
					damage /= 3;
					return damage;
				}
				else if (wepWeight >= 1 && wepWeight <= 3) // Put these weights lower down, since it's less likely, so very slight better performance with less unneeded checks.
				{
					atkStrength /= 5;
					damage += atkStrength;
					damage /= 4;
					return damage;
				}
				else if (wepWeight >= 13 && wepWeight <= 35) // 35 would be highest weight with a 8.75 base item and the extra weight enchant.
				{
					atkStrength /= 5;
					damage = (damage * 5) + atkStrength;
					damage /= 3;
					return damage;
				}
				else if (wepWeight >= 36) // 36 and higher weight would be a "bug fixed" steel or daedric warhammer with extra weight, 48 Kg.
				{
					atkStrength /= 5;
					damage = (damage * 7) + atkStrength;
					damage /= 3;
					return damage;
				}
				else // Basically any value that would be 0, which I don't think is even possible since this number is rounded up, so even feather weight would be 1 I believe.
				{
					atkStrength /= 20;
					damage = (damage / 2) + atkStrength;
					damage /= 5;
					return damage;
				}
			}
			else
			{
				if (shieldCheck)
					damage /= 2;
				
				if (itemMat == (int)ArmorMaterialTypes.Chain || itemMat == (int)ArmorMaterialTypes.Chain2)
					damage /= 2;
				
				if (matDifference < 0)
				{
					matDifference *= -1;
					matDifference += 1;
					atkStrength /= 10;
					damage = (damage * matDifference) + atkStrength;
					damage /= 2;
					return damage;
				}
				else if (matDifference == 0)
				{
					atkStrength /= 10;
					damage += atkStrength;
					damage /= 2;
					return damage;
				}
				else
				{
					atkStrength /= 10;
					damage = (damage / matDifference) + atkStrength;
					damage /= 3;
					return damage;
				}
			}
		}
		
		/// Applies condition damage to an item based on physical hit damage.
        private static void ApplyConditionDamageThroughWeaponDamage(DaggerfallUnityItem item, DaggerfallEntity owner, int damage, bool bluntWep, bool shtbladeWep, bool missileWep, int wepEqualize) // Possibly add on so that magic damage also damages worn equipment.
        {
			//Debug.LogFormat("Item Group Index is {0}", item.GroupIndex);
			//Debug.LogFormat("Item Template Index is {0}", item.TemplateIndex);
			
			if (item.ItemGroup == ItemGroups.Armor) // Target gets their armor/shield condition damaged.
            {
                int amount = item.IsShield ? damage * 2: damage * 4;
                item.LowerCondition(amount, owner);
				
				/*int percentChange = 100 * amount / item.maxCondition;
                if (owner == GameManager.Instance.PlayerEntity){
                    Debug.LogFormat("Target Had {0} Damaged by {1}, cond={2}", item.LongName, amount, item.currentCondition);
					Debug.LogFormat("Had {0} Damaged by {1}%, of Total Maximum. There Remains {2}% of Max Cond.", item.LongName, percentChange, item.ConditionPercentage);} // Percentage Change */
            }
			else // Attacker gets their weapon damaged, if they are using one, otherwise this method is not called.
			{
				int amount = (10 * damage) / 50;
				if ((amount == 0) && Dice100.SuccessRoll(40))
					amount = 1;
					
				if (missileWep)
					amount = SpecificWeaponConditionDamage(item, amount, wepEqualize);

				item.LowerCondition(amount, owner);
				
				/*int percentChange = 100 * amount / item.maxCondition;
				if (owner == GameManager.Instance.PlayerEntity){
					Debug.LogFormat("Attacker Damaged {0} by {1}, cond={2}", item.LongName, amount, item.currentCondition);
					Debug.LogFormat("Had {0} Damaged by {1}%, of Total Maximum. There Remains {2}% of Max Cond.", item.LongName, percentChange, item.ConditionPercentage);} // Percentage Change */
			}
        }
		
		/// Applies condition damage to an item based on physical hit damage. Specifically for unarmed attacks.
        private static void ApplyConditionDamageThroughUnarmedDamage(DaggerfallUnityItem item, DaggerfallEntity owner, int damage)
        {
			//Debug.LogFormat("Item Group Index is {0}", item.GroupIndex);
			//Debug.LogFormat("Item Template Index is {0}", item.TemplateIndex);
			
			if (item.ItemGroup == ItemGroups.Armor) // Target gets their armor/shield condition damaged.
            {
                int amount = item.IsShield ? damage: damage * 2;
                item.LowerCondition(amount, owner);
				
				/*int percentChange = 100 * amount / item.maxCondition;
                if (owner == GameManager.Instance.PlayerEntity){
                    Debug.LogFormat("Target Had {0} Damaged by {1}, cond={2}", item.LongName, amount, item.currentCondition);
					Debug.LogFormat("Had {0} Damaged by {1}%, of Total Maximum. There Remains {2}% of Max Cond.", item.LongName, percentChange, item.ConditionPercentage);} // Percentage Change */
			}
		}
		
		/// Does a roll for based on the critical strike chance of the attacker, if this roll is successful critSuccess is returned as 'true'.
		private static bool CriticalStrikeHandler(DaggerfallEntity attacker)
		{
			PlayerEntity player = GameManager.Instance.PlayerEntity;
			int attackerLuckBonus = (int)Mathf.Floor((float)(attacker.Stats.LiveLuck - 50) / 25f);
			Mathf.Clamp(attackerLuckBonus, -2, 2); // This is meant to disallow crit odds from going higher than 50%, incase luck is allowed to go over 100 points.

			if (attacker == player)
			{
				if (Dice100.SuccessRoll(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / (4 - attackerLuckBonus))) // Player has a 25% chance of critting at level 100. 33% with 75 luck, and 50% with 100 luck.
					return true;
				else
					return false;
			}
			else
			{
				if (Dice100.SuccessRoll(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / (5 - attackerLuckBonus))) // Monsters have a 20% chance of critting at level 100, or level 14.
					return true;
				else
					return false;
			}
			
		}
		
		#endregion
		
		#region Helper Methods
		
		// Finds the material that an armor item is made from, then returns the multiplier that will be used later based on this material check.
		private static int ArmorMaterialIdentifier (DaggerfallUnityItem armor)
        {
            if (!armor.IsShield)
            {
                int itemMat = armor.GetMaterialArmorValue();
                itemMat /= 2 - (int)0.5;
                return itemMat;
            }
            else
            {
                int itemMat = armor.NativeMaterialValue;

                switch (itemMat)
                {
                    case (int)ArmorMaterialTypes.Leather:
                        return 1;
                    case (int)ArmorMaterialTypes.Chain:
                    case (int)ArmorMaterialTypes.Chain2:
                        return 2;
                    case (int)ArmorMaterialTypes.Iron:
                        return 3;
                    case (int)ArmorMaterialTypes.Steel:
                    case (int)ArmorMaterialTypes.Silver:
                        return 4;
                    case (int)ArmorMaterialTypes.Elven:
                        return 5;
                    case (int)ArmorMaterialTypes.Dwarven:
                        return 6;
                    case (int)ArmorMaterialTypes.Mithril:
                    case (int)ArmorMaterialTypes.Adamantium:
                        return 7;
                    case (int)ArmorMaterialTypes.Ebony:
                        return 8;
                    case (int)ArmorMaterialTypes.Orcish:
                        return 9;
                    case (int)ArmorMaterialTypes.Daedric:
                        return 10;
                }
            }
            return 1;
        }
		
		// If the player has equipment that is below a certain percentage of condition, this will check if they should be warned with a pop-up message about said piece of equipment.
		private static void WarningMessagePlayerEquipmentCondition(DaggerfallUnityItem item)
		{
			string roughItemMessage = "";
			string damagedItemMessage = "";
			
			if (item.ConditionPercentage <= 49)
			{
				if (item.TemplateIndex == (int)Armor.Boots || item.TemplateIndex == (int)Armor.Gauntlets || item.TemplateIndex == (int)Armor.Greaves) // Armor With Plural Names Text
				{
					roughItemMessage = String.Format("My {0} Are In Rough Shape", item.shortName);
					damagedItemMessage = String.Format("My {0} Are Falling Apart", item.shortName);
				}
				else if (item.GetWeaponSkillIDAsShort() == 29 || item.GetWeaponSkillIDAsShort() == 28 || item.GetWeaponSkillIDAsShort() == 31) // Bladed Weapons Text
				{
					roughItemMessage = String.Format("My {0} Could Use A Sharpening", item.shortName);
					damagedItemMessage = String.Format("My {0} Looks As Dull As A Butter Knife", item.shortName);
				}
				else if (item.GetWeaponSkillIDAsShort() == 32) // Blunt Weapoons Text
				{
					roughItemMessage = String.Format("My {0}'s Shaft Has Some Small Cracks", item.shortName);
					damagedItemMessage = String.Format("My {0}'s Shaft Is Nearly Split In Two", item.shortName);
				}
				else if (item.GetWeaponSkillIDAsShort() == 33) // Archery Weapons Text
				{
					roughItemMessage = String.Format("The Bowstring On My {0} Is Losing Its Twang", item.shortName);
					damagedItemMessage = String.Format("The Bowstring On My {0} Looks Ready To Snap", item.shortName);
				}
				else // Text for any other Valid Items
				{
					roughItemMessage = String.Format("My {0} Is In Rough Shape", item.shortName);
					damagedItemMessage = String.Format("My {0} Is Falling Apart", item.shortName);
				}
				
				if (item.ConditionPercentage <= 49 && item.ConditionPercentage >= 47) // 49 & 45 // This will work for now, until I find a more elegant solution.
					DaggerfallUI.AddHUDText(roughItemMessage, 2.00f); // Possibly make a random between a few of these lines to mix it up or something.				
				else if (item.ConditionPercentage <= 16 && item.ConditionPercentage >= 14) // 16 & 12
					DaggerfallUI.AddHUDText(damagedItemMessage, 2.00f);
			}
		}
		
		// Retrieves the multiplier based on the condition modifier of a material, the idea being that items will take around the same amount of damage as other items in that category.
		private static int EqualizeMaterialConditions (DaggerfallUnityItem item)
		{
			int itemMat = item.NativeMaterialValue;
			
			if (itemMat <= 9 && itemMat >= 0) // Checks if the item material is for weapons, and leather armor.
			{
                switch (itemMat)
                {
                    case (int)WeaponMaterialTypes.Iron:
                    case (int)WeaponMaterialTypes.Steel:
                    case (int)WeaponMaterialTypes.Silver:
                        return 1;
                    case (int)WeaponMaterialTypes.Elven:
                        return 2;
                    case (int)WeaponMaterialTypes.Dwarven:
                        return 3;
                    case (int)WeaponMaterialTypes.Mithril:
                        return 4;
                    case (int)WeaponMaterialTypes.Adamantium:
                        return 5;
                    case (int)WeaponMaterialTypes.Ebony:
                        return 6;
                    case (int)WeaponMaterialTypes.Orcish:
                        return 7;
                    case (int)WeaponMaterialTypes.Daedric:
                        return 8;
                    default:
                        return 1; // Leather should default to this.
                }
			}
			else if (itemMat <= 521 && itemMat >= 256) // Checks if the item material is for armors.
			{
                switch (itemMat)
                {
                    case (int)ArmorMaterialTypes.Chain:
                    case (int)ArmorMaterialTypes.Chain2:
                    case (int)ArmorMaterialTypes.Iron:
                    case (int)ArmorMaterialTypes.Steel:
                    case (int)ArmorMaterialTypes.Silver:
                        return 1;
                    case (int)ArmorMaterialTypes.Elven:
                        return 2;
                    case (int)ArmorMaterialTypes.Dwarven:
                        return 3;
                    case (int)ArmorMaterialTypes.Mithril:
                        return 4;
                    case (int)ArmorMaterialTypes.Adamantium:
                        return 5;
                    case (int)ArmorMaterialTypes.Ebony:
                        return 6;
                    case (int)ArmorMaterialTypes.Orcish:
                        return 7;
                    case (int)ArmorMaterialTypes.Daedric:
                        return 8;
                    default:
                        return 1;
                }
			}
			else
				return 1;
		}
		
		// Finds the material that an armor item is made from, then returns the multiplier that will be used later based on this material check.
		private static int ArmorMaterialModifierFinder (DaggerfallUnityItem armor)
		{
			int itemMat = armor.NativeMaterialValue;

            switch (itemMat)
            {
                case (int)ArmorMaterialTypes.Leather:
                    return 1;
                case (int)ArmorMaterialTypes.Chain:
                case (int)ArmorMaterialTypes.Chain2:
                case (int)ArmorMaterialTypes.Iron:
                    return 2;
                case (int)ArmorMaterialTypes.Steel:
                case (int)ArmorMaterialTypes.Silver:
                    return 3;
                case (int)ArmorMaterialTypes.Elven:
                    return 4;
                case (int)ArmorMaterialTypes.Dwarven:
                case (int)ArmorMaterialTypes.Mithril:
                case (int)ArmorMaterialTypes.Adamantium:
                    return 5;
                case (int)ArmorMaterialTypes.Ebony:
                    return 6;
                case (int)ArmorMaterialTypes.Orcish:
                    return 7;
                case (int)ArmorMaterialTypes.Daedric:
                    return 8;
                default:
                    return 1;
            }
		}
		
		// For dealing with special cases of specific weapons in terms of condition damage amount.
		private static int SpecificWeaponConditionDamage(DaggerfallUnityItem weapon, int damageWep, int materialValue)
		{
			if (weapon.TemplateIndex == (int)Weapons.Long_Bow)
			{
				if (materialValue == 1) // iron, steel, silver
					damageWep = 1;
				else if (materialValue == 2) // elven
					damageWep = 2;
				else // dwarven, mithril, adamantium, ebony, orcish, daedric 
					damageWep = 3; 
			}
			else if (weapon.TemplateIndex == (int)Weapons.Short_Bow)
			{
				if (materialValue == 1) // iron, steel, silver
					damageWep = 1;
				else // elven, dwarven, mithril, adamantium, ebony, orcish, daedric
					damageWep = 2;
			}
			return damageWep;
		}
		
		// Check whether the struck body part of the target was covered by armor or shield, returns true if yes, false if no.
		private static bool ArmorStruckVerification(DaggerfallEntity target, int struckBodyPart)
		{
			if (shieldBlockSuccess)
				return true;
			else
			{
				EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
				DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
				if (armor != null)
					return true;
			}
			return false;
		}
		
		// Checks for if a shield block was successful and returns true if so, false if not.
		private static bool ShieldBlockChanceCalculation(DaggerfallEntity target, bool shieldStrongSpot, DaggerfallUnityItem shield)
		{
			float hardBlockChance = 0f;
            float softBlockChance = 0f;
            int targetAgili = target.Stats.LiveAgility - 50;
			int targetSpeed = target.Stats.LiveSpeed - 50;
			int targetStren = target.Stats.LiveStrength - 50;
			int targetEndur = target.Stats.LiveEndurance - 50;
			int targetWillp = target.Stats.LiveWillpower - 50;
			int targetLuck = target.Stats.LiveLuck - 50;

            switch (shield.TemplateIndex)
            {
                case (int)Armor.Buckler:
                    hardBlockChance = 30f;
                    softBlockChance = 20f;
                    break;
                case (int)Armor.Round_Shield:
                    hardBlockChance = 35f;
                    softBlockChance = 10f;
                    break;
                case (int)Armor.Kite_Shield:
                    hardBlockChance = 45f;
                    softBlockChance = 5f;
                    break;
                case (int)Armor.Tower_Shield:
                    hardBlockChance = 55f;
                    softBlockChance = -5f;
                    break;
                default:
                    hardBlockChance = 40f;
                    softBlockChance = 0f;
                    break;
            }
			
			if (shieldStrongSpot)
			{
                hardBlockChance += (targetAgili*.3f);
                hardBlockChance += (targetSpeed*.3f);
                hardBlockChance += (targetStren*.3f);
                hardBlockChance += (targetEndur*.2f);
                hardBlockChance += (targetWillp*.1f);
                hardBlockChance += (targetLuck*.1f);
				
				Mathf.Clamp(hardBlockChance, 7f, 95f);
				int blockChanceInt = (int)Mathf.Round(hardBlockChance);
				
				if (Dice100.SuccessRoll(blockChanceInt))
				{
					//Debug.LogFormat("$$$. Shield Blocked A Hard-Point, Chance Was {0}%", blockChanceInt);
					return true;
				}
				else
				{
					//Debug.LogFormat("!!!. Shield FAILED To Block A Hard-Point, Chance Was {0}%", blockChanceInt);
					return false;
				}
			}
			else
			{
                softBlockChance += (targetAgili*.3f);
                softBlockChance += (targetSpeed*.2f);
                softBlockChance += (targetStren*.2f);
                softBlockChance += (targetEndur*.1f);
                softBlockChance += (targetWillp*.1f);
                softBlockChance += (targetLuck*.1f);
				
				Mathf.Clamp(softBlockChance, 0f, 50f);
				int blockChanceInt = (int)Mathf.Round(softBlockChance);
				
				if (Dice100.SuccessRoll(blockChanceInt))
				{
					//Debug.LogFormat("$$$. Shield Blocked A Soft-Point, Chance Was {0}%", blockChanceInt);
					return true;
				}
				else
				{
					//Debug.LogFormat("!!!. Shield FAILED To Block A Soft-Point, Chance Was {0}%", blockChanceInt);
					return false;
				}
			}
		}
		
		// Compares the damage reduction of the struck shield, with the armor under the part that was struck, and returns true if the shield has the higher reduction value, or false if the armor under has a higher reduction value. This is to keep a full-suit of daedric armor from being worse while wearing a leather shield, which when a block is successful, would actually take more damage than if not wearing a shield.
		private static bool CompareShieldToUnderArmor(DaggerfallEntity target, int struckBodyPart, float naturalDamResist)
		{
			int redDamShield = 100;
			int redDamUnderArmor = 100;
			int armorMaterial = 0;
			bool shieldQuickCheck = true;

			DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
			armorMaterial = ArmorMaterialIdentifier(shield);
			
			redDamShield = PercentageReductionAverage(shield, armorMaterial, redDamShield, naturalDamResist, shieldQuickCheck);
			shieldQuickCheck = false;
			
			
			EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
			DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
			if (armor != null)
			{
				armorMaterial = ArmorMaterialIdentifier(armor);
				
				redDamUnderArmor = PercentageReductionAverage(armor, armorMaterial, redDamUnderArmor, naturalDamResist, shieldQuickCheck);
			}
			else // If the body part struck in 'naked' IE has no armor protecting it.
			{
				redDamUnderArmor = (int)Mathf.Round(redDamUnderArmor*(1f - naturalDamResist));
			}
			
			if (redDamShield <= redDamUnderArmor)
			{
				//Debug.Log("$$$: Shield Is Stronger Than Under Armor, Shield Being Used");
				return true;
			}
			else
			{
				//Debug.Log("!!!: Shield Is Weaker Than Under Armor, Armor Being Used Instead");
				return false;
			}
		}
		
		// Currently being used to compare the damage reduction of a shield to the under armor it is covering. This is the average of all different types of damage reduction for simplification of this.
		private static int PercentageReductionAverage(DaggerfallUnityItem item, int armorMaterial, int damage, float naturalDamResist, bool shieldQuickCheck)
		{
			if (shieldQuickCheck)
			{
                switch (armorMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (.68f - naturalDamResist));
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (.64f - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (.58f - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (.52f - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (.49f - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (.45f - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (.41f - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (.38f - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (.34f - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (.30f - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                }
			}
			else
			{
                switch (armorMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (.83f - naturalDamResist));
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (.81f - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (.86f - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (.78f - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (.73f - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (.65f - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (.58f - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (.51f - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (.42f - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (.35f - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                }
			}
		}
		
		#endregion
		
		#region Base Methods Duplicated/Overridden Due To Private Access Modifiers, FormulaHelper.cs
		
		private static bool IsRingOfNamira(DaggerfallUnityItem item)
        {
            return item != null && item.ContainsEnchantment(DaggerfallConnect.FallExe.EnchantmentTypes.SpecialArtifactEffect, (int)ArtifactsSubTypes.Ring_of_Namira);
        }
		
		private static int CalculateStruckBodyPart()
        {
            //int[] bodyParts = { 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 6 }; // Default Values.
			int[] bodyParts = { 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 5, 5, 5, 5, 6, 6 }; // Changed slightly. Head Now 5%, Left-Right Arms 15%, Chest 20%, Hands 15%, Legs 20%, Feet 10%. Plan on doing more with this, making it so when different parts of the body take damage, do different things, like extra damage, or less damage, but attribute drain until health restored or something.
            return bodyParts[UnityEngine.Random.Range(0, bodyParts.Length)];
        }
		
		private static int CalculateBackstabChance(PlayerEntity player, DaggerfallEntity target, int enemyAnimStateRecord)
        {
            // If enemy is facing away from player
            if (enemyAnimStateRecord % 5 > 2)
            {
                player.TallySkill(DFCareer.Skills.Backstabbing, 1);
                return player.Skills.GetLiveSkillValue(DFCareer.Skills.Backstabbing);
            }
            else
                return 0;
        }
		
		private static int CalculateBackstabDamage(int damage, int backstabbingLevel)
        {
            if (backstabbingLevel > 1 && Dice100.SuccessRoll(backstabbingLevel))
            {
				damage *= 3;
				string backstabMessage = HardStrings.successfulBackstab;
				DaggerfallUI.Instance.PopupMessage(backstabMessage);
            }
            return damage;
        }
		
		static int GetBonusOrPenaltyByEnemyType(DaggerfallEntity attacker, EnemyEntity AITarget)
        {
            if (attacker == null || AITarget == null)
                return 0;
            int attackerWillpMod = (int)Mathf.Round((attacker.Stats.LiveWillpower - 50) / 5);
            int confidenceMod = Mathf.Max(10 + ((attackerWillpMod - AITarget.Level) / 2), 0);
            int courageMod = Mathf.Max(AITarget.Level - attackerWillpMod, 0);

            int damage = 0;
            // Apply bonus or penalty by opponent type.
            // In classic this is broken and only works if the attack is done with a weapon that has the maximum number of enchantments.
            if (AITarget.GetEnemyGroup() == DFCareer.EnemyGroups.Undead)
            {
                if (((int)attacker.Career.UndeadAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                {
                    damage += confidenceMod;
                }
                if (((int)attacker.Career.UndeadAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                {
                    damage -= courageMod;
                }
            }
            else if (AITarget.GetEnemyGroup() == DFCareer.EnemyGroups.Daedra)
            {
                if (((int)attacker.Career.DaedraAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                {
                    damage += confidenceMod;
                }
                if (((int)attacker.Career.DaedraAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                {
                    damage -= courageMod;
                }
            }
            else if (AITarget.GetEnemyGroup() == DFCareer.EnemyGroups.Humanoid || AITarget.EntityType == EntityTypes.EnemyClass)
            {
                if (((int)attacker.Career.HumanoidAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                {
                    damage += confidenceMod;
                }
                if (((int)attacker.Career.HumanoidAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                {
                    damage -= courageMod;
                }
            }
            else if (AITarget.GetEnemyGroup() == DFCareer.EnemyGroups.Animals)
            {
                if (((int)attacker.Career.AnimalsAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                {
                    damage += confidenceMod;
                }
                if (((int)attacker.Career.AnimalsAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                {
                    damage -= courageMod;
                }
            }
            return damage;
        }

        #endregion
    }
}