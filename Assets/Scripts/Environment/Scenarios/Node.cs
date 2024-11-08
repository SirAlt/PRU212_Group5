using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour, ITriggerable
{
    [SerializeField] private List<Node> childNodes;
    [SerializeField] private List<Condition> successConditions = new();
    [SerializeField] private List<Condition> failureConditions = new();
    [SerializeField] private List<Operation> setupOperations = new();
    [SerializeField] private List<Operation> failureOperations = new();
    [SerializeField] private List<Operation> successOperations = new();
    [SerializeField] private ChildActivationBehavior childActivationBehavior;
    [SerializeField] private bool autoStart;
    [SerializeField] private bool autoRetry;
    [SerializeField] private bool replayable;
    [SerializeField] private bool resolveSuccessImmediately;

    public enum ChildActivationBehavior
    {
        AllAtOnce,
        OneByOne,
    }

    protected Node parentNode;

    [Header("Debug")]
    [SerializeField] protected bool _activated;
    [SerializeField] protected bool _condFulfilled;
    [SerializeField] protected bool _completed;
    [SerializeField] protected bool _executing;
    [SerializeField] protected bool _failureResolved;
    [SerializeField] protected bool _successResolved;

    [SerializeField] protected int _currentPhase = -1;

    protected bool InProgress => _activated && !Resolved;
    protected bool Resolved => _failureResolved || _successResolved;

    protected void OnEnable()
    {
        _condFulfilled = false;
        _completed = false;
        _executing = false;
        _failureResolved = false;
        _successResolved = false;

        _currentPhase = -1;
    }

    protected void OnDisable()
    {
        _activated = false;

        foreach (var condition in successConditions)
            condition.ConditionFulfilled -= Fulfill;
        foreach (var condition in failureConditions)
            condition.ConditionFulfilled -= ResolveFailure;
    }

    protected void Start()
    {
        if (autoStart) PerformSetup();
    }

    bool ITriggerable.IsOn => InProgress;

    void ITriggerable.TriggerOn() => PerformSetup();
    void ITriggerable.TriggerOff() => ResolveFailure();

    public void Fulfill()
    {
        _condFulfilled = true;
        if (CheckCompletion()) Complete();
    }

    protected void OnChildCompleted()
    {
        if (childActivationBehavior == ChildActivationBehavior.OneByOne) AdvancePhase();
        if (CheckCompletion()) Complete();
    }

    private bool CheckCompletion()
    {
        if (!_condFulfilled)
        {
            return _completed = false;
        }
        foreach (var child in childNodes)
        {
            // Destroyed => Completed
            if (child && !child._completed)
            {
                return _completed = false;
            }
        }
        return _completed = true;
    }

    private void Complete()
    {
        if (parentNode != null)
        {
            if (resolveSuccessImmediately) ResolveSuccess();
            parentNode.OnChildCompleted();
        }
        else
        {
            ResolveSuccess();
        }
    }

    // This should be protected, but C# does not allow cross-hierarchy protected method calls - which we want.
    public void SetParentTo(Node parent)
    {
#if UNITY_EDITOR

        if (parent == parentNode)
        {
            Debug.Log($"Attempt to assign the same parent node to scenario node on {gameObject.name}.", gameObject);
        }
        else if (parentNode != null)
        {
            Debug.Log($"Scenario node on {gameObject.name} has been re-parented to {parentNode}.", gameObject);
        }
#endif
        parentNode = parent;
    }

    protected virtual void PerformSetup()
    {
        if (_executing || _activated || (_successResolved && !replayable)) return;
        _executing = true;

        OnSetup();

        foreach (var work in setupOperations)
            work.Execute();

        switch (childActivationBehavior)
        {
            case ChildActivationBehavior.AllAtOnce:
                foreach (var child in childNodes)
                {
                    if (!child) continue;
                    child.SetParentTo(this);
                    child.PerformSetup();
                }
                break;

            case ChildActivationBehavior.OneByOne:
                AdvancePhase();
                break;
        }

        foreach (var condition in successConditions)
            condition.ConditionFulfilled += Fulfill;
        foreach (var condition in failureConditions)
            condition.ConditionFulfilled += ResolveFailure;

        foreach (var condition in successConditions)
            condition.Activate();
        foreach (var condition in failureConditions)
            condition.Activate();

        _executing = false;
        _activated = true;
    }

    private void AdvancePhase()
    {
        while (++_currentPhase < childNodes.Count)
        {
            var child = childNodes[_currentPhase];
            if (child)
            {
                child.SetParentTo(this);
                ((ITriggerable)child).TriggerOn();
                return;
            }
            Debug.LogWarning($"Could not find phase {_currentPhase} of consecutive node on {gameObject.name}. Skipping.", gameObject);
        }
        Debug.Log($"All phases of consecutive node on {gameObject.name} have finished.", gameObject);
    }

    protected virtual void ResolveFailure()
    {
        if (_executing || !InProgress) return;
        _executing = true;

        foreach (var condition in successConditions)
            condition.ConditionFulfilled -= Fulfill;
        foreach (var condition in failureConditions)
            condition.ConditionFulfilled -= ResolveFailure;

        foreach (var child in childNodes)
            if (child) child.ResolveFailure();

        foreach (var work in failureOperations)
            work.Execute();

        OnFailure();

        _executing = false;
        _failureResolved = true;

        if (autoRetry) PerformSetup();
    }

    protected virtual void ResolveSuccess()
    {
        if (_executing || !InProgress) return;
        _executing = true;

        foreach (var condition in successConditions)
            condition.ConditionFulfilled -= Fulfill;
        foreach (var condition in failureConditions)
            condition.ConditionFulfilled -= ResolveFailure;

        foreach (var child in childNodes)
            if (child) child.ResolveSuccess();

        foreach (var work in successOperations)
            work.Execute();

        OnSuccess();

        _executing = false;
        _successResolved = true;
    }

    protected virtual void OnSetup()
    {
        gameObject.SetActive(true);
    }

    protected virtual void OnFailure()
    {
        gameObject.SetActive(false);
    }

    protected virtual void OnSuccess()
    {
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }
}
