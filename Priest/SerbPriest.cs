using System.Collections.Generic;
using System.Linq;
using Geometry;
using Newtonsoft.Json;
using ReBot.API;
using System;

namespace ReBot
{
	public abstract class SerbPriest : SerbUtils
	{
		public string IfInterrupt;
		public string Spell = "";

		// Check

		// Get

		public GUID AutoTarget {
			get {
				if (GroupMembers.Count > 0) {
					if (Tank != null)
						return Tank.GUID;
					Player = GroupMembers.Where (u => !u.IsDead).DefaultIfEmpty (null).FirstOrDefault ();
					if (Player != null)
						return Player.GUID;
				}
				Unit = API.CollectUnits (40).Where (u => u.IsEnemy && !u.IsDead && u.IsInLoS && u.IsAttackable && u.InCombat && Range (40, u)).OrderBy (u => u.CombatRange).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null)
					return Unit.GUID;
				return Me.GUID;
			}
		}

		public void SetTarget ()
		{
			if (Tank != null) {
				Me.SetTarget (Tank);
			}
			if (Target == null && HealTarget != null) {
				Me.SetTarget (HealTarget);
			}
		}


		public List<PlayerObject> GroupMembers {
			get {
				if (InPG) {
					var pgGroup = new List<PlayerObject> ();
					var t = API.Units.Where (p => p != null && !p.IsDead && p.IsValid).ToList ();
					if (t.Any ()) {
						foreach (var unit in t) {
							if (PgUnits.Contains (unit.Name)) {
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

		public PlayerObject HealTarget {
			get {
				return GroupMembers.Where (u => !u.IsDead && u.HealthFraction <= 0.9 && u.IsInLoS && Range (40, u)).OrderBy (u => u.HealthFraction).DefaultIfEmpty (null).FirstOrDefault ();
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

		// Combo

		public bool Interrupt ()
		{
			var targets = Adds;
			targets.Add (Target);

			if (Usable ("Silence")) {
				if (InArena || InBg) {
					Unit = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsCastingAndInterruptible () && u.IsInLoS && Range (30, u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Silence (Unit))
						return true;
				} else {
					Unit = targets.Where (u => u.IsCastingAndInterruptible () && u.IsInLoS && Range (30, u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Silence (Unit))
						return true;
				}
			}
			if (Usable ("Psychic Horror") && Orb >= 1) {
				if (InArena || InBg) {
					Unit = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsCasting && u.IsInLoS && Range (30, u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && PsychicHorror (Unit))
						return true;
				} else {
					Unit = targets.Where (u => u.IsCasting && !IsBoss (u) && (IsElite (u) || IsPlayer (u)) && u.IsInLoS && Range (30, u) && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && PsychicHorror (Unit))
						return true;
				}
			}
			return false;
		}

		public bool ShadowHeal ()
		{
			if (Health (Me) < 0.4) {
				if (Healthstone ())
					return true;
			}
			if (Health (Me) < 0.5 && (IsElite () || IsPlayer ())) {
				if (Shadowfiend ())
					return true;
			}
			if (Health (Me) <= 0.8 && !Me.HasAura ("Power Word: Shield") && !Me.HasAura ("Weakened Soul")) {
				if (PowerWordShield (Me))
					return true;
			}
			if ((Health (Me) <= 0.6 || (Me.HasAura ("Power Word: Shield") && Health (Me) < 0.8)) && !Me.IsMoving) {
				if (DesperatePrayer ())
					return true;
				if (FlashHeal (Me))
					return true;
			}
			return false;
		}

		// Spell

		public bool VampiricTouch (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Vampiric Touch") && Range (40, u) && !Me.IsMoving && C ("Vampiric Touch", u);
		}

		public bool VoidEntropy (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Void Entropy") && Orb >= 3 && Range (40, u) && !Me.IsMoving && C ("Void Entropy", u);
		}

		public bool DevouringPlague (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Devouring Plague") && Orb >= 3 && Range (40, u) && C ("Devouring Plague", u);
		}

		public bool SearingInsanity (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Searing Insanity") && Range (40, u) && !Me.IsMoving && C ("Searing Insanity", u);
		}

		public bool Insanity (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Insanity") && Range (40, u) && !Me.IsMoving && C ("Insanity", u);
		}

		public bool Silence (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Silence") && u.IsInLoS && Range (30, u) && C ("Silence", u);
		}

		public bool PsychicHorror (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Psychic Horror") && Orb >= 1 && !IsBoss (u) && Range (30, u) && C ("Psychic Horror", u);
		}

		public bool Dispersion ()
		{
			return Usable ("Dispersion") && C ("Dispersion");
		}

		public bool DesperatePrayer ()
		{
			return Usable ("Desperate Prayer") && C ("Desperate Prayer");
		}

		public bool PowerInfusion ()
		{
			return Usable ("Power Infusion") && Danger () && C ("Power Infusion");
			// GCD = 0
		}

		public bool PowerWordFortitude (UnitObject u = null)
		{
			u = u ?? Me;
			return Usable ("Power Word: Fortitude") && u.AuraTimeRemaining ("Power Word: Fortitude") < 300 && Range (40, u) && C ("Power Word: Fortitude", u);
		}

		public bool Mindbender (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mindbender") && Range (40, u) && C ("Mindbender", u);
		}

		public bool Shadowfiend (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shadowfiend") && Range (40, u) && Danger (u) && C ("Shadowfiend", u);
		}

		public bool ShadowWordPain (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shadow Word: Pain") && Range (40, u) && C ("Shadow Word: Pain", u);
		}

		public bool Penance (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Penance") && Range (40, u) && (HasGlyph (119866) || !Me.IsMoving) && C ("Penance", u);
		}


		// Utils

		public bool CaseInterrupt (UnitObject u = null)
		{
			u = u ?? Target;
			// interrupt_if=cooldown.mind_blast.remains<=0.1
			if (IfInterrupt == "ChainM") {
				if (Cooldown ("Mind Blast") == 0.1) {
					IfInterrupt = "";
					API.ExecuteMacro ("/stopcasting");
					return true;
				}
				return false;
			}
			// interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (IfInterrupt == "ChainMS") {
				if (Cooldown ("Mind Blast") == 0 || Cooldown ("Shadow Word: Death") == 0) {
					IfInterrupt = "";
					API.ExecuteMacro ("/stopcasting");
					return true;
				}
				return false;
			}
			// interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1|shadow_orb=5)
			if (IfInterrupt == "ChainMSO") {
				if (Cooldown ("Mind Blast") == 0 || Cooldown ("Shadow Word: Death") == 0 || Orb == 5) {
					IfInterrupt = "";
					API.ExecuteMacro ("/stopcasting");
					return true;
				}
				return false;
			}
			// interrupt_if=(cooldown.mind_blast.remains<=0.1|(cooldown.shadow_word_death.remains<=0.1&target.health.pct<20))
			if (IfInterrupt == "ChainMSH") {
				if (Cooldown ("Mind Blast") == 0 || (Cooldown ("Shadow Word: Death") == 0 && Health (u) < 0.2)) {
					IfInterrupt = "";
					API.ExecuteMacro ("/stopcasting");
					return true;
				}
				return false;
			}
			return false;
		}

		public bool PowerWordSolace (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Power Word: Solace") && (Range (30, u) || (HasGlyph (119853) && Range (40, u))) && C ("Power Word: Solace");
		}

		public bool HolyFire (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Holy Fire") && (Range (30, u) || (HasGlyph (119853) && Range (40, u))) && C ("Holy Fire");
		}

		public bool Smite (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Smite") && (Range (30, u) || (HasGlyph (119853) && Range (40, u))) && !Me.IsMoving && C ("Smite");
		}

		public bool PowerWordShield (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Power Word: Shield") && !u.HasAura ("Power Word: Shield") && !u.HasAura ("Weakened Soul") && Range (40, u) && C ("Power Word: Shield");
		}

		public bool FlashHeal (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Flash Heal") && Range (40, u) && (Me.HasAura ("Surge of Light") || !Me.IsMoving) && C ("Flash Heal");
		}

		public bool Heal (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Heal") && Range (40, u) && C ("Heal");
		}

		public bool PrayerofMending (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Prayer of Mending") && !u.HasAura ("Prayer of Mending") && Range (40, u) && !Me.IsMoving && C ("Prayer of Mending");
		}

		public bool ClarityofWill (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Clarity of Will") && Range (40, u) && !Me.IsMoving && C ("Clarity of Will", u);
		}

		public bool Levitate (UnitObject u = null)
		{
			u = u ?? Me;
			return Usable ("Mindbender") && !HasAura ("Levitate") && Range (40, u) && C ("Levitate", u);
		}

		public bool Archangel ()
		{
			return Usable ("Archangel") && C ("Archangel");
		}

		public bool SetShieldAll ()
		{
			if (InArena) {
				Unit = GroupMembers.Where (u => !u.IsDead && Range (40, u) && !u.HasAura ("Power Word: Shield")).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && PowerWordShield (Unit))
					return true;
			}
			if (Health (Me) < 0.99) {
				if (PowerWordShield (Me))
					return true;
			}
			return false;
		}

		public bool Shadowform ()
		{
			return Usable ("Shadowform") && !Me.HasAura ("Shadowform") && C ("Shadowform");
		}

		public bool ShadowWordDeath (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shadow Word: Death") && Range (40, u) && Health (u) <= 0.2 && C ("Shadow Word: Death", u);
		}

		public bool MindBlast (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mind Blast") && Range (40, u) && C ("Mind Blast", u);
		}

		public bool MindSear (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mind Sear") && Range (40, u) && C ("Mind Sear", u);
		}

		public bool MindFlay (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mind Flay") && Range (40, u) && !Me.IsMoving && C ("Mind Flay", u);
		}

		public bool MindSpike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mind Spike") && Range (40, u) && !Me.IsMoving && C ("Mind Spike", u);
		}

		public bool Halo (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Halo") && u.IsInLoS && Range (30, u) && C ("Halo", u);
		}

		public bool Cascade (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Cascade") && u.IsInLoS && Range (40, u) && C ("Cascade", u);
		}

		public bool DivineStar (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Divine Star") && u.IsInLoS && Range (30, u) && C ("Divine Star", u);
		}

		public bool DispelAll ()
		{
			var AllForDispel = GroupMembers.Where (u => u.IsInLoS && Range (30, u) && u.Auras.Any (a => a.IsDebuff && "Magic,Disease".Contains (a.DebuffType)));
			Unit = AllForDispel.DefaultIfEmpty (null).FirstOrDefault ();
			if (Unit != null && AllForDispel.ToList ().Count > 3) {
				if (MassDispel (Unit))
					return true;
			}
			Unit = GroupMembers.Where (u => u.IsInLoS && Range (30, u) && u.Auras.Any (a => a.IsDebuff && "Magic,Disease".Contains (a.DebuffType))).DefaultIfEmpty (null).FirstOrDefault (); 
			if (Unit != null && Purify (Unit))
				return true;
			return false;
		}

		public bool MassDispel (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Mass Dispel", u.Position, () => Usable ("Mass Dispel") && u.IsInLoS && Range (30, u));
		}

		public bool Purify (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Purify", u.Position, () => Usable ("Purify") && u.IsInLoS && Range (30, u));
		}

		public bool SavingGrace (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Saving Grace") && u.IsInLoS && Range (40, u) && C ("Saving Grace", u);
		}

		public bool PainSuppression (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Pain Suppression") && u.IsInLoS && Range (40, u) && C ("Pain Suppression", u);
		}

		public UnitObject CascadeTarget {
			get {
				List<PlayerObject> CascadeCounts;
				if (GroupMembers.Count < 6) {
					CascadeCounts = GroupMembers.Where (u => !u.IsDead && Range (40, u) && Health (u) <= 0.85).ToList ();
					if (CascadeCounts.Count () >= 2)
						return CascadeCounts.FirstOrDefault ();
				}
				if (GroupMembers.Count > 5) {
					CascadeCounts = GroupMembers.Where (u => !u.IsDead && Range (40, u) && Health (u) <= 0.8).ToList ();
					if (CascadeCounts.Count () >= 5)
						return CascadeCounts.FirstOrDefault ();
				}
				return null;
			}
		}

		public UnitObject HaloTarget {
			get {
				List<PlayerObject> HaloCounts;
				if (GroupMembers.Count < 6) {
					HaloCounts = GroupMembers.Where (u => !u.IsDead && Range (30, u) && Health (u) <= 0.85).ToList ();
					if (HaloCounts.Count () >= 2)
						return HaloCounts.FirstOrDefault ();
				}
				if (GroupMembers.Count > 5) {
					HaloCounts = GroupMembers.Where (u => !u.IsDead && Range (30, u) && Health (u) <= 0.8).ToList ();
					if (HaloCounts.Count () >= 5)
						return HaloCounts.FirstOrDefault ();
				}
				return null;
			}
		}

		public bool CastSpell (string s)
		{
			if (s == "Shadow Word: Death") {
				if (ShadowWordDeath ())
					return true;
			}
			if (s == "Mind Blast") {
				if (MindBlast ())
					return true;
			}
			return false;
		}

	}
}
	