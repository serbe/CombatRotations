using ReBot.API;

namespace ReBot
{
	public abstract class SerbWarrior : CombatRotation
	{
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;

		public double TimeToDie (UnitObject u = null)
		{
			u = u ?? Target;
			return u.Health / Ttd;
		}

		public bool IsBoss (UnitObject u = null)
		{
			u = u ?? Target;
			return(u.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || u.Level >= Me.Level + BossLevelIncrease;
		}

		public bool IsPlayer (UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsPlayer;
		}

		public bool IsElite (UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsElite ();
		}

		public int Rage {
			get {
				return Me.GetPower (WoWPowerType.Rage);
			}
		}

		public bool HasRage (int r)
		{
			if (Me.HasAura ("Spirits of the Lost"))
				r = r - 5;
			return r <= Rage;
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

		public double Range (UnitObject u = null)
		{
			u = u ?? Target;
			return u.CombatRange;
		}

		public bool Usable (string s)
		{ 
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public double Frac (string s)
		{
			string scurrentCharges = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" + s + "\"); return currentCharges";
			string smaxCharges = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" + s + "\"); return maxCharges";
			string scooldownStart = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" + s + "\"); return cooldownStart";
			string scooldownDuration = "local currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" + s + "\"); return cooldownDuration";

			double currentCharges = API.ExecuteLua<double> (scurrentCharges);
			double maxCharges = API.ExecuteLua<double> (smaxCharges);
			double cooldownStart = API.ExecuteLua<double> (scooldownStart);
			double cooldownDuration = API.ExecuteLua<double> (scooldownDuration);

			double f = currentCharges;

			if (f != maxCharges) {
				double currentTime = API.ExecuteLua<double> ("return GetTime()");
				f = f + ((currentTime - cooldownStart) / cooldownDuration);
			}

			return f;
		}

		public double DamageTaken ()
		{
			return API.ExecuteLua<double> ("local ResolveName = GetSpellInfo(158300);local n,_,_,_,_,dur,expi,_,_,_,id,_,_,_,val1,val2,val3 = UnitAura(\"player\", ResolveName, nil, \"HELPFUL\");return val2");
		}

		//		function f.UNIT_AURA(...)
		//			local n,_,_,_,_,dur,expi,_,_,_,id,_,_,_,val1,val2,val3 = UnitAura("player", ResolveName, nil, "HELPFUL");
		//			local ResolveValue,DamageTaken
		//
		//			ResolveValue = val1 or 0
		//			DamageTaken = val2 or 0
		//
		//			f.ResolveMax = ResolveValue > f.ResolveMax and ResolveValue or f.ResolveMax
		//			f.DamageMax = DamageTaken > f.DamageMax and DamageTaken or f.DamageMax
		//			tinsert(f.ResolveData,ResolveValue)
		//			tinsert(f.DamageData,DamageTaken)
		//
		//			f.StatusBar:SetValue(ResolveValue)
		//			if ResolveStatusDB.dmgtaken then
		//				f.StatusBar.Text:SetText(strformat("%s%% (%s)",ResolveValue,DamageTaken))
		//			else
		//				f.StatusBar.Text:SetText(strformat("%s%%",ResolveValue))
		//			end
		//		end
		//



		// ------- Spells

		public bool Charge (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Charge", () => Usable ("Charge") && u.IsInLoS && Range (u) >= 8 && (Range (u) <= 25 || (HasGlyph (58097) && Range (u) <= 30)), u);
		}

		public bool BloodFury ()
		{
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer () || EnemyInRange (10) > 2));
		}

		public bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer () || EnemyInRange (10) > 2));
		}

		public bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Target.IsInCombatRangeAndLoS && (IsElite () || IsPlayer () || EnemyInRange (10) > 2));
		}

		public bool BerserkerRage ()
		{
			return CastSelf ("Berserker Rage", () => Usable ("Berserker Rage") && Target.IsInLoS);
		}

		public bool ShieldBlock ()
		{
			return CastSelf ("Shield Block", () => Usable ("Shield Block") && HasRage (60));
		}

		public bool ShieldBarrier ()
		{
			return CastSelf ("Shield Barrier", () => Usable ("Shield Barrier") && HasRage (20));
		}

		public bool DemoralizingShout ()
		{
			return CastSelf ("Demoralizing Shout", () => Usable ("Demoralizing Shout"));
		}

		public bool EnragedRegeneration ()
		{
			return CastSelf ("Enraged Regeneration", () => Usable ("Enraged Regeneration"));
		}

		public bool ShieldWall ()
		{
			return CastSelf ("Shield Wall", () => Usable ("Shield Wall"));
		}

		public bool LastStand ()
		{
			return CastSelf ("Last Stand", () => Usable ("Last Stand"));
		}

		//		public bool DraenicArmor ()
		//		{
		//			return CastSelf ("Last Stand", () => Usable ("Last Stand"));
		//		}
	}
}

