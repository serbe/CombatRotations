using System;
using ReBot.API;
using Newtonsoft.Json;

namespace ReBot
{
	public abstract class SerbPriest : CombatRotation
	{
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int TTD = 10;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;

		public SerbPriest ()
		{
		}

		public bool Usable (string s)
		{ 
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public double TimeToDie (UnitObject u = Target)
		{
			return u.Health / TTD;
		}

		public bool IsBoss (UnitObject u = Target)
		{
			return(u.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || u.Level >= Me.Level + BossLevelIncrease;
		}



		// Spell

		public virtual bool BloodFury ()
		{
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && Target.IsInCombatRangeAndLoS && (Target.IsElite || Target.IsPlayer));
		}

		public virtual bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Target.IsInCombatRangeAndLoS && (Target.IsElite || Target.IsPlayer));
		}

		public virtual bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Target.IsInCombatRangeAndLoS && (Target.IsElite || Target.IsPlayer));
		}

		public virtual bool PowerInfusion ()
		{
			return CastSelf ("Power Infusion", () => Usable ("Power Infusion") && Target.IsInCombatRangeAndLoS && (Target.IsElite || Target.IsPlayer));
		}

		public bool PowerWordFortitude (UnitObject u = Me)
		{
			return Cast ("Power Word: Fortitude", u, () => Usable ("Power Word: Fortitude") && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool DraenicIntellect (UnitObject u = Target)
		{
			return API.HasItem (109218) && API.ItemCooldown (109218) <= 0 && API.UseItem (109218);
		}

		public bool Mindbender (UnitObject u = Target)
		{
			return Cast ("Mindbender", u, () => Usable ("Mindbender") && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool Shadowfiend (UnitObject u = Target)
		{
			return Cast ("Shadowfiend", u, () => Usable ("Shadowfiend") && u.IsInLoS && u.CombatRange <= 40 && (u.IsPlayer && u.IsElite));
		}

		public bool ShadowWordPain (UnitObject u = Target)
		{
			return Cast ("Shadow Word: Pain", u, () => Usable ("Shadow Word: Pain") && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool Penance (UnitObject u = Target)
		{
			return Cast ("Penance", u, () => Usable ("Penance") && u.IsInLoS && u.CombatRange <= 40 && (HasGlyph (119866) || !Me.IsMoving));
		}

		public bool PowerWordSolace (UnitObject u = Target)
		{
			return Cast ("Power Word: Solace", u, () => Usable ("Power Word: Solace") && u.IsInLoS && (u.CombatRange <= 30 || (HasGlyph (119853) && u.CombatRange <= 40)));
		}

		public bool HolyFire (UnitObject u = Target)
		{
			return Cast ("Holy Fire", u, () => Usable ("Holy Fire") && u.IsInLoS && (u.CombatRange <= 30 || (HasGlyph (119853) && u.CombatRange <= 40)));
		}

		public bool Smite (UnitObject u = Target)
		{
			return Cast ("Smite", u, () => Usable ("Smite") && u.IsInLoS && (u.CombatRange <= 30 || (HasGlyph (119853) && u.CombatRange <= 40)) && !Me.IsMoving);
		}

		public bool PowerWordShield (UnitObject u = Target)
		{
			return Cast ("Power Word: Shield", u, () => Usable ("Power Word: Shield") && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool FlashHeal (UnitObject u = Target)
		{
			return Cast ("Flash Heal", u, () => Usable ("Flash Heal") && u.IsInLoS && u.CombatRange <= 40 && (Me.HasAura ("Surge of Light") || !Me.IsMoving));
		}

		public bool Heal (UnitObject u = Target)
		{
			return Cast ("Heal", u, () => Usable ("Heal") && u.IsInLoS && u.CombatRange <= 40);
		}
		
		//		public bool Mindbender (UnitObject u = Target)
		//		{
		//			return Cast ("Mindbender", u, () => Usable ("Mindbender") && u.IsInLoS && u.CombatRange <= 40);
		//		}
		//
		//		public bool Mindbender (UnitObject u = Target)
		//		{
		//			return Cast ("Mindbender", u, () => Usable ("Mindbender") && u.IsInLoS && u.CombatRange <= 40);
		//		}
		//
		//		public bool Mindbender (UnitObject u = Target)
		//		{
		//			return Cast ("Mindbender", u, () => Usable ("Mindbender") && u.IsInLoS && u.CombatRange <= 40);
		//		}
		//
		//		public bool Mindbender (UnitObject u = Target)
		//		{
		//			return Cast ("Mindbender", u, () => Usable ("Mindbender") && u.IsInLoS && u.CombatRange <= 40);
		//		}

	}
}

