using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadMe : MonoBehaviour {
/*
 * There is little point in reading this file.
 * 
 * Overall Workflow of this programme:
 * Start => AnniePanel Initialize, enemy panel initialize
 * 
 * ---Calculation---
 * hero contains a function called Update(Spellcast = null)
 * 
 * The processor determines whether a Spellcast is pass to the hero on each update
 * 
 *
 * ---RunePage---
 * public class RunePathData is the data structure between the app and json files
 * RunePathData is translated to RunePathInfo, which is used by RunePage to generate RunePaths
 * 
 * When "save"-ing, RunePage collects and submits RuneInfo, which is used by HeroInfo to calculate the base stats
 * 
 * ---Hierarchy---
 * Hero
 *   HeroInfo
 *     RunePage
 *     Inventory
 *     Attributes
 *     SpellPanel
 *     
 *     
 * ---Possible Changes to make---
 * Reuse RunePage GameObject
 * */
}
