using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot.API;
using ReBot;
using System.ComponentModel;

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
	[Rotation ("Paladine4", "McFly", "Divine Knight v1.2", WoWClass.Paladin, Specialization.PaladinHoly, 40)]
	public class Paladine4 : CombatRotation
	{
		
		// Defining all variables used in CR
		
		public bool Debug = true;

		string mySealSpell;
		string myHealTalent1;
		string myHealTalent2;
		AutoResetDelay FlameChangeDelay = new AutoResetDelay (2000);

		
		public enum engineeringLootingItems
		{
			DontUse = 0,
			LootARang = 60854,
			FindlesLootARang = 109167,
			Both = 2,
		}

		public Int32 focusrune = 118632;

		public enum Crystals
		{
			DontUse = 0,
			Insanity = 86569,
			Whispering = 118922,
		}

		public enum Flasks
		{
			DontUse = 0,
			Intellect = 109147,
			Greater = 109155,
		}

		public enum Menu
		{
			Ultimate,
			Aggressive,
			Normal,
			HolyPower,
			Auto,
		}

		[JsonProperty ("Select Mana Playstyle (Ultimate not usable under Auto)"), JsonConverter (typeof(StringEnumConverter))]
		public Menu Choice { get; set; }

		public Menu Playstyle {
			get {
				if (Choice == Menu.Auto) {
					if (Me.ManaFraction >= 0.75)
						return Menu.Aggressive;
					if (Me.ManaFraction >= 0.45)
						return Menu.Normal;
					else
						return Menu.HolyPower;
				}
				return Choice;
			}
		}

		public bool useengiitems = true;

		[JsonProperty ("Use Denounce to help DPS"), Description ("If there is nothing to heal, why not help dps?")]
		public bool denounce { get; set; }

		[JsonProperty ("Use Crusader Strike for more Holy Power"), Description ("Get in melee range, help dps and create Holy Power")]
		public bool strike { get; set; }

		[JsonProperty ("Auto use Beacons"), Description ("Let us put beacons where needed, or do it yourself. Turn off if you just want to put them on tanks.")]
		public bool beacon { get; set; }

		[JsonProperty ("Auto Cast Sacred Shield on Tanks"), Description ("Let us Shield the Tanks for you")]
		public bool shieldTanks { get; set; }

		[JsonProperty ("Auto-Avenging Wrath"), Description ("Let us pop Avenging wrath for you when needed, or do it when you want to.")]
		public bool AW { get; set; }

		[JsonProperty ("Auto-Divine Shield on you"), Description ("Let us save you, or pop that puppy as you wish")]
		public bool DS { get; set; }

		[JsonProperty ("Heal Target"), Description ("Setting a target will throw heals on him/her. Mana User beware")]
		public bool target { get; set; }

		[JsonProperty ("Auto-Hand of Sacrifice Focus"), Description ("Set Tank as focus, we can save him, or save it for raid call")]
		public bool HOS { get; set; }

		[JsonProperty ("Auto-Hand of Protection"), Description ("We know when to use Hand of Protection, or maybe you know better")]
		public bool HOP { get; set; }

		[JsonProperty ("Auto-Lay of Hands Focus"), Description ("We like to use Lay of Hands, but maybe you do too ")]
		public bool LOH { get; set; }

		[JsonProperty ("Auto Use Trinket 1 at HP %               "), Description ("Is it beneficial to use your Trink at a certain Heath level?")]
		public double trinket1Health = 0;
		[JsonProperty ("Auto Use Trinket 2 at HP %               "), Description ("Is it beneficial to use your Trink at a certain Heath level?")]
		public double trinket2Health = 0;
		[JsonProperty ("Auto Use Trinket 1 at Mana %             "), Description ("Is it beneficial to use your Trink at a certain Mana level?")]
		public double trinket1mana = 0;
		[JsonProperty ("Auto Use Trinket 2 at Mana %             "), Description ("Is it beneficial to use your Trink at a certain Mana Level level?")]
		public double trinket2mana = 0;
		[JsonProperty ("Crystals"), JsonConverter (typeof(StringEnumConverter)), Description ("You got the damn thing - Use it!")] public Crystals selectedCrystal = Crystals.Insanity;
		[JsonProperty ("Use Draenic Intellect Potions"), Description ("Will use it only when target below 20% What raiders call the second pot.")]
		public bool usePot = false;
		[JsonProperty ("Flasks"), JsonConverter (typeof(StringEnumConverter))] public Flasks selectedFlask = Flasks.Intellect;
		[JsonProperty ("Health pool of mob for Potions"), Description ("How much health mob has to be considered Boss or Elite. We only want to use pots on them.")]
		public int PotionsFat = 1000000;
		[JsonProperty ("Use Healing Tonic"), Description ("Never thought these would be useful, did you?")]
		public bool useTonic = false;
		[JsonProperty ("Healing tonic health percent (1-100)"), Description ("Try to go below 50%, or you'll waste some")]
		public int TonicPercent = 30;
		[JsonProperty ("Use Focus Augment Rune"), Description ("Will use Focus Augment Rune on CD")]
		public bool usefocus = false;
		[JsonProperty ("Use Potions on pull"), Description ("If checked will check DBM or BW pull timer and use Potion on 2 seconds to pull")]
		public bool usePrepot = false;
		[JsonProperty ("Engineering Looting"), JsonConverter (typeof(StringEnumConverter)), Description ("Use that nifty toy!")]
		public engineeringLootingItems engiLootItem = engineeringLootingItems.DontUse;
		[JsonProperty ("Rezz other people"), Description ("They died, be nice or forget 'em")]
		public bool RezzOther = true;
		
		public bool move = false;
		
		//important cooldown timers
		
		public double BeerTimer { get { return API.ExecuteLua<double> ("return BeerTimer;"); } }

		public double PotionCooldown { get { return API.ExecuteLua<double> ("local _, duration, _= GetItemCooldown(109219); return duration;"); } }

		public double TonicCooldown { get { return API.ExecuteLua<double> ("local _, duration, _= GetItemCooldown(109223); return duration;"); } }


		//Timer for pull to establish pre potting
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
		
		//even more variables

		private int crystal = 0;
		private int iflask = 0;
		
		// Meat and Potatoes, This section defines health of friendlies to heal. adjust up if you are a mana god.
		
		public double WoGHeal = 0.97;
		public double HSHeal = 0.98;
		public double HPHeal = 0.99;

		
		public double HLHeal {
			get {
				switch (Playstyle) {
				case Menu.Ultimate:
					return 0.95;
				case Menu.Aggressive:
					return 0.85;			
				case Menu.Normal:
				default:
					return 0.75;
				case Menu.HolyPower:
					return 0;
				}
			}
		}

		public double FLHeal {
			get {
				switch (Playstyle) {
				case Menu.Ultimate:
					return 0.75;
				case Menu.Aggressive:
					return 0.40;			 
				case Menu.Normal:
				default:
					return 0.35;
				case Menu.HolyPower:
					return 0;
				}
			}
		}

		public double HRHeal {
			get {
				switch (Playstyle) {
				case Menu.Ultimate:
					return 0.80;
				case Menu.Aggressive:
					return 0.40;		
				case Menu.Normal:
				default:
					return 0.35;
				case Menu.HolyPower:
					return 0.70;
				}
			}
		}

		public double HoS {
			get {
				switch (Playstyle) {
				case Menu.Ultimate:
					return 0.75;
				case Menu.Aggressive:
					return 0.60;			
				case Menu.Normal:
				default:
					return 0.45;
				case Menu.HolyPower:
					return 0;
				}
			}
		}

		public double HoP {
			get {
				switch (Playstyle) {
				case Menu.Ultimate:
					return 0.55;
				case Menu.Aggressive:
					return 0.35;			
				case Menu.Normal:
				default:
					return 0.20;
				case Menu.HolyPower:
					return 0;
				}
			}
		}
		
		
		// Holy Power Variable
		
		private int _HP;

		public int HP { get { return Me.GetPower (WoWPowerType.PaladinHolyPower); } }
		
		// Lets Print to the log
		
		private void DebugWrite (string text)
		{
			if (Debug)
				API.Print (text);
		}
		
		// We need to set pull timer and seal selections
		
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
		
		//Loot-a-Rang equation
		public bool checkIfMobsToBeLooted ()
		{
			foreach (UnitObject deadmob in API.CollectUnits(40)) {
				if (deadmob.IsLootable) {
					return true;
				}
			}
			return false;
		}

		public bool haveItemAndOffCooldownUseItem (int itemID)
		{
			if (itemID != 0 && API.ItemCooldown (itemID) == 0) {
				if (API.UseItem (itemID)) {
					return true;
				}
			}
			return false;
		}

		public bool useEngiLootItems ()
		{
			if (Adds.Count == 0 && checkIfMobsToBeLooted ()) {
				switch ((int)engiLootItem) {
				case 0:
					return false;
					break;
				case (int)engineeringLootingItems.LootARang:
					if (haveItemAndOffCooldownUseItem ((int)engineeringLootingItems.LootARang)) {
						return true;
					}
					break;
				case (int)engineeringLootingItems.FindlesLootARang:
					if (haveItemAndOffCooldownUseItem ((int)engineeringLootingItems.FindlesLootARang)) {
						return true;
					}
					break;
				case (int)engineeringLootingItems.Both:
					if (haveItemAndOffCooldownUseItem ((int)engineeringLootingItems.FindlesLootARang)) {
						return true;
					}
					break;
					if (haveItemAndOffCooldownUseItem ((int)engineeringLootingItems.LootARang)) {
						return true;
					}
					break;
				}
			}
			return false;
		}
		
		// trinket equations
		
		public bool useTrinket1 {
			get {
				if (trinket1Health != 0 && Me.HealthFraction <= trinket1Health) {
					return true;
				}
				if (trinket1mana != 0 && Me.ManaFraction <= trinket1mana) {
					return true;
				}
				return false;
			}
		}

		public bool useTrinket2 {
			get {
				if (trinket2Health != 0 && Me.HealthFraction <= trinket2Health) {
					return true;
				}
				if (trinket2mana != 0 && Me.ManaFraction <= trinket2mana) {
					return true;
				}
				return false;
			}
		}

		public void Trinket1 ()
		{
			if (useTrinket1 && API.ExecuteLua<double> ("local _, duration, _= GetItemCooldown(GetInventoryItemID(\"player\", 13)); return duration;") == 0) {
				API.ExecuteMacro ("/use 13");
			}
		}

		public void Trinket2 ()
		{
			if (useTrinket2 && API.ExecuteLua<double> ("local _, duration, _= GetItemCooldown(GetInventoryItemID(\"player\", 14)); return duration;") == 0) {
				API.ExecuteMacro ("/use 14");
			}
		}
		
		//Setting healing tonic usage
        
		public void Tonic ()
		{
			if (useTonic && API.ItemCount (109223) > 0 && TonicCooldown == 0 && Me.HealthFraction * 100 <= TonicPercent)
				API.UseItem (109223);
		}

		public override bool OutOfCombat ()
		{
			//GLOBAL CD CHECK
			
			if (HasGlobalCooldown ())
				return false;
			
			// Casting Seals and Buffs

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
			
			// Let's defuff after Fight
			
			if (CastSelf ("Cleanse", () => Me.Auras.Any (x => x.IsDebuff && "Disease,Poison".Contains (x.DebuffType))))
				return true;
			
			// We killed the basteard, lets throw that loot-a-rang
			
			if (useengiitems) {
				if (useEngiLootItems ()) {
					return true;
				}
			}
			
			// Prepot on DBM pull. If they false alarm, kick their asses

			if (usePrepot && BeerTimer < 2 && BeerTimer != 0) {
				if (API.ItemCount (109218) > 0)
					API.UseItem (109218);
			}
           
			// Use your focus rune, glad we didn't forget?
		  
			if (API.HasItem (focusrune) && usefocus && !HasAura (175457) && API.ItemCooldown (focusrune) == 0) {
				API.UseItem (focusrune);
			}
	
			// Let's rez the dead after fight if they deserve it
			
			if (CurrentBotName == "Combat" && RezzOther) {
				List<PlayerObject> members = Group.GetGroupMemberObjects ();
				if (members.Count > 0) {
					PlayerObject deadPlayer = members.FirstOrDefault (x => x.IsDead);
					if (Cast ("Redemption", deadPlayer, () => deadPlayer != null))
						return true;
				}
			}
			
			
			// Use our selected crystal
			
			crystal = (int)selectedCrystal;
			if (API.HasItem (crystal) && crystal != 0 && API.ItemCooldown (crystal) == 0) {
				if (API.UseItem (crystal))
					return true;
			}
			
			// use selected flask. Don't have this option on if just farting around, they are expensive
			
			iflask = (int)selectedFlask;
			if (API.HasItem (iflask) && iflask != 0 && !HasAura ("Draenic Intellect Flask") && !HasAura ("Greater Draenic Intellect Flask")) {
				API.UseItem (iflask);
				return true;
			}
			
			return false;
		}
		
		// Let's start defining the healing
		
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
				if (Cast ("Light of Dawn")) {
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
			if (useTrinket1) {
				Trinket1 ();
				return;
			}
			if (useTrinket2) {
				Trinket2 ();
				return;
			}
			
			var grpAndMe = Group.GetGroupMemberObjects ().Where (p => p.HealthFraction > 0 && p.IsInCombatRange && p.IsInLoS && !p.IsDead).Concat (new[] { Me }).ToArray ();
			var lowestPlayer = grpAndMe.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			
			//second pot, hit it
			
			if (usePot && Target.MaxHealth > PotionsFat &&
			    API.ItemCount (109218) > 0 &&
			    !HasAura (156428) &&
			    PotionCooldown == 0 &&
			    Target.HealthFraction <= 0.2f)
				API.UseItem (109218);
				
			// technical crap, we did the worrying for you
				
			List<PlayerObject> members = Group.GetGroupMemberObjects ();
			if (members.Count > 0) {
				List<PlayerObject> Tanks = members.FindAll (x => x.IsTank);
				PlayerObject Tank1 = Tanks.FirstOrDefault ();
		
				if (shieldTanks) {
					if (Tank1 != null) {
						if (Cast ("Sacred Shield", () => Tank1.HealthFraction <= 1 && !Tank1.HasAura ("Sacred Shield"), Tank1))
							return;
			
					}
				}

			
				if (Tanks.Count > 1) {
					PlayerObject Tank2 = Tanks.Last ();
					if (Tank2 != null) {
						if (Cast ("Sacred Shield", () => Tank2.HealthFraction <= 1 && !Tank2.HasAura ("Sacred Shield"), Tank2))
							return;
				
					}	
				}
			}	
			if (Me.Focus.Target != null) {
				if (Me.Focus.Target.IsEnemy && Me.Focus.Target.IsInCombatRange) {
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
						if (Cast ("Hand of Sacrifice", () => Me.Focus.HealthFraction <= HoS && !Me.Focus.HasAura ("Hand of Sacrifice", true), Me.Focus))
							;
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
					if (CastSelf ("Beacon of Faith", () => beacon && !Me.HasAura ("Beacon of Faith", true)))
						return;
				}
			}
			
			if (Me.Focus != null) {
				if (Me.Focus.IsFriendly && Me.Focus.IsInLoS && Me.Focus.IsInCombatRange) {
					if (Cast ("Beacon of Light", () => beacon && !Me.Focus.HasAura ("Beacon of Light", true), Me.Focus))
						;
				}
			} else if (FlameChangeDelay.IsReady) {
				if (Cast ("Beacon of Light", () => beacon && !lowestPlayer.HasAura ("Beacon of Light", true) && !lowestPlayer.HasAura ("Beacon of Faith", true), lowestPlayer))
					;
			}
			
			/// Target heal that special someone
	
			if (Target != null) {
				if (Target.IsFriendly && Target.IsInCombatRange) {
					if (target) {
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
			
			//well we do AOE heals too, right?
				
			int AECount = 3;
			if (grpAndMe.Length > 5)
				AECount = 3;
			var lowPlayerCount = grpAndMe.Count (p => p.HealthFraction < 0.98);			
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
			if (DoEF (grpAndMe))
				return;
			if (LoD (grpAndMe))
				return;
			if (DoFL (grpAndMe))
				return;
			if (DoHL (grpAndMe))
				return;
			
			
			// Let's create holy power or help the damage count
			
			if (Target != null) {
				if (Target.IsEnemy && Target.IsInCombatRange) {
					if (Cast ("Hammer of Wrath", () => Target.HealthFraction <= 0.2))
						return;
					Cast ("Judgment");
					Cast ("Denounce", () => denounce);
					Cast ("Crusader Strike", () => strike);
				}
			}
			return;				
		}
		
		

	}
}
