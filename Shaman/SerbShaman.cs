using System;
using ReBot.API;
using System.Linq;

namespace ReBot
{
	public abstract class SerbShaman : CombatRotation
	{
		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;

		public SerbShaman ()
		{
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

		public int EnemyInRange (int range)
		{
			var targets = Adds;
			targets.Add (Target);

			return targets.Where (t => t.CombatRange <= range).ToList ().Count;
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

		public bool Bloodlust ()
		{
			return CastSelf ("Bloodlust", () => Usable ("Bloodlust") && (IsBoss (Target) || IsPlayer) && Target.IsInCombatRangeAndLoS);
		}

		public virtual bool BloodFury ()
		{
			return CastSelf ("BloodFury", () => Usable ("Blood Fury") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool ElementalMastery ()
		{
			return CastSelf ("Elemental Mastery", () => Usable ("Elemental Mastery") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool FeralSpirit ()
		{
			return Cast ("Feral Spirit", () => Usable ("Feral Spirit") && Target.IsInLoS && Target.CombatRange < 30 && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool AncestralSwiftness ()
		{
			return CastSelf ("Ancestral Swiftness", () => Usable ("Ancestral Swiftness") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool Ascendance ()
		{
			return CastSelf ("Ascendance", () => Usable ("Ascendance") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public bool StormElementalTotem ()
		{
			return CastSelf ("Storm Elemental Totem", () => Usable ("Storm Elemental Totem") && (IsBoss (Target) || IsPlayer || EnemyInRange (10) > 5) && Target.IsInCombatRangeAndLoS);
		}

		public bool FireElementalTotem ()
		{
			return CastSelf ("Fire Elemental Totem", () => Usable ("Fire Elemental Totem") && (IsBoss (Target) || IsPlayer || EnemyInRange (10) > 5) && Target.IsInCombatRangeAndLoS);
		}

		public bool SearingTotem()
		{
			return CastSelf ("Searing Totem", () => Usable ("Searing Totem") && Target.CombatRange <= 25);
		}

		public bool UnleashElements() {
			return Cast ("Unleash Elements", () => Usable ("Unleash Elements") && Target.IsInLoS && Target.CombatRange <= 40);
		}

		public bool ElementalBlast() {
			return Cast ("Elemental Blast", () => Usable ("Elemental Blast") && Target.IsInLoS && Target.CombatRange <= 40 && (!Me.IsMoving || Me.HasAura("Ancestral Swiftness")));
		}

		public bool LightningBolt() {
			return Cast ("Lightning Bolt", () => Usable ("Lightning Bolt") && Target.IsInLoS && Target.CombatRange <= 30 && (!Me.IsMoving || Me.HasAura("Ancestral Swiftness")));
		}
	}
}

