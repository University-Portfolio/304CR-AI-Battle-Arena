using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Read in a decision tree structure from an xml file
/// </summary>
public class DecisionTree
{
	/// <summary>
	/// The agent profiles that can be used with this tree
	/// </summary>
	private VariableCollection[] agentProfiles;

	/// <summary>
	/// The agent variables that are currently being used
	/// </summary>
	private VariableCollection agentProfile = new VariableCollection();
	/// <summary>
	/// The global variables that are avaliable to all profiles
	/// </summary>
	private VariableCollection globalVars = new VariableCollection();

	/// <summary>
	/// All actions which can be performed by this tree
	/// </summary>
	private Dictionary<string, ActionCallback> actionStates = new Dictionary<string, ActionCallback>();

	/// <summary>
	/// The root of the decision tree
	/// </summary>
	private IDecision rootDecision;

	/// <summary>
	/// The action to currently execute
	/// </summary>
	private ActionCallback currentAction;



	public DecisionTree()
	{
	}

	public void DebugMake()
	{
		DecisionIf ifDec = new DecisionIf("AliveCount", "32", DecisionIf.Operand.Equals);
		ifDec.trueDecision = new DecisionState(actionStates["Skirt"]);
		ifDec.falseDecision = new DecisionState(actionStates["Flee"]);
		rootDecision = ifDec;
	}
	
	/// <summary>
	/// Execute the current action
	/// </summary>
	public void Run()
	{
		if (currentAction != null)
			currentAction();
	}

	/// <summary>
	/// Run through the tree and update the current action
	/// </summary>
	public void Recalculate()
	{
		currentAction = rootDecision != null ? rootDecision.Process(globalVars, agentProfile) : null;
	}

	/// <summary>
	/// Register a specific action with this tree
	/// </summary>
	/// <param name="name">The unique name to use for this action</param>
	/// <param name="callback">The function to callback when this state is called</param>
	public void RegisterActionState(string name, ActionCallback callback)
	{
		actionStates[name] = callback;
	}

	/// <summary>
	/// Sets the value of a global variable
	/// </summary>
	/// <param name="name">The name of this variable</param>
	/// <param name="value">The value to give this variable</param>
	public void SetGlobalVar(string name, float value)
	{
		globalVars.SetVar(name, value);
	}

	/// <summary>
	/// Gets the global variable of this name
	/// </summary>
	/// <param name="name">The name of this variable</param>
	/// <param name="defaultValue">The default value to return in the event it is not found</param>
	/// <returns></returns>
	public float GetGlobalVar(string name, float defaultValue = 0.0f)
	{
		return globalVars.GetVar(name, defaultValue);
	}
}
