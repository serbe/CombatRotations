using Newtonsoft.Json;
using ReBot.API;
using System.Linq;

namespace ReBot
{
	[Rotation ("Serb Priest Discipline", "Serb", WoWClass.Priest, Specialization.PriestDiscipline, 40, 25)]

	public class SerbPriestDisciplineSc : SerbPriest
	{
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("Auto target enemy or party membet")]
		public bool UseAutoTarget = true;
		[JsonProperty ("Fight in instance")]
		public bool FightInInstance = true;
		[JsonProperty ("Heal %/100")]
		public double HealPr = 0.8;
		[JsonProperty ("Heal tank %/100")]
		public double TankPr = 0.9;

		public SerbPriestDisciplineSc ()
		{
			GroupBuffs = new [] {
				"Power Word: Fortitude"
			};
			PullSpells = new [] {
				"Shadow Word: Pain",
				"Smite"
			};
		}

		//		public UnitObject DamageTarget {
		//			get {
		//				if (!Target.IsEnemy) {
		//					Unit = API.CollectUnits (40).Where (u => u.IsEnemy && !u.IsDead && u.IsInLoS && u.IsAttackable && u.InCombat && Range (40, u)).OrderBy (u => u.CombatRange).DefaultIfEmpty (null).FirstOrDefault ();
		//					if (Unit != null)
		//						return Unit;
		//				}
		//				return null;
		//			}
		//		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_intellect_flask
			//	actions.precombat+=/food,type=salty_squid_roll
			//	actions.precombat+=/power_word_fortitude,if=!aura.stamina.up
			if (PowerWordFortitude ())
				return true;
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_intellect
			//	actions.precombat+=/smite

//			if (HealTarget != null) {
//				if (HealTarget.HealthFraction < HealPr) {
//					if (Healing (Player))
//						return true;
//				}
//			}

			if (Health (Me) < 0.9 && !Me.IsMoving && !Me.IsMounted) {
				if (Heal (Me))
					return true;
			}

			if (DispelAll ())
				return true;

			if (Me.FallingTime > 2) {
				if (Levitate ())
					return true;
			}

			return false;
		}

		public override void Combat ()
		{
			if (MeIsBusy)
				return;

			if (SetShieldAll ())
				return;

			if (Group.GetNumGroupMembers () == 0)
				Solo ();
			if (Group.GetNumGroupMembers () < 6)
				Dungeon ();
			if (Group.GetNumGroupMembers () > 2)
				Raid ();


//
//			if (InGroup) {
//				if (LowestPlayer != null && Tank != null && Health (LowestPlayer) <= TankPr && (Health (Tank) < 0.4 || LowestPlayer == Tank)) {
//					if (HealTank (Tank))
//						return;
//				}
//
//				if (DispelAll ())
//					return;
//
//				if (LowestPlayer != null && Health (LowestPlayer) <= HealPr) {
//					if (Healing (LowestPlayer))
//						return;
//				}
//
//				if (FightInInstance) {
//					if (Damage ())
//						return;
//				}
//			} else {
//				if (Health (Me) < 0.5) {
//					if (Heal (Me))
//						return;
//				}
//
//				if (DispelAll ())
//					return;
//
//				if (Damage ())
//					return;
//			}
//
//			if (CurrentBotName == "Quest") {
//				if (Damage ())
//					return;
//			}
		}

		public void Solo ()
		{
			if (useHealthstone ()) {
				Healthstone ();
				return;
			}
			if (useHealTonic ()) {
				HealTonic ();
				return;
			}
			if (useTrinket1) {
				Trinket1 ();
				return;
			}
			if (useTrinket2) {
				Trinket2 ();
				return;
			}

			if (Cast (DISPEL_MAGIC, () => HasSpell (DISPEL_MAGIC)
			    && SpellCooldown (DISPEL_MAGIC) <= 0
			    && Me.Auras.Any (a => a.IsDebuff
			    && "Magic".Contains (a.DebuffType))))
				return;

			if (Cast (PURIFY, () => HasSpell (PURIFY)
			    && SpellCooldown (PURIFY) <= 0
			    && Me.Auras.Any (a => a.IsDebuff
			    && "Magic,Disease".Contains (a.DebuffType))))
				return;

			if (CastSelfPreventDouble (ARCHANGEL, () => HasSpell (ARCHANGEL) && Me.HasAura (EVANGELISM) && AuraStackCount (EVANGELISM) >= 5))
				return;
			if (CastSelfPreventDouble (PAIN_SUPRESSION, () => HasSpell (PAIN_SUPRESSION) && Me.HealthFraction <= 0.45))
				return;
			if (CastSelfPreventDouble (POWER_INFUSION, () => HasSpell (POWER_INFUSION) && Target.MaxHealth > Me.MaxHealth * 2))
				return;



			if (Cast (MANAFIEND, () => HasSpell (MANAFIEND) && Target.MaxHealth > Me.MaxHealth * 2 && SpellCooldown (MANAFIEND) <= 0))
				return;
			if (Cast (MINDBENDER, () => HasSpell (MINDBENDER) && Me.ManaFraction <= MindbenderMana && SpellCooldown (MINDBENDER) <= 0))
				return;
			if (CastSelfPreventDouble (PWS, () => Me.HealthFraction <= 90 && !Me.HasAura (PWS)))
				return;
			if (CastSelfPreventDouble (DESPERATE_PRAYER, () => HasSpell (DESPERATE_PRAYER) && Me.HealthFraction <= desperatePrayerHealth))
				return;
			if (CastPreventDouble (HOLY_FIRE, () => HasSpell (HOLY_FIRE) && Target.Distance <= dpsRange && SpellCooldown (HOLY_FIRE) <= 0))
				return;
			if (CastPreventDouble (PWSOLACE, () => HasSpell (PWSOLACE) && Target.Distance <= dpsRange && SpellCooldown (PWSOLACE) <= 0))
				return;
			if (Cast (SWP, () => HasSpell (SWP) && noSWPTarget != null, noSWPTarget))
				return;
			if (CastPreventDouble (PENANCE, () => HasSpell (PENANCE) && Target.Distance <= dpsRange && SpellCooldown (PENANCE) <= 0))
				return;
			if (Cast (SMITE, () => HasSpell (SMITE) && Target.Distance <= dpsRange))
				return;

		}

		public void DungeonHealing ()
		{
			if (useHealthstone ()) {
				Healthstone ();
				return;
			}
			if (useHealTonic ()) {
				HealTonic ();
				return;
			}
			if (useTrinket1) {
				Trinket1 ();
				return;
			}
			if (useTrinket2) {
				Trinket2 ();
				return;
			}

			if (Cast (DISPEL_MAGIC, () => HasSpell (DISPEL_MAGIC) && dispelTarget != null && SpellCooldown (DISPEL_MAGIC) <= 0, dispelTarget))
				return;
			if (Cast (PURIFY, () => HasSpell (PURIFY) && purifyTarget != null && SpellCooldown (PURIFY) <= 0, purifyTarget))
				return;

			if (CastSelfPreventDouble (ARCHANGEL, () => HasSpell (ARCHANGEL) && Me.HasAura (EVANGELISM) && AuraStackCount (EVANGELISM) >= 5))
				return;

			if (CastSelfPreventDouble (POWER_INFUSION, () => HasSpell (POWER_INFUSION) && SpellCooldown (POWER_INFUSION) <= 0 && needPowerInfusion))
				return;

			if (Cast (MANAFIEND, () => HasSpell (MANAFIEND) && tankTarget != null && SpellCooldown (MANAFIEND) <= 0, tankTarget))
				return;

			if (Cast (MINDBENDER, () => HasSpell (MINDBENDER)
			    && mindbenderTarget != null
			    && Me.ManaFraction <= MindbenderMana
			    && SpellCooldown (MINDBENDER) <= 0, mindbenderTarget))
				return;

			if (Cast (PWS, () => HasSpell (PWS) && pwsTarget != null, pwsTarget))
				return;

			if (Cast (CLARITY_OF_WILL, () => HasSpell (CLARITY_OF_WILL) && clarityTarget != null, clarityTarget))
				return;

			if (Cast (PAIN_SUPRESSION, () => HasSpell (PAIN_SUPRESSION) && tankPainSupTarget != null && SpellCooldown (PAIN_SUPRESSION) <= 0, tankPainSupTarget))
				return;

			if (Cast (PWSOLACE, () => HasSpell (PWSOLACE)
			    && tankTarget != null
			    && SpellCooldown (PWSOLACE) <= 0
			    && !Me.IsNotInFront (tankTarget), tankTarget))
				return;

			if (Cast (CASCADE, () => HasSpell (CASCADE) && cascadeTarget != null && SpellCooldown (CASCADE) <= 0, cascadeTarget))
				return;

			if (Cast (HALO, () => HasSpell (HALO) && haloTarget != null && SpellCooldown (HALO) <= 0, haloTarget))
				return;

			if (Cast (PENANCE, () => HasSpell (PENANCE)
			    && penanceTarget != null
			    && SpellCooldown (PENANCE) <= 0, penanceTarget))
				return;

			if (Cast (PRAYER_OF_MENDING, () => HasSpell (PRAYER_OF_MENDING)
			    && PomTarget != null
			    && SpellCooldown (PRAYER_OF_MENDING) <= 0, PomTarget))
				return;

			if (Cast (PRAYER_OF_HEALING, () => HasSpell (PRAYER_OF_HEALING)
			    && pohTarget != null
			    && SpellCooldown (PRAYER_OF_HEALING) <= 0, pohTarget))
				return;

			if (Cast (FLASH_HEAL, () => HasSpell (FLASH_HEAL)
			    && flashTarget != null, flashTarget))
				return;

			if (Cast (HEAL, () => HasSpell (HEAL)
			    && healTarget != null, healTarget))
				return;

			if (CastSelfPreventDouble (HOLY_NOVA, () => needHolyNova))
				return;

			if (Cast (HOLY_FIRE, () => HasSpell (HOLY_FIRE)
			    && tankTarget != null && SpellCooldown (HOLY_FIRE) <= 0
			    && tankTarget.Distance <= dpsRange
			    && !Me.IsNotInFront (tankTarget), tankTarget))
				return;

			if (Cast (SMITE, () => HasSpell (SMITE)
			    && Atonement
			    && Me.ManaFraction > AtonementMana && tankTarget != null
			    && !Me.IsNotInFront (tankTarget), tankTarget))
				return;
		}

		public void RaidHealing ()
		{
			if (useHealthstone ()) {
				Healthstone ();
				return;
			}
			if (useHealTonic ()) {
				HealTonic ();
				return;
			}
			if (useTrinket1) {
				Trinket1 ();
				return;
			}
			if (useTrinket2) {
				Trinket2 ();
				return;
			}

			if (Cast (DISPEL_MAGIC, () => HasSpell (DISPEL_MAGIC) && dispelTarget != null && SpellCooldown (DISPEL_MAGIC) <= 0, dispelTarget))
				return;
			if (Cast (PURIFY, () => HasSpell (PURIFY) && purifyTarget != null && SpellCooldown (PURIFY) <= 0, purifyTarget))
				return;

			if (CastSelfPreventDouble (ARCHANGEL, () => HasSpell (ARCHANGEL) && Me.HasAura (EVANGELISM) && AuraStackCount (EVANGELISM) >= 5))
				return;

			if (CastSelfPreventDouble (POWER_INFUSION, () => HasSpell (POWER_INFUSION) && SpellCooldown (POWER_INFUSION) <= 0 && needPowerInfusion))
				return;

			if (Cast (MANAFIEND, () => HasSpell (MANAFIEND) && tankTarget != null && SpellCooldown (MANAFIEND) <= 0, tankTarget))
				return;

			if (Cast (MINDBENDER, () => HasSpell (MINDBENDER)
			    && mindbenderTarget != null
			    && Me.ManaFraction <= MindbenderMana
			    && SpellCooldown (MINDBENDER) <= 0, mindbenderTarget))
				return;

			if (Cast (PWS, () => HasSpell (PWS) && pwsTarget != null, pwsTarget))
				return;

			if (Cast (CLARITY_OF_WILL, () => HasSpell (CLARITY_OF_WILL) && clarityTarget != null, clarityTarget))
				return;

			if (Cast (PAIN_SUPRESSION, () => HasSpell (PAIN_SUPRESSION) && tankPainSupTarget != null && SpellCooldown (PAIN_SUPRESSION) <= 0, tankPainSupTarget))
				return;

			if (Cast (PWSOLACE, () => HasSpell (PWSOLACE)
			    && tankTarget != null
			    && SpellCooldown (PWSOLACE) <= 0
			    && !Me.IsNotInFront (tankTarget), tankTarget))
				return;

			if (Cast (CASCADE, () => HasSpell (CASCADE) && cascadeTarget != null && SpellCooldown (CASCADE) <= 0, cascadeTarget))
				return;

			if (Cast (HALO, () => HasSpell (HALO) && haloTarget != null && SpellCooldown (HALO) <= 0, haloTarget))
				return;

			if (Cast (PENANCE, () => HasSpell (PENANCE)
			    && penanceTarget != null
			    && SpellCooldown (PENANCE) <= 0, penanceTarget))
				return;

			if (Cast (PRAYER_OF_MENDING, () => HasSpell (PRAYER_OF_MENDING)
			    && PomTarget != null
			    && SpellCooldown (PRAYER_OF_MENDING) <= 0, PomTarget))
				return;

			if (Cast (PRAYER_OF_HEALING, () => HasSpell (PRAYER_OF_HEALING)
			    && pohTarget != null
			    && SpellCooldown (PRAYER_OF_HEALING) <= 0, pohTarget))
				return;

			if (Cast (FLASH_HEAL, () => HasSpell (FLASH_HEAL)
			    && flashTarget != null, flashTarget))
				return;

			if (Cast (HEAL, () => HasSpell (HEAL)
			    && healTarget != null, healTarget))
				return;

			if (CastSelfPreventDouble (HOLY_NOVA, () => needHolyNova))
				return;

			if (Cast (HOLY_FIRE, () => HasSpell (HOLY_FIRE)
			    && tankTarget != null && SpellCooldown (HOLY_FIRE) <= 0
			    && tankTarget.Distance <= dpsRange
			    && !Me.IsNotInFront (tankTarget), tankTarget))
				return;

			if (Cast (SMITE, () => HasSpell (SMITE)
			    && Atonement
			    && Me.ManaFraction > AtonementMana && tankTarget != null
			    && !Me.IsNotInFront (tankTarget), tankTarget))
				return;
		}


		public const string
			ARCHANGEL = "Archangel",
			CASCADE = "Cascade",
			CLARITY_OF_WILL = "Clarity of Will",
			DESPERATE_PRAYER = "Desperate Prayer",
			DISPEL_MAGIC = "Dispel Magic",
			EVANGELISM = "Evangelism",
			FLASH_HEAL = "Flash Heal",
			HALO = "Halo",
			HEAL = "Heal",
			HOLY_FIRE = "Holy Fire",
			HOLY_NOVA = "Holy Nova",
			MANAFIEND = "Shadowfiend",
			MINDBENDER = "Mindbender",
			PAIN_SUPRESSION = "Pain Suppression",
			PENANCE = "Penance",
			PWS = "Power Word: Shield",
			PWSOLACE = "Power Word: Solace",
			POWER_INFUSION = "Power Infusion",
			PRAYER_OF_MENDING = "Prayer of Mending",
			PRAYER_OF_HEALING = "Prayer of Healing",
			PURIFY = "Purify",
			SWP = "Shadow Word: Pain",
			SMITE = "Smite",
			WEAKENED_SOUL = "Weakened Soul",
			einde = "einde";


		#region targets

		//if(Cast("Nature's Cure", () => Me.Auras.Any(a => a.IsDebuff && "Magic,Curse,Poison".Contains(a.DebuffType)))) return;

		public UnitObject purifyTarget {
			get {
				var t = PartyMembers ().Where (p => !p.IsDead
				        && p.IsInCombatRangeAndLoS
				        && p.Auras.Any (a => a.IsDebuff && "Magic,Disease".Contains (a.DebuffType))).FirstOrDefault ();
				return t != null ? t : null;
			}
		}

		public UnitObject dispelTarget {
			get {
				var t = PartyMembers ().Where (p => !p.IsDead
				        && p.IsInCombatRangeAndLoS
				        && p.Auras.Any (a => a.IsDebuff && "Magic".Contains (a.DebuffType))).FirstOrDefault ();
				return t != null ? t : null;
			}
		}

		public bool isTank (UnitObject unit)
		{
			return Tanks ().Contains (unit); 
		}

		public UnitObject clarityTarget {
			get {
				UnitObject t = null;
				if (Group.GetNumGroupMembers () < 6) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= clarityOfWillHealth).FirstOrDefault ();
				}
				if (Group.GetNumGroupMembers () > 5) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= clarityOfWillHealth).FirstOrDefault ();
				}
				return t != null ? t : null;
			}
		}

		public UnitObject mindbenderTarget {
			get {
				var t = API.Units.Where (p => p.InCombat
				        && !p.IsFriendly
				        && p.IsInCombatRangeAndLoS).OrderByDescending (p => p.HealthFraction).FirstOrDefault ();
				return t != null ? t : null;
			}
		}

		public bool needPowerInfusion {
			get {
				var t = new List<UnitObject> ();
				if (Group.GetNumGroupMembers () < 6) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= 0.65).ToList ();
					if (t.Count () >= 3)
						return true;
				}
				if (Group.GetNumGroupMembers () > 5) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= 0.65).ToList ();
					if (t.Count () >= 5)
						return true;
				}
				return false;
			}
		}

		public bool needHolyNova {
			get {
				var t = new List<UnitObject> ();
				if (Group.GetNumGroupMembers () < 6) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= holyNovaHealthD
					&& p.Distance2DTo (Me.Position) <= 10).ToList ();
					if (t.Count () >= holyNovaPlayersD)
						return true;
				}
				if (Group.GetNumGroupMembers () > 5) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= holyNovaHealthR
					&& p.Distance2DTo (Me.Position) <= 10).ToList ();
					if (t.Count () >= holyNovaPlayersR)
						return true;
				}
				return false;
			}
		}

		public UnitObject healTarget {
			get {
				UnitObject t = null;
				if (Group.GetNumGroupMembers () < 6) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= healHealthD).FirstOrDefault ();
				}
				if (Group.GetNumGroupMembers () > 5) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= healHealthR).FirstOrDefault ();
				}
				return t != null ? t : null;
			}
		}

		public UnitObject flashTarget {
			get {
				UnitObject t = null;
				if (Group.GetNumGroupMembers () < 6) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= flashHealD).FirstOrDefault ();
				}
				if (Group.GetNumGroupMembers () > 5) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= flashHealR).FirstOrDefault ();
				}
				return t != null ? t : null;
			}
		}

		public UnitObject haloTarget {
			get {
				var t = new List<UnitObject> ();
				if (Group.GetNumGroupMembers () < 6) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= haloHealthD).ToList ();
					if (t.Count () >= haloPlayersD)
						return t.FirstOrDefault ();
				}
				if (Group.GetNumGroupMembers () > 5) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= haloHealthR).ToList ();
					if (t.Count () >= haloPlayersR)
						return t.FirstOrDefault ();
				}
				return null;
			}
		}

		public UnitObject cascadeTarget {
			get {
				var t = new List<UnitObject> ();
				if (Group.GetNumGroupMembers () < 6) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= cascadeHealthD).ToList ();
					if (t.Count () >= cascadePlayersD)
						return t.FirstOrDefault ();
				}
				if (Group.GetNumGroupMembers () > 5) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= cascadeHealthR).ToList ();
					if (t.Count () >= cascadePlayersR)
						return t.FirstOrDefault ();
				}
				return null;
			}
		}

		public UnitObject pohTarget {
			get {
				var t = new List<UnitObject> ();
				if (Group.GetNumGroupMembers () < 6) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= PoHHealthD).ToList ();
					if (t.Count () >= PoHPlayersD)
						return t.FirstOrDefault ();
				}
				if (Group.GetNumGroupMembers () > 5) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= PoHHealthR).ToList ();
					if (t.Count () >= PoHPlayersR)
						return t.FirstOrDefault ();
				}
				return null;
			}
		}

		public UnitObject PomTarget {
			get {
				UnitObject t = null;
				if (Group.GetNumGroupMembers () < 6) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= PoMHealthD).FirstOrDefault ();
				}
				if (Group.GetNumGroupMembers () > 5) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= PoMHealthR).FirstOrDefault ();
				}
				return t != null ? t : null;
			}
		}

		public UnitObject penanceTarget {
			get {
				UnitObject t = null;
				if (Group.GetNumGroupMembers () < 6) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= penanceHealthD).FirstOrDefault ();
				}
				if (Group.GetNumGroupMembers () > 5) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& p.HealthFraction <= penanceHealthR).FirstOrDefault ();
				}
				return t != null ? t : null;
			}
		}

		public UnitObject tankTarget {
			get {
				bool haveTank = false;
				foreach (UnitObject unit in Tanks()) {
					if (unit != null && !unit.IsDead && unit.IsInCombatRangeAndLoS && unit.InCombat)
						haveTank = true;
				}
				if (!haveTank) {
					var t = API.Units.Where (p => !p.IsDead
					        && p.InCombat
					        && !p.IsFriendly
					        && p.IsInCombatRangeAndLoS).OrderBy (p => p.Distance).FirstOrDefault ();
					return t != null ? t : null;
				} else if (haveTank) {
					var unit = Tanks ().Where (p => !p.IsDead
					           && p.Target != null
					           && p.IsInCombatRangeAndLoS).OrderBy (p => p.Distance).FirstOrDefault ();
					if (unit != null) {
						UnitObject newTarget = unit.Target;
						return newTarget;
					}
				}
				return null;
			}
		}

		public UnitObject tankPainSupTarget {
			get {
				var t = Tanks ().Where (p => !p.IsDead
				        && p.IsInCombatRangeAndLoS
				        && p.HealthFraction <= painSupHealth).OrderBy (p => p.Distance).FirstOrDefault ();
				return t != null ? t : null;
			}
		}

		public UnitObject pwsTarget {
			get {
				UnitObject t = null;
				if (Group.GetNumGroupMembers () < 6) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& !p.HasAura (PWS)
					&& !p.HasAura (WEAKENED_SOUL)
					&& (p.HealthFraction <= PWSHealthD || (isTank (p) && pwsTank))).FirstOrDefault ();
				}
				if (Group.GetNumGroupMembers () > 5) {
					t = PartyMembers ().Where (p => !p.IsDead
					&& p.IsInCombatRangeAndLoS
					&& !p.HasAura (PWS)
					&& !p.HasAura (WEAKENED_SOUL)
					&& (p.HealthFraction <= PWSHhealthR || (isTank (p) && pwsTank))).FirstOrDefault ();
				}
				return t != null ? t : null;
			}
		}

		public UnitObject noSWPTarget {
			get {
				var t = new List<UnitObject> ();
				t = API.Units.Where (p => p != null
				&& !p.IsDead
				&& p.IsValid
				&& (p.InCombat && p.IsTargetingMeOrPets)
				&& !Me.IsNotInFront (p)
				&& p.Distance <= dpsRange
				&& p.IsInLoS
				&& !p.HasAura (SWP)).ToList ();
				return t.Count () > 0 ? t.FirstOrDefault () : null;
			}
		}

		#endregion

		/*public Keys pressedKey
        {
            get
            {
                if (API.LuaIf("IsShiftKeyDown()")) return Keys.Shift;
                if (API.LuaIf("IsControlKeyDown()")) return Keys.Ctrl;
                if (API.LuaIf("IsAltKeyDown()")) return Keys.Alt;
                return Keys.None;
            }
        }*/

		public bool useTrinket1 {
			get {
				if (trinket1Health != 0 && Me.HealthFraction <= trinket1Health) {
					return true;
				}
				if (trinket1mana != 0 && Me.ManaFraction <= trinket1mana) {
					return true;
				}
				return false;
			}
		}

		public bool useTrinket2 {
			get {
				if (trinket2Health != 0 && Me.HealthFraction <= trinket2Health) {
					return true;
				}
				if (trinket2mana != 0 && Me.ManaFraction <= trinket2mana) {
					return true;
				}
				return false;
			}
		}

		public void Trinket1 ()
		{
			if (useTrinket1 && API.ExecuteLua<double> ("local _, duration, _= GetItemCooldown(GetInventoryItemID(\"player\", 13)); return duration;") == 0) {
				API.ExecuteMacro ("/use 13");
			}
		}

		public void Trinket2 ()
		{
			if (useTrinket2 && API.ExecuteLua<double> ("local _, duration, _= GetItemCooldown(GetInventoryItemID(\"player\", 13)); return duration;") == 0) {
				API.ExecuteMacro ("/use 13");
			}
		}

		public bool useHealTonic ()
		{
			return Me.HealthFraction <= healTonicHealth;
		}

		public void HealTonic ()
		{
			if (useHealTonic () && API.ItemCount (healTonicItem) > 0 && API.ItemCooldown (healTonicItem) <= 0 && Me.HealthFraction <= healTonicHealth) {
				API.UseItem (healTonicItem);
			}
		}

		public bool useHealthstone ()
		{
			return Me.HealthFraction <= healthstoneHealth;
		}

		public void Healthstone ()
		{
			if (useHealthstone () && API.ItemCount (healthstoneItem) > 0 && API.ItemCooldown (healthstoneItem) <= 0 && Me.HealthFraction <= healthstoneHealth) {
				API.UseItem (healthstoneItem);
			}
		}


		public bool Damage ()
		{
			//	actions=potion,name=draenic_intellect,if=buff.bloodlust.react|target.time_to_die<=40
			if (IsBoss () && (Me.HasAura ("Bloodlust") || TimeToDie () <= 40)) {
				if (DraenicIntellect ())
					return true;
			}
			//	actions+=/mindbender,if=talent.mindbender.enabled
			if (HasSpell ("Mindbender")) {
				if (Mindbender ())
					return true;
			}
			//	actions+=/shadowfiend,if=!talent.mindbender.enabled
			if (!HasSpell ("Mindbender")) {
				if (Shadowfiend ())
					return true;
			}
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/power_infusion,if=talent.power_infusion.enabled
			if (HasSpell ("Power Infusion"))
				PowerInfusion ();
			//	actions+=/shadow_word_pain,if=!ticking
			if (Usable ("Shadow Word: Pain")) {
				Unit = Enemy.Where (u => !u.IsDead && Range (40, u) && u.InCombat && u.IsAttackable && !u.HasAura ("Shadow Word: Pain", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (ShadowWordPain (Unit))
						return true;
				}
			}
			//	actions+=/penance
			if (Penance ())
				return true;
			//	actions+=/power_word_solace,if=talent.power_word_solace.enabled
			if (HasSpell ("Power Word: Solace")) {
				if (PowerWordSolace ())
					return true;
			}
			//	actions+=/holy_fire,if=!talent.power_word_solace.enabled
			if (!HasSpell ("Power Word: Solace")) {
				if (HolyFire ())
					return true;
			}
			//	actions+=/smite,if=glyph.smite.enabled&(dot.power_word_solace.remains+dot.holy_fire.remains)>cast_time
			if (HasGlyph (55692) && (Target.AuraTimeRemaining ("Power Word: Solace") + Target.AuraTimeRemaining ("Holy Fire")) > 1.5) {
				if (Smite ())
					return true;
			}
			//	actions+=/shadow_word_pain,if=remains<(duration*0.3)
			if (Usable ("Shadow Word: Pain")) {
				Unit = Enemy.Where (u => !u.IsDead && Range (40, u) && u.InCombat && u.IsAttackable && u.AuraTimeRemaining ("Shadow Word: Pain", true) < 5.4).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (ShadowWordPain (Unit))
						return true;
				}
			}
			//	actions+=/smite
			if (Smite ())
				return true;
			//	actions+=/shadow_word_pain
			if (ShadowWordPain ())
				return true;

			return false;
		}

		public bool Healing (UnitObject u = null)
		{
			u = u ?? Target;

			if (Me.HasAura ("Evangelism"))
				Archangel ();
			//	actions=mana_potion,if=mana.pct<=75
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/power_infusion,if=talent.power_infusion.enabled
			PowerInfusion ();
			//	actions+=/power_word_solace,if=talent.power_word_solace.enabled
			if (PowerWordSolace (u))
				return true;
			//	actions+=/mindbender,if=talent.mindbender.enabled&mana.pct<80
			if (HasSpell ("Mindbender") && Me.ManaFraction < 0.8) {
				if (Mindbender (u))
					return true;
			}
			//	actions+=/shadowfiend,if=!talent.mindbender.enabled
			if (!HasSpell ("Mindbender")) {
				if (Shadowfiend (u))
					return true;
			}
			//	actions+=/power_word_shield
			if (PowerWordShield (u))
				return true;
			//	actions+=/penance_heal,if=buff.borrowed_time.up
			if (Me.HasAura ("Borrowed Time")) {
				if (Penance (u))
					return true;
			}
			//	actions+=/penance_heal
			if (Penance (u))
				return true;
			//	actions+=/flash_heal,if=buff.surge_of_light.react
			if (Me.HasAura ("Surge of Light")) {
				if (FlashHeal (u))
					return true;
			}
			//	actions+=/prayer_of_mending
			if (PrayerofMending (u))
				return true;
			//	actions+=/clarity_of_will
			if (ClarityofWill (u))
				return true;
			//	actions+=/heal,if=buff.power_infusion.up|mana.pct>20
			if (Me.HasAura ("Power Infusion") || Me.ManaFraction > 0.2) {
				if (Heal (u))
					return true;
			}
			//	actions+=/heal
			if (Heal (u))
				return true;
			if (FlashHeal (u))
				return true;

			return false;
		}

		public bool HealTank (UnitObject u)
		{
			if (PowerWordShield (u))
				return true;
			if (PrayerofMending (u))
				return true;
			if (ClarityofWill (u))
				return true;
			if (DivineStar (u))
				return true;

			if (Health (u) <= 0.3) {
				if (FlashHeal (u))
					return true;
			}
			if (Health (u) <= 0.7 && !Me.HasAura ("Saving Grace")) {						
				if (SavingGrace (u))
					return true;
			}
			if (Health (u) <= 0.7) {
				if (PainSuppression (u))
					return true;
			}
			if (Health (u) <= 0.8) {
				if (Heal (u))
					return true;
			} 
			if (Health (u) <= 0.9) {
				if (Penance (u))
					return true;
			}
				
			return false;
		}

		public bool DungeonHealing (UnitObject u)
		{
			if (Health (Me) < 0.45) { 
				if (Healthstone ())
					return true;
			}
//			if (useHealTonic()) { HealTonic(); return; }
//			if (useTrinket1) { Trinket1(); return; }
//			if (useTrinket2) { Trinket2(); return; }
		
			u = u ?? Target;

			if (Me.HasAura ("Evangelism") && AuraStackCount ("Evangelism") >= 5)
				Archangel ();
			//	actions=mana_potion,if=mana.pct<=75
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/power_infusion,if=talent.power_infusion.enabled
			PowerInfusion ();
			//	actions+=/power_word_solace,if=talent.power_word_solace.enabled
			if (Tank != null && Tank.Target != null) {
				if (PowerWordSolace (Tank.Target))
					return true;
			}
			//	actions+=/mindbender,if=talent.mindbender.enabled&mana.pct<80
			if (HasSpell ("Mindbender") && Me.ManaFraction < 0.8) {
				if (Mindbender (u))
					return true;
			}
			//	actions+=/shadowfiend,if=!talent.mindbender.enabled
			if (!HasSpell ("Mindbender")) {
				if (Shadowfiend (u))
					return true;
			}
			//	actions+=/power_word_shield
			if (PowerWordShield (u))
				return true;

			if (CascadeTarget != null) {
				if (Cascade (CascadeTarget))
					return true;
			}

			if (HaloTarget != null) {
				if (Halo (HaloTarget))
					return true;
			}

			//	actions+=/penance_heal,if=buff.borrowed_time.up
			if (Me.HasAura ("Borrowed Time")) {
				if (Penance (u))
					return true;
			}
			//	actions+=/penance_heal
			if (Penance (u))
				return true;
			//	actions+=/flash_heal,if=buff.surge_of_light.react
			if (Me.HasAura ("Surge of Light")) {
				if (FlashHeal (u))
					return true;
			}
			//	actions+=/prayer_of_mending
			if (PrayerofMending (u))
				return true;
			//	actions+=/clarity_of_will
			if (ClarityofWill (u))
				return true;
			//	actions+=/heal,if=buff.power_infusion.up|mana.pct>20
			if (Me.HasAura ("Power Infusion") || Me.ManaFraction > 0.2) {
				if (Heal (u))
					return true;
			}
			//	actions+=/heal
			if (Heal (u))
				return true;
			if (FlashHeal (u))
				return true;

			return false;
		}
	}
}

////		public override void Combat()
////		{
////
////			else
////			{
////				DPS();
////			}
////			//Dummy zapping
////			if (Target.DisplayId == 28048 || Target.DisplayId == 27510)
////			{
////				DPS();
////			}
////		}
////
////		void Healer()
////		{
////			// setting group
////			List<PlayerObject> members = Group.GetGroupMemberObjects();
////			int membercount = members.Count + 1;
////
////			if (membercount > 0)
////			{
////				// Finding Tank
////				List<PlayerObject> Tanks = members.FindAll(x => x.IsTank && x.IsInCombatRange && !x.IsDead);
////				PlayerObject Tank1 = Tanks.FirstOrDefault();
////
////				// Group Healing
////				if (Halo_talent == true && Use_Halo == true)
////				{
////					Halo();
////				}
////				else if (Cascade_talent == true)
////				{
////					Cascade();
////				}
////
////				List<PlayerObject> GrpHeal2 = members.FindAll(x => x.HealthFraction <= 0.7 && x.IsInCombatRange && !x.IsDead && x.Distance < 40);
////				PlayerObject healtest = GrpHeal2.FirstOrDefault(x => !x.IsDead && x.IsInCombatRange);
////
////				if (GrpHeal2.Count >= 2)
////				{
////					if (Tank1 != null)
////					{
////						if (Tank1.HealthFraction > healtest.HealthFraction)
////						{
////							Cast("Prayer of Healing", GrpHeal2.First());
////							DebugWrite("Casting Payer of Healing");
////							return;
////						}
////						else
////						{
////							return;
////						}
////					}
////				}
////				Holy_Nova();
////
////
////
////
////				// Tank 2
////				if (Tanks.Count > 1)
////				{
////					PlayerObject Tank2 = Tanks.Last();
////
////					if (Tank2 != null && Tank2.IsInCombatRange)
////					{
////
////
////						if (Cast("Power Word: Shield", () => !Tank2.HasAura("Weakened Soul") && !Tank2.HasAura("Power Word: Shield") && Tank2.IsInCombatRange && !Tank2.IsDead, Tank2))
////						{
////							DebugWrite("Shielding " + Tank2);
////						}
////						if (Cast("Prayer of Mending", () => !Tank2.HasAura("Prayer of Mending") && Tank2.IsInCombatRange && !Tank2.IsDead, Tank2))
////						{
////							DebugWrite("POM on " + Tank2);
////						}
////						if (Tank2.HealthFraction <= 0.3)
////						{
////							Cast("Flash Heal", () => Tank2.IsInCombatRange && !Tank2.IsDead, Tank2);
////							DebugWrite("Casting Flash Heal on " + Tank2);
////						}
////						else if (Tank2.HealthFraction <= 0.5 && !Me.HasAura("Saving Grace") && Saving_Grace == true && Tank2.IsInCombatRange)
////						{
////							Cast("Saving Grace", Tank2);
////						}
////						else if (Tank2.HealthFraction <= 0.7 && SpellCooldown("Pain Suppression") < 0)
////						{
////							Cast("Pain Suppression", () => Tank2.IsInCombatRange && !Tank2.IsDead, Tank2);
////							DebugWrite("Casting Pain Supression on " + Tank2);
////
////						}
////						else if (Tank2.HealthFraction <= 0.8)
////						{
////							Cast("Heal", () => Tank2.IsInCombatRange && !Tank2.IsDead, Tank2);
////							DebugWrite("Casting Heal on " + Tank2);
////
////						}
////						else if (Tank2.HealthFraction <= 0.9 && SpellCooldown("Penance") < 0)
////						{
////							Cast("Penance", () => Tank2.IsInCombatRange && !Tank2.IsDead, Tank2);
////							DebugWrite("Casting Penance on " + Tank2);
////
////						}
////					}
////					else
////					{
////						return;
////					}
////					//Second part of group healing, single target healing
////
////				}
////
////				foreach (var player in Group.GetGroupMemberObjects().Where(x => x.HealthFraction < 0.9 && x.IsInCombatRange))
////				{
////					Cast("Power Word: Shield", player, () => !player.HasAura("Weakened Soul") && !player.HasAura("Power Word: Shield") && player.IsInCombatRange && !player.IsDead && SpellCooldown("Power Word: Shield") < 0);
////					Cast("Saving Grace", player, ()=> player.HealthFraction <= 0.5 && !Me.HasAura("Saving Grace") && Saving_Grace == true && player.IsInCombatRangeAndLoS);
////					Cast("Flash Heal", player, () => !player.IsTank && !player.IsDead && player.HealthFraction < 0.5 && !player.IsDead && !Me.IsCasting);
////					Cast("Penance", player, () => !player.IsTank && !player.IsDead && player.HealthFraction < 0.75 && !player.IsDead && !Me.IsCasting && SpellCooldown("Penance") < 0);
////					Cast("Heal", player, () => !player.IsTank && !player.IsDead && player.HealthFraction < 0.8 && player.HealthFraction > 0.4 && !player.IsDead && !Me.IsCasting);
////				}
////				//Healing me
////				//------------Desperate prayer
////				if (Desperate_Prayer == true)
////				{
////					DesperatePrayer();
////				}
////				CastSelf("Power Word: Shield", () => !Me.HasAura("Weakened Soul") && !Me.HasAura("Power Word: Shield") && Me.HealthFraction < 0.99);
////				CastSelf("Heal", () => Me.HealthFraction <= 0.9);
////				CastSelf("Flash Heal", () => Me.HealthFraction <= 0.4);
////				//------------Desperate Prayer end
////				//Regen mana if low
////				//------------Mindbender
////				if (Mindbender == true)
////				{
////					MindBender();
////				}
////				//------------Mindbender end
////				if (Cast("Shadowfiend", () => Me.ManaFraction < 0.5 && Mindbender == false))
////				{
////					DebugWrite("Low Mana Releasing Shadowfiend");
////					return;
////				}
////
////
////
////			}
////		}
////
////		void DPS()
////		{
////			//Global cooldown check
////			if (HasGlobalCooldown())
////				return;
////			List<PlayerObject> members = Group.GetGroupMemberObjects();
////			List<PlayerObject> GrpHeal1 = members.FindAll(x => x.HealthFraction <= 0.85 && x.IsInCombatRangeAndLoS && !x.IsDead);
////
////			List<PlayerObject> Tanks = members.FindAll(x => x.IsTank && x.IsInCombatRange && !x.IsDead);
////			PlayerObject Tank1 = Tanks.FirstOrDefault();
////			if (Tank1 != null)
////			{
////				Cast("Power Word: Shield", () => !Tank1.HasAura("Weakened Soul") && !Tank1.HasAura("Power Word: Shield") && Tank1.IsInCombatRange && !Tank1.IsDead, Tank1);
////			}
////
////			if (!Me.Target.IsEnemy || Me.HealthFraction < 0.5 || GrpHeal1.Count >= 2 || Tank1Healing() == true)
////			{
////				Healer();
////			}
////			else if (Me.Target.IsEnemy && Me.HealthFraction > 0.5 && GrpHeal1.Count < 2 && Me.IsChanneling == false && Tank1Healing() == false)
////			{
////				if (HasGlobalCooldown())
////					return;
////				//------------Power Word: Solace
////				PowerWordSolace();
////				//------------Power Word: Solace end
////				//finding adds for Dots
////				List<UnitObject> mobs = Adds.FindAll(x => x.DistanceSquared < SpellMaxRangeSq("Shadow Word: Pain") && x.IsEnemy);
////				if (mobs.Count > 0)
////				{
////
////					//Finds adds for SWpain dot
////					foreach (var SWpain in mobs.Where(x => !x.HasAura("Shadow Word: Pain") && x.IsEnemy))
////					{
////						//Dots mobs in range
////						UnitObject SWP = SWpain;
////						CastPreventDouble("Shadow Word: Pain", () => !SWP.IsDead && SWP.IsEnemy, SWP);
////
////						return;
////					}
////				}
////
////				//mana regen
////				//-----------------Mindbender
////				if (Mindbender == true)
////				{
////					MindBender();
////				}
////				//-----------------Mindbender end
////				if (Cast("Shadowfiend", () => Me.ManaFraction < 0.5)) return;
////				//-------------Void Tendrils
////				if (Void_Tendrils == true)
////				{
////					VoidTendrils();
////				}
////				//-------------Void Tendrils end
////
////				// attack rotation
////				if (Cast("Penance", () => Target.IsEnemy)) return;
////
////
////
////				Cast("Holy Fire", () => !Target.HasAura("Holy Fire") && SpellCooldown("Holy Fire") < 0 && Target.IsEnemy);
////				Cast("Shadow Word: Pain", () => !Target.HasAura("Shadow Word: Pain") && Target.IsEnemy);
////				List<UnitObject> MS = Adds.FindAll(x => x.DistanceSquaredTo(Target) < 10 * 10 && x.IsEnemy && x.HasAura("Shadow Word: Pain"));
////				if (MS.Count >= 2 || Target.DisplayId == 28048)
////				{
////					Mind_Sear();
////				}
////
////				if (Cast("Smite", () => Me.ChannelingSpellID != 48045 && !Me.IsCasting && Me.Target.IsEnemy)) return;
////
////			}
////		}
////
////		//--------------Talent Start --------------
////		void DesperatePrayer() // Rotated in
////		{
////			if (CastPreventDouble("Desperate Prayer", () => Me.HealthFraction < 0.5 && !Me.IsCasting)) return;
////		}
////		void AngelicFeather() //Rotated in
////		{
////			if (CastOnTerrain("Angelic Feather", Me.PositionPredicted, () => Me.MovementSpeed > 0 && !HasAura("Angelic Feather"))) return;
////		}
////		void MindBender() //Rotated in
////		{
////			if (Cast("Mindbender", () => Me.ManaFraction < 0.5 && Me.Target.IsEnemy, Adds.FirstOrDefault()))
////			{
////				DebugWrite("Low Mana Casting Mindbender");
////			}
////		}
////		void PowerWordSolace() //Rotated in
////		{
////			List<UnitObject> mobs = Adds.FindAll(x => x.DistanceSquared < SpellMaxRangeSq("Power Word: Solace") && x.IsEnemy);
////			if (mobs.Count > 0)
////			{
////				//Solace CD Check
////				if (SpellCooldown("Power Word: Solace") < 0)
////				{
////
////					//Cast solace on add
////					var Solmob = mobs.FirstOrDefault();
////					CastPreventDouble("Power Word: Solace", () => !Solmob.IsDead && Solmob.IsInCombatRangeAndLoS && Me.Target.IsEnemy, Solmob);
////					DebugWrite("Casting Solace on " + Solmob + SpellCooldown("Power Word: Solace"));
////					return;
////				}
////			}
////
////			if (Cast("Power Word: Solace")) return;
////		}
////		void VoidTendrils() //Rotated in
////		{
////			List<UnitObject> mobs = Adds.FindAll(x => x.DistanceSquaredTo(Target) < 10 * 10);
////			if (CastPreventDouble("Void Tendrils", () => mobs.Count > 2, Adds.FirstOrDefault())) return;
////		}
////		void PsychicScream()
////		{
////			List<UnitObject> mobs = Adds.FindAll(x => x.DistanceSquaredTo(Me) < 8 * 8);
////			if (CastPreventDouble("Psychic Scream", () => mobs.Count > 2)) return;
////		}
////		void DominateMind()
////		{
////			//No idea how to get this working at the min :D
////		}
////		void SpiritShell()
////		{
////			if (CastPreventDouble("Spirit Shell", () => !Target.IsEnemy && Target.HealthFraction < 0.9 && Target.IsInCombatRangeAndLoS && !Me.HasAura("Spirit Shell"))) return;
////		}
////		void Cascade()//Rotated in
////		{
////			List<PlayerObject> members = Group.GetGroupMemberObjects();
////			if (members.Count > 0)
////			{
////				List<PlayerObject> GrpHeal1 = members.FindAll(x => x.HealthFraction <= 0.85 && x.IsInCombatRangeAndLoS && !x.IsDead);
////				PlayerObject healtarget = GrpHeal1.FirstOrDefault();
////				if (GrpHeal1.Count > 2)
////				{
////					if (CastPreventDouble("Cascade", () => Target.HealthFraction < 0.9 && SpellCooldown("Cascade") < 0, healtarget)) return;
////				}
////			}
////		}
////
////
////		void DivineStar()//Rotated in
////		{
////			List<PlayerObject> members = Group.GetGroupMemberObjects();
////			if (members.Count > 0)
////			{
////				// Finding Tank
////				List<PlayerObject> Tanks = members.FindAll(x => x.IsTank && x.IsInCombatRangeAndLoS && !x.IsDead);
////				PlayerObject Tank = Tanks.FirstOrDefault();
////				if (Tank != null)
////				{
////					Cast("Divine Start", () => !Tank.IsDead && Tank.HealthFraction < 0.8, Tank);
////					return;
////				}
////			}
////		}
////		void Halo()// Rotated in
////		{
////			List<PlayerObject> members = Group.GetGroupMemberObjects();
////			if (members.Count > 0)
////			{
////				List<PlayerObject> GrpHeal1 = members.FindAll(x => x.HealthFraction <= 0.85 && x.IsInCombatRangeAndLoS && !x.IsDead && x.DistanceSquaredTo(Me) < SpellMaxRangeSq("Halo"));
////				if (GrpHeal1.Count > 2)
////				{
////					if (CastPreventDouble("Halo", () => Target.HealthFraction < 0.9 && SpellCooldown("Halo") < 0)) return;
////				}
////			}
////		}
////		void ClarityOfWill()
////		{
////			List<PlayerObject> members = Group.GetGroupMemberObjects();
////			int membercount = members.Count + 1;
////
////			if (membercount > 0)
////			{
////
////				// Finding Tank
////				List<PlayerObject> Tanks = members.FindAll(x => x.IsTank && x.IsInCombatRange && !x.IsDead);
////				PlayerObject Tank1 = Tanks.FirstOrDefault();
////				if (Cast("Clarity of Will", ()=> Tank1.HealthMissing > 40000 && Me.Mana > 5000 && !Tank1.HasAura("Clarity of Will"), Tank1)) return;
////			}
////		}
////
////		//------------Talents End-----------
////
////		//------------Holy Nova
////		void Holy_Nova()
////		{
////			List<PlayerObject> members = Group.GetGroupMemberObjects();
////			if (members.Count > 0)
////			{
////				List<PlayerObject> GrpHeal1 = members.FindAll(x => x.HealthFraction <= 0.85 && x.IsInCombatRangeAndLoS && !x.IsDead && x.DistanceSquaredTo(Me) < SpellMaxRangeSq("Holy Nova"));
////				if (GrpHeal1.Count > 2)
////				{
////					if (CastPreventDouble("Holy Nova", () => Target.HealthFraction < 0.9 && SpellCooldown("Holy Nova") < 0)) return;
////				}
////			}
////		}
////
////		//------------Holy Nova end
////		void Mind_Sear()
////		{
////			Cast("Mind Sear", () => !Me.IsCasting && Me.Target.HasAura("Shadow Word: Pain"));
////		}
////
////	}
////}
////
////
////public void SoloRotation()
////{
////	if (useHealthstone()) { Healthstone(); return; }
////	if (useHealTonic()) { HealTonic(); return; }
////	if (useTrinket1) { Trinket1(); return; }
////	if (useTrinket2) { Trinket2(); return; }
////
////	if (Cast(DISPEL_MAGIC, () => HasSpell(DISPEL_MAGIC)
////		&& SpellCooldown(DISPEL_MAGIC) <= 0
////		&& Me.Auras.Any(a => a.IsDebuff
////			&& "Magic".Contains(a.DebuffType)))) return;
////
////	if (Cast(PURIFY, () => HasSpell(PURIFY)
////		&& SpellCooldown(PURIFY) <= 0
////		&& Me.Auras.Any(a => a.IsDebuff
////			&& "Magic,Disease".Contains(a.DebuffType)))) return;
////
////	if (CastSelfPreventDouble(ARCHANGEL, () => HasSpell(ARCHANGEL) && Me.HasAura(EVANGELISM) && AuraStackCount(EVANGELISM) >= 5)) return;
////	if (CastSelfPreventDouble(PAIN_SUPRESSION, () => HasSpell(PAIN_SUPRESSION) && Me.HealthFraction <= 0.45)) return;
////	if (CastSelfPreventDouble(POWER_INFUSION, () => HasSpell(POWER_INFUSION) && Target.MaxHealth > Me.MaxHealth * 2)) return;
////
////
////
////	if (Cast(MANAFIEND, () => HasSpell(MANAFIEND) && Target.MaxHealth > Me.MaxHealth * 2 && SpellCooldown(MANAFIEND) <= 0)) return;
////	if (Cast(MINDBENDER, () => HasSpell(MINDBENDER) && Me.ManaFraction <= MindbenderMana && SpellCooldown(MINDBENDER) <= 0)) return;
////	if (CastSelfPreventDouble(PWS, () => Me.HealthFraction <= 90 && !Me.HasAura(PWS))) return;
////	if (CastSelfPreventDouble(DESPERATE_PRAYER, () => HasSpell(DESPERATE_PRAYER) && Me.HealthFraction <= desperatePrayerHealth)) return;
////	if (CastPreventDouble(HOLY_FIRE, () => HasSpell(HOLY_FIRE) && Target.Distance <= dpsRange && SpellCooldown(HOLY_FIRE) <= 0)) return;
////	if (CastPreventDouble(PWSOLACE, () => HasSpell(PWSOLACE) && Target.Distance <= dpsRange && SpellCooldown(PWSOLACE) <= 0)) return;
////	if (Cast(SWP, () => HasSpell(SWP) && noSWPTarget != null, noSWPTarget)) return;
////	if (CastPreventDouble(PENANCE, () => HasSpell(PENANCE) && Target.Distance <= dpsRange && SpellCooldown(PENANCE) <= 0)) return;
////	if (Cast(SMITE, () => HasSpell(SMITE) && Target.Distance <= dpsRange)) return;
////
////}
////
////
////public void RaidHealing()
////{
////	if (useHealthstone()) { Healthstone(); return; }
////	if (useHealTonic()) { HealTonic(); return; }
////	if (useTrinket1) { Trinket1(); return; }
////	if (useTrinket2) { Trinket2(); return; }
////
////	if (Cast(DISPEL_MAGIC, () => HasSpell(DISPEL_MAGIC) && dispelTarget != null && SpellCooldown(DISPEL_MAGIC) <= 0, dispelTarget)) return;
////	if (Cast(PURIFY, () => HasSpell(PURIFY) && purifyTarget != null && SpellCooldown(PURIFY) <= 0, purifyTarget)) return;
////
////	if (CastSelfPreventDouble(ARCHANGEL, () => HasSpell(ARCHANGEL) && Me.HasAura(EVANGELISM) && AuraStackCount(EVANGELISM) >= 5)) return;
////
////	if (CastSelfPreventDouble(POWER_INFUSION, () => HasSpell(POWER_INFUSION) && SpellCooldown(POWER_INFUSION) <= 0 && needPowerInfusion)) return;
////
////	if (Cast(MANAFIEND, () => HasSpell(MANAFIEND) && tankTarget != null && SpellCooldown(MANAFIEND) <= 0, tankTarget)) return;
////
////	if (Cast(MINDBENDER, () => HasSpell(MINDBENDER)
////		&& mindbenderTarget != null
////		&& Me.ManaFraction <= MindbenderMana
////		&& SpellCooldown(MINDBENDER) <= 0, mindbenderTarget)) return;
////
////	if (Cast(PWS, () => HasSpell(PWS) && pwsTarget != null, pwsTarget)) return;
////
////	if (Cast(CLARITY_OF_WILL, () => HasSpell(CLARITY_OF_WILL) && clarityTarget != null, clarityTarget)) return;
////
////	if (Cast(PAIN_SUPRESSION, () => HasSpell(PAIN_SUPRESSION) && tankPainSupTarget != null && SpellCooldown(PAIN_SUPRESSION) <= 0, tankPainSupTarget)) return;
////
////	if (Cast(PWSOLACE, () => HasSpell(PWSOLACE)
////		&& tankTarget != null
////		&& SpellCooldown(PWSOLACE) <= 0
////		&& !Me.IsNotInFront(tankTarget), tankTarget)) return;
////
////	if (Cast(CASCADE, () => HasSpell(CASCADE) && cascadeTarget != null && SpellCooldown(CASCADE) <= 0, cascadeTarget)) return;
////
////	if (Cast(HALO, () => HasSpell(HALO) && haloTarget != null && SpellCooldown(HALO) <= 0, haloTarget)) return;
////
////	if (Cast(PENANCE, () => HasSpell(PENANCE)
////		&& penanceTarget != null
////		&& SpellCooldown(PENANCE) <= 0, penanceTarget)) return;
////
////	if (Cast(PRAYER_OF_MENDING, () => HasSpell(PRAYER_OF_MENDING)
////		&& PomTarget != null
////		&& SpellCooldown(PRAYER_OF_MENDING) <= 0, PomTarget)) return;
////
////	if (Cast(PRAYER_OF_HEALING, () => HasSpell(PRAYER_OF_HEALING)
////		&& pohTarget != null
////		&& SpellCooldown(PRAYER_OF_HEALING) <= 0, pohTarget)) return;
////
////	if (Cast(FLASH_HEAL, () => HasSpell(FLASH_HEAL)
////		&& flashTarget != null, flashTarget)) return;
////
////	if (Cast(HEAL, () => HasSpell(HEAL)
////		&& healTarget != null, healTarget)) return;
////
////	if (CastSelfPreventDouble(HOLY_NOVA, () => needHolyNova)) return;
////
////	if (Cast(HOLY_FIRE, () => HasSpell(HOLY_FIRE)
////		&& tankTarget != null && SpellCooldown(HOLY_FIRE) <= 0
////		&& tankTarget.Distance <= dpsRange
////		&& !Me.IsNotInFront(tankTarget), tankTarget)) return;
////
////	if (Cast(SMITE, () => HasSpell(SMITE)
////		&& Atonement
////		&& Me.ManaFraction > AtonementMana && tankTarget != null
////		&& !Me.IsNotInFront(tankTarget), tankTarget)) return;
////}
////
