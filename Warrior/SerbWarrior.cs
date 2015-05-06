using ReBot.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ReBot
{
	public abstract class SerbWarrior : CombatRotation
	{
		public enum WarCry
		{
			CommandingShout = 0,
			BattleShout = 1,
		}

		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("Auto change stance")]
		public bool UseStance = true;

		public bool InCombat;
		public DateTime StartBattle;
		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public UnitObject CycleTarget;

		// Get

		public bool AttackPowerBuff {
			get {
				return Me.HasAura ("Battle Shout") || Me.HasAura ("Horn of Winter");
			}
		}


		public List<UnitObject> Enemy {
			get {
				var targets = Adds;
				targets.Add (Target);
				return targets;
			}
		}

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

		public int ActiveEnemies (int range)
		{
			int x = 0;
			foreach (UnitObject u in API.CollectUnits(range)) {
				if ((u.IsEnemy || Me.Target == u) && !u.IsDead && u.IsAttackable && u.InCombat) {
					x++;
				}
			}
			return x;
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

		public bool Range (int r, UnitObject u = null, int l = 0)
		{
			u = u ?? Target;
			if (l != 0)
				return u.IsInLoS && u.CombatRange <= r && u.CombatRange >= l;
			return u.IsInLoS && u.CombatRange <= r;
		}

		public bool Danger (UnitObject u = null, int r = 0, int e = 2)
		{
			u = u ?? Target;
			if (r != 0)
				return Range (r, u) && (IsElite (u) || IsPlayer (u) || ActiveEnemies (10) > e);
			return u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || ActiveEnemies (10) > e);
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

		// Combo

		public bool Buff (WarCry Shout)
		{
			if (CastSelf ("Commanding Shout",	() => Shout == WarCry.CommandingShout && (AttackPowerBuff || !Me.HasAura ("Commanding Shout"))))
				return true;
			if (CastSelf ("Battle Shout", () => Shout == WarCry.BattleShout && !AttackPowerBuff))
				return true;

			return false;
		}

		public bool Interrupt ()
		{
			return false;
		}

		// Items

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

		// ------- Spells

		public bool Charge (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Charge", () => Usable ("Charge") && (Range (25, u, 8) || (HasGlyph (58097) && Range (30, u, 8))), u);
		}

		public bool BloodFury ()
		{
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && Danger ());
		}

		public bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Danger ());
		}

		public bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Danger ());
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
			return Cast ("Storm Bolt", () => Usable ("Storm Bolt") && Range (30, u), u);
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
			return Cast ("Execute", () => Usable ("Execute") && u.IsInCombatRangeAndLoS && ((HasRage (30) && Health (u) <= 0.2) || Me.HasAura ("Sudden Death")), u);
		}

		public bool Devastate (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Devastate", () => Usable ("Devastate") && u.IsInCombatRangeAndLoS, u);
		}

		public bool ThunderClap (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Thunder Clap", () => Usable ("Thunder Clap") && Range (8, u));
		}

		public bool Bladestorm (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Bladestorm", () => Usable ("Bladestorm") && u.IsInCombatRangeAndLoS);
		}

		public bool Shockwave (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shockwave", () => Usable ("Shockwave") && Range (10, u), u);
		}

		public bool ShieldCharge (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shield Charge", () => Usable ("Shield Charge") && HasRage (20) && Range (10, u), u);
		}

		public bool HeroicLeap (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Heroic Leap", u.Position, () => Usable ("Heroic Leap") && Range (40, u, 8) && ((HasGlyph (63325) && Range (25, u)) || Range (40, u)));
		}

		public bool HeroicThrow (UnitObject u = null)
		{
			u = u ?? Target;
			return CastPreventDouble ("Heroic Throw", () => Usable ("Heroic Throw") && Range (30, u, 8), u, 2000);
		}

		public bool DefensiveStance ()
		{
			return CastSelf ("Defensive Stance", () => Usable ("Defensive Stance") && !(Me.HasAura ("Defensive Stance") || Me.HasAura ("Improved Defensive Stance")));
		}

		public bool BattleStance ()
		{
			return CastSelf ("Battle Stance", () => Usable ("Battle Stance") && !IsInShapeshiftForm ("Battle Stance"));
		}

		public bool RallyingCry ()
		{
			return CastSelf ("Rallying Cry", () => Usable ("Rallying Cry"));
		}
	}
}
