using System;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot.Rogue
{

	public enum PoisonMaindHand
	{
		DeadlyPoison = 2823,
		WoundPoison = 8679,
		InstantPoison = 157584,
	}

	public enum PoisonOffHand
	{
		CripplingPoison = 3408,
		LeechingPoison = 108211,
	}


	public abstract class SerbRogue : CombatRotation
	{

		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;
		[JsonProperty ("Use range attack")]
		public bool UseRangedAttack;
		[JsonProperty ("Run to enemy")]
		public bool Run;
		[JsonProperty ("Use multitarget")]
		public bool Multitarget = true;
		[JsonProperty ("AOE")]
		public bool Aoe = true;
		[JsonProperty ("Use Burst Of Speed in no combat")]
		public bool UseBurstOfSpeed = true;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public DateTime StartBattle;
		public DateTime StartSleepTime;
		public bool InCombat;
		public UnitObject CycleTarget;
		public String RangedAttack = "Throw";
		public Int32 OraliusWhisperingCrystalId = 118922;
		public Int32 CrystalOfInsanityId = 86569;

		public bool IsSolo {
			get {
				return Group.GetNumGroupMembers () != 1;
			}
		}

		public int EnergyMax {
			get {
				int energy = 100;
				if (HasSpell ("Venom Rush"))
					energy = energy + 15;
				if (HasGlyph (159634))
					energy = energy + 20;
				return energy;
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

		public double Health (UnitObject u = null)
		{
			u = u ?? Me;
			return u.HealthFraction;
		}

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

		public bool IsInEnrage (UnitObject o = null)
		{
			o = o ?? Target;
			if (o.HasAura ("Enrage") || o.HasAura ("Berserker Rage") || o.HasAura ("Demonic Enrage") || o.HasAura ("Aspect of Thekal") || o.HasAura ("Charge Rage") || o.HasAura ("Electric Spur") || o.HasAura ("Cornered and Enraged!") || o.HasAura ("Draconic Rage") || o.HasAura ("Brood Rage") || o.HasAura ("Determination") || o.HasAura ("Charged Fists") || o.HasAura ("Beatdown") || o.HasAura ("Consuming Bite") || o.HasAura ("Delirious") || o.HasAura ("Angry") || o.HasAura ("Blood Rage") || o.HasAura ("Berserking Howl") || o.HasAura ("Bloody Rage") || o.HasAura ("Brewrific") || o.HasAura ("Desperate Rage") || o.HasAura ("Blood Crazed") || o.HasAura ("Combat Momentum") || o.HasAura ("Dire Rage") || o.HasAura ("Dominate Slave") || o.HasAura ("Blackrock Rabies") || o.HasAura ("Burning Rage") || o.HasAura ("Bloodletting Howl"))
				return true;
			return false;
		}

		public bool IsNotForDamage (UnitObject o = null)
		{
			o = o ?? Target;
			if (o.HasAura ("Fear") || o.HasAura ("Polymorph") || o.HasAura ("Gouge") || o.HasAura ("Paralysis") || o.HasAura ("Blind") || o.HasAura ("Hex"))
				return true;
			return false;
		}

		public int Energy {
			get {
				return Me.GetPower (WoWPowerType.Energy);
			}
		}

		public int ComboPoints {
			get {
				return Me.ComboPoints;
			}
		}

		public int Anticipation {
			get {
				return SpellCharges ("Anticipation");
			}
		}

		public double EnergyRegen {
			get {
				string activeRegen = API.ExecuteLua<string> ("inactiveRegen, activeRegen = GetPowerRegen(); return activeRegen");
				return Convert.ToDouble (activeRegen);
			}
		}

		public double Time {
			get {
				TimeSpan combatTime = DateTime.Now.Subtract (StartBattle);
				return combatTime.TotalSeconds;
			}
		}

		public double SleepTime {
			get {
				TimeSpan currentSleepTime = DateTime.Now.Subtract (StartSleepTime);
				return currentSleepTime.TotalSeconds;
			}
		}

		public double TimeToRegen (double e)
		{ 
			if (e > Energy)
				return (e - Energy) / EnergyRegen;
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
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && mob.IsAttackable) {
					x++;
				}
			}
			return x;
		}

		public bool IncapacitatedInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && mob.IsAttackable && IsNotForDamage (mob)) {
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

		public bool Usable (string s, double d = 0)
		{ 
			if (d == 0)
				return HasSpell (s) && Cooldown (s) == 0;
			return HasSpell (s) && Cooldown (s) <= d;
		}

		public int AmbushCost {
			get {
				int cost = 60;
				if (HasSpell ("Shadow Focus"))
					cost = 15;
				if (HasSpell ("Shadow Dance") && Me.HasAura ("Shadow Dance"))
					cost = 40;
				return cost;
			}
		}

		public double Cost (double i)
		{
			if (Me.HasAura ("Shadow Focus"))
				i = Math.Floor (i * 0.25);
			return i;
		}

		public bool HasCost (double i)
		{
			if (Me.HasAura ("Shadow Focus"))
				i = Math.Floor (i * 0.25);
			return Energy >= i;
		}

		public double TimeToDie (UnitObject o)
		{
			if (o != null)
				return o.Health / Ttd;
			return 0;
		}

		public bool Interrupt ()
		{
			var targets = Adds;
			targets.Add (Target);

			if (Usable ("Kick")) {
				if (EnemyInRange (6) > 1 && Multitarget) {
					CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Kick (CycleTarget))
							return true;
					}
				} else if (Target.IsCastingAndInterruptible () && Target.IsInCombatRangeAndLoS && Target.RemainingCastTime > 0)
				if (Kick (Target))
					return true;
			}
			if (Usable ("Deadly Throw") && (ComboPoints == 5 || (HasSpell ("Anticipation") && SpellCharges ("Anticipation") > 0))) {
				if (EnemyInRange (6) > 1 && Multitarget) {
					CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange <= 30 && x.IsCasting && !IsBoss (x) && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (DeadlyThrow (CycleTarget))
							return true;
					}
				} else if (Target.IsCasting && Target.IsInLoS && Target.CombatRange <= 30 && !IsBoss (Target) && Target.RemainingCastTime > 0)
				if (DeadlyThrow (Target))
					return true;
			}
			if (Usable ("Gouge") && (InArena || InBg) && Multitarget) {
				CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && x.IsCasting && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Gouge (CycleTarget))
						return true;
				}
			}

			return false;
		}

		public bool Kick (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Kick", () => Usable ("Kick") && u.IsInCombatRangeAndLoS, u);
		}

		public bool Shiv (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shiv", () => Usable ("Shiv") && HasCost (20) && u.IsInCombatRangeAndLoS, u);
		}

		public  bool DeadlyThrow (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Deadly Throw", () => Usable ("Deadly Throw") && HasCost (35) && u.IsInLoS && u.CombatRange <= 30, u);
		}

		public  bool Gouge (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Gouge", () => Usable ("Gouge") && HasCost (45) && u.IsInCombatRangeAndLoS, u);
		}

		public  bool UnEnrage ()
		{
			var targets = Adds;
			targets.Add (Target);

			if (HasSpell ("Shiv") && Cooldown ("Shiv") == 0 && HasCost (20)) {
				if (EnemyInRange (6) > 1 && Multitarget) {
					CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && IsInEnrage (x) && !IsBoss (x)).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Shiv (CycleTarget))
							return true;
					}
				} else if (!IsBoss () && IsInEnrage ()) {
					if (Shiv ())
						return true;
				}
			}
			return false;
		}

		public bool MainHandPoison (PoisonMaindHand mH)
		{
			return HasSpell ((int)mH) && CastSelfPreventDouble ((int)mH, () => !Me.HasAura ((int)mH) || Me.AuraTimeRemaining ((int)mH) < 300);
		}

		public bool OffHandPoison (PoisonOffHand oH)
		{
			return HasSpell ((int)oH) && CastSelfPreventDouble ((int)oH, () => !Me.HasAura ((int)oH) || Me.AuraTimeRemaining ((int)oH) < 300);
		}

		public bool Stealth ()
		{
			return CastSelf ("Stealth", () => Usable ("Stealth") && !Me.HasAura ("Stealth") && !Me.HasAura ("Vanish") && !Me.HasAura ("Shadow Dance") && !Me.HasAura ("Subterfuge"));
		}

		public bool CloakofShadows ()
		{
			return CastSelf ("Cloak of Shadows", () => Usable ("Cloak of Shadows"));
		}

		public bool CombatReadiness ()
		{
			return CastSelf ("Combat Readiness", () => Usable ("Combat Readiness"));
		}

		public bool Evasion ()
		{
			return CastSelf ("Evasion", () => Usable ("Evasion"));
		}

		public bool Ambush (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Ambush", () => Energy >= AmbushCost && (Me.HasAura ("Stealth") || Me.HasAura ("Vanish") || Me.HasAura ("Shadow Dance") || Me.HasAura ("Subterfuge")) && u.IsInCombatRangeAndLoS, u);
		}

		public bool Recuperate ()
		{
			return Cast ("Recuperate", () => HasCost (30) && ComboPoints > 0 && !Me.HasAura ("Recuperate"));
		}

		public bool BurstofSpeed ()
		{
			return CastSelf ("Burst of Speed", () => Usable ("Burst of Speed") && !Me.HasAura ("Sprint") && !Me.HasAura ("Burst of Speed") && HasCost (30));
		}

		public bool Preparation ()
		{
			return CastSelf ("Preparation", () => Usable ("Preparation"));
		}

		public bool BloodFury (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("BloodFury", () => Usable ("Blood Fury") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool Berserking (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Berserking", () => Usable ("Berserking") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool ArcaneTorrent (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool BladeFlurry ()
		{
			return CastSelf ("Blade Flurry", () => Usable ("Blade Flurry"));
		}

		public bool ShadowReflection (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shadow Reflection", () => Usable ("Shadow Reflection") && u.CombatRange <= 20 && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool Vanish ()
		{
			return CastSelf ("Vanish", () => Usable ("Vanish"));
		}

		public bool SliceandDice ()
		{
			return Cast ("Slice and Dice", () => HasCost (25) && ComboPoints > 0);
		}

		public bool MarkedforDeath (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Marked for Death", () => Usable ("Marked for Death") && u.IsInLoS && u.CombatRange <= 30, u);
		}

		public bool AdrenalineRush (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Adrenaline Rush", () => Usable ("Adrenaline Rush") && (IsPlayer (u) || IsElite (u) || EnemyInRange (10) > 2) && u.IsInCombatRangeAndLoS);
		}

		public bool KillingSpree (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Killing Spree", () => Usable ("Killing Spree") && (IsPlayer (u) || IsElite (u) || EnemyInRange (10) > 2) && u.CombatRange <= 10, u);
		}

		public bool RevealingStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Revealing Strike", () => Usable ("Revealing Strike") && HasCost (40) && u.IsInCombatRangeAndLoS, u);
		}

		public bool SinisterStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Sinister Strike", () => Usable ("Sinister Strike") && HasCost (50) && u.IsInCombatRangeAndLoS, u);
		}

		public bool DeathfromAbove (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Death from Above", () => Usable ("Death from Above") && HasCost (50) && u.IsInLoS && u.CombatRange <= 15 && ComboPoints > 0, u);
		}

		public bool Eviscerate (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Eviscerate", () => Usable ("Eviscerate") && HasCost (35) && ComboPoints > 0 && u.IsInCombatRangeAndLoS, u);
		}

		public bool KidneyShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Kidney Shot", () => Usable ("Kidney Shot") && HasCost (25) && ComboPoints > 0 && !Me.HasAura ("Stealth") && !Me.HasAura ("Vanish") && !Me.HasAura ("Shadow Dance") && !Me.HasAura ("Subterfuge") && u.IsInCombatRangeAndLoS, u);
		}

		public bool CrimsonTempest (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Crimson Tempest", () => Usable ("Crimson Tempest") && HasCost (35) && ComboPoints > 0 && u.IsInCombatRangeAndLoS, u);
		}

		public bool Shadowstep (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shadowstep", () => Usable ("Shadowstep") && u.IsInLoS && u.CombatRange <= 25, u);
		}

		public bool Sprint ()
		{
			return CastSelf ("Sprint", () => Usable ("Sprint"));
		}

		public bool Healthstone ()
		{
			if (API.HasItem (5512) && API.ItemCooldown (5512) == 0)
				return API.UseItem (5512);
			return false;
		}

		public bool CrystalOfInsanity ()
		{
			if (!InArena && API.HasItem (CrystalOfInsanityId) && !HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityId) == 0)
				return (API.UseItem (CrystalOfInsanityId));
			return false;
		}

		public bool OraliusWhisperingCrystal ()
		{
			if (API.HasItem (OraliusWhisperingCrystalId) && !HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalId) == 0)
				return API.UseItem (OraliusWhisperingCrystalId);
			return false;
		}

		public bool TricksoftheTrade ()
		{
			if (Usable ("Tricks of the Trade")) {
				CycleTarget = Group.GetGroupMemberObjects ().Where (x => !x.IsDead && x.IsInLoS && x.CombatRange < 100 && x.IsTank).OrderBy (x => Health (x)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Cast ("Tricks of the Trade", CycleTarget, () => CycleTarget != null))
					return true;
			}
			return false;
		}

		public bool FanofKnives ()
		{
			return Cast ("Fan of Knives", () => Usable ("Fan of Knives") && HasCost (35));
		}

		public bool Backstab (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Backstab", () => Usable ("Backstab") && HasCost (35) && Me.IsNotInFront (u) && u.IsInCombatRangeAndLoS, u);
		}

		public bool Hemorrhage (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Hemorrhage", () => Usable ("Hemorrhage") && HasCost (30) && u.IsInCombatRangeAndLoS, u);
		}

		public bool ShurikenToss ()
		{
			return Cast ("Shuriken Toss", () => Usable ("Shuriken Toss") && HasCost (40) && Target.IsInLoS && Target.CombatRange > 10 && Target.CombatRange <= 30);
		}

		public  bool Rupture (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Rupture", () => Usable ("Rupture") && HasCost (25) && u.IsInCombatRangeAndLoS, u);
		}

		public  bool Freedom ()
		{
			return WilloftheForsaken () || EveryManforHimself ();
		}

		public  bool WilloftheForsaken ()
		{
			return CastSelf ("Will of the Forsaken", () => Usable ("Will of the Forsaken"));
		}

		public  bool EveryManforHimself ()
		{
			return CastSelf ("Every Man for Himself", () => Usable ("Every Man for Himself"));
		}

		public  bool SmokeBomb ()
		{
			return CastSelf ("Smoke Bomb", () => Usable ("Smoke Bomb"));
		}

		public  bool Premeditation ()
		{
			return CastSelf ("Premeditation", () => Usable ("Premeditation") && Me.HasAura ("Stealth"));
		}

		public  bool Feint ()
		{
			return CastSelf ("Feint", () => Usable ("Feint") && HasCost (20) && !Me.HasAura ("Feint"));
		}

		public  bool CheapShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Cheap Shot", () => Usable ("Cheap Shot") && HasCost (40) && Me.HasAura ("Stealth") && u.IsInCombatRangeAndLoS, u);
		}

		public  bool Blind (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Blind", () => Usable ("Blind") && HasCost (15) && u.IsInCombatRangeAndLoS, u);
		}

		public  bool ShadowDance ()
		{
			return CastSelf ("Shadow Dance", () => Usable ("Shadow Dance"));
		}

		public  bool Garrote (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Garrote", () => Usable ("Garrote") && HasCost (45) && u.IsInCombatRangeAndLoS, u);
		}

		public  bool Shadowmeld ()
		{
			return CastSelf ("Shadowmeld", () => Usable ("Shadowmeld"));
		}

		public  bool CastSpell (String s)
		{
			switch (s) {
			case "Garrote":
				return Garrote ();
			case "Ambush":
				return Ambush ();
			case "Shadow Dance":
				return ShadowDance ();
			case "Shadowmeld":
				return Shadowmeld ();
			case "Vanish":
				return Vanish ();
			}
			return false;
		}

		public bool Mutilate (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Mutilate", () => Usable ("Mutilate") && HasCost (55) && u.IsInCombatRangeAndLoS, u);
		}

		public  bool Vendetta (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Vendetta", () => Usable ("Vendetta") && u.IsInCombatRangeAndLoS && (IsPlayer (u) || IsElite (u)), u);
		}

		public  bool Envenom (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Envenom", () => Usable ("Envenom") && HasCost (35) && ComboPoints > 0 && u.IsInCombatRangeAndLoS, u);
		}

		public bool Dispatch (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Dispatch", () => Usable ("Dispatch") && ((HasCost (30) && Health (u) < 0.35) || Me.HasAura ("Blindside")), u);
		}

		public bool Heal ()
		{
			var targets = Adds;
			targets.Add (Target);

			if ((!InRaid && !InInstance && Health () < 0.9) || (!InRaid && Health () < 0.3)) {
				if (Recuperate ())
					return true;
			}
			if (Health () < 0.6 && Me.Auras.Any (x => x.IsDebuff && x.DebuffType.Contains ("magic")))
				CloakofShadows ();
			if (Health () < 0.65)
				CombatReadiness ();
			if (Health () < 0.4)
				Evasion ();
			if (Health () < 0.45) {
				if (Healthstone ())
					return true;
			}
			if (!Me.IsMoving && Health () < 0.5) {
				if (SmokeBomb ())
					return true;
			}
			if (Usable ("Feint")) {
				if ((InArena || InBg) && Health () < 0.7) {
					if (Feint ())
						return true;
				}
				if (Health () < 0.8 && (InRaid || InInstance)) {
					var useFeint = targets.Where (x => IsBoss (x) && x.CombatRange <= 30 && x.IsCasting && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (useFeint != null) {
						if (Feint ())
							return true;
					}
				}
				if (IsBoss (Target) && Target.IsCasting && Target.RemainingCastTime > 0) {
					if (Feint ())
						return true;
				}
			}

			return false;
		}

		public bool Cc ()
		{
			if (Target.CanParticipateInCombat) {
				if (CheapShot ())
					return true;
			}

			if (InArena || InBg) {
				if (Usable ("Gouge") && EnemyInRange (8) == 2 && Multitarget) {
					CycleTarget = API.Players.Where (p => p.IsPlayer && p.IsEnemy && !p.IsDead && p.IsInCombatRangeAndLoS && p.CanParticipateInCombat && Target != p).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Gouge (CycleTarget))
							return true;
					}
				}

				if (Usable ("Blind") && EnemyInRange (8) == 2 && Multitarget) {
					CycleTarget = API.Players.Where (p => p.IsPlayer && p.IsEnemy && !p.IsDead && p.IsInLoS && p.CombatRange <= 15 && p.CanParticipateInCombat && Target != p).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Blind (CycleTarget))
							return true;
					}
				}
			} else {
				if (!InRaid && Usable ("Gouge") && EnemyInRange (8) == 2 && Multitarget) {
					CycleTarget = Adds.Where (x => x.IsInCombatRangeAndLoS && x.CanParticipateInCombat && Target != x).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Gouge (CycleTarget))
							return true;
					}
				}

				if (!InRaid && Usable ("Blind") && EnemyInRange (8) == 2 && Multitarget) {
					CycleTarget = Adds.Where (x => x.IsInLoS && x.CombatRange <= 15 && x.CanParticipateInCombat && Target != x).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Blind (CycleTarget))
							return true;
					}
				}			
			}

			return false;
		}

		public bool RogueRangedAttack ()
		{
			return ShurikenToss () || Throw ();
		}

		public bool Throw ()
		{
			return Cast ("Throw", () => Usable ("Throw") && !Me.IsMoving && Target.IsInLoS && Target.CombatRange > 10 && Target.CombatRange <= 30);

		}
	}
}
