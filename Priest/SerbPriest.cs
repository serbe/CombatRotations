using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot.Priest
{
	public abstract class SerbPriest : CombatRotation
	{
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public UnitObject CycleTarget;


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

		public virtual List<PlayerObject> GroupMembers {
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

		// Spell

		public virtual bool BloodFury ()
		{
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && Target.IsInCombatRangeAndLoS && (Target.IsElite () || Target.IsPlayer));
		}

		public virtual bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Target.IsInCombatRangeAndLoS && (Target.IsElite () || Target.IsPlayer));
		}

		public virtual bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Target.IsInCombatRangeAndLoS && (Target.IsElite () || Target.IsPlayer));
		}

		public virtual bool PowerInfusion ()
		{
			return CastSelf ("Power Infusion", () => Usable ("Power Infusion") && Target.IsInCombatRangeAndLoS && (Target.IsElite () || Target.IsPlayer));
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
			return Cast ("Clarity of Will", u, () => Usable ("Clarity of Will") && u.IsInLoS && u.CombatRange <= 40 && !Me.IsMoving);
		}

		public bool Levitate (UnitObject u = null)
		{
			u = u ?? Me;
			return Cast ("Levitate", u, () => Usable ("Mindbender") && !HasAura ("Levitate") && u.IsInLoS && u.CombatRange <= 40);
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
	}
}

