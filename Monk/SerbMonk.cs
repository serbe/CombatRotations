using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;
using System;
using System.Security.Cryptography;

namespace ReBot
{
	public abstract class SerbMonk : CombatRotation
	{
		// Vars Consts

		[JsonProperty ("Maximum Energy")] 
		public int MaxEnergy = 100;
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public Int32 OraliusWhisperingCrystalID = 118922;
		public Int32 CrystalOfInsanityID = 86569;
		public DateTime StartBattle;
		public bool InCombat;
		public UnitObject CycleTarget;

		// Get

		public double EnergyRegen {
			get {
				string activeRegen = API.ExecuteLua<string> ("inactiveRegen, activeRegen = GetPowerRegen(); return activeRegen");
				return Convert.ToDouble (activeRegen);
			}
		}

		public double TimeToMaxEnergy {
			get {
				return (MaxEnergy - Energy) / EnergyRegen;
			}
		}

		public double Health (UnitObject u = null)
		{
			u = u ?? Target;
			return u.HealthFraction;
		}

		public int Chi {
			get { return Me.GetPower (WoWPowerType.MonkLightForceChi); }
		}

		public int ChiMax {
			get {
				int Max = 4;
				if (HasSpell ("Ascension"))
					Max = Max + 1;
				if (HasSpell ("Empowered Chi"))
					Max = Max + 1;
				return Max;
			}
		}

		public double Cooldown (string s)
		{ 
			if (SpellCooldown (s) > 0)
				return SpellCooldown (s);
			return 0;
		}

		public int Energy { 
			get {
				return Me.GetPower (WoWPowerType.Energy);
			}
		}

		public double Time {
			get {
				TimeSpan CombatTime = DateTime.Now.Subtract (StartBattle);
				return CombatTime.TotalSeconds;
			}
		}

		public double TimeToDie (UnitObject u = null)
		{
			u = u ?? Target;
			return u.Health / Ttd;
		}

		public int EnemyInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && mob.IsAttackable) {
					x++;
				}
			}
			return x;
		}

		public float Range (UnitObject u = null)
		{
			u = u ?? Target;
			return u.CombatRange;
		}

		public double DamageTaken (float t)
		{
			var damage = API.ExecuteLua<double> ("local ResolveName = GetSpellInfo(158300);local n,_,_,_,_,dur,expi,_,_,_,id,_,_,_,val1,val2,val3 = UnitAura(\"player\", ResolveName, nil, \"HELPFUL\");return val2");
			if (Time < 10) {
				if (Time < t / 1000)
					return damage;
				return damage / Time * (t / 1000);
			}

			return damage / 10 * (t / 1000);
		}

		public int ElusiveBrewStacks {
			get {
				foreach (var a in Me.Auras) {
					if (a.SpellId == 128939)
						return a.StackCount;
				}
				return  0;
			}
		}
			
		// Check

		public bool IsBoss (UnitObject u = null)
		{
			u = u ?? Target;
			return(u.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || u.Level >= Me.Level + BossLevelIncrease;
		}

		public bool IsPlayer (UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsPlayer;
		}

		public bool IsElite (UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsElite ();
		}

		public bool Usable (string s)
		{ 
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public bool InRaid {
			get {
				return API.MapInfo.Type == MapType.Raid;
			}
		}

		public bool InInstance {
			get {
				return API.MapInfo.Type == MapType.Instance;
			}
		}

		public bool InArena {
			get {
				return API.MapInfo.Type == MapType.Arena;
			}
		}

		public bool InBg {
			get {
				return API.MapInfo.Type == MapType.PvP;
			}
		}

		// Combo

		public bool Freedom ()
		{
			if (TigersLust ())
				return true;
			if (NimbleBrew ())
				return true;
			if (WilloftheForsaken ())
				return true;
			if (EveryManforHimself ())
				return true;
			return false;
		}

		public bool Interrupt ()
		{
			if (Usable ("Leg Sweep") || Usable ("Spear Hand Strike")) {
				var targets = Adds;
				targets.Add (Target);

				CycleTarget = targets.Where (u => u.IsCastingAndInterruptible () && u.IsInLoS && Range (u) <= 5 && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (SpearHandStrike (CycleTarget))
						return true;
					if (LegSweep (CycleTarget))
						return true;
				}
			}

			return false;
		}

		public bool AggroDizzyingHaze ()
		{
			if (Usable ("Dizzying Haze")) {
				var targets = Adds;
				targets.Add (Target);

				CycleTarget = targets.Where (u => u.InCombat && u.Target != Me && u.IsInLoS && Range (u) > 8 && Range (u) <= 40).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (DizzyingHaze (CycleTarget))
						return true;
				}
			}

			return false;
		}

		// Spell

		public bool DizzyingHaze (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrainPreventDouble ("Dizzying Haze", u.PositionPredicted, () => Usable ("Dizzying Haze") && u.IsInLoS && Range (u) <= 40, 2500);
		}

		public bool ChiBrew ()
		{
			return CastSelf ("Chi Brew", () => Usable ("Chi Brew") && ChiMax - Chi >= 2);
		}

		public bool BloodFury ()
		{
			return CastSelf ("BloodFury", () => Usable ("Blood Fury") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer ()));
		}

		public bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer ()));
		}

		public bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer ()));
		}

		public bool NimbleBrew ()
		{
			return CastSelf ("Nimble Brew", () => Usable ("Nimble Brew"));
		}

		public bool WilloftheForsaken ()
		{
			return CastSelf ("Will of the Forsaken", () => Usable ("Will of the Forsaken"));
		}

		public bool EveryManforHimself ()
		{
			return CastSelf ("Every Man for Himself", () => Usable ("Every Man for Himself"));
		}

		public bool LegacyoftheWhiteTiger (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Legacy of the White Tiger", () => Usable ("Legacy of the White Tiger") && u.AuraTimeRemaining ("Legacy of the White Tiger") < 300 && u.AuraTimeRemaining ("Blessing of Kings") < 300, u);
		}

		public bool OxStance ()
		{
			return CastSelf ("Stance of the Sturdy Ox", () => Usable ("Stance of the Sturdy Ox") && !IsInShapeshiftForm ("Stance of the Sturdy Ox"));
		}

		public bool DampenHarm ()
		{
			return CastSelf ("Dampen Harm", () => Usable ("Dampen Harm") && !Me.HasAura ("Dampen Harm"));
		}

		public bool FortifyingBrew ()
		{
			return CastSelf ("Fortifying Brew", () => Usable ("Fortifying Brew") && !Me.HasAura ("Fortifying Brew"));
		}

		public bool ElusiveBrew ()
		{
			return CastSelf ("Elusive Brew", () => Usable ("Elusive Brew"));
		}

		public bool InvokeXuentheWhiteTiger (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Invoke Xuen, the White Tiger", () => Usable ("Invoke Xuen, the White Tiger") && u.IsInLoS && Range (u) <= 40 && ((IsElite (u) && EnemyInRange (40) > 2) || IsPlayer (u)), u);
		}

		public bool Serenity ()
		{
			return CastSelf ("Serenity", () => Usable ("Serenity"));
		}

		public bool TouchofDeath (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Touch of Death", () => Usable ("Touch of Death") && (Chi >= 3 || HasGlyph (123391)) && ((IsBoss (u) && Health (u) < 0.1) || u.Health < Me.MaxHealth) && u.IsInLoS && Range (u) <= 5 && Me.HasAura ("Death Note"), u);
		}

		public bool PurifyingBrew ()
		{
			return CastSelf ("Purifying Brew", () => Usable ("Purifying Brew") && (Chi >= 1 || Me.HasAura ("Purifier")));
		}

		public bool BlackoutKick (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Blackout Kick", () => Usable ("Blackout Kick") && (Chi >= 2 || Me.HasAura ("Combo Breaker: Blackout Kick")) && u.IsInLoS && Range (u) <= 5, u);
		}

		public bool ChiExplosion (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Chi Explosion", () => Usable ("Chi Explosion") && (Chi >= 1 || Me.HasAura ("Combo Breaker: Chi Explosion")) && u.IsInLoS, u);
		}

		public bool Guard ()
		{
			return CastSelf ("Guard", () => Usable ("Guard") && Chi >= 2);
		}

		public bool KegSmash (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Keg Smash", () => Usable ("Keg Smash") && Energy >= 40 && u.IsInLoS && (Range (u) <= 5 || (HasGlyph (159495) && Range (u) <= 10)), u);
		}

		public bool ChiBurst (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Chi Burst", () => Usable ("Chi Burst") && u.IsInLoS && Range (u) <= 40 && !Me.IsMoving, u);
		}

		public bool ChiWave (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Chi Wave", () => Usable ("Chi Wave") && u.IsInLoS && Range (u) <= 40, u);
		}

		public bool SpearHandStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Spear Hand Strike", () => Usable ("Spear Hand Strike") && u.IsInLoS && Range (u) <= 5, u);
		}

		public bool LegSweep (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Leg Sweep", () => Usable ("Leg Sweep") && u.IsInLoS && Range (u) <= 5);
		}

		public bool ZenSphere (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Zen Sphere", () => Usable ("Zen Sphere") && u.IsInLoS && Range (u) <= 40 && !u.HasAura ("Zen Sphere") && u.IsFriendly, u);
		}

		public bool ExpelHarm (UnitObject u = null)
		{
			return CastSelf ("Expel Harm", () => Usable ("Expel Harm") && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && (Energy >= 40 || (HasGlyph (159487) && Health (Me) < 0.35 && Energy >= 35))) || Me.HasAura ("Stance of the Wise Serpent")));
		}

		public bool Jab (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Jab", () => Usable ("Jab") && u.IsInLoS && Range (u) <= 5 && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && (Energy >= 40 || (Me.HasAura ("Heightened Senses") && Energy >= 10))) || Me.HasAura ("Stance of the Wise Serpent")), u);
		}

		public bool TigerPalm (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Tiger Palm", () => Usable ("Tiger Palm") && u.IsInLoS && Range (u) <= 40 && (Chi >= 1 || Me.HasAura ("Combo Breaker: Tiger Palm") || Me.Specialization == Specialization.MonkBrewmaster), u);
		}

		public bool RushingJadeWind ()
		{
			return CastSelf ("Rushing Jade Wind", () => Usable ("Rushing Jade Wind") && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && Energy >= 40) || Me.HasAura ("Stance of the Wise Serpent")));
		}

		public bool SurgingMist (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Surging Mist", () => Usable ("Surging Mist") && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && Energy >= 30 && u.IsInLoS && Range (u) <= 40) || (Me.HasAura ("Stance of the Wise Serpent") && (u.IsInLoS && Range (u) <= 40 || HasGlyph (120483)))), u);
		}

		public bool Detox (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Detox", () => Usable ("Detox") && u.IsInLoS && Range (u) <= 40 && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && Energy >= 40) || Me.HasAura ("Stance of the Wise Serpent")), u);
		}

		public bool TigersLust ()
		{
			return CastSelf ("Tiger's Lust", () => Usable ("Tiger's Lust"));
		}

		public bool SpinningCraneKick ()
		{
			return CastSelf ("Spinning Crane Kick", () => Usable ("Spinning Crane Kick") && (((Me.HasAura ("Stance of the Fierce Tiger") || Me.Specialization == Specialization.MonkBrewmaster) && Energy >= 40) || Me.HasAura ("Stance of the Wise Serpent")));
		}

		// Items

		public bool Healthstone ()
		{
			if (API.HasItem (5512) && API.ItemCooldown (5512) == 0)
				return API.UseItem (5512);
			return false;
		}

		public bool CrystalOfInsanity ()
		{
			if (!InArena && API.HasItem (CrystalOfInsanityID) && !HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityID) == 0)
				return API.UseItem (CrystalOfInsanityID);
			return false;
		}

		public bool OraliusWhisperingCrystal ()
		{
			if (API.HasItem (OraliusWhisperingCrystalID) && !HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalID) == 0)
				return API.UseItem (OraliusWhisperingCrystalID);
			return false;
		}
	}
}

/*
		[JsonProperty("Use Tiger's Lust")] 
		public bool UTL = true;
		[JsonProperty("Use Surging Mist")] 
		public bool USM = true;

		[JsonProperty("Run to enemy")]
		public bool Run = false;
		[JsonProperty("Use multitarget")]
		public bool Multitarget = true;
		[JsonProperty("AOE")]
		public bool AOE = true;



		public bool IsSolo {
			get {
				if (Group.GetNumGroupMembers() == 1) return false;
				return true;
			}
		}

		public bool InRaid {
			get {
				return API.MapInfo.Type == MapType.Raid;
			}
		}
		public bool InInstance {
			get {
				return API.MapInfo.Type == MapType.Instance;
			}
		}

		public bool InBG {
			get {
				return API.MapInfo.Type == MapType.PvP;
			}
		}

		
		public bool IsInEnrage(UnitObject o) {
			if (o.HasAura("Enrage") || o.HasAura("Berserker Rage") || o.HasAura("Demonic Enrage") || o.HasAura("Aspect of Thekal") || o.HasAura("Charge Rage") || o.HasAura("Electric Spur") || o.HasAura("Cornered and Enraged!") || o.HasAura("Draconic Rage") || o.HasAura("Brood Rage") || o.HasAura("Determination") || o.HasAura("Charged Fists") || o.HasAura("Beatdown") || o.HasAura("Consuming Bite") || o.HasAura("Delirious") || o.HasAura("Angry") || o.HasAura("Blood Rage") || o.HasAura("Berserking Howl") || o.HasAura("Bloody Rage") || o.HasAura("Brewrific") || o.HasAura("Desperate Rage") || o.HasAura("Blood Crazed") || o.HasAura("Combat Momentum") || o.HasAura("Dire Rage") || o.HasAura("Dominate Slave") || o.HasAura("Blackrock Rabies") || o.HasAura("Burning Rage") || o.HasAura("Bloodletting Howl"))
				return true;
			else
				return false;
		}

		public bool IsNotForDamage(UnitObject o) {
			if (o.HasAura("Fear") || o.HasAura("Polymorph") || o.HasAura("Gouge") || o.HasAura("Paralysis") || o.HasAura("Blind") || o.HasAura("Hex")) return true;
			else return false;
		}


		public double StaggerPercent {
			get {
				double currentStagger = API.ExecuteLua<double>("return UnitStagger('player')");
				double maxHealth = API.ExecuteLua<double>("return UnitHealthMax('player')");
				return currentStagger / maxHealth;
			}
		}


		public double SleepTime {
			get {
				TimeSpan CurrentSleepTime = DateTime.Now.Subtract(StartSleepTime);
				return CurrentSleepTime.TotalSeconds;
			}
		}

		public double TimeToRegen(double e)	{ 
			if (e > Energy) return (e - Energy) / EnergyRegen;
			else return 0;
		}

		public double TimeToStartBattle {
			get {
				return API.ExecuteLua<double>("return GetBattlefieldInstanceRunTime()") / 1000;
			}
		}

		public bool IncapacitatedInRange(int range) {
			int x = 0;
			foreach(UnitObject mob in API.CollectUnits(range)) {
				if((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && IsNotForDamage(mob)) {
					x++;
				}
			}
			if (x > 0) return true;
			else return false;
		}

		

		public double CooldownById(Int32 i)	{ 
			if (SpellCooldown(i) < 0) return 0;
			else return SpellCooldown(i);
		}

				public bool HasCost(double i) {
			return Energy >= i;
		}


		// public SerbRogueCombat() {
		// 	if (HasSpell("Shuriken Toss"))
		// 		RangedAtk = "Shuriken Toss";
		// 	PullSpells = new string[] {
		// 		"Cheap Shot",
		// 		"Ambush",
		// 	};
		// }

		

		// public virtual bool UnEnrage() {
		// 	var targets = Adds;
		// 	targets.Add(Target);

		// 	if (HasSpell("Shiv") && Cooldown("Shiv") == 0 && HasCost(20)) {
		// 		if (EnemyInRange(6) > 1 && Multitarget) {
		// 			CycleTarget = targets.Where(x => x.IsInCombatRangeAndLoS && IsInEnrage(x) && !IsBoss(x)).DefaultIfEmpty(null).FirstOrDefault();
		// 			if (Cast("Shiv", CycleTarget, () => CycleTarget != null)) return true; 
		// 		} else
		// 			if (Cast("Shiv", () => !IsBoss(Target) && IsInEnrage(Target) && !IsBoss(Target))) return true;
		// 	}
		// 	return false;
		// }

		// public virtual bool MainHandPoison() {
		// 	if (HasSpell((int)MH)) {
		// 		return CastSelfPreventDouble((int)MH, () => !Me.HasAura((int)MH) || Me.AuraTimeRemaining((int)MH) < 300);
		// 	}
		// 	return false;
		// } 

		// public virtual bool OffHandPoison() {
		// 	if (HasSpell((int)OH)) {
		// 		return CastSelfPreventDouble((int)OH, () => !Me.HasAura((int)OH) || Me.AuraTimeRemaining((int)OH) < 300);
		// 	}
		// 	return false;
		// } 

		// public virtual bool Stealth() {
		// 	return CastSelf("Stealth", () => Usable("Stealth") && !Me.HasAura("Stealth") && !Me.HasAura("Vanish") && !Me.HasAura("Shadow Dance") && !Me.HasAura("Subterfuge"));
		// }

		// public virtual bool CloakofShadows() {
		// 	return CastSelf("Cloak of Shadows", () => Usable("Cloak of Shadows"));
		// }

		// public virtual bool CombatReadiness() {
		// 	return CastSelf("Combat Readiness", () => Usable("Combat Readiness"));
		// }

		// public virtual bool Evasion() {
		// 	return CastSelf("Evasion", () => Usable("Evasion"));
		// }

		// public virtual bool Ambush () {
		// 	return Cast("Ambush", () => Energy >= AmbushCost && (Me.HasAura("Stealth") || Me.HasAura("Vanish") || Me.HasAura("Shadow Dance") || Me.HasAura("Subterfuge")));
		// }

		// public virtual bool Recuperate() {
		// 	return Cast("Recuperate", () => HasCost(30) && ComboPoints > 0 && !Me.HasAura("Recuperate"));
		// }

		// public virtual bool BurstofSpeed() {
		// 	return CastSelf("Burst of Speed", () => Usable("Burst of Speed") && !Me.HasAura("Sprint") && !Me.HasAura("Burst of Speed") && HasCost(30));
		// }

		// public virtual bool Preparation() {
		// 	return CastSelf("Preparation", () => Usable("Preparation"));
		// }

		// public virtual bool BladeFlurry() {
		// 	return CastSelf("Blade Flurry", () => Usable("Blade Flurry"));
		// }

		// public virtual bool ShadowReflection() {
		// 	return Cast("Shadow Reflection", () => Usable("Shadow Reflection") && Target.CombatRange <= 20 && (IsElite || IsPlayer));
		// }

		// public virtual bool Vanish() {
		// 	return CastSelf("Vanish", () => Usable("Vanish"));
		// }

		// public virtual bool SliceandDice() {
		// 	return Cast("Slice and Dice", () => HasCost(25) && ComboPoints > 0);
		// }

		// public virtual bool MarkedforDeath() {
		// 	return Cast("Marked for Death", () => Usable("Marked for Death") && Target.CombatRange <= 30);
		// }

		// public virtual bool AdrenalineRush() {
		// 	return CastSelf("Adrenaline Rush", () => Usable("Adrenaline Rush") && (IsPlayer || IsElite) && Target.IsInCombatRangeAndLoS);
		// }

		// public virtual bool KillingSpree() {
		// 	return Cast("Killing Spree", () => Usable("Killing Spree") && (IsPlayer || IsElite) && Target.CombatRange <= 10);
		// }

		// public virtual bool RevealingStrike() {
		// 	return Cast("Revealing Strike", () => Usable("Revealing Strike") && HasCost(40));
		// }

		// public virtual bool SinisterStrike() {
		// 	return Cast("Sinister Strike", () => Usable("Sinister Strike") && HasCost(50));
		// }

		// public virtual bool DeathfromAbove() {
		// 	return Cast("Death from Above", () => Usable("Death from Above") && HasCost(50) && Target.CombatRange <= 15 && ComboPoints > 0);
		// }

		// public virtual bool Eviscerate() {
		// 	return Cast("Eviscerate", () => Usable("Eviscerate") && HasCost(35) && ComboPoints > 0);
		// }

		// public virtual bool KidneyShot() {
		// 	return Cast("Kidney Shot", () => Usable("Kidney Shot") && HasCost(25) && ComboPoints > 0 && !Me.HasAura("Stealth") && !Me.HasAura("Vanish") && !Me.HasAura("Shadow Dance") && !Me.HasAura("Subterfuge"));
		// }

		// public virtual bool CrimsonTempest() {
		// 	return Cast("Crimson Tempest", () => Usable("Crimson Tempest") && HasCost(35) && ComboPoints > 0);
		// }

		// public virtual bool Shadowstep() {
		// 	return Cast("Shadowstep", () => Usable("Shadowstep") && Target.CombatRange <= 25);
		// }

		// public virtual bool Sprint() {
		// 	return CastSelf("Sprint", () => Usable("Sprint"));
		// }

		// public virtual bool Healthstone() {
		// 	if (API.HasItem(5512) && API.ItemCooldown(5512) == 0)
		// 		return API.UseItem(5512);
		// 	return false;
		// }

		// public virtual bool TricksoftheTrade() {
		// 	if (Usable("Tricks of the Trade")) {
		// 		CycleTarget = Group.GetGroupMemberObjects().Where(x => !x.IsDead && x.IsInLoS && x.CombatRange < 100 && x.IsTank).OrderBy(x => x.HealthFraction).DefaultIfEmpty(null).FirstOrDefault();
		// 		if (Cast("Tricks of the Trade", CycleTarget, () => CycleTarget != null)) return true;
		// 	}
		// 	return false;
		// }

		// public virtual bool FanofKnives() {
		// 	return Cast("Fan of Knives", () => Usable("Fan of Knives") && HasCost(35));
		// }

		// public virtual bool Backstab() {
		// 	return Cast("Backstab", () => Usable("Backstab") && HasCost(35) && Me.IsNotInFront(Target));
		// }

		// public virtual bool Hemorrhage() {
		// 	return Cast("Hemorrhage", () => Usable("Hemorrhage") && HasCost(30));
		// }

		// public virtual bool ShurikenToss() {
		// 	return Cast("Shuriken Toss", () => Usable("Shuriken Toss") && HasCost(40));
		// }

		// public virtual bool Rupture(UnitObject o) {
		// 	return Cast("Rupture", o, () => Usable("Rupture") && HasCost(25));
		// }

		// public virtual bool SmokeBomb() {
		// 	return CastSelf("Smoke Bomb", () => Usable("Smoke Bomb"));
		// }

		// public virtual bool Premeditation() {
		// 	return CastSelf("Premeditation", () => Usable("Premeditation") && Me.HasAura("Stealth"));
		// }

		// public virtual bool Feint() {
		// 	return CastSelf("Feint", () => Usable("Feint") && HasCost(20) && !Me.HasAura("Feint"));
		// }

		// public virtual bool CheapShot() {
		// 	return Cast("Cheap Shot", () => Usable("Cheap Shot") && HasCost(40) && Me.HasAura("Stealth"));
		// }

		// public virtual bool Blind(UnitObject o) {
		// 	return Cast("Blind", o, () => Usable("Blind") && HasCost(15));
		// }
	}
}
*/