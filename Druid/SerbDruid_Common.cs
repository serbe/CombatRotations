﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot
{

	public abstract class SerbDruid : CombatRotation
	{
		[JsonProperty ("Maximum Energy")] 
		public int EnergyMax = 100;
		[JsonProperty ("Use StarFall")]
		public bool UseStarFall;
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int TTD = 10;
		[JsonProperty ("Run to enemy")]
		public bool Run;
		[JsonProperty ("Use multitarget")]
		public bool Multitarget = true;
		[JsonProperty ("AOE")]
		public bool AOE = true;
		[JsonProperty ("Use GCD")]
		public bool GCD = true;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public DateTime StartBattle;
		public DateTime StartSleepTime;
		public bool InCombat;
		public UnitObject CycleTarget;
		public Int32 OraliusWhisperingCrystalID = 118922;
		public Int32 CrystalOfInsanityID = 86569;

		public bool IsSolo {
			get {
				return Group.GetNumGroupMembers () != 1;
			}
		}

		// public int EnergyMax {
		// 	get {
		// 		int energy = 100;
		// 		if (HasSpell("Venom Rush")) energy = energy + 15;
		// 		if (HasGlyph(159634)) energy = energy + 20;
		// 		return energy;
		// 	}
		// }

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

		public bool InBG {
			get {
				return API.MapInfo.Type == MapType.PvP;
			}
		}

		public double Health {
			get {
				return Me.HealthFraction;
			}
		}

		public double TargetHealth {
			get {
				return Target.HealthFraction;
			}
		}

		public bool IsBoss (UnitObject o)
		{
			return(o.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || o.Level >= Me.Level + BossLevelIncrease;
		}

		public bool IsPlayer {
			get { 
				return Target.IsPlayer;
			}
		}

		public bool IsElite { 
			get {
				return Target.IsElite ();
			}
		}

		// public bool isInterruptable { get { return Target.IsCastingAndInterruptible(); } }
		public bool IsInEnrage (UnitObject o)
		{
			if (o.HasAura ("Enrage") || o.HasAura ("Berserker Rage") || o.HasAura ("Demonic Enrage") || o.HasAura ("Aspect of Thekal") || o.HasAura ("Charge Rage") || o.HasAura ("Electric Spur") || o.HasAura ("Cornered and Enraged!") || o.HasAura ("Draconic Rage") || o.HasAura ("Brood Rage") || o.HasAura ("Determination") || o.HasAura ("Charged Fists") || o.HasAura ("Beatdown") || o.HasAura ("Consuming Bite") || o.HasAura ("Delirious") || o.HasAura ("Angry") || o.HasAura ("Blood Rage") || o.HasAura ("Berserking Howl") || o.HasAura ("Bloody Rage") || o.HasAura ("Brewrific") || o.HasAura ("Desperate Rage") || o.HasAura ("Blood Crazed") || o.HasAura ("Combat Momentum") || o.HasAura ("Dire Rage") || o.HasAura ("Dominate Slave") || o.HasAura ("Blackrock Rabies") || o.HasAura ("Burning Rage") || o.HasAura ("Bloodletting Howl"))
				return true;
			else
				return false;
		}

		public bool IsNotForDamage (UnitObject o)
		{
			if (o.HasAura ("Fear") || o.HasAura ("Polymorph") || o.HasAura ("Gouge") || o.HasAura ("Paralysis") || o.HasAura ("Blind") || o.HasAura ("Hex"))
				return true;
			else
				return false;
		}

		public string Direction {
			get {
				return API.ExecuteLua<string> ("return GetEclipseDirection()");
			}
		}

		public int Eclipse {
			get {
				return API.ExecuteLua<int> ("return UnitPower('player', SPELL_POWER_ECLIPSE)");
			}
		}

		public int Energy { get { return Me.GetPower (WoWPowerType.Energy); } }

		public int ComboPoints {
			get {
				return Me.ComboPoints;
			}
		}

		public double EnergyRegen {
			get {
				string activeRegen = API.ExecuteLua<string> ("inactiveRegen, activeRegen = GetPowerRegen(); return activeRegen");
				return Convert.ToDouble (activeRegen);
			}
		}

		// public bool TargettingMe { get { return Target.Target ==(UnitObject)Me; } }
		public double EnergyTimeToMax {
			get {
				return EnergyMax - Energy / EnergyRegen;
			}
		}


		public double Time {
			get {
				TimeSpan CombatTime = DateTime.Now.Subtract (StartBattle);
				return CombatTime.TotalSeconds;
			}
		}

		public double SleepTime {
			get {
				TimeSpan CurrentSleepTime = DateTime.Now.Subtract (StartSleepTime);
				return CurrentSleepTime.TotalSeconds;
			}
		}

		public double TimeToRegen (double e)
		{ 
			if (e > Energy)
				return (e - Energy) / EnergyRegen;
			else
				return 0;
		}

		public double TimeToStartBattle {
			get {
				return API.ExecuteLua<double> ("return GetBattlefieldInstanceRunTime()") / 1000;
			}
		}

		public int EnemyInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead) {
					x++;
				}
			}
			return x;
		}

		public bool IncapacitatedInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && IsNotForDamage (mob)) {
					x++;
				}
			}
			return x > 0;
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public double CooldownById (Int32 i)
		{ 
			return SpellCooldown (i) < 0 ? 0 : SpellCooldown (i);
		}

		public bool Usable (string s)
		{ 
			// Analysis disable once CompareOfFloatsByEqualityOperator
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public bool HasEnergy (double i)
		{
			return Energy >= i;
		}

		public double TimeToDie (UnitObject o)
		{
			return o.Health / TTD;
		}

		public double EclipseChange {
			get {
				double TimeToChange = 20;
				if (Direction == "sun") {
					if (Eclipse > 0 && Eclipse <= 100)
						TimeToChange = 10 + (100 - Eclipse) / 10;
					if (Eclipse > -100 && Eclipse < 0)
						TimeToChange = (0 - Eclipse) / 10;
				} 
				if (Direction == "moon") {
					if (Eclipse > 0 && Eclipse < 100)
						TimeToChange = Eclipse / 10;
					if (Eclipse >= -100 && Eclipse < 0)
						TimeToChange = 10 + (100 + Eclipse) / 10;
				}
				if (Eclipse == 0)
					TimeToChange = 20;
				return TimeToChange;
			}
		}

		public SerbDruid ()
		{
		}

		public virtual bool Interrupt ()
		{
			var targets = Adds;
			targets.Add (Target);

			if (Usable ("Mighty Bash")) {
				if (EnemyInRange (6) > 1 && Multitarget) {
					CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange <= 5 && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (MightyBash (CycleTarget))
							return true;
					}
				} else {
					if (Target.IsCastingAndInterruptible () && Target.IsInLoS && Target.CombatRange <= 5 && Target.RemainingCastTime > 0) {
						if (MightyBash (Target))
							return true;
					}
				}
			}
			if (Usable ("Solar Beam")) {
				if (EnemyInRange (40) > 1 && Multitarget) {
					CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange <= 40 && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (SolarBeam (CycleTarget))
							return true;
					} 
				} else {
					if (Target.IsCastingAndInterruptible () && Target.IsInLoS && Target.CombatRange <= 40 && Target.RemainingCastTime > 0) {
						if (SolarBeam (Target))
							return true;
					}
				}
			}
			if (Usable ("Skull Bash")) {
				if (EnemyInRange (13) > 1 && Multitarget) {
					CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange <= 13 && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (SkullBash (CycleTarget))
							return true;
					} 
				} else {
					if (Target.IsCastingAndInterruptible () && Target.IsInLoS && Target.CombatRange <= 13 && Target.RemainingCastTime > 0) {
						if (SkullBash (Target))
							return true;
					}
				}
			}
			if (Usable ("Wild Charge")) {
				if (EnemyInRange (25) > 1 && Multitarget) {
					CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange >= 8 && x.CombatRange <= 25 && x.IsCasting && !IsBoss (x) && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (WildCharge (CycleTarget))
							return true;
					} 
				} else {
					if (Target.IsInLoS && Target.CombatRange >= 8 && Target.CombatRange <= 25 && Target.IsCasting && !IsBoss (Target) && Target.RemainingCastTime > 0) {
						if (WildCharge (Target))
							return true;
					}
				}
			}
			if (Usable ("Maim") && ComboPoints >= 3) {
				if (EnemyInRange (6) > 1 && Multitarget) {
					CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange <= 5 && x.IsCasting && !IsBoss (x) && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Maim (CycleTarget))
							return true;
					} 
				} else {
					if (Target.IsInLoS && Target.CombatRange <= 5 && Target.IsCasting && !IsBoss (Target) && Target.RemainingCastTime > 0) {
						if (Maim (Target))
							return true;
					}
				}
			}
			return false;
		}

		public virtual bool MightyBash (UnitObject o)
		{
			return Cast ("Mighty Bash", o, () => Usable ("Mighty Bash"));
		}

		public virtual bool SolarBeam (UnitObject o)
		{
			return Cast ("Solar Beam", o, () => Usable ("Solar Beam"));
		}

		public virtual bool MarkoftheWild ()
		{
			return CastSelf ("Mark of the Wild", () => Usable ("Mark of the Wild") && !HasAura ("Mark of the Wild") && !HasAura ("Blessing of Kings"));
		}

		public virtual bool Rejuvenation ()
		{
			return CastSelf ("Rejuvenation", () => Usable ("Rejuvenation"));
		}

		public virtual bool Rejuvenation (UnitObject o)
		{
			return Cast ("Rejuvenation", o, () => Usable ("Rejuvenation"));
		}

		public virtual bool HealingTouch ()
		{
			return CastSelf ("Healing Touch", () => Usable ("Healing Touch"));
		}

		public virtual bool HealingTouch (UnitObject o)
		{
			return Cast ("Healing Touch", o, () => Usable ("Healing Touch"));
		}

		public virtual bool RemoveCorruption ()
		{
			return CastSelf ("Remove Corruption", () => Usable ("Remove Corruption"));
		}

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

		public virtual bool FerociousBite(UnitObject o) {
			return Cast("Ferocious Bite", o, () => Usable("Ferocious Bite") && HasEnergy(25) && ComboPoints > 0);
		 }

		public virtual bool FerociousBite() {
			return Cast("Ferocious Bite", () => Usable("Ferocious Bite") && HasEnergy(25) && ComboPoints > 0);
		}

		public virtual bool CenarionWard ()
		{
			return CastSelf ("Cenarion Ward", () => Usable ("Cenarion Ward") && !Me.HasAura ("Cenarion Ward"));
		}

		public virtual bool Barkskin ()
		{
			return CastSelf ("Barkskin", () => Usable ("Barkskin"));
		}

		public virtual bool MoonkinForm ()
		{
			return CastSelf ("Moonkin Form", () => Usable ("Moonkin Form") && !Me.HasAura ("Moonkin Form"));
		}

		public virtual bool FaerieSwarm (UnitObject o)
		{
			return Cast ("Faerie Swarm", o, () => HasEnergy (30) && o.CombatRange < 35 && o.IsInLoS);
		}

		public virtual bool SkullBash (UnitObject o)
		{
			return Cast ("Skull Bash", o, () => Usable ("Skull Bash"));
		}

		public virtual bool WildCharge (UnitObject o)
		{
			return Cast ("Wild Charge", o, () => Usable ("Wild Charge"));
		}

		public virtual bool Starfall ()
		{
			return Cast ("Starfall", () => Usable ("Starfall") && Target.IsInCombatRangeAndLoS);
		}

		public virtual bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer));
		}

		public virtual bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer));
		}

		public virtual bool ForceofNature ()
		{
			return Cast ("Force of Nature", () => Usable ("Force of Nature") && Target.IsInLoS && Target.CombatRange <= 40);
		}

		public virtual bool Starsurge ()
		{
			return Cast ("Starsurge", () => Usable ("Starsurge") && Target.IsInCombatRangeAndLoS);
		}

		public virtual bool Vanish ()
		{
			return CastSelf ("Celestial Alignment", () => Usable ("Celestial Alignment") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer));
		}

		public virtual bool IncarnationChosenofElune ()
		{
			return CastSelf ("Incarnation: Chosen of Elune", () => Usable ("Incarnation: Chosen of Elune") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer));
		}

		public virtual bool Sunfire ()
		{
			return Cast ("Sunfire", () => Usable ("Sunfire") && Eclipse > 0 && Target.IsInCombatRangeAndLoS);
		}

		public virtual bool Sunfire (UnitObject o)
		{
			return Cast ("Sunfire", o, () => Usable ("Sunfire") && Eclipse > 0 && o.IsInCombatRangeAndLoS);
		}

		public virtual bool StellarFlare ()
		{
			return Cast ("Stellar Flare", () => Usable ("Stellar Flare") && Target.IsInCombatRangeAndLoS);
		}

		public virtual bool StellarFlare (UnitObject o)
		{
			return Cast ("Stellar Flare", o, () => Usable ("Stellar Flare") && o.IsInCombatRangeAndLoS);
		}

		public virtual bool Moonfire ()
		{
			return Cast ("Moonfire", () => Usable ("Moonfire") && Eclipse <= 0 && Target.IsInCombatRangeAndLoS);
		}

		public virtual bool Moonfire (UnitObject o)
		{
			return Cast ("Moonfire", o, () => Usable ("Moonfire") && Eclipse <= 0 && o.IsInCombatRangeAndLoS);
		}

		public virtual bool Wrath ()
		{
			return Cast ("Wrath", () => Usable ("Wrath") && Target.IsInCombatRangeAndLoS);
		}

		public virtual bool Starfire ()
		{
			return Cast ("Starfire", () => Usable ("Starfire") && Target.IsInCombatRangeAndLoS);
		}

		public virtual bool Maim (UnitObject o)
		{
			return Cast ("Maim", o, () => Usable ("Maim") && HasEnergy (35) && ComboPoints > 0);
		}

		public virtual bool Berserk ()
		{
			return CastSelf ("Berserk", () => Usable ("Berserk") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer));
		}

		public virtual bool BloodFury ()
		{
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer));
		}

		public virtual bool TigersFury ()
		{
			return CastSelf ("Tiger's Fury", () => Usable ("Tiger's Fury"));
		}

		public virtual bool IncarnationKingoftheJungle ()
		{
			return CastSelf ("Incarnation: King of the Jungle", () => Usable ("Incarnation: King of the Jungle") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer));
		}

		public virtual bool Shadowmeld ()
		{
			return CastSelf ("Shadowmeld", () => Usable ("Shadowmeld"));
		}

		public virtual bool Healthstone ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (5512) && API.ItemCooldown (5512) == 0)
				return API.UseItem (5512);
			return false;
		}

		public virtual bool CrystalOfInsanity ()
		{
			if (API.HasItem (CrystalOfInsanityID) && !Me.HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityID) == 0)
				return API.UseItem (CrystalOfInsanityID);
			return false;
		}

		public virtual bool OraliusWhisperingCrystal ()
		{
			if (API.HasItem (OraliusWhisperingCrystalID) && !Me.HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalID) == 0)
				return API.UseItem (OraliusWhisperingCrystalID);
			return false;
		}

		// public virtual bool TricksoftheTrade() {
		// 	if (Usable("Tricks of the Trade")) {
		// 		CycleTarget = Group.GetGroupMemberObjects().Where(x => !x.IsDead && x.IsInLoS && x.CombatRange < 100 && x.IsTank).OrderBy(x => x.HealthFraction).DefaultIfEmpty(null).FirstOrDefault();
		// 		if (Cast("Tricks of the Trade", CycleTarget, () => CycleTarget != null)) return true;
		// 	}
		// 	return false;
		// }

		public virtual bool CatForm ()
		{
			return CastSelf ("Cat Form", () => !Me.HasAura ("Claws of Shirvallah") && !Me.HasAura ("Cat Form"));
		}

		public virtual bool Rake ()
		{
			return Cast ("Rake", () => Usable ("Rake") && HasEnergy (35));
		}

		public virtual bool SavageRoar() {
			return CastSelf("Savage Roar", () => Usable("Savage Roar") && HasEnergy(25) && ComboPoints > 0);
		 }

		public virtual bool Rip() {
			return Cast("Rip", () => Usable("Rip") && HasEnergy(30));
		 }

		public virtual bool Rip(UnitObject o) {
			return Cast("Rip", o, () => Usable("Rip") && HasEnergy(30));
		 }

		// public virtual bool Freedom() {
		// 	if (WilloftheForsaken()) return true;
		// 	if (EveryManforHimself()) return true;
		//           return false;
		// }

		// public virtual bool WilloftheForsaken() {
		// 	return CastSelf("Will of the Forsaken", () => Usable("Will of the Forsaken"));
		// }

		// public virtual bool EveryManforHimself() {
		// 	return CastSelf("Every Man for Himself", () => Usable("Every Man for Himself"));
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
		// 	return Cast("Cheap Shot", () => Usable("Cheap Shot") && HasCost(40) && (Me.HasAura("Stealth") || Me.HasAura("Subterfuge") || Me.HasAura("Shadow Dance")));
		// }
	}
}