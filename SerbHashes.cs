using System.Collections.Generic;
using ReBot.API;

namespace ReBot
{
	public abstract class SerbHashes : CombatRotation
	{
		public HashSet<string> PgUnits = new HashSet<string> {
			"Oto the Protector",
			"Sooli the Survivalist",
			"Kavan the Arcanist",
			"Ki the Assassin"
		};

		public HashSet<string> BurstAura = new HashSet<string> {
			"Adrenaline Rush",
			"Vendetta",
			"Breath of Sindragosa",
			"Celestial Alignment",
			"Berserk",
			"Incarnation: Chosen of Elune",
			"Incarnation: King of the Jungle",
			"Incarnation: Son of Ursoc",
			"Bestial Wrath",
			"Frenzy",
			"Barrage",
			"Rapid Fire",
			"Serenity",
			"Avenging Wrath",
			"Hand of Protection"
		};

		public HashSet<string> DefAura = new HashSet<string> {
			
		};

		public HashSet<string> RangeDefAura = new HashSet<string> {
			"Zen Meditation"
		};

		public HashSet<string> MagicDefAura = new HashSet<string> {
			"Diffuse Magic",
			"Devotion Aura"
		};

		public HashSet<string> AoeSkill = new HashSet<string> {
			"Blizzard"
		};

		public HashSet<string> EnrageSkill = new HashSet<string> {
			"Adrenaline",
			"Angry",
			"Aspect of Thekal",
			"Beatdown",
			"Berserker Rage",
			"Berserking Howl",
			"Blackrock Rabies",
			"Blood Crazed",
			"Blood Rage",
			"Blood Scent",
			"Bloodletting Howl",
			"Bloody Rage",
			"Brewrific",
			"Brood Rage",
			"Burning Rage",
			"Charge Rage",
			"Charged Fists",
			"Combat Momentum",
			"Consuming Bite",
			"Cornered and Enraged!",
			"Delirious",
			"Demonic Enrage",
			"Desperate Rage",
			"Determination",
			"Dire Rage",
			"Dominate Slave",
			"Draconic Rage",
			"Electric Spur",
			"Enrage",
			"Enrage - Battle Pet",
			"Enrage Grublings",
			"Enrage Hatchling",
			"Enraged",
			"Enraged Assault",
			"Enraged Howl",
			"Enraged Mother",
			"Feed Pet",
			"Fel Ragamania",
			"Fellust",
			"Feral Instincts",
			"Ferocious Yell",
			"Ferocity",
			"Fervor",
			"Fixate",
			"Fixated",
			"Flip Out",
			"Flurry",
			"Flurry of Blows",
			"Frenzied Assault",
			"Frenzied Dive",
			"Frenzied for Blood",
			"Frenzied Leap",
			"Frenzy",
			"Frothing Rage",
			"Furious Anger",
			"Furious Roar",
			"Fury",
			"Getting Angry",
			"Hard Worker",
			"Held to Task",
			"Howl of Rage",
			"Hozen Rage",
			"Hunger For Blood",
			"Incite Frenzy",
			"Incite Rage",
			"Indomitable",
			"Iron Battle-Rage",
			"Kafa Rush",
			"Killing Rage",
			"Last of the Herd",
			"Laughing Skull",
			"Might of the Warsong",
			"Mighty!",
			"Mixture of Harnessed Rage",
			"Molten Fury",
			"Nimble Hands",
			"Overtime",
			"Primal Rage",
			"Protective Frenzy",
			"Protective Fury",
			"Protective Rage",
			"Rage",
			"Rage of Kros",
			"Rage of Ragnaros",
			"Rage of the Zandalari",
			"Rampage",
			"Reckless Zeal",
			"Red Frenzy",
			"Sadistic Frenzy",
			"Salivate",
			"Sara's Anger",
			"Savage Howl",
			"Seethe",
			"Sha Inspiration",
			"Shadowflame Axes",
			"Slaver's Rage",
			"Squealing Terror",
			"Stinger Rage",
			"Storm's Fury",
			"Swamp Rodent Surprise",
			"Titanic Strength",
			"Tormented Roar",
			"Tortured Enrage",
			"Treant's Rage",
			"Unflinching Resolve",
			"Unholy Frenzy",
			"Unholy Rage",
			"Unleashed Anger",
			"Unstable Strength",
			"Unstoppable Enrage",
			"Venomous Rage",
			"Victory Rush",
			"Warrior's Will",
			"Whip Rage",
			"Wild Beatdown",
			"Will of the Empress",
			"Wounded Pride",
			"Wrath of Ogudei"
		};

		public HashSet<string> SheepAura = new HashSet<string> {
			"Fear",
			"Polymorph",
			"Gouge",
			"Paralysis",
			"Blind",
			"Hex"
		};

	}
}

