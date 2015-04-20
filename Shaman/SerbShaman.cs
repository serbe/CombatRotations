using System.Linq;
using ReBot.API;

namespace ReBot.Shaman
{
	public abstract class SerbShaman : CombatRotation
	{
		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;

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

		// public bool isInterruptable { get { return Target.IsCastingAndInterruptible(); } }
		public bool IsInEnrage (UnitObject o)
		{
			if (o.HasAura ("Enrage") || o.HasAura ("Berserker Rage") || o.HasAura ("Demonic Enrage") || o.HasAura ("Aspect of Thekal") || o.HasAura ("Charge Rage") || o.HasAura ("Electric Spur") || o.HasAura ("Cornered and Enraged!") || o.HasAura ("Draconic Rage") || o.HasAura ("Brood Rage") || o.HasAura ("Determination") || o.HasAura ("Charged Fists") || o.HasAura ("Beatdown") || o.HasAura ("Consuming Bite") || o.HasAura ("Delirious") || o.HasAura ("Angry") || o.HasAura ("Blood Rage") || o.HasAura ("Berserking Howl") || o.HasAura ("Bloody Rage") || o.HasAura ("Brewrific") || o.HasAura ("Desperate Rage") || o.HasAura ("Blood Crazed") || o.HasAura ("Combat Momentum") || o.HasAura ("Dire Rage") || o.HasAura ("Dominate Slave") || o.HasAura ("Blackrock Rabies") || o.HasAura ("Burning Rage") || o.HasAura ("Bloodletting Howl"))
				return true;
			return false;
		}

		public bool IsNotForDamage (UnitObject o)
		{
			if (o.HasAura ("Fear") || o.HasAura ("Polymorph") || o.HasAura ("Gouge") || o.HasAura ("Paralysis") || o.HasAura ("Blind") || o.HasAura ("Hex"))
				return true;
			return false;
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

		public bool Usable (string s)
		{ 
			// Analysis disable once CompareOfFloatsByEqualityOperator
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public bool LightningShield ()
		{
			return CastSelf ("Lightning Shield", () => Usable ("Lightning Shield") && !Me.HasAura ("Lightning Shield"));
		}

		public bool Bloodlust (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Bloodlust", () => Usable ("Bloodlust") && (IsBoss (u) || IsPlayer (u)) && u.IsInCombatRangeAndLoS);
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

		public bool ElementalMastery (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Elemental Mastery", () => Usable ("Elemental Mastery") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool FeralSpirit (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Feral Spirit", () => Usable ("Feral Spirit") && u.IsInLoS && u.CombatRange < 30 && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool AncestralSwiftness (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Ancestral Swiftness", () => Usable ("Ancestral Swiftness") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool Ascendance (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Ascendance", () => Usable ("Ascendance") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool StormElementalTotem (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Storm Elemental Totem", () => Usable ("Storm Elemental Totem") && (IsBoss (u) || IsPlayer (u) || EnemyInRange (10) > 5) && u.IsInCombatRangeAndLoS);
		}

		public bool FireElementalTotem (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Fire Elemental Totem", () => Usable ("Fire Elemental Totem") && (IsBoss (u) || IsPlayer (u) || EnemyInRange (10) > 5) && u.IsInCombatRangeAndLoS);
		}

		public bool SearingTotem (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Searing Totem", () => Usable ("Searing Totem") && u.CombatRange <= 25);
		}

		public bool UnleashElements (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool ElementalBlast (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Elemental Blast", () => Usable ("Elemental Blast") && u.IsInLoS && u.CombatRange <= 40 && (!Me.IsMoving || Me.HasAura ("Ancestral Swiftness")), u);
		}

		public bool LightningBolt (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Lightning Bolt", () => Usable ("Lightning Bolt") && u.IsInLoS && u.CombatRange <= 30 && (!Me.IsMoving || Me.HasAura ("Ancestral Swiftness")), u);
		}

		//		public bool Windstrike (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("Windstrike", () => Usable ("Windstrike") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

	}
}

