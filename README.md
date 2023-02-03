# Top Down Third Person Game

## TODOs (Current)

- [ ] Polish Pass
    - [ ] Type 4 Character - Grenadier
        - [X] Primary Ability (All Damage/HP/Effects)
        - [X] Secondary Ability (All Damage/HP/Effects)
        - [ ] Tertiary Ability (All Damage/HP/Effects)
            - Satchel tries to go to crosshair location. It fails if the range is too far since it has a max launch velocity with Gravity
            - Has a relatively small radius of impact
            - Cannot be affected through walls. Needs LOS
            - Satchel applies a short duration of high acceleration
            - Then the drops acceleration to 0
            - Gravity keeps affecting and velocity also slowly decreases
            - The player needs to look towards the direction they want to go in and also press the appropriate key
            - Literally a stronger jump with an initial direction applied
        - [ ] Ultimate Ability (All Damage/HP/Effects)

## TODOs (Future)

> Characters don't take Fall Damage

- [ ] Polish Pass
    - [ ] Type 5 Character - Engineer
        - [ ] Primary Ability (All Damage/HP/Effects)
        - [ ] Secondary Ability (All Damage/HP/Effects)
        - [ ] Tertiary Ability (All Damage/HP/Effects)
        - [ ] Ultimate Ability (All Damage/HP/Effects)
- [ ] Animation Polish
    - [ ] Type 1
    - [ ] Type 2
    - [ ] Type 3
    - [ ] Type 4
    - [ ] Type 5
- [ ] For Networking currently use Netcode for GameObjects as of now

## TODOs (Done)

- [X] Player Movement
    - [X] Basic Player Movement
    - [X] Top-down camera similar to old Pokemon games but can be moved 360 deg
    - [X] Basic movement should be similar to Bullet Hell games
        - [X] WASD Movement
        - [X] Run
    - [X] Ability System for Movement
    - [X] Primary Ability
    - [X] Secondary Ability
    - [X] Ultimate Ability
    - [X] Abilities modify everything (Movement/Shooting/Controls etc.)
- [X]  Type 1 - Default Call of Duty Character
    - [X]  Ability Primary - Simple Shooting
    - [X]  Ability Secondary - Sojourn Charge
    - [X]  Ability Tertiary - Dash
    - [X]  Ability Ultimate - All abilities cooldowns decreased for allies in range
- [X]  Type 2 - Water Bender
    - [X]  Ability Primary - Water slash attacks since if the player swings around their camera too much the game
      experience will not be good
    - [X]  Ability Secondary - Create an Ice Wall that block incoming damage (Can be Broken)
    - [X]  Ability Tertiary - Yelan's Movement Ability from Genshin Impact. Player does not take damage in this state
    - [X]  Ability Ultimate - Water bubble to trap the enemy
- [X]  Type 3 - Darkness (Fade)
    - [X]  Ability Primary - Kasumi but not as overpowered
    - [X]  Ability Secondary - Dark Slash. Targets marked with this take more damage from Primary for sometime
    - [X]  Ability Tertiary - Shadow Movement. Similar to how Rogue from Sabertooth moved (Andro Dash). There is a delay
      between when gravity starts affecting.
    - [X]  Ability Ultimate - Inflict Fade Ultimate and Paranoia as a pulse
- [X]  Type 4 - Grenadier (Raze)
    - [X]  Ability Primary - Shoot Plasma bombs that stick to the ground and hurt enemies in range. They disappear after
      sometime (SlaughterSpine)
    - [X]  Ability Secondary - Soldier 76 Grenades homing grenades but has a slow targeting range so can be dodged
    - [X]  Ability Tertiary - Fine… Raze Satchel
    - [X]  Ability Ultimate - Here comes the party! :smile:
- [X]  Type 5 - Engineer (Killjoy)
    - [X]  Ability Primary - Place a Turret that deals damage
    - [X]  Ability Secondary - Grenade Cluster that also stuns enemies a small amount
    - [X]  Ability Tertiary - Gibraltar shield from Apex
    - [X]  Ability Ultimate - Allies get a movement speed buff and a protective shield around them
- [X] Switch to new Unity Input system
- [X] Add Damage and Health Systems (Both must be modifiable by effects)
- [X] Use UIToolkit to make the UI
- [X] Polish Pass
    - [X] General Character (Effects and Movement Speed)
        - [X] Idle
        - [X] Walking
        - [X] Running
        - [X] Falling
        - [X] Landing
    - [X] Type 1 Character - Default Call of Duty Character
        - [X] Primary Ability (All Damage/HP/Effects)
        - [X] Secondary Ability (All Damage/HP/Effects)
        - [X] Tertiary Ability (All Damage/HP/Effects)
        - [X] Ultimate Ability (All Damage/HP/Effects)
    - [X] Type 2 Character - Water Bender
        - [X] Primary Ability (All Damage/HP/Effects)
        - [X] Secondary Ability (All Damage/HP/Effects)
        - [X] Tertiary Ability (All Damage/HP/Effects)
        - [X] Ultimate Ability (All Damage/HP/Effects)
    - [X] Type 3 Character - Darkness Incarnated
        - [X] Primary Ability (All Damage/HP/Effects)
        - [X] Secondary Ability (All Damage/HP/Effects)
        - [X] Tertiary Ability (All Damage/HP/Effects)
        - [X] Ultimate Ability (All Damage/HP/Effects)
- [X] ShootController that returns the correct direction where the projectile should be launched to hit the center of
  the screen
- [X] Cooldown system for abilities. Can be faster cooldown, fixed amount time cooldown, percent amount cooldown...
- [X] Add a Windup Time for every Ultimate

## Colors
- Purple - 5200FF
- Red - FF0000
- White - FFFFFF
- Light Blue - 00EBFF

## External Credits

- https://opengameart.org/content/lava-meets-water
- https://opengameart.org/content/snow-flake
