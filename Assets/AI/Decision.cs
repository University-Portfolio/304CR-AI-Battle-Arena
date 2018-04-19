using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public delegate void ActionCallback();


/// <summary>
/// A collection of variables
/// </summary>
public class VariableCollection
{
	/// <summary>
	/// All the variables which are exposed to this tree
	/// </summary>
	public Dictionary<string, float> variables { get; private set; }


	public VariableCollection()
	{
		variables = new Dictionary<string, float>();
	}

	public bool HasVar(string name)
	{
		return variables.ContainsKey(name);
	}

	public void SetVar(string name, float value)
	{
		variables[name] = value;
	}

	public float GetVar(string name, float defaultValue = 0.0f)
	{
		if (variables.ContainsKey(name))
			return variables[name];
		else
		{
			// Var might be a hard coded value
			float result;
			if (float.TryParse(name, out result))
				return result;

			//Debug.LogWarning("Name variable '" + name + "' can be found for this variable collection");
			return defaultValue;
		}
	}
}


/// <summary>
/// Some form of logical decision can be made here
/// </summary>
public interface IDecision
{
	ActionCallback Process(VariableCollection globalVars, VariableCollection agentProfile);
}


/// <summary>
/// If 2 variables are true or false when compared to some condition
/// </summary>
public class DecisionIf : IDecision
{
	public enum Operand
	{
		Equals,
		NotEquals,
		LessThan,
		GreaterThan,
		LessThanEquals,
		GreaterThanEquals
	}

	public readonly string varA;
	public readonly string varB;
	public readonly Operand operand;

	public IDecision trueDecision;
	public IDecision falseDecision;

	public DecisionIf(string varA, string varB, Operand operand)
	{
		this.varA = varA;
		this.varB = varB;
		this.operand = operand;
	}

	public ActionCallback Process(VariableCollection globalVars, VariableCollection agentProfile)
	{
		// Fetch value
		float valA, valB;
		if (agentProfile.HasVar(varA))
			valA = agentProfile.GetVar(varA);
		else
			valA = globalVars.GetVar(varA);

		if (agentProfile.HasVar(varB))
			valB = agentProfile.GetVar(varB);
		else
			valB = globalVars.GetVar(varB);

		// Calculate condition
		bool condition = false;

		switch (operand)
		{
			case Operand.Equals:
				condition = (valA == valB);
				break;
			case Operand.NotEquals:
				condition = (valA != valB);
				break;

			case Operand.GreaterThan:
				condition = (valA > valB);
				break;
			case Operand.GreaterThanEquals:
				condition = (valA >= valB);
				break;

			case Operand.LessThan:
				condition = (valA < valB);
				break;
			case Operand.LessThanEquals:
				condition = (valA <= valB);
				break;
		}


		if (condition)
			return trueDecision != null ? trueDecision.Process(globalVars, agentProfile) : null;
		else
			return falseDecision != null ? falseDecision.Process(globalVars, agentProfile) : null;
	}
}


/// <summary>
/// If variable is within range
/// </summary>
public class DecisionRange : IDecision
{
	private struct Range
	{
		public readonly float max;
		public readonly IDecision decision;

		public Range(float max, IDecision decision)
		{
			this.max = max;
			this.decision = decision;
		}
	}

	public readonly string var;
	public readonly bool useGreaterThan;
	private List<Range> ranges = new List<Range>();
	public IDecision defaultDecision;


	public DecisionRange(string var, bool useGreaterThan)
	{
		this.var = var;
		this.useGreaterThan = useGreaterThan;
	}

	public void AddRange(float max, IDecision decision)
	{
		ranges.Add(new Range(max, decision));
		ranges.Sort((a, b) => useGreaterThan ? -a.max.CompareTo(b.max) : a.max.CompareTo(b.max));
	}

	public ActionCallback Process(VariableCollection globalVars, VariableCollection agentProfile)
	{
		// Fetch value
		float val;
		if (agentProfile.HasVar(var))
			val = agentProfile.GetVar(var);
		else
			val = globalVars.GetVar(var);

		// Find value to make decision based off
		foreach (Range range in ranges)
			if (useGreaterThan ? val > range.max : val < range.max)
				return range.decision != null ? range.decision.Process(globalVars, agentProfile) : null;

		return defaultDecision != null ? defaultDecision.Process(globalVars, agentProfile) : null;
	}
}

/// <summary>
/// Represents a specific action in a decision tree
/// </summary>
public class DecisionState : IDecision
{
	public readonly ActionCallback action;

	public DecisionState(ActionCallback action)
	{
		this.action = action;
	}

	public ActionCallback Process(VariableCollection globalVars, VariableCollection agentProfile)
	{
		return action;
	}
}
