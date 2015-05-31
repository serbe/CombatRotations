using System;
using Newtonsoft.Json;
using ReBot.API;
using System.Linq;

namespace ReBot
{
	public abstract class SerbWarrior : SerbUtils
	{
		public enum WarCry
		{
			CommandingShout = 0,
			BattleShout = 1,
		}

		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("Auto change stance")]
		public bool UseStance = true;
		[JsonProperty ("Use Berserker Rage in fear")]
		public bool UseBerserkerRage;

		public bool WaitBloodthirst;
		public DateTime PrevBloodthirst;
		public DateTime PrevRavager;

		// Get

		public bool PrevGcdBloodthirst {
			get {
				TimeSpan CombatTime = DateTime.Now.Subtract (PrevBloodthirst);
				return CombatTime.TotalSeconds < 3;
			}
		}

		public bool PrevGcdRavager {
			get {
				TimeSpan CombatTime = DateTime.Now.Subtract (PrevRavager);
				return CombatTime.TotalSeconds < 3;
			}
		}

		public bool AttackPowerBuff {
			get {
				return Me.HasAura ("Battle Shout") || Me.HasAura ("Horn of Winter");
			}
		}

		public double Frac (string s)
		{
			string scurrentCharges = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges (\"" + s + "\"); return currentCharges";
			string smaxCharges = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges (\"" + s + "\"); return maxCharges";
			string scooldownStart = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges (\"" + s + "\"); return cooldownStart";
			string scooldownDuration = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges (\"" + s + "\"); return cooldownDuration";

			double currentCharges = API.ExecuteLua<double> (scurrentCharges);
			double maxCharges = API.ExecuteLua<double> (smaxCharges);
			double cooldownStart = API.ExecuteLua<double> (scooldownStart);
			double cooldownDuration = API.ExecuteLua<double> (scooldownDuration);

			double f = currentCharges;

			if (f != maxCharges) {
				double currentTime = API.ExecuteLua<double> ("return GetTime ()");
				f = f + ((currentTime - cooldownStart) / cooldownDuration);
			}

			return f;
		}

		// Check

		public bool UseShieldBlock {
			get {
				int x = 0;
				foreach (UnitObject u in API.CollectUnits (5)) {
					if (!u.IsDead && !u.IsCasting && u.Target == Me && u.InCombat) {
						x++;
					}
				}
				return x > 0;
			}
		}

		public bool HasRage (int r)
		{
			if (Me.HasAura ("Spirits of the Lost"))
				r = r - 5;
			return r <= Rage;
		}

		// Combo

		public bool Heal ()
		{
			if (Health (Me) <= 0.35 && !HasAura ("Die by the Sword")) {
				if (RallyingCry ())
					return true;
			}
			if (Health (Me) <= 0.8) {
				if (EnragedRegeneration ())
					return true;
			}
			if (Health (Me) <= 0.8) {
				if (ImpendingVictory ())
					return true;
			}
			if (!Me.CanParticipateInCombat && UseBerserkerRage) {
				if (BerserkerRage ())
					return true;
			}
			if (Health (Me) < 0.9 && Me.HasAura ("Victorious")) {
				if (VictoryRush ())
					return true;
			}
			if (Health (Me) < 0.9 && HasAura ("Victorious")) {
				if (ImpendingVictory ())
					return true;
			}

//			if (CastSelf ("Defensive Stance", () => Me.HealthFraction <= (DefenseStance / 100) && !IsInShapeshiftForm ("Defensive Stance")))
//				return; //Defensive stance
//			if (CastSelf ("Battle Stance", () => Me.HealthFraction >= (BattleStance / 100) && !IsInShapeshiftForm ("Battle Stance")))
//				return;
//			if (CastSelf ("Shield Barrier", () => Me.HealthFraction >= .4 && IsInShapeshiftForm ("Defensive Stance") && Me.GetPower (WoWPowerType.Rage) >= 30 && !HasAura ("Shield Barrier")))
//				return;
//			if (CastSelf ("Berserker Rage", () => !HasAura ("Enrage") && !EnrageFear))
//				return;


//			if (Health (Me) <= DbtSwordHP) {
//				if (DiebytheSword ())
//					return;
//			}
//
//			if (CastSelf ("Shield Wall", () => MyHealth <= ShieldWallHP))
//				return;
//			if (Cast ("Shield Block", () => SpellCharges ("Shield Block") == 2 && MyRage >= 60 && MyHealth <= ShieldBlockHP && !Me.HasAura ("Shield Block")))
//				return;
//			if (Cast ("Shield Barrier", () => MyRage >= 20 && !Me.HasAura ("Shield Barrier") && MyHealth <= ShieldBarrHP))
//				return;
//			if (CastSelf ("Rallying Cry", () => MyHealth <= RallyingCryHP))
//				return;
//			if (CastSelf ("Demoralizing Shout",	() => MyHealth <= DemoralShoutHP))
//				return;

			return false;
		}

		public bool Buff (WarCry shout)
		{
			if (shout == WarCry.CommandingShout && (AttackPowerBuff || !Me.HasAura ("Commanding Shout")) && Usable ("Commanding Shout") && CS ("Commanding Shout"))
				return true;
			if (shout == WarCry.BattleShout && !AttackPowerBuff && Usable ("Battle Shout") && CS ("Battle Shout"))
				return true;

			return false;
		}

		public bool Interrupt ()
		{
			if (Usable ("Pummel")) {
				Unit = Enemy.Where (u => u.IsCastingAndInterruptible () && Range (6, u) && u.RemainingCastTime > 0 && (u.Target == Me && !Me.HasAura ("Spell Reflect")) && !Me.HasAura ("Mass Spell Reflection")).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Pummel (Unit))
					return true;
			}
			if (Usable ("Storm Bolt")) {
				Unit = Enemy.Where (u => u.IsCasting && !IsBoss (u) && Range (30, u) && u.RemainingCastTime > 0 && (u.Target == Me && !Me.HasAura ("Spell Reflect")) && !Me.HasAura ("Mass Spell Reflection")).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && StormBolt (Unit))
					return true;
			}
			if (Target.HasAura ("Divine Shield") || Target.HasAura ("Blessing of Protection") || Target.HasAura ("Ice Block")) {
				if (ShatteringThrow ())
					return true; //Break the Bubble
			}

			return false;
		}

		public bool Reflect ()
		{
			if (Usable ("Spell Reflection") && !HasGlobalCooldown ()) {
				Unit = Enemy.Where (u => u.IsCasting && u.RemainingCastTime > 0 && u.Target == Me && !Me.HasAura ("Mass Spell Reflection")).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && SpellReflection ())
					return true;
			}
			if (Usable ("Mass Spell Reflection") && !HasGlobalCooldown ()) {
				Unit = Enemy.Where (u => u.IsCasting && u.RemainingCastTime > 0 && u.Target == Me && !Me.HasAura ("Spell Reflection")).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && MassSpellReflection ())
					return true;
			}

			return false;
		}

		// Items


		// ------- Spells

		public bool Charge (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Charge") && (Range (25, u, 8) || (HasGlyph (58097) && Range (30, u, 8))) && C ("Charge", u);
		}

		public bool BloodFury ()
		{
			return Usable ("Blood Fury") && Danger () && CS ("Blood Fury");
		}

		public bool MassSpellReflection ()
		{
			return Usable ("Mass Spell Reflection") && CS ("Mass Spell Reflection");
		}

		public bool SpellReflection ()
		{
			return Usable ("Spell Reflection") && CS ("Spell Reflection");
		}

		public bool DiebytheSword ()
		{
			return Usable ("Die by the Sword") && CS ("Die by the Sword");
		}

		public bool BerserkerRage ()
		{
			return Usable ("Berserker Rage") && Range (5) && CS ("Berserker Rage");
		}

		public bool ShieldBlock ()
		{
			return Usable ("Shield Block") && HasRage (60) && CS ("Shield Block");
		}

		public bool ShieldBarrier ()
		{
			return Usable ("Shield Barrier") && HasRage (20) && CS ("Shield Barrier");
		}

		public bool DemoralizingShout ()
		{
			return Usable ("Demoralizing Shout") && CS ("Demoralizing Shout");
		}

		public bool EnragedRegeneration ()
		{
			return Usable ("Enraged Regeneration") && CS ("Enraged Regeneration");
		}

		public bool ShieldWall ()
		{
			return Usable ("Shield Wall") && CS ("Shield Wall");
		}

		public bool LastStand ()
		{
			return Usable ("Last Stand") && CS ("Last Stand");
		}

		public bool Stoneform ()
		{
			return Usable ("Stoneform") && CS ("Stoneform");
		}

		public bool HeroicStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Heroic Strike") && (HasRage (30) || Me.HasAura ("Ultimatum")) && Range (5, u) && C ("Heroic Strike", u);
		}

		public bool Bloodbath ()
		{
			return Usable ("Bloodbath") && CS ("Bloodbath");
		}

		public bool Avatar ()
		{
			return Usable ("Avatar") && Danger () && CS ("Avatar");
		}

		public bool ShieldSlam (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shield Slam") && Range (5, u) && C ("Shield Slam", u);
		}

		public bool ShatteringThrow (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shattering Throw") && Range (30, u) && !Me.IsMoving && C ("Shattering Throw", u);
		}

		public bool Revenge (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Revenge") && Range (5, u) && C ("Revenge", u);
		}

		public bool Rend (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Rend") && Range (5, u) && HasRage (5) && C ("Rend", u);
		}

		public bool SweepingStrikes ()
		{
			return Usable ("Sweeping Strikes") && !Me.HasAura ("Sweeping Strikes") && Rage >= 10 && CS ("Sweeping Strikes");
		}

		public bool Slam (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Slam") && Range (5, u) && HasRage (10) && C ("Slam", u);
		}

		public bool ColossusSmash (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Colossus Smash") && Range (5, u) && Rage >= 10 && C ("Colossus Smash", u);
		}

		public bool MortalStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mortal Strike") && Range (5, u) && HasRage (20) && C ("Mortal Strike", u);
		}

		public bool Ravager (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Ravager") && Range (5, u) && C ("Ravager", u);
		}

		public bool StormBolt (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Storm Bolt") && Range (30, u) && C ("Storm Bolt", u);
		}

		public bool DragonRoar ()
		{
			return Usable ("Dragon Roar") && Target.IsInCombatRangeAndLoS && CS ("Dragon Roar");
		}

		public bool ImpendingVictory (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Impending Victory") && HasRage (10) && Range (5, u) && C ("Impending Victory", u);
		}

		public bool VictoryRush (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Victory Rush") && Range (5, u) && Me.HasAura ("Victorious") && C ("Victory Rush", u);
		}

		public bool Execute (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Execute") && Range (5, u) && ((HasRage (30) && Health (u) <= 0.2) || Me.HasAura ("Sudden Death")) && C ("Execute", u);
		}

		public bool Devastate (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Devastate") && Range (5, u) && C ("Devastate", u);
		}

		public bool ThunderClap ()
		{
			return Usable ("Thunder Clap") && (ActiveEnemies (8) > 0 || (HasGlyph (63324) && ActiveEnemies (12) > 0)) && CS ("Thunder Clap");
		}

		public bool Bladestorm ()
		{
			return Usable ("Bladestorm") && Danger () && CS ("Bladestorm");
		}

		public bool Shockwave (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shockwave") && Range (10, u) && C ("Shockwave", u);
		}

		public bool ShieldCharge (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shield Charge") && HasRage (20) && Range (10, u) && C ("Shield Charge", u);
		}

		public bool HeroicLeap (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Heroic Leap") && Range (40, u, 8) && ((HasGlyph (63325) && Range (25, u)) || Range (40, u)) && COT ("Heroic Leap", u);
		}

		public bool HeroicThrow (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Heroic Throw") && Range (30, u, 8) && CPD ("Heroic Throw", u, 2000);
		}

		public bool DefensiveStance ()
		{
			return Usable ("Defensive Stance") && !(Me.HasAura ("Defensive Stance") || Me.HasAura ("Improved Defensive Stance")) && CS ("Defensive Stance");
		}

		public bool BattleStance ()
		{
			return Usable ("Battle Stance") && !IsInShapeshiftForm ("Battle Stance") && CS ("Battle Stance");
		}

		public bool RallyingCry ()
		{
			return Usable ("Rallying Cry") && CS ("Rallying Cry");
		}

		public bool Recklessness ()
		{
			return Usable ("Recklessness") && DangerBoss () && CS ("Recklessness");
		}

		public bool WildStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Wild Strike") && (Rage >= 45 || (Me.HasAura ("Furious Strikes") && Rage >= 20)) && Range (5, u) && C ("Wild Strike", u);
		}

		public bool Bloodthirst (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Bloodthirst") && Range (5, u) && C ("Bloodthirst", u);
		}

		public bool Siegebreaker (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Siegebreaker") && Range (5, u) && C ("Siegebreaker", u);
		}

		public bool RagingBlow (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Raging Blow") && Rage >= 10 && Range (5, u) && C ("Raging Blow", u);
		}

		public bool Pummel (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Pummel") && Range (5, u) && C ("Pummel", u);
		}

		public bool Whirlwind (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Whirlwind") && HasRage (20) && (Range (8, u) || (HasGlyph (63324) && Range (12, u))) && C ("Whirlwind", u);
		}
	}
}
