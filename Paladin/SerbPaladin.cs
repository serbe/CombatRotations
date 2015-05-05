using ReBot.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ReBot
{
	public abstract class SerbPaladin : CombatRotation
	{
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;

		// Consts && Vars

		public bool InCombat;
		public DateTime StartBattle;
		public UnitObject CycleTarget;
		public UnitObject LastJudgmentTarget;
		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public float OraliusWhisperingCrystalId = 118922;
		public float CrystalOfInsanityId = 86569;

		// Get

		public List<UnitObject> Enemy {
			get {
				var targets = Adds;
				targets.Add (Target);
				return targets;
			}
		}

		public int HolyPower {
			get {
				return Me.GetPower (WoWPowerType.PaladinHolyPower);
			}
		}

		public double Cooldown (string s)
		{
			return SpellCooldown (s) > 0 ? SpellCooldown (s) : 0;
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

		public double Time {
			get {
				TimeSpan combatTime = DateTime.Now.Subtract (StartBattle);
				return combatTime.TotalSeconds;
			}
		}

		public double Health (UnitObject u = null)
		{
			u = u ?? Target;
			return u.HealthFraction;
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

		// Spell

		public bool SpeedofLight ()
		{
			return CastSelf ("Speed of Light", () => Usable ("Speed of Light"));
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

		public bool HolyAvenger ()
		{
			return CastSelf ("Holy Avenger", () => Usable ("Holy Avenger") && Danger ());
		}

		public bool Seraphim (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Seraphim", () => Usable ("Seraphim") && u.IsInCombatRangeAndLoS);
		}

		public bool DivineProtection ()
		{
			return CastSelf ("Divine Protection", () => Usable ("Divine Protection"));
		}

		public bool GuardianofAncientKings (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Guardian of Ancient Kings", () => Usable ("Guardian of Ancient Kings") && Range (30, u), u);
		}

		public bool ArdentDefender ()
		{
			return CastSelf ("Ardent Defender", () => Usable ("Ardent Defender"));
		}

		public bool EternalFlame (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Eternal Flame", () => Usable ("Eternal Flame") && HolyPower >= 1 && Range (40, u), u);
		}

		public bool ShieldoftheRighteous (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shield of the Righteous", () => Usable ("Shield of the Righteous") && HolyPower >= 3 && Range (5, u), u);
		}

		// Spells

		public bool SealofInsight ()
		{
			return CastSelf ("Seal of Insight", () => Usable ("Seal of Insight"));
		}

		public bool SealofRighteousness ()
		{
			return CastSelf ("Seal of Righteousness", () => Usable ("Seal of Righteousness"));
		}

		public bool AvengersShield (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Avenger's Shield", () => Usable ("Avenger's Shield") && Range (30, u), u);
		}

		public bool HammeroftheRighteous (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Hammer of the Righteous", () => Usable ("Hammer of the Righteous") && Range (5, u), u);
		}

		public bool CrusaderStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Crusader Strike", () => Usable ("Crusader Strike") && Range (5, u), u);
		}

		public bool Judgment (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Judgment", () => Usable ("Judgment") && Range (30, u), u);
		}
	}
}

