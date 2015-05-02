using ReBot.API;
using Newtonsoft.Json;
using System;

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
		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public float OraliusWhisperingCrystalId = 118922;
		public float CrystalOfInsanityId = 86569;

		// Get

		public int HolyPower {
			get {
				return Me.GetPower (WoWPowerType.PaladinHolyPower);
			}
		}

		public float Range (UnitObject u = null)
		{
			u = u ?? Target;
			return u.CombatRange;
		}

		public double Cooldown (string s)
		{
			return SpellCooldown (s) > 0 ? SpellCooldown (s) : 0;
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

		// Spell

		public bool SpeedofLight ()
		{
			return CastSelf ("Speed of Light", () => Usable ("Speed of Light"));
		}

		public bool BloodFury (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
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

		public bool HolyAvenger (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Holy Avenger", () => Usable ("Holy Avenger") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool Seraphim (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Seraphim", () => Usable ("Seraphim") && u.IsInCombatRangeAndLoS);
		}

		public bool DivineProtection (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Divine Protection", () => Usable ("Divine Protection") && Health (Me) < 0.8);
		}

		public bool GuardianofAncientKings (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Guardian of Ancient Kings", () => Usable ("Guardian of Ancient Kings") && Health (Me) < 0.4 && u.IsInLoS && Range (u) <= 30, u);
		}

		public bool ArdentDefender (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Ardent Defender", () => Usable ("Ardent Defender") && Health (Me) < 0.2);
		}

		public bool EternalFlame (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Eternal Flame", () => Usable ("Eternal Flame") && HolyPower >= 1 && u.IsInLoS && Range (u) <= 40, u);
		}

		public bool ShieldoftheRighteous (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shield of the Righteous", () => Usable ("Shield of the Righteous") && HolyPower >= 3 && u.IsInLoS && Range (u) <= 5, u);
		}
	}
}

