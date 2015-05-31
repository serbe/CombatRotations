using System;
using Newtonsoft.Json;
using ReBot.API;
using Geometry;
using System.Collections.Generic;

namespace ReBot
{
	public abstract class SerbWarlock : SerbUtils
	{
		[JsonProperty ("Use multitarget")]
		public bool Multitarget = true;
		[JsonProperty ("AOE")]
		public bool Aoe = true;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;

		public double HandRange;
		public DateTime StartHandTime;
		public bool HandInFlight = false;


		// Check


		// Get

		public double SpellHaste {
			get {
				double haste = API.ExecuteLua<double> ("return GetCombatRating(CR_HASTE_SPELL);");
				if (haste == 0)
					haste = 0.00001;
				return haste;
			}
		}

		//		public double CastTimeSB {
		//			get {
		//				return API.ExecuteLua<double> ("local _, _, _, castTime, _, _ = GetSpellInfo(686); return castTime;");
		//			}
		//		}

		public double HandTravelTime {
			get {
				return Target.CombatRange * 2 / 40;
			}
		}

		public double HandFlightTime {
			get {
				TimeSpan HandTime = DateTime.Now.Subtract (StartHandTime);
				return HandTime.TotalSeconds;
			}
		}

		// Combo

		public bool SummonPet ()
		{
			//			if (Cast ("Felguard", () => !HasSpell ("Demonic Servitude") && (!HasSpell ("Grimoire of Supremacy") && (!HasSpell ("Grimoire of Service") || !Me.HasAura ("Grimoire of Service")))))
			//				return true;
			//			if (Cast ("Wrathguard", () => !HasSpell ("Demonic Servitude") && (HasSpell ("Grimoire of Supremacy") && (!HasSpell ("Grimoire of Service") || !Me.HasAura ("Grimoire of Service")))))
			//				return true;
			return false;
		}


		// Spell

		public bool DarkIntent ()
		{
			return Usable ("Dark Intent") && !Me.HasAura ("Dark Intent") && !Me.HasAura ("Mind Quickening") && !Me.HasAura ("Swiftblade's Cunning") && !Me.HasAura ("Windflurry") && !Me.HasAura ("Arcane Brilliance") && CS ("Dark Intent");
		}

		public bool MannorothsFury ()
		{
			return Usable ("Mannoroth's Fury") && Danger () && CS ("Mannoroth's Fury");
		}

		public bool DarkSoul ()
		{
			if (CastSelf ("Dark Soul: Misery", () => Usable ("Dark Soul: Misery") && Danger ()))
				return true; 
			if (CastSelf ("Dark Soul: Instability", () => Usable ("Dark Soul: Instability") && Danger ()))
				return true; 
			if (CastSelf ("Dark Soul: Knowledge", () => Usable ("Dark Soul: Knowledge") && Danger ()))
				return true; 
			return false;
		}

		public bool ImpSwarm (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Imp Swarm") && Danger () && C ("Imp Swarm", u);
		}

		public bool LifeTap ()
		{
			return Usable ("Life Tap") && Mana (Me) < 0.8 && Health (Me) > 0.3 && CS ("Life Tap");
		}

		public bool Felstorm (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Felstorm") && !Me.Pet.IsDead && Vector3.Distance (u.Position, Me.Pet.Position) <= 8 && C ("Felstorm", u);
		}

		public bool Wrathstorm (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Wrathstorm") && !Me.Pet.IsDead && Vector3.Distance (u.Position, Me.Pet.Position) <= 8 && C ("Wrathstorm", u);
		}

		public bool MortalCleave (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mortal Cleave") && !Me.Pet.IsDead && C ("Mortal Cleave", u);
		}

		public bool HandofGuldan (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Hand of Gul'dan") && Range (40, u) && C ("Hand of Gul'dan", u);
		}

		public bool GrimoireofService (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Grimoire of Service") && Range (40, u) && CS ("Grimoire of Service");
		}

		public bool SummonDoomguard (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Summon Doomguard") && Range (40, u) && DangerBoss (u) && C ("Summon Doomguard", u);
		}

		public bool SummonInfernal (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Summon Infernal") && Range (40, u) && u.Level >= Me.Level - 10 && C ("Summon Infernal", u);
		}

		public bool KiljaedensCunning ()
		{
			return Usable ("Kil'jaeden's Cunning") && CS ("Kil'jaeden's Cunning");
		}

		public bool Cataclysm (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Cataclysm", u.Position, () => Usable ("Cataclysm") && Range (40, u));
		}

		public bool ImmolationAura ()
		{
			return Usable ("Immolation Aura") && Me.HasAura ("Metamorphosis") && !Me.HasAura ("Immolation Aura") && C ("Immolation Aura");
		}

		public bool ShadowBolt (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shadow Bolt") && Range (40, u) && ((!Me.HasAura ("Metamorphosis") && Mana () >= 0.055) || (Me.HasAura ("Metamorphosis") && DemonicFury >= 40)) && C ("Shadow Bolt", u);
		}

		public bool Doom (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Doom") && Range (40, u) && Me.HasAura ("Metamorphosis") && DemonicFury >= 60 && C ("Doom", u);
		}

		public bool Corruption (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Corruption") && Range (40, u) && C ("Corruption", u);
		}

		public bool Metamorphosis ()
		{
			return Usable ("Metamorphosis") && CS ("Metamorphosis");
		}

		public bool TouchofChaos (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Touch of Chaos") && Range (40, u) && Me.HasAura ("Metamorphosis") && DemonicFury >= 40 && C ("Touch of Chaos", u);
		}


		public bool ChaosWave (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Chaos Wave") && Range (40, u) && Me.HasAura ("Metamorphosis") && DemonicFury >= 80 && C ("Chaos Wave", u);
		}

		public bool SoulFire (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Soul Fire") && Range (40, u) && !Me.IsMoving && C ("Soul Fire", u);
		}

		public bool Hellfire ()
		{
			return Usable ("Hellfire") && !Me.HasAura ("Metamorphosis") && !Me.HasAura ("Hellfire") && C ("Hellfire");
		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		// Items

	}
}

