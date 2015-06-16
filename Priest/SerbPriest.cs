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
		[JsonProperty ("Mindbender Mana %")]
		public double MindbenderMana = 0.60;
		[JsonProperty ("Keep Power Word: Shield on tank")]
		public bool PWSTank = true;

		public string IfInterrupt;
		public string Spell = "";

		public double PainSuppressionHealth = 0.45;
		double PWSHealthDungeon = 0.95;
		double PWSHealthRaid = 0.85;
		double ClarityofWillHealth = 0.40;
		double CascadeHealthDungeon = 0.85;
		double CascadeHealthRaid = 0.80;
		int CascadePlayersDungeon = 2;
		int CascadePlayersRaid = 5;
		double HaloHealthDungeon = 0.85;
		double HaloHealthRaid = 0.80;
		int HaloPlayersDungeon = 2;
		int HaloPlayersRaid = 5;

		// Check

		public bool UsePowerInfusion {
			get {
				return MyGroupAndMe.Where (u => Health (u) <= 0.65).ToList ().Count >= AOECount;
			}
		}

		// Get

		//		public GUID AutoTarget {
		//			get {
		//				if (GroupMembers.Count > 0) {
		//					if (Tank != null)
		//						return Tank.GUID;
		//					Player = GroupMembers.Where (u => !u.IsDead).DefaultIfEmpty (null).FirstOrDefault ();
		//					if (Player != null)
		//						return Player.GUID;
		//				}
		//				Unit = API.CollectUnits (40).Where (u => u.IsEnemy && !u.IsDead && u.IsInLoS && u.IsAttackable && u.InCombat && Range (40, u)).OrderBy (u => u.CombatRange).DefaultIfEmpty (null).FirstOrDefault ();
		//				if (Unit != null)
		//					return Unit.GUID;
		//				return Me.GUID;
		//			}
		//		}

		public int ShadowApparitions {
			get {
				int CountOfShadowApparitions = API.Units.Where (u => (u.EntryID == 46954 || u.EntryID == 46954)).ToList ().Count;
				// int CountOfShadowApparitions = API.Units.Where(u => u.EntryID == 46954 && u.CreatedByMe == true).ToList().Count;
				return CountOfShadowApparitions;
			}
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
				Unit = MyGroup.Where (u => !u.IsDead && Range (40, u) && !u.HasAura ("Power Word: Shield")).DefaultIfEmpty (null).FirstOrDefault ();
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

		public bool DispelAll ()
		{
			if (DispelTarget != null && DispelMagic (DispelTarget))
				return true;
			if (PurifyTarget != null && Purify (PurifyTarget))
				return true;
			return false;
		}





		public UnitObject CascadeHealthTarget {
			get {
				var CascadeHealth = CascadeHealthDungeon;
				var CascadePlayer = CascadePlayersDungeon;
				if (GroupMemberCount > 5) {
					CascadeHealth = CascadeHealthRaid;
					CascadePlayer = CascadePlayersRaid;
				}
				if (LowestPlayerCount (CascadeHealth) > CascadePlayer)
					return LowestPlayer;
				return null;
			}
		}

		public UnitObject HaloHealthTarget {
			get {
				var HaloHealth = HaloHealthDungeon;
				var HaloPlayer = HaloPlayersDungeon;
				if (GroupMemberCount > 5) {
					HaloHealth = HaloHealthRaid;
					HaloPlayer = HaloPlayersRaid;
				}
				if (LowestPlayerCount (HaloHealth, 30) > HaloPlayer)
					return LowestPlayer;
				return null;
			}
		}

		public UnitObject PurifyTarget {
			get {
				return MyGroupAndMe.Where (u => Range (30, u) && u.Auras.Any (a => a.IsDebuff && "Magic,Disease".Contains (a.DebuffType))).DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		public UnitObject DispelTarget {
			get {
				return MyGroupAndMe.Where (u => Range (30, u) && u.Auras.Any (a => a.IsDebuff && "Magic".Contains (a.DebuffType))).DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		public UnitObject SWPTarget (double r = 0)
		{
			return API.Units.Where (u => u != null && !u.IsDead && u.IsAttackable && (u.InCombat && u.IsTargetingMeOrPets) && !Me.IsNotInFront (u) && Range (30, u) && ((r == 0 && !u.HasAura ("Shadow Word: Pain", true)) || (r > 0 && u.HasAura ("Shadow Word: Pain", true) && u.AuraTimeRemaining ("Shadow Word: Pain", true) <= r))).DefaultIfEmpty (null).FirstOrDefault ();
		}

		public UnitObject PWSTarget {
			get {
				var PWSHealth = PWSHealthDungeon;
				if (GroupMemberCount > 5)
					PWSHealth = PWSHealthRaid;
				return MyGroupAndMe.Where (p => !p.HasAura ("Power Word: Shield") && !p.HasAura ("Weakened Soul") && (Health (p) <= PWSHealth || (IsTank (p) && PWSTank))).DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		public UnitObject MindbenderTarget {
			get {
				return API.Units.Where (u => !u.IsDead && u.IsAttackable && u.InCombat && u.IsEnemy && Range (40, u)).OrderByDescending (p => Health (p)).DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		public UnitObject ClarityofWillTarget {
			get {
				return MyGroupAndMe.Where (u => Health (u) <= ClarityofWillHealth).DefaultIfEmpty (null).FirstOrDefault ();
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


		// Spells

		public bool Purify (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Purify") && Range (30, u) && C ("Purify", u);
		}

		public bool DispelMagic (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Dispel Magic") && Range (30, u) && C ("Dispel Magic", u);
		}

		public bool MassDispel (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mass Dispel") && Range (30, u) && COT ("Mass Dispel", u);
		}

		public bool SavingGrace (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Saving Grace") && Range (40, u) && C ("Saving Grace", u);
		}

		public bool PainSuppression (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Pain Suppression") && Range (40, u) && C ("Pain Suppression", u);
		}

		public bool DivineStar (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Divine Star") && Range (30, u) && C ("Divine Star", u);
		}

		public bool PowerInfusion ()
		{
			return Usable ("Power Infusion") && C ("Power Infusion");
			// GCD = 0
		}

		public bool Penance (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Penance") && Range (40, u) && (HasGlyph (119866) || !Me.IsMoving) && C ("Penance", u);
		}

		public bool Smite (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Smite") && (Range (30, u) || (HasGlyph (119853) && Range (40, u))) && !Me.IsMoving && C ("Smite");
		}


		// Def
		//
		// Pain Suppression

		// Burst
		//
		// Power Infusion

	}
}
	