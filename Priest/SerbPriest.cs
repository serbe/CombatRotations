﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;
using Geometry;

namespace ReBot.Priest
{
	public abstract class SerbPriest : CombatRotation
	{
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public UnitObject CycleTarget;
		public IEnumerable<UnitObject> MaxCycle;
		public string Interrupt;

		public bool InRaid {
			get {
				return API.MapInfo.Type == MapType.Raid;
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

		public bool InInstance {
			get {
				return API.MapInfo.Type == MapType.Instance;
			}
		}

		public void AutoTarget ()
		{
			CycleTarget = API.CollectUnits (40).Where (u => u.IsEnemy && !u.IsDead && u.IsInLoS && u.IsAttackable).OrderByDescending (u => u.CombatRange).DefaultIfEmpty (null).FirstOrDefault ();
			if (CycleTarget != null)
				Me.SetTarget (CycleTarget);
		}

		public void SetTarget ()
		{
			if (Tank != null) {
				if (Me.Focus == null)
					Me.SetFocus (Tank);
				Me.SetTarget (Tank);
			}
			if (Target == null && HealTarget != null) {
				Me.SetTarget (HealTarget);
			}
		}

		public bool Usable (string s)
		{ 
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public double TimeToDie (UnitObject u = null)
		{
			u = u ?? Target;
			if (u != null)
				return u.Health / Ttd;
			return 0;
		}

		public bool IsBoss (UnitObject u = null)
		{
			u = u ?? Target;
			return(u.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || u.Level >= Me.Level + BossLevelIncrease;
		}

		public int Orb {
			get {
				return Me.GetPower (WoWPowerType.PriestShadowOrbs);
			}
		}

		public double Health (UnitObject u = null)
		{
			u = u ?? Me;
			return u.HealthFraction;
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

		public List<PlayerObject> GroupMembers {
			get {
				return Group.GetGroupMemberObjects ();
			}
		}

		public PlayerObject Tank {
			get {
				return GroupMembers.Where (x => x.IsTank && x.IsInCombatRangeAndLoS && !x.IsDead).OrderByDescending (x => x.HealthFraction).DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		public IOrderedEnumerable<PlayerObject> HealGroups {
			get {
				return GroupMembers.Where (x => !x.IsDead && x.HealthFraction <= 0.9 && x.IsInCombatRangeAndLoS).OrderByDescending (x => x.HealthFraction);
			}
		}

		public UnitObject HealTarget {
			get {
				return HealGroups.DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		public int ShadowApparitions {
			get {
				int CountOfShadowApparitions = API.Units.Where (u => (u.EntryID == 46954 || u.EntryID == 46954)).ToList ().Count;
				// int CountOfShadowApparitions = API.Units.Where(u => u.EntryID == 46954 && u.CreatedByMe == true).ToList().Count;
				return CountOfShadowApparitions;
			}
		}

		public UnitObject BestTarget (int SpellRange, int AoeRange, int MinCount)
		{
			var targets = Adds;
			targets.Add (Target);

			var bestTarget = targets.Where (u => u.IsInLoS && u.CombatRange <= SpellRange).OrderByDescending (u => targets.Count (o => Vector3.Distance (u.Position, o.Position) <= AoeRange)).DefaultIfEmpty (null).FirstOrDefault ();
			if (bestTarget != null) {
				if (targets.Where (u => Vector3.Distance (u.Position, bestTarget.Position) <= AoeRange).ToList ().Count >= MinCount)
					return bestTarget;
			}
			return null;
		}
			
		// Spell

		public bool BloodFury ()
		{
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && Target.IsInCombatRangeAndLoS && (Target.IsElite () || Target.IsPlayer));
		}

		public bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Target.IsInCombatRangeAndLoS && (Target.IsElite () || Target.IsPlayer));
		}

		public bool ArcaneTorrent (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && u.IsInCombatRangeAndLoS && (u.IsElite () || u.IsPlayer));
		}

		public bool PowerInfusion (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Power Infusion", () => Usable ("Power Infusion") && u.IsInCombatRangeAndLoS && (u.IsElite () || u.IsPlayer));
		}

		public bool PowerWordFortitude (UnitObject u = null)
		{
			u = u ?? Me;
			return Cast ("Power Word: Fortitude", u, () => Usable ("Power Word: Fortitude") && u.AuraTimeRemaining ("Power Word: Fortitude") < 300 && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool DraenicIntellect ()
		{
			return API.HasItem (109218) && API.ItemCooldown (109218) <= 0 && API.UseItem (109218);
		}

		public bool Mindbender (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Mindbender", u, () => Usable ("Mindbender") && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool Shadowfiend (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shadowfiend", u, () => Usable ("Shadowfiend") && u.IsInLoS && u.CombatRange <= 40 && (u.IsPlayer || u.IsElite ()));
		}

		public bool ShadowWordPain (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shadow Word: Pain", u, () => Usable ("Shadow Word: Pain") && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool Penance (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Penance", u, () => Usable ("Penance") && u.IsInLoS && u.CombatRange <= 40 && (HasGlyph (119866) || !Me.IsMoving));
		}

		public bool PowerWordSolace (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Power Word: Solace", u, () => Usable ("Power Word: Solace") && u.IsInLoS && (u.CombatRange <= 30 || (HasGlyph (119853) && u.CombatRange <= 40)));
		}

		public bool HolyFire (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Holy Fire", u, () => Usable ("Holy Fire") && u.IsInLoS && (u.CombatRange <= 30 || (HasGlyph (119853) && u.CombatRange <= 40)));
		}

		public bool Smite (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Smite", u, () => Usable ("Smite") && u.IsInLoS && (u.CombatRange <= 30 || (HasGlyph (119853) && u.CombatRange <= 40)) && !Me.IsMoving);
		}

		public bool PowerWordShield (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Power Word: Shield", u, () => Usable ("Power Word: Shield") && !u.HasAura ("Power Word: Shield") && !u.HasAura ("Weakened Soul") && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool FlashHeal (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Flash Heal", u, () => Usable ("Flash Heal") && u.IsInLoS && u.CombatRange <= 40 && (Me.HasAura ("Surge of Light") || !Me.IsMoving));
		}

		public bool Heal (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Heal", u, () => Usable ("Heal") && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool PrayerofMending (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Prayer of Mending", u, () => Usable ("Prayer of Mending") && u.IsInLoS && u.CombatRange <= 40 && !Me.IsMoving);
		}

		public bool ClarityofWill (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Clarity of Will", () => Usable ("Clarity of Will") && u.IsInLoS && u.CombatRange <= 40 && !Me.IsMoving, u);
		}

		public bool Levitate (UnitObject u = null)
		{
			u = u ?? Me;
			return Cast ("Levitate", () => Usable ("Mindbender") && !HasAura ("Levitate") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool Archangel ()
		{
			return CastSelf ("Archangel", () => Usable ("Archangel"));
		}

		public bool SetShieldAll ()
		{
			CycleTarget = GroupMembers.Where (m => !m.IsDead && m.IsInCombatRangeAndLoS && !m.HasAura ("Power Word: Shield")).DefaultIfEmpty (null).FirstOrDefault ();
			if (CycleTarget != null) {
				if (PowerWordShield (CycleTarget))
					return true;
			}
			return false;
		}

		public bool Shadowform ()
		{
			return CastSelf ("Shadowform", () => Usable ("Shadowform") && !Me.HasAura ("Shadowform"));
		}

		public bool ShadowWordDeath (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shadow Word: Death", () => Usable ("Shadow Word: Death") && u.IsInLoS && u.CombatRange <= 40 && Health (u) <= 0.2, u);
		}

		public bool MindBlast (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Mind Blast", () => Usable ("Mind Blast") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool DevouringPlague (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Devouring Plague", () => Usable ("Devouring Plague") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool ShadowWordPain (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shadow Word: Pain", () => Usable ("Shadow Word: Pain") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool MindSear (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Mind Sear", () => Usable ("Mind Sear") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool MindFlay (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Mind Flay", () => Usable ("Mind Flay") && u.IsInLoS && u.CombatRange <= 40, u);
		}

	}
}
	