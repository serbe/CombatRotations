﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot;
using ReBot.API;

// Welcome to the "Paladine4 - Divine Knight" Paladin Holy Script.
// Made by McFly
//
// Special Thanks to:
// Fireone for the mana usage code, it's great! Also like to thank Jizar for the bases
// of this code and much has been implemented here.
// Last Shout out to Beerhunter who has made many codes possible with his help!!
//
// If you want to use this script as a reference, dont forget to give credit to everyone
// who was helping to make this script possible. This contains a lot of work. Thank you!

namespace ReBot
{
	[Rotation ("Paladine4", "McFly", "Divine Knight", WoWClass.Paladin, Specialization.PaladinHoly, 40)]
	public class Paladine4 : CombatRotation
	{
		public bool Debug = true;

		string mySealSpell;
		string myHealTalent1;
		string myHealTalent2;
		AutoResetDelay FlameChangeDelay = new AutoResetDelay (2000);


		public enum EngineeringLootingItems
		{
			DontUse = 0,
			LootARang = 60854,
			FindlesLootARang = 109167,
			Both = 2,
		}

		public Int32 Focusrune = 118632;

		public enum Crystals
		{
			DontUse = 0,
			Insanity = 86569,
			Whispering = 118922,
		}

		public enum Menu
		{
			Aggressive,
			Normal,
			Conservative,
			Auto,
		}

		[JsonProperty ("Select Mana Playstyle         "), JsonConverter (typeof(StringEnumConverter))]
		public Menu Choice { get; set; }

		public Menu Playstyle {
			get {
				if (Choice == Menu.Auto) {
					if (Me.ManaFraction >= 0.85)
						return Menu.Aggressive;
					if (Me.ManaFraction >= 0.45)
						return Menu.Normal;
					return Menu.Conservative;
				}
				return Choice;
			}
		}

		public bool Useengiitems = true;

		[JsonProperty ("Auto-Avenging Wrath")]
		public bool AW { get; set; }

		[JsonProperty ("Auto-Divine Shield on you")]
		public bool DS { get; set; }

		[JsonProperty ("Heal Target")]
		public bool HealTarget { get; set; }

		[JsonProperty ("Auto-Hand of Sacrifice Focus")]
		public bool HOS { get; set; }

		[JsonProperty ("Auto-Hand of Protection")]
		public bool HOP { get; set; }

		[JsonProperty ("Auto-Lay of Hands Focus")]
		public bool LOH { get; set; }

		[JsonProperty ("Auto Use Trinket 1 at HP %               ")]
		public double Trinket1Health = 0;
		[JsonProperty ("Auto Use Trinket 2 at HP %               ")]
		public double Trinket2Health = 0;
		[JsonProperty ("Auto Use Trinket 1 at Mana %             ")]
		public double Trinket1mana = 0;
		[JsonProperty ("Auto Use Trinket 2 at Mana %             ")]
		public double Trinket2mana = 0;
		[JsonProperty ("Crystals"), JsonConverter (typeof(StringEnumConverter))]
		public Crystals SelectedCrystal = Crystals.Insanity;
		[JsonProperty ("Use Draenic Intellect Potions"), Description ("Will use it only when target below 20%")]
		public bool UsePot = false;
		[JsonProperty ("Health pool of mob for Potions"), Description ("How much health mob has to be considered Boss or Elite")]
		public int PotionsFat = 1000000;
		[JsonProperty ("Use Healing Tonic")]
		public bool UseTonic = false;
		[JsonProperty ("Healing tonic health percent (1-100)"), Description ("Try to go below 50%, or you'll waste some")]
		public int TonicPercent = 30;
		[JsonProperty ("Use Focus Augment Rune"), Description ("Will use Focus Augment Rune on CD")]
		public bool Usefocus = false;
		[JsonProperty ("Use Potions on pull"), Description ("If checked will check DBM or BW pull timer and use Potion on 2 seconds to pull")]
		public bool UsePrepot = false;
		[JsonProperty ("Engineering Looting                      "), JsonConverter (typeof(StringEnumConverter))]
		public EngineeringLootingItems EngiLootItem = EngineeringLootingItems.DontUse;
		[JsonProperty ("Rezz other people")]
		public bool RezzOther = true;

		public bool Move = false;

		public double BeerTimer { get { return API.ExecuteLua<double> ("return BeerTimer;"); } }

		public double PotionCooldown { get { return API.ExecuteLua<double> ("local _, duration, _= GetItemCooldown(109219); return duration;"); } }

		public double TonicCooldown { get { return API.ExecuteLua<double> ("local _, duration, _= GetItemCooldown(109223); return duration;"); } }


		//Timer for pull
		public void BeerTimersInit ()
		{
			if (API.ExecuteLua<int> ("return BeerTimerInit;") != 1)
				API.ExecuteLua ("local f = CreateFrame(\"Frame\");" +
				"BeerTimer = 0;" +
				"BeerTimerInit = 1;" +
				"f:RegisterEvent(\"CHAT_MSG_ADDON\");" +
				"f:SetScript(\"OnEvent\", function(self, event, prefix, msg, channel, sender) if prefix == \"D4\" then local dbmPrefix, arg1, arg2, arg3, arg4 = strsplit(\"\t\", msg); if dbmPrefix == \"PT\" then BeerTimer = arg1 end end end);" +
				"f:SetScript(\"OnUpdate\", function(self, e) BeerTimer = BeerTimer - e; if BeerTimer < 0 then BeerTimer = 0 end end);");
		}

		private int crystal = 0;

		public double WoGHeal = 0.95;
		public double HSHeal = 0.99;
		public double HPHeal = 0.98;


		public double HLHeal {
			get {
				switch (Playstyle) {
				case Menu.Aggressive:
					return 0.85;			
				default:
					return 0.80;
				case Menu.Conservative:
					return 0.75;
				}
			}
		}

		public double FLHeal {
			get {
				switch (Playstyle) {
				case Menu.Aggressive:
					return 0.55;			 
				default:
					return 0.50;
				case Menu.Conservative:
					return 0.45;
				}
			}
		}

		public double HRHeal {
			get {
				switch (Playstyle) {
				case Menu.Aggressive:
					return 0.70;		
				default:
					return 0.65;
				case Menu.Conservative:
					return 0.60;
				}
			}
		}

		public double HoS {
			get {
				switch (Playstyle) {
				case Menu.Aggressive:
					return 0.50;			
				default:
					return 0.45;
				case Menu.Conservative:
					return 0.40;
				}
			}
		}

		public double HoP {
			get {
				switch (Playstyle) {
				case Menu.Aggressive:
					return 0.25;			
				default:
					return 0.20;
				case Menu.Conservative:
					return 0.15;
				}
			}
		}


		public int HP { get { return Me.GetPower (WoWPowerType.PaladinHolyPower); } }



		private void DebugWrite (string text)
		{
			if (Debug)
				API.Print (text);
		}

		public Paladine4 ()
		{
			BeerTimersInit ();

			GroupBuffs = new[] { "Blessing of Kings" };

			if (HasSpell ("Seal of Insight"))
				mySealSpell = "Seal of Insight";
			else
				mySealSpell = "Seal of Command";

			if (HasSpell ("Eternal Flame"))
				myHealTalent1 = "Eternal Flame";
			else
				myHealTalent1 = "Word of Glory";

			if (HasSpell ("Sacred Shield"))
				myHealTalent1 = "Sacred Shield";
			else
				myHealTalent1 = "Word of Glory";

			if (HasSpell ("Execution Sentence"))
				myHealTalent2 = "Execution Sentence";
			if (HasSpell ("Holy Prism"))
				myHealTalent2 = "Holy Prism";	
			if (HasSpell ("Light's Hammer"))
				myHealTalent2 = "Light's Hammer";






		}

		//Loot-a-Rang
		public bool CheckIfMobsToBeLooted ()
		{
			foreach (UnitObject deadmob in API.CollectUnits(40)) {
				if (deadmob.IsLootable) {
					return true;
				}
			}
			return false;
		}

		public bool HaveItemAndOffCooldownUseItem (int itemID)
		{
			if (itemID != 0 && API.ItemCooldown (itemID) == 0) {
				if (API.UseItem (itemID)) {
					return true;
				}
			}
			return false;
		}

		public bool UseEngiLootItems ()
		{
			if (Adds.Count == 0 && CheckIfMobsToBeLooted ()) {
				switch ((int)EngiLootItem) {
				case 0:
					return false;
				case (int)EngineeringLootingItems.LootARang:
					if (HaveItemAndOffCooldownUseItem ((int)EngineeringLootingItems.LootARang)) {
						return true;
					}
					break;
				case (int)EngineeringLootingItems.FindlesLootARang:
					if (HaveItemAndOffCooldownUseItem ((int)EngineeringLootingItems.FindlesLootARang)) {
						return true;
					}
					break;
				case (int)EngineeringLootingItems.Both:
					if (HaveItemAndOffCooldownUseItem ((int)EngineeringLootingItems.FindlesLootARang)) {
						return true;
					}
					break;
				}
			}
			return false;
		}


		public bool UseTrinket1 {
			get {
				if (Trinket1Health != 0 && Me.HealthFraction <= Trinket1Health) {
					return true;
				}
				if (Trinket1mana != 0 && Me.ManaFraction <= Trinket1mana) {
					return true;
				}
				return false;
			}
		}

		public bool UseTrinket2 {
			get {
				if (Trinket2Health != 0 && Me.HealthFraction <= Trinket2Health) {
					return true;
				}
				if (Trinket2mana != 0 && Me.ManaFraction <= Trinket2mana) {
					return true;
				}
				return false;
			}
		}

		public void Trinket1 ()
		{
			if (UseTrinket1 && API.ExecuteLua<double> ("local _, duration, _= GetItemCooldown(GetInventoryItemID(\"player\", 13)); return duration;") == 0) {
				API.ExecuteMacro ("/use 13");
			}
		}

		public void Trinket2 ()
		{
			if (UseTrinket2 && API.ExecuteLua<double> ("local _, duration, _= GetItemCooldown(GetInventoryItemID(\"player\", 14)); return duration;") == 0) {
				API.ExecuteMacro ("/use 14");
			}
		}

		public void Tonic ()
		{
			if (UseTonic && API.ItemCount (109223) > 0 && TonicCooldown == 0 && Me.HealthFraction * 100 <= TonicPercent)
				API.UseItem (109223);
		}

		public override bool OutOfCombat ()
		{
			//GLOBAL CD CHECK

			if (HasGlobalCooldown ())
				return false;

			if (Cast (mySealSpell, () => !IsInShapeshiftForm (mySealSpell)))
				return true;

			bool hasKings = HasAura ("Blessing of Kings") || HasAura (115921) || HasAura (1126); //monk vermächtnis des Kaisers & Mal der Wildnis
			if (hasKings) {
				if (CastSelf ("Blessing of Might", () => !HasAura ("Blessing of Kings", true) && !HasAura ("Blessing of Might")))
					return true;
			} else if (HasAura ("Blessing of Might")) {
				if (CastSelf ("Blessing of Kings", () => !HasAura ("Blessing of Might", true) && !HasAura ("Blessing of Kings")))
					return true;
			} else {
				if (CastSelf ("Blessing of Kings"))
					return true;
			}
			if (CastSelf ("Cleanse", () => Me.Auras.Any (x => x.IsDebuff && "Disease,Poison".Contains (x.DebuffType))))
				return true;



			if (Useengiitems) {
				if (UseEngiLootItems ()) {
					return true;
				}
			}



			if (UsePrepot && BeerTimer < 2 && BeerTimer != 0) {
				if (API.ItemCount (109218) > 0)
					API.UseItem (109218);
			}

			if (API.HasItem (Focusrune) && Usefocus && !HasAura ("Focus Augmentation") && API.ItemCooldown (Focusrune) == 0) {
				API.UseItem (Focusrune);
			}








			if (CurrentBotName == "Combat" && RezzOther) {
				List<PlayerObject> members = Group.GetGroupMemberObjects ();
				if (members.Count > 0) {
					PlayerObject deadPlayer = members.FirstOrDefault (x => x.IsDead);
					if (Cast ("Redemption", deadPlayer, () => deadPlayer != null))
						return true;
				}
			}




			crystal = (int)SelectedCrystal;
			if (API.HasItem (crystal) && crystal != 0 && API.ItemCooldown (crystal) == 0) {
				if (API.UseItem (crystal))
					return true;
			}

			return false;
		}


		bool DoFL (PlayerObject[] group)
		{
			bool done = false;
			var lowestPlayer = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (Me.HasAura ("Infusion of Light") || Me.HasAura (53576)) {
				if (Cast ("Holy Light", () => lowestPlayer.HealthFraction < HLHeal, lowestPlayer)) {
					DebugWrite ("Holy Light on " + lowestPlayer.Name);
					done = true;
				}
			} else {
				if (Cast ("Flash of Light", () => lowestPlayer.HealthFraction <= FLHeal, lowestPlayer)) {
					done = true;
					DebugWrite ("Flash of Light on " + lowestPlayer.Name);	
				}
			}
			return done;
		}



		bool DoHR (PlayerObject[] group)
		{
			bool done = false;
			var lowestPlayer = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (Cast ("Holy Radiance", () => lowestPlayer.HealthFraction < HRHeal, lowestPlayer)) {
				done = true;
				DebugWrite ("Holy Radiance on " + lowestPlayer.Name);
			}
			return done;
		}

		bool DoEF (PlayerObject[] group)
		{
			bool done = false;
			var lowestPlayer = group.Where (p => p.HealthFraction > 0 && !p.IsDead).Where (p => !p.HasAura (myHealTalent1)).OrderBy (p => p.HealthFraction).First ();
			if (HP > 3) {
				if (Cast (myHealTalent1, () => lowestPlayer.HealthFraction < WoGHeal, lowestPlayer)) {
					DebugWrite (myHealTalent1 + " on " + lowestPlayer.Name);
					done = false;
				}
			}
			return done;
		}

		bool DoHL (PlayerObject[] group)
		{
			bool done = false;
			var lowestPlayer = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();

			if (Cast ("Holy Light", () => lowestPlayer.HealthFraction < HLHeal, lowestPlayer)) {
				DebugWrite ("Holy Light on " + lowestPlayer.Name);
				done = true;
			}
			return done;
		}

		bool LoD (PlayerObject[] group)
		{
			bool done = false;
			var lowestPlayer = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (HP >= 3) {
				if (Cast ("Light of Dawn", lowestPlayer)) {
					DebugWrite ("Light of Dawn");
					done = true;
				}
			}
			return done;
		}

		bool GetHP (PlayerObject[] group)
		{
			bool done = false;
			int ghlimit = 3; // In groups
			if (group.Length > 5)
				ghlimit = 5; // in raids
			var lowPlayerCount = group.Where (p => p.HealthFraction > 0 && !p.IsDead).Count (p => p.HealthFraction < 0.44);
			var lowestPlayer = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (HP <= 5) {
				if (lowPlayerCount >= ghlimit) {
					if (Me.HasAura ("Daybreak")) {
						if (Cast ("Holy Shock", null, lowestPlayer)) {
							DebugWrite ("Holy Shock on " + lowestPlayer.Name);
							done = true;
						}
					} else {
						if (Cast ("Holy Radiance", () => lowestPlayer.HealthFraction <= HRHeal, lowestPlayer)) {
							DebugWrite ("Holy Radiance on " + lowestPlayer.Name);
							done = true;
						}
					}
				} else {
					if (Cast ("Holy Shock", null, lowestPlayer)) {
						done = true;
						DebugWrite ("Holy Shock on " + lowestPlayer.Name);
					}
				}
			}
			return done;
		}

		public override void Combat ()
		{
			if (HasGlobalCooldown ())
				return;
			if (Me.IsChanneling)
				return;
			if (Me.IsCasting)
				return;
			if (Me.HasAura ("Drinking"))
				return;
			Tonic ();
			if (UseTrinket1) {
				Trinket1 ();
				return;
			}
			if (UseTrinket2) {
				Trinket2 ();
				return;
			}

			var grpAndMe = Group.GetGroupMemberObjects ().Where (p => p.HealthFraction > 0 && p.IsInCombatRange && p.IsInLoS && !p.IsDead).Concat (new[] { Me }).ToArray ();
			var lowestPlayer = grpAndMe.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();

			if (UsePot && Target.MaxHealth > PotionsFat &&
			    API.ItemCount (109218) > 0 &&
			    !HasAura (156428) &&
			    PotionCooldown == 0 &&
			    Target.HealthFraction <= 0.2f)
				API.UseItem (109218);

			if (Target != null) {
				if (Target.IsEnemy && Target.IsInCombatRange) {
					Cast ("Holy Prism");
				}
			}

			if (CastSelfPreventDouble ("Hand of Freedom", () => !Me.CanParticipateInCombat))
				return;
			if (DS) {
				if (CastSelf ("Divine Shield", () => Me.HealthFraction <= 0.25))
					return;
			}
			if (LOH) {
				if (CastPreventDouble ("Lay on Hands", () => Me.Focus.HealthFraction <= 0.25))
					return;
			}

			if (AW) {
				int burstLimit = 3;
				if (grpAndMe.Length > 5)
					burstLimit = 5;
				var lowPlayerCount2 = grpAndMe.Count (p => p.HealthFraction < 0.5);
				if (lowPlayerCount2 >= burstLimit) {
					CastSelf ("Avenging Wrath");
					return;
				}
			}

			if (HOP) {
				var hopTargets = grpAndMe.Where (p => p.HealthFraction <= HoP && p.IsInLoS && !p.IsDead && (!p.IsTank || p == Me || p.IsHealer)).ToArray ();
				if (SpellCooldown ("Hand of Protection") < 0.2 && hopTargets.Length > 0) {
					foreach (var p in hopTargets)
						if (Cast ("Hand of Protection", p))
							return;
				}
			}

			if (HOS) {
				if (Me.Focus != null) {
					if (Me.Focus.IsFriendly && Me.Focus.IsInLoS && Me.Focus.IsInCombatRange) {
						if (Cast ("Hand of Sacrifice", () => Me.Focus.HealthFraction <= HoS && !Me.Focus.HasAura ("Hand of Sacrifice", true), Me.Focus)) {
						}
					}
				}
			}

			if (Me.HasAura ("Divine Purpose")) {
				if (LoD (grpAndMe))
					return;
			}

			if (HasSpell ("Beacon of Faith")) {
				var bof = Group.GetGroupMemberObjects ().FirstOrDefault (p => p.HasAura ("Beacon of Faith", true));
				if (bof == null) {
					if (CastSelf ("Beacon of Faith", () => !Me.HasAura ("Beacon of Faith", true)))
						return;
				}
			}

			if (Me.Focus != null) {
				if (Me.Focus.IsFriendly && Me.Focus.IsInLoS && Me.Focus.IsInCombatRange) {
					if (Cast ("Beacon of Light", () => !Me.Focus.HasAura ("Beacon of Light", true), Me.Focus)) {
					}
				}
			} else if (FlameChangeDelay.IsReady) {
				if (Cast ("Beacon of Light", () => !lowestPlayer.HasAura ("Beacon of Light", true) && !lowestPlayer.HasAura ("Beacon of Faith", true), lowestPlayer)) {
				}
			}

			// Target heal
			if (Target != null) {
				if (Target.IsFriendly && Target.IsInCombatRange) {
					if (HealTarget) {
						if (Cast ("Holy Shock", () => HP < 5 && Target.HealthFraction <= 0.9))
							return;
						if (Cast ("Word of Glory", () => HP >= 3 && Target.HealthFraction <= 0.8))
							return;
						if (Cast ("Holy Light", () => Target.HealthFraction <= HLHeal))
							return;
						if (Cast ("Flash of Light", () => Target.HealthFraction <= FLHeal))
							return;
					}
				}
			}

			int AECount = 3;
			if (grpAndMe.Length > 5)
				AECount = 3;
			var lowPlayerCount = grpAndMe.Count (p => p.HealthFraction < 0.95);			
			if (lowPlayerCount >= AECount) {

				if (LoD (grpAndMe))
					return;
				if (GetHP (grpAndMe))
					return;
				if (DoHR (grpAndMe))
					return;
			}
			if (lowestPlayer.HealthFraction > FLHeal) {
				if (GetHP (grpAndMe))
					return;
			}

			//if (DoHP(grpAndMe)) return;
			if (DoFL (grpAndMe))
				return;
			if (DoEF (grpAndMe))
				return;
			if (DoHL (grpAndMe))
				return;
			if (LoD (grpAndMe))
				return;
			if (Target != null) {
				if (Target.IsEnemy && Target.IsInCombatRange) {
					if (Cast ("Hammer of Wrath", () => Target.HealthFraction <= 0.2))
						return;
					Cast ("Judgment");
					Cast ("Denounce");
				}
			}
			return;				
		}



	}
}