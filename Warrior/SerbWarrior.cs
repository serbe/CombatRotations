using ReBot.API;

namespace ReBot
{
	public abstract class SerbWarrior : CombatRotation
	{
		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;

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

		public bool Usable (string s)
		{ 
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		// ------- Spells

		public bool Charge (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Charge", () => Usable ("Charge") && u.IsInLoS && Range (u) >= 8 && (Range (u) <= 25 || (HasGlyph (58097) && Range (u) <= 30)), u);
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
			return CastSelf ("Shield Block", () => Usable ("Shield Block") && Rage >= 60);
		}
	}
}

