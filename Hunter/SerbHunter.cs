using System;
using System.Linq;
using Geometry;
using Newtonsoft.Json;
using ReBot.API;
using System.Collections.Generic;

namespace ReBot
{
	public abstract class SerbHunter : SerbUtils
	{
		// Vars

		public enum ExoticMunitionsType
		{
			NoExoticMunitions,
			PoisonedAmmo,
			IncendiaryAmmo,
			FrozenAmmo,
		}

		public enum PetSlot
		{
			PetSlot1,
			PetSlot2,
			PetSlot3,
			PetSlot4,
			PetSlot5,
		}

		public enum UsePet
		{
			UsePet,
			NoPet,
			LoneWolfCrit,
			LoneWolfMastery,
			LoneWolfHaste,
			LoneWolfStats,
			LoneWolfStamina,
			LoneWolfMultistrike,
			LoneWolfVersatility,
			LoneWolfSpellpower,
		}

		[JsonProperty ("Run to enemy")]
		public bool Run;
		[JsonProperty ("Use multitarget")]
		public bool Multitarget = true;
		[JsonProperty ("AOE")]
		public bool Aoe = true;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;

		// Check

		public bool IsDispeling (UnitObject o)
		{
			bool result1 = false;
			bool result2 = false;
			if (o.HasAura ("Freezing Fog") || o.HasAura ("Mark of the Wild") || o.HasAura ("Rejuvenation") || o.HasAura ("Predator's Swiftness") || o.HasAura ("Nature's Swiftness") || o.HasAura ("Arcane Intellect") || o.HasAura ("Ice Barrier") || o.HasAura ("Earth Shield") || o.HasAura ("Spiritwalker's Grace") || o.HasAura ("Dark Intent") || o.HasAura ("Alter Time") || o.HasAura ("Arcane Power") || o.HasAura ("Presence of Mind") || o.HasAura ("Brain Freeze") || o.HasAura ("Icy Veins") || o.HasAura ("Hand of Protection") || o.HasAura ("Hand of Freedom") || o.HasAura ("Hand of Sacrifice") || o.HasAura ("Blessing of Might") || o.HasAura ("Eternal Flame") || o.HasAura ("Selfless Healer"))
				result1 = true;
			if (o.HasAura ("Execution Sentence") || o.HasAura ("Hand of Purity") || o.HasAura ("Speed of Light") || o.HasAura ("Long Arm of the Law") || o.HasAura ("Illuminated Healing") || o.HasAura ("Power Word: Shield") || o.HasAura ("Power Word: Fortitude") || o.HasAura ("Fear Ward") || o.HasAura ("Prayer of Mending") || o.HasAura ("Power Infusion") || o.HasAura ("Angelic Feather") || o.HasAura ("Body and Soul") || o.HasAura ("Borrowed Time") || o.HasAura ("Unleash Fury") || o.HasAura ("Elemental Overload") || o.HasAura ("Ancestral Swiftness"))
				result2 = true;
			return result1 || result2;
		}

		public bool HasFocus (double i)
		{
			if (Me.HasAura ("Burning Adrenaline"))
				i = 0;
			return Focus >= i;
		}

		// Multi-Shot and Aimed Shot
		public bool HasAmFocus (double i)
		{
			if (Me.HasAura ("Multi-Shot"))
				i = i - 20;
			if (Me.HasAura ("Thrill of the Hunt"))
				i = i - 20;
			if (Me.HasAura ("Burning Adrenaline"))
				i = 0;
			return Focus >= i;
		}

		// Arcane Shot
		public bool HasArcaneFocus (double i)
		{
			if (Me.HasAura ("Thrill of the Hunt"))
				i = i - 20;
			if (Me.HasAura ("Burning Adrenaline"))
				i = 0;
			return Focus >= i;
		}
	
		// Getters

		// Combo

		public bool ExoticMunitions (ExoticMunitionsType e)
		{
			if (e == ExoticMunitionsType.PoisonedAmmo) {
				return Usable ("Poisoned Ammo") && !Me.HasAura ("Poisoned Ammo") && CSPD ("Poisoned Ammo");
			}
			if (e == ExoticMunitionsType.IncendiaryAmmo) {
				return Usable ("Incendiary Ammo") && !Me.HasAura ("Incendiary Ammo") && CSPD ("Incendiary Ammo");
			}
			if (e == ExoticMunitionsType.FrozenAmmo) {
				return Usable ("Frozen Ammo") && !Me.HasAura ("Frozen Ammo") && CSPD ("Frozen Ammo");
			}
			return false;
		}

		public bool SummonPet (PetSlot s)
		{
			if (CSPD ("Revive Pet", 5000))
				return true;
			if (s == PetSlot.PetSlot1 && CSPD ("Call Pet 1", 5000))
				return true;
			if (s == PetSlot.PetSlot2 && CSPD ("Call Pet 2", 5000))
				return true;
			if (s == PetSlot.PetSlot3 && CSPD ("Call Pet 3", 5000))
				return true;
			if (s == PetSlot.PetSlot4 && CSPD ("Call Pet 4", 5000))
				return true;
			if (s == PetSlot.PetSlot5 && CSPD ("Call Pet 5", 5000))
				return true;
			return false;
		}

		public bool LoneWolf (UsePet p)
		{
			if (Me.HasAlivePet) {
				Me.PetDismiss ();
				return true;
			}
			if (p == UsePet.LoneWolfCrit && !Me.HasAura ("Lone Wolf: Ferocity of the Raptor") && CSPD ("Lone Wolf: Ferocity of the Raptor", 1500))
				return true;
			if (p == UsePet.LoneWolfMastery && !Me.HasAura ("Lone Wolf: Grace of the Cat") && CSPD ("Lone Wolf: Grace of the Cat", 1500))
				return true;
			if (p == UsePet.LoneWolfHaste && !Me.HasAura ("Lone Wolf: Haste of the Hyena") && CSPD ("Lone Wolf: Haste of the Hyena", 1500))
				return true;
			if (p == UsePet.LoneWolfStats && !Me.HasAura ("Lone Wolf: Power of the Primates") && CSPD ("Lone Wolf: Power of the Primates", 1500))
				return true;
			if (p == UsePet.LoneWolfStamina && !Me.HasAura ("Lone Wolf: Fortitude of the Bear") && CSPD ("Lone Wolf: Fortitude of the Bear", 1500))
				return true;
			if (p == UsePet.LoneWolfMultistrike && !Me.HasAura ("Lone Wolf: Quickness of the Dragonhawk") && CSPD ("Lone Wolf: Quickness of the Dragonhawk", 1500))
				return true;
			if (p == UsePet.LoneWolfVersatility && !Me.HasAura ("Lone Wolf: Versatility of the Ravager") && CSPD ("Lone Wolf: Versatility of the Ravager", 1500))
				return true;
			if (p == UsePet.LoneWolfSpellpower && !Me.HasAura ("Lone Wolf: Wisdom of the Serpent") && CSPD ("Lone Wolf: Wisdom of the Serpent", 1500))
				return true;
			return false;
		}

		public bool Tranquilizing ()
		{
			if (Usable ("Tranquilizing Shot")) {
				if (InArena || InBg) {
					Unit = Enemy.Where (x => x.IsInCombatRangeAndLoS && x.IsPlayer && x.Auras.Any (a => a.IsStealable)).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && TranquilizingShot (Unit))
						return true;
				} else {
					Unit = Enemy.Where (x => x.IsInCombatRangeAndLoS && x.Auras.Any (a => a.IsStealable)).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && TranquilizingShot (Unit))
						return true;
				}
			}
			return false;
		}

		public bool UseMisdirection ()
		{
			if (Usable ("Misdirection") && Me.HasAlivePet) {
				if (InInstance || InRaid) {
					Player = MyGroup.Where (u => !u.IsDead && Range (100, u) && u.IsTank).DefaultIfEmpty (null).FirstOrDefault ();
					if (Player != null && Misdirection (Player))
						return true;
				}
				if (Me.Focus != null && Misdirection (Me.Focus))
					return true;
				if (HasGlyph (56829) && Misdirection (Me.Pet, 8000))
					return true;
				if (Misdirection (Me.Pet))
					return true;
			}
			return false;
		}

		public bool Interrupt ()
		{
			if (Usable ("Counter Shot")) {
				Unit = Enemy.Where (x => x.IsInCombatRangeAndLoS && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && CounterShot (Unit))
					return true;
			}
			return false;
		}

		// Spells

		public bool TrapLauncher ()
		{
			return Usable ("Trap Launcher") && CS ("Trap Launcher");
		}

		public bool ConcussiveShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Concussive Shot") && !u.HasAura ("Concussive Shot") && Range (40, u) && C ("Concussive Shot", u);
		}

		public bool CounterShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Counter Shot") && Range (40, u) && C ("Counter Shot", u);
		}

		public bool MendPet ()
		{
			return Usable ("Mend Pet") && Me.HasAlivePet && Me.Pet.HasAura ("Mend Pet") && Me.Pet.CombatRange <= 45 && C ("Mend Pet");
		}

		public bool TranquilizingShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Tranquilizing Shot") && (HasGlyph (119384) || HasFocus (50)) && Range (40, u) && C ("Tranquilizing Shot", u);
		}

		public bool BindingShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Binding Shot") && Range (30, u) && COT ("Binding Shot", u);
		}

		public bool FreezingTrap (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Freezing Trap") && Me.HasAura ("Trap Launcher") && Range (40, u) && COT ("Freezing Trap", u);
		}

		public bool MastersCall ()
		{
			return Usable ("Master's Call") && Me.HasAlivePet && Me.Pet.CombatRange <= 40 && CS ("Master's Call");
		}

		public bool LastStand ()
		{
			return Usable ("Last Stand") && Me.HasAlivePet && CS ("Last Stand");
		}

		public bool RoarofSacrifice ()
		{
			return Usable ("Roar of Sacrifice") && Me.HasAlivePet && Me.Pet.CombatRange <= 40 && CS ("Roar of Sacrifice");
		}

		public bool Exhilaration ()
		{
			return Usable ("Exhilaration") && C ("Exhilaration");
		}

		public bool Deterrence ()
		{
			return Usable ("Deterrence") && C ("Deterrence");
		}

		public bool FeignDeath ()
		{
			return Usable ("Feign Death") && !Me.HasAura ("Feign Death") && C ("Feign Death");
		}

		public bool Stampede (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Stampede") && Danger (u) && C ("Stampede", u);
		}

		public bool DireBeast ()
		{
			return Usable ("Dire Beast") && C ("Dire Beast");
		}

		public bool FocusFire ()
		{
			return Usable ("Focus Fire") && C ("Focus Fire");
		}

		public bool BestialWrath ()
		{
			return Usable ("Bestial Wrath") && C ("Bestial Wrath");
		}

		public bool MultiShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Multi-Shot") && HasAmFocus (40) && Range (40, u) && C ("Multi-Shot", u);
		}

		public bool Barrage (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Barrage") && Focus >= 60 && Range (40, u) && C ("Barrage", u);
		}

		public bool ExplosiveTrap (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Explosive Trap") && Range (40, u) && COT ("Explosive Trap", u);
		}

		public bool KillCommand (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Kill Command") && Me.HasAlivePet && HasFocus (40) && Vector3.Distance (Me.Pet.Position, u.Position) <= 25 && C ("Kill Command", u);
		}

		public bool AMurderofCrows (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("A Murder of Crows") && Focus >= 30 && Range (40, u) && C ("A Murder of Crows", u);
		}

		public bool KillShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Kill Shot") && (Health (u) < 0.20 || (HasSpell ("Enhanced Kill Shot") && Health (u) < 0.35)) && Range (40, u) && C ("Kill Shot", u);
		}

		public bool FocusingShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Focusing Shot") && !Me.IsMoving && Range (40, u) && C ("Focusing Shot", u);
		}

		public bool CobraShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Cobra Shot") && Range (40, u) && C ("Cobra Shot", u);
		}

		public bool GlaiveToss (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Glaive Toss") && Focus >= 15 && Range (40, u) && C ("Glaive Toss", u);
		}

		public bool Powershot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Powershot") && Focus >= 15 && Range (40, u) && C ("Powershot", u);
		}

		public bool ArcaneShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Arcane Shot") && HasArcaneFocus (30) && Range (40, u) && C ("Arcane Shot", u);
		}

		public bool Misdirection (UnitObject u = null, int d = 800)
		{
			u = u ?? Me.Pet;
			return Usable ("Misdirection") && Range (100, u) && CPD ("Misdirection", u, d);
		}
	}
}

