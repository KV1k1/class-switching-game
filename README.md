# RACE SWAP: INFINITE

*A dark-fantasy single-player action roguelite about a world after
Judgment, two weapons, and the souls worth bringing home.*

> **Status:** Early concept / prototype\
> **Genre:** Action roguelite / horde survival / dungeon crawler\
> **Working title:** The Last Testament\
> **Camera:** Already set in the current prototype\
> **Controls:** Players can switch between two control schemes in the main menu or pause panel settings (in both schemes, skills aim where the mouse cursor is hovered):
> - **Scheme 1 (Click-to-Move):** Click Left Mouse Button to move to cursor, Hold Left Mouse Button to continuously move, Right Click to auto-attack, `1`, `2`, `3` for skills, `Q` for class switching.
> - **Scheme 2 (WASD):** `WASD` movement, Left Click to auto-attack, `1`, `2`, `3` for skills, `Q` for class switching.

## 1. High concept

The world has ended. In the aftermath of a failed or incomplete
Judgment, the boundaries between Heaven, Hell, nature, and humanity's
stories have collapsed. Celestial forces are corrupted, familiar places
are hostile, and monsters rule the remains of creation.

The player is **The Witness**, the last human vessel carrying humanity's
collective memories. They remember a world that no longer exists and
confront the entities responsible for its ruin. Through corrupted
regions shaped by cosmic forces, broken myths, and twisted fairytales,
the Witness defeats bosses, discovers weapons, rescues survivors, and
rebuilds a sanctuary.

The central question: can the world be restored---and should it be?

## 2. Core pillars

-   **Fluid multi-weapon combat:** Equip up to three weapon-classes and switch
    instantly. Combo skills reward timing.
-   **Horde survival:** Fight waves, survive sudden rushes, gain XP, and
    prepare for timed bosses.
-   **Meaningful weapon discoveries:** Boss drops can fill open slots or replace equipped
    weapons to shape the run's arsenal.
-   **Run builds:** Level-ups offer passive cards that meaningfully
    alter combat, stats, and playstyle.
-   **Growing hub:** Rescued NPCs return to a sanctuary, offering quests
    or useful services.
-   **Permanent progression:** Quests, resources, player and weapon
    skill trees, weapon unlocks, and regional changes make completed
    runs matter.
-   **Dark interconnected storytelling:** Cosmic horror, failed
    Judgment, and broken fairytales.

## 3. World and narrative

### The aftermath of Judgment

Judgment came, failed, or was interrupted. Heaven and Hell are no longer
separate; nature has been twisted by forces that once governed the
cosmos. Celestial bodies may be corrupted entities or godlike forces
shaping regions, monsters, and bosses.

The setting combines:
- **Cosmic apocalypse:** corrupted celestial powers and world-ending entities.
- **Failed Judgment:** broken divine order, fallen angels, demons, and unanswered questions.
- **Broken fairytales:** recognizable stories transformed into unsettling characters and questlines.

### The Witness

The protagonist is not an amnesiac. They carry memories of people,
places, relationships, wars, and lives they never personally
experienced. Their goal is to confront the forces that destroyed the
world and pursue its restoration. The story gradually complicates what
restoration means---and who or what the Witness really is.

### Tone

Gothic, eerie, grotesque, and edgy without relying on darkness for its
own sake. Familiar fairytale and religious motifs are made strange. The
overarching mystery is serious, but individual characters and quests
should be memorable. The player's immediate objective should remain
understandable even when the larger truth is unclear.

## 4. Combat and class system

### Weapons are classes

Each weapon is a distinct combat class with its own attacks, skills,
strengths, and progression. Examples:
- **Warrior:** heavy attacks, slashes, stagger, fast sword.
- **Gunner:** ranged damage, precision, firing patterns, area denial.
- **Mage:** spells, area damage, crowd control.
- **Bloodstalker:** high damage, sacrifices health, squishy, high skill ceiling.
- **Executioner:** axe, throwing knife, mobility, fast combos, precision, burst.
- **Creeper:** sweeping attacks, execution, sustain.
- **Judge:** chains, pulling, grouping, space control.
- **Souleater:** curses, summons, delayed effects.
- **Holy Blade:** radiant damage, protection, anti-corruption.

The roster and identities are open for design.

### Instant switching

-   Switch between equipped classes instantly with **Q**.
-   Each class has independent skill cooldowns.
-   Skills remain available on their own cooldowns regardless of the
    active weapon.
-   Switching is a core combat action, not a menu interaction.

### Switch-combo skills

Some abilities transition into a combo skill when the player switches
weapons during their animation. For example, switching during Warrior
Skill 2 can trigger a combo transition into the other class.

Switch combos reward timing and encourage chaining skills between
classes. The prototype currently supports Warrior/Gunner Skill 2
switching; the intended system has combo interactions between class
pairings where appropriate.

### Starting classes

New players begin with **Warrior + Gunner**, each with three skills.
This loadout introduces combat and switching before players discover
other classes. Permanent progression may eventually let players replace
one starter with a class such as Bloodstalker.

### Boss weapon drops

Bosses drop weapons that introduce classes or variants. The player can
equip the drop into an open slot, replace an equipped weapon, or leave the drop. A dropped weapon remains
on the ground for **2 minutes** and can be picked up again during that
window.

Decisions should consider level, rarity, current skill investment, class
synergy, upcoming encounters, and build role. A drop should be a
meaningful choice, not an automatic upgrade.

## 5. Run weapon slots

The baseline run progression supports **three equipped class slots**.

- **Start of run:** The player starts with **two equipped classes** (Warrior and Gunner), leaving the third slot open.
- **First boss:** The first boss drops a third weapon/class (Mage in the initial progression), which the player picks up to fill the **third slot**.
- **After acquiring the third class:** All three slots are filled, and the player can carry and freely switch between all three equipped classes for the remainder of the run.
- **Full slots & replacing classes:** Once all three slots are filled, picking up a new class/weapon drop requires dropping and replacing one of the currently equipped classes (or leaving the new drop behind).

Only one weapon of a given class can be equipped at once. If a boss drops a weapon for a class the player already has equipped, it replaces that class's current weapon rather than occupying another slot.

Potential run passives can modify this baseline:
- **+1 slot:** increases the run's class capacity from **three to four**.
- **Random Legendary:** grants a random Legendary weapon but reduces the number of equipped class slots by one.

These effects may interact; final rules require prototyping. The +1 slot passive is an optional upgrade, not a prerequisite for reaching three classes.

## 6. Timed bosses and enemy rushes

Runs feature horde survival with escalating pressure:
- Enemies spawn throughout the map.
- Sudden rushes create short bursts of danger.
- Kills grant XP and contribute to the run build.
- Timed bosses arrive at designated intervals.
- Bosses test the current build and provide weapon drops and other rewards.
- Exploration/building periods are punctuated by intense rushes and boss encounters.

Timing, difficulty scaling, and event variety will be tuned through
playtesting.

## 7. Runs and exploration

A run is an expedition into a region. The player enters with two
weapons (and three total class slots), fights hordes, explores, completes objectives, and pursues
rewards.

Motivations include:
- XP and temporary builds.
- Boss weapon discovery.
- NPC discovery and recruitment.
- Resources and currency.
- Secrets, events, routes, and exploration.
- Regional story and quest progression.

Possible activities: **Expedition** (explore/resources), **Hunt**
(specific elite or boss), **Rescue** (find and extract an NPC),
**Contract** (objective or kill requirement), **Dungeon** (multi-stage
progression), and **Purge** (reduce regional corruption). These need not
be separate modes; objectives can overlap within a run.

The progression should explicitly show the third class filling the third slot:
1. Start with Warrior and Gunner, filling two of the three initial class slots.
2. Enter the first region, fight hordes, gain XP, and choose temporary cards.
3. Rushes increase pressure while the build develops.
4. The first timed boss arrives and is defeated.
5. The boss drops Mage, filling the run's third class slot. The player now carries Warrior, Gunner, and Mage together (with all 3 slots filled; any future weapon/class pickup will require dropping one of the three).
6. Continue exploring, completing objectives, gathering resources, and finding NPCs.
7. Complete an NPC quest; its completion persists even if the run later fails.
8. Finish the run and return with meaningful rewards.
9. The rescued NPC joins the hub and offers quests or unlocks a service.
10. Spend resources and quest-earned points on permanent player or weapon progression.
11. Unlock weapon drop possibilities, rarity chances, starting-class options, or optional slot-capacity upgrades.
12. Prepare a new starting combination and enter the next expedition.

## 8. NPC discovery, quests, and recruitment

### The hub is home

The hub is the persistent sanctuary and primary home for
rescued/recruited NPCs. They are characters discovered in the world, not
merely menu interfaces.

### Recruitment loop

1.  **Discover** an NPC in a region: hidden, trapped, threatened, or
    encountered in an event.
2.  **Interact** and learn what they need.
3.  **Quest:** kill enemies, find an item, explore, defeat a boss, or
    make a decision.
4.  **Rescue/recruit:** fulfil the requirements for them to leave the
    region.
5.  **Return:** the NPC appears in the hub.
6.  **Continue:** they offer quests, lore, services, or unlocks.

### Quest persistence

-   Fully completed quests persist if the player dies or fails to finish
    the run.
-   Incomplete run progress is generally lost on death.
-   Completed quests should give meaningful rewards, story, unlocks, or
    recruitment.
-   Recruited NPCs may offer further quests.
-   Failure, delay, and alternate quest outcomes are still to be
    designed.

### NPC roles

NPCs can provide narrative value (questlines, character development,
secrets) and/or hub services:
- Blacksmith: upgrade/craft/modify weapons.
- Archivist: class mastery and weapon trees.
- Merchant: buy/sell.
- Storage Keeper: equipment and resources.
- Cartographer: regions/routes/exploration.
- Shrine Keeper: permanent progression.
- Executioner: challenges and boss rematches.

Not every NPC needs a service; some may exist mainly for their story.

### Quest philosophy

Clear objectives can lead to unexpected truths. A missing person,
monster hunt, or delivery may uncover a larger mystery. Fairytales can
be reimagined and assumptions about heroes, villains, victims, and
monsters challenged.

Questlines may span runs/regions, unlock NPCs/services/weapons/classes,
change regions, offer choices, or reveal hidden bosses.

**Example --- Red Riding Hood:** A girl asks for help reaching her
grandmother's cottage and blames a monstrous wolf. Hunting the wolf
reveals it may have protected the forest. The trail leads to Hell, where
the grandmother's true identity is revealed; the story expands into an
ancient witch, regional corruption, and potentially a Witch class.

## 9. Hub and permanent progression

The sanctuary grows as NPCs return. It is where the player chooses
weapons, manages progression, spends resources, speaks with NPCs,
accepts quests, unlocks regions, and prepares.

### Player skill tree

After certain quests, the player unlocks a permanent player-wide tree
for quality-of-life and run-preparation upgrades, distinct from weapon
trees.

Possible upgrades:
- Increased chance for a particular weapon to drop.
- Increased weapon rarity chance.
- Improved starting conditions.
- Additional run weapon slot.
- Other progression/QoL upgrades.

Some upgrades may allow choosing or replacing a starter class. Default
starters are Warrior and Gunner; a player might unlock the option to
replace Warrior with Bloodstalker. These upgrades are earned through
quests/progression.

### Weapon skill trees

Each weapon/class has its own permanent tree:
- Newly acquired classes start with three core skills.
- Additional skills and upgrades unlock through that weapon's tree.
- Upgrades can improve, add effects to, or alter abilities.
- Weapon trees are separate from the player-wide tree.
- Players invest in preferred classes and build combinations.

### Level, rarity, and mastery

These are separate:
- Rarity affects quality/potential properties.
- Weapon level affects current power.
- Skill-tree investment affects abilities and mastery.
- Player-tree upgrades may influence drop chance or rarity.

A high-rarity weapon is not automatically better than a developed
weapon.

## 10. Run completion, death, and economy

### Completing a run

A successful run should provide more than survival:
- Resources and currency.
- Quest completion and persistent rewards.
- Rescued/recruited NPCs.
- Weapon discoveries and rewards according to completion rules.
- Regional and permanent progression.

Completed quests persist even if the player later dies or fails; other
run progress generally does not.

### Death

Death carries risk:
- Most run progress is lost.
- Weapons cannot be brought out of an incomplete run.
- Completed quests remain completed.
- Previously earned persistent progression is retained.

The loss/reward balance should make failure meaningful without making
time feel wasted.

### Gold and resources

Gold is a potential primary currency, earned by selling weapons or loot
brought back from completed runs and spent on useful items, upgrades,
merchant services, or crafting. Other resources may fund weapon
upgrades, player-tree progression, crafting, and region systems.

The economy is not finalized. Its purpose is to make run rewards useful
and create tangible spending decisions between expeditions.

## 11. Long-term goals

-   Discover and collect weapon-classes.
-   Develop individual class mastery.
-   Experiment with multi-class combinations.
-   Recruit NPCs and expand the hub.
-   Complete regional and character questlines.
-   Unlock player-wide and weapon-specific skill trees.
-   Improve weapon drop chances and rarity.
-   Unlock starting-class options.
-   Gather resources and improve the sanctuary.
-   Unlock maps, dungeons, and bosses.
-   Discover the truth behind Judgment and the Witness.

## 12. Core gameplay loops

### Moment-to-moment

**Fight → gain XP → level up → choose a passive → build the run → switch
weapons and combine abilities → survive.**

### Run

**Choose two starting weapons → enter a region → fight hordes/rushes → gain XP
and passives → explore and complete objectives → defeat timed bosses →
fill third slot or replace an equipped weapon → collect rewards → return to hub.**

### NPC

**Explore → discover NPC → accept quest → complete objective → bring
them to hub → unlock further quests, story, or services.**

### Permanent progression

**Return to hub → spend resources → upgrade player/weapon trees → unlock
weapons and starting options → recruit NPCs → prepare a new combination
→ enter another expedition.**

### Narrative

**Confront regional corruption → defeat major entity → permanently
change region → uncover history → approach the truth of Judgment and the
Witness.**

## 13. Example progression cycle

1.  Start with Warrior and Gunner (2 of 3 class slots filled).
2.  Enter the first region, fight hordes, gain XP, and choose temporary
    cards.
3.  Rushes increase pressure while the build develops.
4.  The first timed boss arrives and is defeated.
5.  Mage drops and fills the 3rd class slot. Now all 3 slots are full, so picking up any subsequent class requires dropping one of the equipped classes.
6.  Continue exploring, completing objectives, gathering resources, and
    finding NPCs.
7.  Complete an NPC quest; its completion persists even if the run later
    fails.
8.  Finish the run and return with meaningful rewards.
9.  The rescued NPC joins the hub and offers quests or unlocks a
    service.
10. Spend resources and quest-earned points on permanent player or
    weapon progression.
11. Unlock weapon drop possibilities, rarity chances, or a
    starting-class option.
12. Prepare a new starting build and enter the next expedition.

The cycle repeats as the hub, arsenal, NPC roster, skill trees, and
world expand.

## 14. Current prototype

The prototype currently establishes:
- Warrior and Gunner starter classes.
- Three skills per starter class.
- Instant Q switching.
- Switch-combo skills, currently Warrior/Gunner Skill 2.
- A small spawning enemy horde.
- A boss encounter.
- Boss defeat rewards Mage, currently with three skills.
- Mage switch-combo skills are not yet implemented.

The prototype validates combat feel, switching, skill interactions,
horde combat, and boss rewards before the larger systems are built.

## 15. Design principles and open questions

### Principles

-   Three class slots make weapon choices and switching central to combat.
-   Switching is a core combat skill.
-   Combo skills reward timing and experimentation.
-   Run level-ups are exciting and create distinct builds.
-   Temporary passives are varied and impactful.
-   Timed bosses and rushes create pacing.
-   Boss drops create meaningful decisions once slots are filled.
-   Exploration rewards curiosity with NPCs, quests, resources, and
    secrets.
-   Recruited NPCs make the hub feel alive.
-   Completed quests persist after failure.
-   Permanent progression makes future runs meaningfully different.
-   The world is disturbing and mysterious, but immediate goals remain
    clear.
-   Players have reasons to run beyond defeating the next boss.

### Open design questions

-   Boss count and timing per run; rush pacing and telegraphs.
-   Passive-card pool, rarities, and upgrade levels.
-   Rules for slot-changing passives and their interactions.
-   Weapon level, rarity, and mastery balance.
-   Resource and gold economy; merchant inventory and purchase value.
-   Successful-run rewards and quest persistence/failure rules.
-   Starting-class unlock requirements.
-   Switch-combo coverage across class pairings.
-   Balancing builds against boss resistances.
-   Main-region progression and permanent map changes.
-   **Single-player is the current direction.**

These are intentionally open for prototyping and playtesting.

------------------------------------------------------------------------

## One-sentence pitch

**In a world shattered by Judgment, the last vessel of humanity's
memories fights through corrupted realms with interchangeable
weapon-classes, builds a powerful run through temporary passive
upgrades, hunts the entities that have consumed creation, and rescues
the strange survivors who will rebuild a sanctuary---and perhaps the
world itself.**
