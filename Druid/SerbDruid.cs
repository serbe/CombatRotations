using System;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot
{
	public abstract class SerbDruid : SerbUtils
	{
		[JsonProperty ("Maximum Energy")] 
		public int EnergyMax = 100;
		[JsonProperty ("Healing Me %/100")]
		public double HealingMe = 0.8;
		[JsonProperty ("Healing Party %/100")]
		public double HealingParty = 0.8;
		[JsonProperty ("Run to enemy")]
		public bool Run;
		[JsonProperty ("Use multitarget")]
		public bool Multitarget = true;
		[JsonProperty ("AOE")]
		public bool Aoe = true;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("Use healing touch in combat")]
		public bool UseHealingTouch = true;

		// Check

		public bool HasEnergy (double i)
		{
			if (InCatForm && Me.HasAura ("Berserk"))
				i = Math.Floor (i / 2);
			if (CatForm () && Me.HasAura ("Clearcasting"))
				i = 0;
			return Energy >= i;
		}

		public bool HasEnergyB (double i)
		{
			if (InCatForm && Me.HasAura ("Berserk"))
				i = Math.Floor (i / 2);
			return Energy >= i;
		}

		public bool InCatForm {
			get {
				return IsInShapeshiftForm ("Cat Form");
			}
		}

		public bool InBearForm {
			get {
				return IsInShapeshiftForm ("Bear Form");
			}
		}

		// Combo

		public bool NoInvisible (UnitObject u = null)
		{
			u = u ?? Target;
			if (u.IsPlayer && (u.Class == WoWClass.Rogue || u.Class == WoWClass.Priest || u.Class == WoWClass.Mage) && !u.HasAura ("Faerie Swarm", true)) {
				if (FaerieSwarm (u))
					return true;
			}
			if (u.IsPlayer && (u.Class == WoWClass.Rogue || u.Class == WoWClass.Priest || u.Class == WoWClass.Mage) && !u.HasAura ("Faerie Fire", true)) {
				if (FaerieFire (u))
					return true;
			}
			return false;
		}

		public bool Interrupt ()
		{
			if (Usable ("Mighty Bash")) {
				if (ActiveEnemies (6) > 1 && Multitarget) {
					var Unit = Enemy.Where (x => Range (5, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && MightyBash (Unit))
						return true;
				} else {
					if (Target.IsCastingAndInterruptible () && Range (5) && Target.RemainingCastTime > 0) {
						if (MightyBash ())
							return true;
					}
				}
			}
			if (Usable ("Solar Beam")) {
				if (ActiveEnemies (40) > 1 && Multitarget) {
					var Unit = Enemy.Where (x => Range (40, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && SolarBeam (Unit))
						return true;
				} else {
					if (Target.IsCastingAndInterruptible () && Range (40) && Target.RemainingCastTime > 0) {
						if (SolarBeam ())
							return true;
					}
				}
			}
			if (Usable ("Skull Bash")) {
				if (ActiveEnemies (13) > 1 && Multitarget) {
					var Unit = Enemy.Where (x => Range (13, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && SkullBash (Unit))
						return true;
				} else {
					if (Target.IsCastingAndInterruptible () && Range (13) && Target.RemainingCastTime > 0) {
						if (SkullBash ())
							return true;
					}
				}
			}
			if (Usable ("Wild Charge")) {
				if (ActiveEnemies (25) > 1 && Multitarget) {
					var Unit = Enemy.Where (x => Range (25, x, 8) && x.IsCasting && !IsBoss (x) && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && WildCharge (Unit))
						return true;
				} else {
					if (Range (25, Target, 8) && Target.IsCasting && !IsBoss (Target) && Target.RemainingCastTime > 0) {
						if (WildCharge ())
							return true;
					}
				}
			}
			if (Usable ("Maim") && ComboPoints >= 3) {
				if (ActiveEnemies (6) > 1 && Multitarget) {
					var Unit = Enemy.Where (x => Range (5, x) && x.IsCasting && !IsBoss (x) && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						if (Maim (Unit))
							return true;
					} 
				} else {
					if (Range (5) && Target.IsCasting && !IsBoss (Target) && Target.RemainingCastTime > 0) {
						if (Maim ())
							return true;
					}
				}
			}

//			if (Cast ("Incapacitating Roar", () => Target.IsCastingAndInterruptible ()))
//				return; // 30 sec Cooldown

			if (Cast ("Typhoon", () => Target.IsCastingAndInterruptible ()))
				return true; // 30 sec Cooldown

			if (InBearForm && Usable ("Faerie Fire") && HasGlyph (114237)) {
				if (ActiveEnemies (35) > 1 && Multitarget) {
					var Unit = Enemy.Where (x => Range (35, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						if (FaerieFire (Unit))
							return true;
					} 
				} else {
					if (Range (35) && Target.IsCastingAndInterruptible () && Target.RemainingCastTime > 0) {
						if (FaerieFire ())
							return true;
					}
				}
			}

			return false;
		}

		public bool RunToTarget (UnitObject u = null)
		{
			u = u ?? Target;
			if (InCatForm && !Me.HasAura ("Prowl") && u.CombatRange >= 20 && u.IsFleeing) {
				if (Dash ())
					return true;
			}
			// // if (CastSelfPreventDouble("Stealth", () => !Me.InCombat && !Me.HasAura("Stealth"))) return;
			// if (Cast("Shadowstep", () => !Me.HasAura("Sprint") && HasSpell("Shadowstep"))) return;
			// // if (CastSelf("Sprint", () => !Me.HasAura("Sprint") && !Me.HasAura("Burst of Speed"))) return;
			// // if (CastSelf("Burst of Speed", () => !Me.HasAura("Sprint") && !Me.HasAura("Burst of Speed") && HasSpell("Burst of Speed") && Energy > 20)) return;
			// if (Cast(RangedAtk, () => Energy >= 40 && !Me.HasAura("Stealth") && Target.IsInLoS)) return;
			return false;
		}

		// public virtual bool UnEnrage() {
		// 	var targets = Adds;
		// 	targets.Add(Target);

		// 	if (HasSpell("Shiv") && Cooldown("Shiv") == 0 && HasCost(20)) {
		// 		if (ActiveEnemies(6) > 1 && Multitarget) {
		// 			Unit = Enemy.Where(x => x.IsInCombatRangeAndLoS && IsInEnrage(x) && !IsBoss(x)).DefaultIfEmpty(null).FirstOrDefault();
		// 			if (Cast("Shiv", Unit, () => Unit != null)) return true;
		// 		} else
		// 			if (Cast("Shiv", () => !IsBoss(Target) && IsInEnrage(Target) && !IsBoss(Target))) return true;
		// 	}
		// 	return false;
		// }

		public bool HealPartyMember ()
		{
			var Unit = MyGroup.Where (x => !x.IsDead && Range (40, x) && Health (x) <= HealingParty && !x.HasAura ("Rejuvenation", true)).DefaultIfEmpty (null).FirstOrDefault ();
			if (Unit != null && Rejuvenation (Unit, true))
				return true;
			if (Me.HasAura ("Predatory Swiftness")) {
				Unit = MyGroup.Where (x => !x.IsDead && Range (40, x) && Health (x) <= HealingParty && Health (x) < Health (Me)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && HealingTouch (Unit, true))
					return true;
			}
			if (InArena && UseHealingTouch && !Me.IsMoving && GroupMemberCount > 1) {
				if (EnemyPlayerTargetToMe == null && LowestPlayer != null && Health (LowestPlayer) < 0.5 && HealingTouch (LowestPlayer))
					return true;
			}
			return false;
		}

		public bool Heal ()
		{
			if (Health (Me) < 0.4) {
				if (EternalWilloftheMartyr ())
					return true;
			}
			if (Health (Me) < 0.45) {
				if (Healthstone ())
					return true;
			}
			if (Health (Me) < 0.5) {
				if (SurvivalInstincts ())
					return true;	
			}
			if (Health (Me) < 0.6) {
				if (Barkskin ())
					return true;
			}
			if (Me.HasAura ("Predatory Swiftness") && Health (Me) < HealingMe) {
				if (HealingTouch (Me))
					return true;
			}
			if (Health (Me) <= HealingMe) {
				if (CenarionWard (Me))
					return true;
			}
			if (Health (Me) <= HealingMe && !Me.HasAura ("Rejuvenation", true)) {
				if (Rejuvenation (Me))
					return true;
			}

			return false;
		}

		public bool HealTank ()
		{
			if (HasSpell ("Dream of Cenarius") && Me.HasAura ("Dream of Cenarius") && Health (Me) < 0.85) {
				if (HealingTouch (Me))
					return true;
			}
			if (InBearForm && Rage > 60 && Health (Me) < 0.4) {
				if (FrenziedRegeneration ())
					return true;
			}
			if (Rage > 60 && !Me.HasAura ("Savage Defense") && Health (Me) < 0.45) {
				if (SavageDefense ())
					return true;
			}
			if (!Me.HasAura ("Survival Instincts") && Health (Me) < 0.25) {
				if (SurvivalInstincts ())
					return true;
			}
			if (Health (Me) < 0.65) {
				if (Barkskin ())
					return true;
			}
			if (!Me.HasAura ("Cenarion Ward") && Health (Me) < 0.8) {
				if (CenarionWard (Me))
					return true;
			}
			if (HasSpell ("Bristling Fur") && Health (Me) < 0.3) {
				if (BristlingFur ())
					return true;
			}
			if (Health (Me) < 0.3)
				Renewal ();
			if (Health (Me) < 0.5)
				HeartoftheWild ();

			return false;
		}

		public bool UnEnrage ()
		{
			if (InArena && Usable ("Soothe")) {
				if (ActiveEnemies (40) > 1) {
					var Unit = Enemy.Where (u => Range (40, u) && IsInEnrage (u)).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Soothe (Unit))
						return true;
				} else if (IsInEnrage ()) {
					if (Soothe ())
						return true;
				}
			}
			return false;
		}

		// Spells

		public bool Soothe (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Soothe") && Range (40, u) && C ("Soothe", u);
		}

		public bool CatForm ()
		{
			return !InCatForm && CS ("Cat Form");
		}

		public bool BearForm ()
		{
			return !InBearForm && CS ("Bear Form");
		}

		public bool MightyBash (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mighty Bash") && Range (5, u) && C ("Mighty Bash", u);
		}

		public bool SolarBeam (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Solar Beam") && Range (40, u) && C ("Solar Beam", u);
		}

		public bool MarkoftheWild (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mark of the Wild") && u.AuraTimeRemaining ("Mark of the Wild") < 300 && u.AuraTimeRemaining ("Blessing of Kings") < 300 && u.AuraTimeRemaining ("Legacy of the Emperor") < 300 && Range (40, u) && C ("Mark of the Wild", u);
		}

		public bool Rejuvenation (UnitObject u = null, bool t = false)
		{
			u = u ?? Target;
			return Usable ("Rejuvenation") && !u.HasAura ("Rejuvenation", true) && Range (40, u) && C ("Rejuvenation", u, t);
		}

		public bool HealingTouch (UnitObject u = null, bool t = false)
		{
			u = u ?? Target;
			return Usable ("Healing Touch") && Range (40, u) && C ("Healing Touch", u, t);
		}

		public bool RemoveCorruption (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Remove Corruption") && Range (40, u) && C ("Remove Corruption", u);
		}

		public bool FerociousBite (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Ferocious Bite") && HasEnergy (25) && ComboPoints > 0 && Range (5, u) && C ("Ferocious Bite", u);
		}

		public bool CenarionWard (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Cenarion Ward") && !u.HasAura ("Cenarion Ward") && Range (40, u) && C ("Cenarion Ward", u);
		}

		public bool Barkskin ()
		{
			return Usable ("Barkskin") && Health (Me) < 0.9 && CS ("Barkskin");
		}

		public bool BristlingFur ()
		{
			return Usable ("Bristling Fur") && CS ("Bristling Fur");
		}

		public bool MoonkinForm ()
		{
			return Usable ("Moonkin Form") && !Me.HasAura ("Moonkin Form") && CS ("Moonkin Form");
		}

		public bool FaerieSwarm (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Faerie Swarm") && Range (35, u) && C ("Faerie Swarm", u);
		}

		public bool FaerieFire (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Faerie Fire") && Range (35, u) && C ("Faerie Fire", u);
		}

		public bool SkullBash (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Skull Bash") && Range (13, u) && C ("Skull Bash", u);
		}

		public bool WildCharge (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Wild Charge") && Range (25, u, 5) && C ("Wild Charge", u);
		}

		public bool Starfall (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Starfall") && Range (40) && C ("Starfall", u);
		}

		public bool BloodFury ()
		{
			return Usable ("Blood Fury") && Danger () && CS ("Blood Fury");
		}

		public bool Berserking ()
		{
			return Usable ("Berserking") && Danger () && CS ("Berserking");
		}

		public bool ArcaneTorrent ()
		{
			return Usable ("Arcane Torrent") && Danger () && CS ("Arcane Torrent");
		}

		public bool ForceofNature (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Force of Nature") && Range (40) && C ("Force of Nature", u);
		}

		public bool Starsurge (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Starsurge") && Range (40, u) && (!Me.IsMoving || Me.HasAura ("Empowered Moonkin") || HasSpell ("Enhanced Starsurge")) && C ("Starsurge", u);
		}

		public bool IncarnationChosenofElune ()
		{
			return Usable ("Incarnation: Chosen of Elune") && Danger () && CS ("Incarnation: Chosen of Elune");
		}

		public bool Sunfire (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Sunfire") && Eclipse > 0 && Range (40, u) && C ("Sunfire", u);
		}

		public bool StellarFlare (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Stellar Flare") && Range (40, u) && (!Me.IsMoving || Me.HasAura ("Empowered Moonkin")) && C ("Stellar Flare", u);
		}

		public bool Moonfire (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Moonfire") && Range (40, u) && C ("Moonfire", u);
		}

		public bool Wrath (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Wrath") && Range (40, u) && (!Me.IsMoving || Me.HasAura ("Empowered Moonkin")) && C ("Wrath", u);
		}

		public bool Starfire (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Starfire") && Range (40, u) && (!Me.IsMoving || Me.HasAura ("Elune's Wrath") || Me.HasAura ("Empowered Moonkin")) && C ("Starfire", u);
		}

		public bool Maim (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Maim") && HasEnergy (35) && ComboPoints > 0 && Range (5, u) && C ("Maim", u);
		}

		public bool Berserk ()
		{
			return Usable ("Berserk") && Danger () && CS ("Berserk");
		}

		public bool TigersFury (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Tiger's Fury") && Range (10, u) && CS ("Tiger's Fury");
		}

		public bool IncarnationKingoftheJungle ()
		{
			return Usable ("Incarnation: King of the Jungle") && Danger () && CS ("Incarnation: King of the Jungle");
		}

		public bool Shadowmeld ()
		{
			return Usable ("Shadowmeld") && !Me.IsMoving && CS ("Shadowmeld");
		}

		public bool CelestialAlignment ()
		{
			return Usable ("Celestial Alignment") && Danger () && CS ("Celestial Alignment");
		}

		public bool Rake (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Rake") && HasEnergy (35) && u.IsInCombatRangeAndLoS && C ("Rake", u);
		}

		public bool SavageRoar ()
		{
			return Usable ("Savage Roar") && HasEnergyB (25) && Range (20) && ComboPoints > 0 && CS ("Savage Roar");
		}

		public bool Rip (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Rip") && HasEnergy (30) && ComboPoints > 0 && u.IsInCombatRangeAndLoS && C ("Rip", u);
		}

		public bool SavageDefense (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Savage Defense") && Rage > 60 && Range (20, u) && CS ("Savage Defense");
		}

		public bool Swipe (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Swipe") && HasEnergy (45) && C ("Swipe", u);
		}

		public bool Shred (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shred") && HasEnergy (40) && Range (5, u) && C ("Shred", u);
		}

		public bool Thrash (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Thrash") && ((InCatForm && HasEnergy (50))) && Range (10, u) && C ("Thrash", u);
		}

		public bool SurvivalInstincts ()
		{
			return Usable ("Survival Instincts") && CS ("Survival Instincts");
		}

		public bool Dash ()
		{
			return Usable ("Dash") && CS ("Dash");
		}


		public bool Maul (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Maul") && (Rage >= 20 || (Rage >= 10 && Me.HasAura ("Tooth and Claw"))) && Range (5, u) && C ("Maul", u);
		}

		public bool Pulverize (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Pulverize") && Range (5, u) && C ("Pulverize", u);
		}

		public bool Lacerate (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Lacerate") && Range (5, u) && C ("Lacerate", u);
		}

		public bool Mangle (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mangle") && Range (5, u) && C ("Mangle", u);
		}

		public bool FrenziedRegeneration ()
		{
			return Usable ("Frenzied Regeneration") && CS ("Frenzied Regeneration");
		}

		public bool Renewal ()
		{
			return Usable ("Renewal") && CS ("Renewal");
		}

		public bool HeartoftheWild ()
		{
			return Usable ("Heart of the Wild") && DangerBoss () && CS ("Heart of the Wild");
		}

		public bool IncarnationSonofUrsoc ()
		{
			return Usable ("Incarnation: Son of Ursoc") && DangerBoss () && CS ("Incarnation: Son of Ursoc");
		}

		public bool NaturesVigil ()
		{
			return Usable ("Nature's Vigil") && CS ("Nature's Vigil");
		}

	}
}
