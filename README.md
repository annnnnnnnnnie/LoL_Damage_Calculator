# LoL_Damage_Calculator
Currently an annie's burst damage calculator for League of Legends patch 8.0+

This project is developed using Unity 3D Personal. The reason why a game engine is used to program a damage calculator is that I am more familiar with Unity.
This project aims to simulate the exact process of dealing and receiving damage in the game League of Legends. Currently, however, the simulation of a dueling scenario is the goal.

Class Hero is the abstraction of champions in the game. The basic attributes of a champion such as level, runes and items are handled by this parent class. Also they have common methods such as CastSpell, ReceiveSpell and Update. 

Update is the most unique and interesting part of this project. It simulates the processing of Buffs, including true damage conversion from the keystone Conqueror, MR reduction from certain Items and even the DoT damage from ignite and Liandry's. However, this part is still under development.

What has been achieved:
1. Saving and Loading of RunePages.
2. Calculation of base Attributes such as AD, MR, health.
3. Calculation of CastSpell and ReceiveSpell.

Currently under development:
1. Further integration of class Hero(Everything should be integrated into one class, which would help in future development when team fight is simulated)
2. How to deal with item passives/actives and runes.
