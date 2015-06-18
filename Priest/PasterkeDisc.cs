using Newtonsoft.Json;
using ReBot.API;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace RebotClasses
{
	[Rotation ("Pasterke-DiscPriest", "Pasterke", WoWClass.Priest, Specialization.PriestDiscipline, 40, 20, false, 0)]
	class DiscPriestPasterke : CombatRotation
	{
		public bool Debug = true;
		public int healTonicItem = 109223;
		public int healthstoneItem = 5515;

		#region settings

		[JsonProperty ("Auto Dispel")]
		public bool AutoDispel = true;

		[JsonProperty ("Use Atonement Healing")]
		public bool Atonement = true;

		[JsonProperty ("Stop Atonement Healing at Mana %")]
		public double AtonementMana = 0.60;

		[JsonProperty ("Keep Power Word: Shield on tank")]
		public bool pwsTank = true;

		[JsonProperty ("Mindbender Mana %")]
		public double MindbenderMana = 0.60;

		[JsonProperty ("Healthstone HP %")]
		public double healthstoneHealth = 0.45;

		[JsonProperty ("Healing Tonic HP %")]
		public double healTonicHealth = 0.40;

		[JsonProperty ("Auto Use Trinket 1 at HP %"), Description ("0 = disabled")]
		public double trinket1Health = 0;

		[JsonProperty ("Auto Use Trinket 2 at HP %"), Description ("0 = disabled")]
		public double trinket2Health = 0;

		[JsonProperty ("Auto Use Trinket 1 at Mana %"), Description ("0 = disabled")]
		public double trinket1mana = 0;

		[JsonProperty ("Auto Use Trinket 2 at Mana %"), Description ("0 = disabled")]
		public double trinket2mana = 0;

		#endregion

		public int dpsRange {
			get { return HasGlyph (119853) ? 40 : 30; }
		}

		#region groups

		public string mapName { get { return API.MapInfo.Name; } }

		public bool MeIsInProvingGrounds {
			get {
				return mapName.Contains ("Proving Grounds");
			}
		}

		public HashSet<string> pgMembers = new HashSet<string> () {
			"Oto the Protector",
			"Sooli the Survivalist",
			"Kavan the Arcanist",
			"Ki the Assassin"
		};

		public IEnumerable<UnitObject> Tanks ()
		{
			if (MeIsInProvingGrounds) {
				return API.Units.Where (p => p.Name == "Oto the Protector");
			}
			return Group.GetGroupMemberObjects ().Where (p => p.IsTank).Distinct ();
		}

		public IEnumerable<UnitObject> PartyMembers ()
		{
			return !MeIsInProvingGrounds ? MyParty () : pgParty ();
		}

		public IEnumerable<UnitObject> MyParty ()
		{
			List<PlayerObject> list;
			list = Group.GetGroupMemberObjects ();
			list.Add (Me);
			return list.Distinct ();
		}

		public IEnumerable<UnitObject> pgParty ()
		{
			var list = new List<UnitObject> ();
			var t = API.Units.Where (p => p != null
			        && !p.IsDead
			        && p.IsValid).ToList ();
			if (t.Count () > 0) {
				foreach (var unit in t) {
					if (pgMembers.Contains (unit.Name)) {
						list.Add (unit);
					}
				}
			}
			list.Add (Me);
			return list.Distinct ();
		}

		#endregion

		#region health settings

		double cascadeHealthD = 0.85;
		double cascadeHealthR = 0.80;
		int cascadePlayersD = 2;
		int cascadePlayersR = 5;
		double haloHealthD = 0.85;
		double haloHealthR = 0.80;
		int haloPlayersD = 2;
		int haloPlayersR = 5;
		double healHealthD = 0.85;
		double healHealthR = 0.75;
		double PoMHealthD = 0.95;
		double PoMHealthR = 0.85;
		double PoHHealthD = 0.75;
		double PoHHealthR = 0.65;
		int PoHPlayersD = 3;
		int PoHPlayersR = 5;
		double holyNovaHealthD = 0.80;
		double holyNovaHealthR = 0.70;
		int holyNovaPlayersD = 2;
		int holyNovaPlayersR = 5;
		double PWSHealthD = 0.95;
		double PWSHhealthR = 0.85;
		double flashHealD = 0.70;
		double flashHealR = 0.50;
		double penanceHealthD = 0.90;
		double penanceHealthR = 0.80;
		double painSupHealth = 0.45;
		double desperatePrayerHealth = 0.35;
		double clarityOfWillHealth = 0.40;

		#endregion

		public override void Combat ()
		{
			if (Group.GetNumGroupMembers () == 0)
				SoloRotation ();
			if (Group.GetNumGroupMembers () < 6)
				DungeonHealing ();
			if (Group.GetNumGroupMembers () > 2)
				RaidHealing ();
		}

		public void SoloRotation ()
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
	}
}
