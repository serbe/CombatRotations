using System.Collections.Generic;
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
		public PlayerObject PartyTarget;
		public IEnumerable<UnitObject> MaxCycle;
		public string Interrupt;

		public bool InGroup {
			get {
				return Group.GetGroupMemberObjects ().Count > 0;
			}
		}

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

		public bool InPG {
			get {
				return API.MapInfo.Name.Contains ("Proving Grounds");
			}
		}

		public GUID AutoTarget {
			get {
				if (GroupMembers.Count > 0) {
					if (Tank != null)
						return Tank.GUID;
					PartyTarget = GroupMembers.Where (u => !u.IsDead).DefaultIfEmpty (null).FirstOrDefault ();
					if (PartyTarget != null)
						return PartyTarget.GUID;
				}
				CycleTarget = API.CollectUnits (40).Where (u => u.IsEnemy && !u.IsDead && u.IsInLoS && u.IsAttackable && u.InCombat && Range (u) <= 40).OrderBy (u => u.CombatRange).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null)
					return CycleTarget.GUID;

				return Me.GUID;
			}
		}

		public HashSet<string> pgUnits = new HashSet<string> () {
			"Oto the Protector",
			"Sooli the Survivalist",
			"Kavan the Arcanist",
			"Ki the Assassin"
		};

		public void SetTarget ()
		{
			if (Tank != null) {
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

		public double Range (UnitObject u = null)
		{
			u = u ?? Target;
			return u.CombatRange;
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
				if (InPG) {
					var pgGroup = new List<PlayerObject> ();
					var t = API.Units.Where (p => p != null && !p.IsDead && p.IsValid).ToList ();
					if (t.Any ()) {
						foreach (var unit in t) {
							if (pgUnits.Contains (unit.Name)) {
								pgGroup.Add ((PlayerObject)unit);
							}
						}
					}
					pgGroup.Add (Me);
					return pgGroup;
				} else {
					var allGroup = Group.GetGroupMemberObjects ();
					allGroup.Add (Me);
					return allGroup;
				}
			}
		}

		public PlayerObject Tank {
			get {
				if (InPG) {
					return (PlayerObject)API.Units.Where (p => p.Name == "Oto the Protector").DefaultIfEmpty (null).FirstOrDefault ();
				}
				return GroupMembers.Where (x => x.IsTank && x.IsInLoS && Range (x) <= 40 && !x.IsDead).OrderBy (x => x.HealthFraction).DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		public PlayerObject HealTarget {
			get {
				return GroupMembers.Where (x => !x.IsDead && x.HealthFraction <= 0.9 && x.IsInLoS && Range (x) <= 40).OrderBy (x => x.HealthFraction).DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		public int ShadowApparitions {
			get {
				int CountOfShadowApparitions = API.Units.Where (u => (u.EntryID == 46954 || u.EntryID == 46954)).ToList ().Count;
				// int CountOfShadowApparitions = API.Units.Where(u => u.EntryID == 46954 && u.CreatedByMe == true).ToList().Count;
				return CountOfShadowApparitions;
			}
		}

		public UnitObject BestTarget (int spellRange, int aoeRange, int minCount)
		{
			var targets = Adds;
			targets.Add (Target);

			var bestTarget = targets.Where (u => u.IsInLoS && u.CombatRange <= spellRange).OrderByDescending (u => targets.Count (o => Vector3.Distance (u.Position, o.Position) <= aoeRange)).DefaultIfEmpty (null).FirstOrDefault ();
			if (bestTarget != null) {
				if (targets.Where (u => Vector3.Distance (u.Position, bestTarget.Position) <= aoeRange).ToList ().Count >= minCount)
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
			return Cast ("Prayer of Mending", u, () => Usable ("Prayer of Mending") && !u.HasAura ("Prayer of Mending") && u.IsInLoS && u.CombatRange <= 40 && !Me.IsMoving);
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
			if (InArena) {
				CycleTarget = GroupMembers.Where (m => !m.IsDead && m.IsInCombatRangeAndLoS && !m.HasAura ("Power Word: Shield")).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (PowerWordShield (CycleTarget))
						return true;
				}
			}
			if (Health (Me) < 0.99) {
				if (PowerWordShield (Me))
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

		public bool MindSpike (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Mind Spike", () => Usable ("Mind Spike") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool Halo (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Halo", () => Usable ("Halo") && u.IsInLoS && u.CombatRange <= 30, u);
		}

		public bool Cascade (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Cascade", () => Usable ("Cascade") && u.IsInLoS && Range (u) <= 40, u);
		}

		public bool DivineStar (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Divine Star", () => Usable ("Divine Star") && u.IsInLoS && u.CombatRange <= 30, u);
		}

		public bool VampiricTouch (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Vampiric Touch", () => Usable ("Vampiric Touch") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool DispelAll ()
		{
			var AllForDispel = GroupMembers.Where (u => u.IsInLoS && Range (u) <= 30 && u.Auras.Any (a => a.IsDebuff && "Magic,Disease".Contains (a.DebuffType)));
			CycleTarget = AllForDispel.DefaultIfEmpty (null).FirstOrDefault ();
			if (CycleTarget != null && AllForDispel.ToList ().Count > 3) {
				if (MassDispel (CycleTarget))
					return true;
			}
			CycleTarget = GroupMembers.Where (u => u.IsInLoS && Range (u) <= 30 && u.Auras.Any (a => a.IsDebuff && "Magic,Disease".Contains (a.DebuffType))).DefaultIfEmpty (null).FirstOrDefault (); 
			if (CycleTarget != null) {
				if (Purify (CycleTarget))
					return true;
			}


			return false;
		}

		public bool MassDispel (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Mass Dispel", u.Position, () => Usable ("Mass Dispel") && u.IsInLoS && Range (u) <= 30);
		}

		public bool Purify (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Purify", u.Position, () => Usable ("Purify") && u.IsInLoS && Range (u) <= 30);
		}

		public bool SavingGrace (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Saving Grace", () => Usable ("Saving Grace") && u.IsInLoS && Range (u) <= 40, u);
		}

		public bool PainSuppression (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Pain Suppression", () => Usable ("Pain Suppression") && u.IsInLoS && Range (u) <= 40, u);
		}

		public bool Healthstone ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (5512) && API.ItemCooldown (5512) == 0)
				return API.UseItem (5512);
			return false;
		}

		public UnitObject CascadeTarget {
			get {
				var CascadeCounts = new List<PlayerObject> ();
				if (GroupMembers.Count < 6) {
					CascadeCounts = GroupMembers.Where (p => !p.IsDead && p.IsInLoS && Range (p) <= 40 && Health (p) <= 0.85).ToList ();
					if (CascadeCounts.Count () >= 2)
						return CascadeCounts.FirstOrDefault ();
				}
				if (GroupMembers.Count > 5) {
					CascadeCounts = GroupMembers.Where (p => !p.IsDead && p.IsInLoS && Range (p) <= 40 && Health (p) <= 0.8).ToList ();
					if (CascadeCounts.Count () >= 5)
						return CascadeCounts.FirstOrDefault ();
				}
				return null;
			}
		}

		public UnitObject HaloTarget {
			get {
				var HaloCounts = new List<PlayerObject> ();
				if (GroupMembers.Count < 6) {
					HaloCounts = GroupMembers.Where (p => !p.IsDead && p.IsInLoS && Range (p) <= 30 && Health (p) <= 0.85).ToList ();
					if (HaloCounts.Count () >= 2)
						return HaloCounts.FirstOrDefault ();
				}
				if (GroupMembers.Count > 5) {
					HaloCounts = GroupMembers.Where (p => !p.IsDead && p.IsInLoS && Range (p) <= 30 && Health (p) <= 0.8).ToList ();
					if (HaloCounts.Count () >= 5)
						return HaloCounts.FirstOrDefault ();
				}
				return null;
			}
		}
	}
}
	