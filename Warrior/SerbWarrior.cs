﻿using ReBot.API;
using Newtonsoft.Json;
using System;

namespace ReBot
{
	public abstract class SerbWarrior : CombatRotation
	{
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("Max rage")]
		public int RageMax = 100;
		[JsonProperty ("Auto change stance")]
		public bool UseStance = true;

		public bool InCombat;
		public DateTime StartBattle;
		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public UnitObject CycleTarget;

		// Get

		public double TimeToDie (UnitObject u = null)
		{
			u = u ?? Target;
			return u.Health / Ttd;
		}

		public int Rage {
			get {
				return Me.GetPower (WoWPowerType.Rage);
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

		public double Range (UnitObject u = null)
		{
			u = u ?? Target;
			return u.CombatRange;
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public double Frac (string s)
		{
			string scurrentCharges = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" + s + "\"); return currentCharges";
			string smaxCharges = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" + s + "\"); return maxCharges";
			string scooldownStart = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" + s + "\"); return cooldownStart";
			string scooldownDuration = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" + s + "\"); return cooldownDuration";

			double currentCharges = API.ExecuteLua<double> (scurrentCharges);
			double maxCharges = API.ExecuteLua<double> (smaxCharges);
			double cooldownStart = API.ExecuteLua<double> (scooldownStart);
			double cooldownDuration = API.ExecuteLua<double> (scooldownDuration);

			double f = currentCharges;

			if (f != maxCharges) {
				double currentTime = API.ExecuteLua<double> ("return GetTime()");
				f = f + ((currentTime - cooldownStart) / cooldownDuration);
			}

			return f;
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

		public double Health (UnitObject u = null)
		{
			u = u ?? Me;
			return u.HealthFraction;
		}

		public double Time {
			get {
				TimeSpan combatTime = DateTime.Now.Subtract (StartBattle);
				return combatTime.TotalSeconds;
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

		public bool HasRage (int r)
		{
			if (Me.HasAura ("Spirits of the Lost"))
				r = r - 5;
			return r <= Rage;
		}

		public bool Usable (string s)
		{ 
			return HasSpell (s) && Cooldown (s) == 0;
		}




		// -----------

		public bool Heal ()
		{
			
			if (Health () < 0.9 && Me.Level < 100) {
				if (VictoryRush ())
					return true;
			}
			if (Health () < 0.6 && Me.Level < 100) {
				if (ImpendingVictory ())
					return true;
			}
//			if (CastSelf ("Rallying Cry", () => Health <= 0.25))
//				return;
//			if (CastSelf ("Enraged Regeneration", () => Health <= 0.5))
//				return;
			if (Health () < 0.6) {
				if (Healthstone ())
					return true;
			}
			return true;
		}



		// ------- Spells

		public bool Charge (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Charge", () => Usable ("Charge") && u.IsInLoS && Range (u) >= 8 && (Range (u) <= 25 || (HasGlyph (58097) && Range (u) <= 30)) && u.InCombat, u);
		}

		public bool BloodFury ()
		{
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer () || EnemyInRange (10) > 2));
		}

		public bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer () || EnemyInRange (10) > 2));
		}

		public bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer () || EnemyInRange (10) > 2));
		}

		public bool BerserkerRage ()
		{
			return CastSelf ("Berserker Rage", () => Usable ("Berserker Rage") && Target.IsInLoS);
		}

		public bool ShieldBlock ()
		{
			return CastSelf ("Shield Block", () => Usable ("Shield Block") && HasRage (60));
		}

		public bool ShieldBarrier ()
		{
			return CastSelf ("Shield Barrier", () => Usable ("Shield Barrier") && HasRage (20));
		}

		public bool DemoralizingShout ()
		{
			return CastSelf ("Demoralizing Shout", () => Usable ("Demoralizing Shout"));
		}

		public bool EnragedRegeneration ()
		{
			return CastSelf ("Enraged Regeneration", () => Usable ("Enraged Regeneration"));
		}

		public bool ShieldWall ()
		{
			return CastSelf ("Shield Wall", () => Usable ("Shield Wall"));
		}

		public bool LastStand ()
		{
			return CastSelf ("Last Stand", () => Usable ("Last Stand"));
		}

		public bool DraenicArmor ()
		{
			if (API.HasItem (109220) && API.ItemCooldown (109220) == 0 && !Me.HasAura ("Draenic Armor Potion"))
				return API.UseItem (109220);
			return false;
		}

		public bool Healthstone ()
		{
			if (API.HasItem (5512) && API.ItemCooldown (5512) == 0)
				return API.UseItem (5512);
			return false;
		}

		public bool Stoneform ()
		{
			return CastSelf ("Stoneform", () => Usable ("Stoneform"));
		}

		public bool HeroicStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Heroic Strike", () => Usable ("Heroic Strike") && HasRage (30) && u.IsInCombatRangeAndLoS, u);
		}

		public bool Bloodbath ()
		{
			return CastSelf ("Bloodbath", () => Usable ("Bloodbath"));
		}

		public bool Avatar ()
		{
			return CastSelf ("Avatar", () => Usable ("Avatar"));
		}

		public bool ShieldSlam (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shield Slam", () => Usable ("Shield Slam") && u.IsInCombatRangeAndLoS, u);
		}

		public bool Revenge (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Revenge", () => Usable ("Revenge") && u.IsInCombatRangeAndLoS, u);
		}

		public bool Ravager (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Ravager", () => Usable ("Ravager") && u.IsInCombatRangeAndLoS, u);
		}

		public bool StormBolt (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Storm Bolt", () => Usable ("Storm Bolt") && u.IsInLoS && Range (u) <= 30, u);
		}

		public bool DragonRoar ()
		{
			return CastSelf ("Dragon Roar", () => Usable ("Dragon Roar") && Target.IsInCombatRangeAndLoS);
		}

		public bool ImpendingVictory (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Impending Victory", () => Usable ("Impending Victory") && HasRage (10) && u.IsInCombatRangeAndLoS, u);
		}

		public bool VictoryRush (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Victory Rush", () => Usable ("Victory Rush") && u.IsInCombatRangeAndLoS && Me.HasAura ("Victorious"), u);
		}

		public bool Execute (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Execute", () => Usable ("Execute") && u.IsInLoS && Range (u) <= 5 && ((HasRage (30) && Health (u) <= 0.2) || Me.HasAura ("Sudden Death")), u);
		}

		public bool Devastate (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Devastate", () => Usable ("Devastate") && u.IsInCombatRangeAndLoS, u);
		}

		public bool ThunderClap (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Thunder Clap", () => Usable ("Thunder Clap") && u.IsInLoS && Range (u) <= 8);
		}

		public bool Bladestorm (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Bladestorm", () => Usable ("Bladestorm") && u.IsInCombatRangeAndLoS);
		}

		public bool Shockwave (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shockwave", () => Usable ("Shockwave") && u.IsInLoS && Range (u) <= 10, u);
		}

		public bool ShieldCharge (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shield Charge", () => Usable ("Shield Charge") && HasRage (20) && u.IsInLoS && Range (u) <= 10, u);
		}

		public bool HeroicLeap (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Heroic Leap", u.Position, () => Usable ("Heroic Leap") && u.IsInLoS && Range (u) >= 8 && ((HasGlyph (63325) && Range (u) <= 25) || Range (u) <= 40));
		}

		public bool HeroicThrow (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Heroic Throw", () => Usable ("Heroic Throw") && u.IsInLoS && Range (u) >= 8 && Range (u) <= 30, u);
		}

		public bool DefensiveStance ()
		{
			return CastSelf ("Defensive Stance", () => Usable ("Defensive Stance") && !(Me.HasAura ("Defensive Stance") || Me.HasAura ("Improved Defensive Stance")));
		}

		public bool BattleStance ()
		{
			return CastSelf ("Battle Stance", () => Usable ("Battle Stance") && !Me.HasAura ("Battle Stance"));
		}
	}
}