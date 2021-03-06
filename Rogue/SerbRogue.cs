﻿using System;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot
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

	public abstract class SerbRogue : SerbUtils
	{

		[JsonProperty ("Use multitarget")]
		public bool Multitarget = true;
		[JsonProperty ("Use Feith")]
		public bool UseFeith = true;

		// Get

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

		// Check

		public bool MeInStealth {
			get {
				return Me.HasAura ("Stealth") || Me.HasAura ("Shadow Dance") || Me.HasAura ("Subterfuge");
			}
		}

		public bool HasCost (double i)
		{
			if (Me.HasAura ("Shadow Focus"))
				i = Math.Floor (i * 0.25);
			return Energy >= i;
		}

		// Combo

		public bool Interrupt ()
		{
			if (Usable ("Kick")) {
				if (InArena) {
					var Unit = API.CollectUnits (u => u.IsAttackable && u.IsPlayer && Range (5, u) && u.IsCastingAndInterruptible () && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Kick (Unit))
						return true;
				} else {
					if (ActiveEnemies (6) > 1 && Multitarget) {
						var Unit = Enemy.Where (u => Range (5, u) && u.IsCastingAndInterruptible () && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
						if (Unit != null && Kick (Unit))
							return true;
					} else if (Target.IsCastingAndInterruptible () && Range (5, Target) && Target.RemainingCastTime > 0)
					if (Kick (Target))
						return true;
				}
			}
			if (Usable ("Deadly Throw") && (ComboPoints == 5 || (HasSpell ("Anticipation") && SpellCharges ("Anticipation") > 0))) {
				if (InArena) {
					var Unit = API.CollectUnits (u => u.IsAttackable && u.IsPlayer && Range (30, u) && u.IsCasting && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && DeadlyThrow (Unit))
						return true;
				} else {
					if (ActiveEnemies (6) > 1 && Multitarget) {
						var Unit = Enemy.Where (u => Range (30, u) && u.IsCasting && !IsBoss (u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
						if (Unit != null && DeadlyThrow (Unit))
							return true;
					} else if (Target.IsCasting && Range (30, Target) && !IsBoss (Target) && Target.RemainingCastTime > 0)
					if (DeadlyThrow (Target))
						return true;
				}
			}
			if (Usable ("Gouge") && (InArena || InBg) && Multitarget) {
				var Unit = API.CollectUnits (u => u.IsAttackable && u.IsPlayer && Range (5, u) && u.IsCasting && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Gouge (Unit))
					return true;
			}

			return false;
		}

		public bool UnEnrage ()
		{
			if (Usable ("Shiv") && HasCost (20)) {
				if (InArena) {
					var Unit = API.CollectUnits (u => u.IsAttackable && u.IsPlayer && Range (5, u) && IsInEnrage (u)).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Shiv (Unit))
						return true;
				} else {
					if (ActiveEnemies (6) > 1 && Multitarget) {
						var Unit = Enemy.Where (u => Range (5, u) && IsInEnrage (u)).DefaultIfEmpty (null).FirstOrDefault ();
						if (Unit != null && Shiv (Unit))
							return true;
					} else if (IsInEnrage ()) {
						if (Shiv ())
							return true;
					}
				}
			}
			return false;
		}

		public bool CastSpell (String s)
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

		public bool Heal ()
		{
			if (Health (Me) < 0.4 && !Me.HasAura ("Combat Readiness"))
				Evasion ();
			if (Health (Me) < 0.45) {
				if (Healthstone ())
					return true;
			}
			if (!Me.IsMoving && Health (Me) < 0.5 && !Me.HasAura ("Evasion")) {
				if (SmokeBomb ())
					return true;
			}
			if (Health (Me) < 0.6 && Me.Auras.Any (a => a.IsDebuff && a.DebuffType.Contains ("magic")))
				CloakofShadows ();
			if (Health (Me) < 0.65 && !Me.HasAura ("Evasion"))
				CombatReadiness ();
			if (Usable ("Feint") && UseFeith) {
				if ((InArena || InBg) && Health (Me) < 0.7) {
					if (Feint ())
						return true;
				}
				if (Health (Me) < 0.8 && (InRaid || InInstance)) {
					var useFeint = Enemy.Where (u => IsBoss (u) && Range (30, u) && u.IsCasting && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (useFeint != null && Feint ())
						return true;
				}
				if (IsBoss (Target) && Target.IsCasting && Target.RemainingCastTime > 0) {
					if (Feint ())
						return true;
				}
			}
			if ((!InRaid && !InInstance && Health (Me) < 0.9) || (!InRaid && Health (Me) < 0.3)) {
				if (ComboPoints > 0 && Recuperate ())
					return true;
			}
			return false;
		}

		public bool Cc ()
		{

			if (Target.CanParticipateInCombat) {
				if (CheapShot ())
					return true;
			}

			if (!Me.HasAura ("Blade Flurry")) {
				if (InArena || InBg) {
					if (Usable ("Blind")) {
						var Unit = API.CollectUnits (u => u.IsAttackable && u.CanParticipateInCombat && u.InCombat && u.IsPlayer && Range (15, u, 8) && u != Target && (HasGlyph (91299) || HasSpell ("Dirty Tricks") || !u.Auras.Any (x => x.IsDebuff && x.DebuffType.Contains ("Poison")))).DefaultIfEmpty (null).FirstOrDefault ();
						if (Unit != null && Blind (Unit))
							return true;
					}
					if (Usable ("Gouge")) {
						var Unit = API.CollectUnits (u => u.IsAttackable && u.CanParticipateInCombat && u.InCombat && u.IsPlayer && Range (5, u) && u != Target).DefaultIfEmpty (null).FirstOrDefault ();
						if (Unit != null && Gouge (Unit))
							return true;
					}
				} else {
					if (!InRaid && Usable ("Gouge") && ActiveEnemies (8) == 2 && Multitarget) {
						var Unit = Enemy.Where (u => Range (5, u) && u.CanParticipateInCombat && u != Target).DefaultIfEmpty (null).FirstOrDefault ();
						if (Unit != null && Gouge (Unit))
							return true;
					}

					if (!InRaid && Usable ("Blind") && ActiveEnemies (15) == 2 && Multitarget) {
						var Unit = Enemy.Where (u => Range (15, u) && u.CanParticipateInCombat && u != Target).DefaultIfEmpty (null).FirstOrDefault ();
						if (Unit != null && Blind (Unit))
							return true;
					}			
				}
			}
			return false;
		}

		public bool UnBurst ()
		{
			if (InArena) {
				var Unit = API.CollectUnits (u => u.IsAttackable && u.CanParticipateInCombat && u.IsPlayer && u.IsTargetingMeOrPets && Range (15, u, 8) && u.Auras.Any (a => BurstAura.Contains (a.Name))).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (Blind (Unit))
						return true;
					if (Gouge (Unit))
						return true;
					if (KidneyShot (Unit))
						return true;
				}
			}

			return false;
		}

		public bool RogueRangedAttack ()
		{
			return ShurikenToss () || Throw ();
		}

		// Skills

		public bool Kick (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Kick") && Range (5, u) && C ("Kick", u);
		}

		public bool Shiv (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shiv") && HasCost (20) && Range (5, u) && C ("Shiv", u, true);
		}

		public bool DeadlyThrow (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Deadly Throw") && HasCost (35) && Range (30, u) && C ("Deadly Throw", u);
		}

		public bool Gouge (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Gouge") && (HasSpell ("Dirty Tricks") || HasCost (45)) && Range (5, u) && C ("Gouge", u, true);
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
			return Usable ("Stealth") && !Me.HasAura ("Vanish") && !MeInStealth && CS ("Stealth");
		}

		public bool CloakofShadows ()
		{
			return Usable ("Cloak of Shadows") && CS ("Cloak of Shadows");
		}

		public bool CombatReadiness ()
		{
			return Usable ("Combat Readiness") && CS ("Combat Readiness");
		}

		public bool Evasion ()
		{
			return Usable ("Evasion") && CS ("Evasion");
		}

		public bool Ambush (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Ambush", () => Energy >= AmbushCost && (Me.HasAura ("Vanish") || MeInStealth) && (Range (5, u) || (HasGlyph (56813) && Range (10, u)) || (HasSpell ("Cloak and Dagger") && Range (40, u))), u);
		}

		public bool Recuperate ()
		{
			return HasCost (30) && ComboPoints > 0 && !Me.HasAura ("Recuperate") && C ("Recuperate");
		}

		public bool BurstofSpeed ()
		{
			return Usable ("Burst of Speed") && !Me.HasAura ("Sprint") && !Me.HasAura ("Burst of Speed") && HasCost (30) && CS ("Burst of Speed");
		}

		public bool Preparation ()
		{
			return Usable ("Preparation") && CS ("Preparation");
		}

		public bool BloodFury ()
		{
			return Usable ("Blood Fury") && Danger () && Range (5) && CS ("BloodFury");
		}

		public bool Berserking ()
		{
			return Usable ("Berserking") && Danger () && Range (5) && CS ("Berserking");
		}

		public bool ArcaneTorrent ()
		{
			return Usable ("Arcane Torrent") && Danger () && Range (5) && CS ("Arcane Torrent");
		}

		public bool BladeFlurry ()
		{
			return Usable ("Blade Flurry") && CS ("Blade Flurry");
		}

		public bool ShadowReflection (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shadow Reflection") && Range (20, u) && Danger (u) && C ("Shadow Reflection", u);
		}

		public bool Vanish ()
		{
			return Usable ("Vanish") && !MeInStealth && CS ("Vanish");
		}

		public bool SliceandDice ()
		{
			return HasCost (25) && ComboPoints > 0 && C ("Slice and Dice");
		}

		public bool MarkedforDeath (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Marked for Death") && Range (30, u) && C ("Marked for Death", u);
		}

		public bool AdrenalineRush (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Adrenaline Rush") && Danger (u) && Range (5, u) && CS ("Adrenaline Rush");
		}

		public bool KillingSpree (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Killing Spree") && Danger (u) && Range (10, u) && C ("Killing Spree", u);
		}

		public bool RevealingStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Revealing Strike") && HasCost (40) && Range (5, u) && C ("Revealing Strike", u);
		}

		public bool SinisterStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Sinister Strike") && HasCost (50) && Range (5, u) && C ("Sinister Strike", u);
		}

		public bool DeathfromAbove (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Death from Above") && HasCost (50) && Range (15, u) && ComboPoints > 0 && C ("Death from Above", u);
		}

		public bool Eviscerate (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Eviscerate") && HasCost (35) && ComboPoints > 0 && Range (5, u) && C ("Eviscerate", u);
		}

		public bool KidneyShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Kidney Shot") && HasCost (25) && ComboPoints > 0 && !Me.HasAura ("Vanish") && !MeInStealth && Range (5, u) && C ("Kidney Shot", u);
		}

		public bool CrimsonTempest (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Crimson Tempest") && HasCost (35) && ComboPoints > 0 && Range (5, u) && C ("Crimson Tempest");
		}

		public bool Shadowstep (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shadowstep") && Range (25, u) && C ("Shadowstep", u);
		}

		public bool Sprint ()
		{
			return Usable ("Sprint") && CS ("Sprint");
		}

		public bool TricksoftheTrade ()
		{
			if (Usable ("Tricks of the Trade")) {
				var Unit = MyGroup.Where (u => !u.IsDead && Range (100, u) && u.IsTank).OrderBy (u => Health (u)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && C ("Tricks of the Trade", Unit))
					return true;
			}
			return false;
		}

		public bool FanofKnives ()
		{
			return Usable ("Fan of Knives") && HasCost (35) && C ("Fan of Knives");
		}

		public bool Backstab (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Backstab") && HasCost (35) && Me.IsNotInFront (u) && Range (5, u) && C ("Backstab", u);
		}

		public bool Hemorrhage (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Hemorrhage") && HasCost (30) && Range (5, u) && C ("Hemorrhage", u);
		}

		public bool ShurikenToss (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shuriken Toss") && HasCost (40) && Range (30, u, 10) && C ("Shuriken Toss", u);
		}

		public bool Rupture (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Rupture") && HasCost (25) && Range (5, u) && C ("Rupture", u);
		}

		public bool SmokeBomb ()
		{
			return Usable ("Smoke Bomb") && CS ("Smoke Bomb");
		}

		public bool Premeditation ()
		{
			return Usable ("Premeditation") && MeInStealth && CS ("Premeditation");
		}

		public bool Feint ()
		{
			return Usable ("Feint") && HasCost (20) && !Me.HasAura ("Feint") && CS ("Feint");
		}

		public bool CheapShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Cheap Shot") && HasCost (40) && MeInStealth && (Range (5, u) || (HasSpell ("Cloak and Dagger") && Range (40, u))) && C ("Cheap Shot", u);
		}

		public bool Blind (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Blind") && (HasSpell ("Dirty Tricks") || HasCost (15)) && Range (15, u) && C ("Blind", u, true);
		}

		public bool ShadowDance ()
		{
			return Usable ("Shadow Dance") && CS ("Shadow Dance");
		}

		public bool Garrote (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Garrote") && HasCost (45) && (Range (5, u) || (HasSpell ("Cloak and Dagger") && Range (40, u))) && C ("Garrote", u);
		}

		public bool Shadowmeld ()
		{
			return Usable ("Shadowmeld") && CS ("Shadowmeld");
		}

		public bool Mutilate (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mutilate") && HasCost (55) && Range (5, u) && C ("Mutilate", u);
		}

		public bool Vendetta (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Vendetta") && Danger (u) && Range (5, u) && C ("Vendetta", u);
		}

		public bool Envenom (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Envenom") && HasCost (35) && ComboPoints > 0 && Range (5, u) && C ("Envenom", u);
		}

		public bool Dispatch (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Dispatch") && ((HasCost (30) && Health (u) < 0.35) || Me.HasAura ("Blindside")) && Range (5, u) && C ("Dispatch", u);
		}

		public bool Throw (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Throw") && !Me.IsMoving && Range (30, u, 10) && C ("Throw", u);

		}


	}
}
