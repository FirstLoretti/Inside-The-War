using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Data;
using InsideTheWar.Interfaces;

namespace InsideTheWar.Entities;

public partial class Unit : CharacterBody2D
{
    public override void _PhysicsProcess(double delta)
    {
        if (CurrentState != UnitStates.Moving &&
            CurrentState != UnitStates.Charging &&
            CurrentState != UnitStates.BattleReady)
        {
            return;
        }

        var deltaFloat = (float)delta;

        if (CurrentState == UnitStates.BattleReady)
        {
            if (TryStartCombat()) { return; }

            TickFormationAdvance(deltaFloat);
            return;
        }

        if (CurrentState == UnitStates.Charging)
        {
            if (TryStartCombat()) { return; }

            if (IsFrontAllyOnCombat())
            {
                SetState(UnitStates.BattleReady);
                return;
            }
        }

        UpdateMovement(deltaFloat, MovementTargetPosition);
    }

    protected virtual void UpdateMovement(float delta, Vector2 targetPosition)
    {
        var distanceToTargetSqr = GlobalPosition.DistanceSquaredTo(targetPosition);
        if (distanceToTargetSqr <= _stoppingDistanceSqr)
        {
            OnReachDestination();
        }
        else
        {
            MoveTo(targetPosition);
        }
    }

    private bool IsFrontAllyOnCombat()
    {
        if (GetFrontAlly() is Unit ally)
        {
            if (ally.CurrentState == UnitStates.Attacking || ally.CurrentState == UnitStates.BattleReady)
            {
                return true;
            }
        }
        return false;
    }

    protected virtual void MoveTo(Vector2 targetPosition) => _movement.MoveTo(targetPosition);

    private void TickFormationAdvance(float delta)
    {
        _formationAdvanceTimer -= delta;
        if (_formationAdvanceTimer > Constants.Zero) { return; }

        _formationAdvanceTimer = _timer;

        if (GetFrontAlly() is Unit frontAlly)
        {
            if (frontAlly.CurrentState == UnitStates.Attacking || frontAlly.CurrentState == UnitStates.BattleReady) { return; }
            //if (IsFormationSpacingMaintained(frontAlly)) { return; }
        }

        var enemy = _combat.FindNearestAvailibleEnemy();
        if (enemy != null)
        {
            TryStartCombat();
            return;
        }

        var stepToAttackDirection = GlobalPosition + _formationLookDirection * FormationData.Spacing;
        SetState(UnitStates.Charging, stepToAttackDirection, _formationLookDirection);
        return;
        // var distanceToTarget = GlobalPosition.DistanceSquaredTo(MovementTargetPosition);
        // if (distanceToTarget > _stoppingDistanceSqr)
        // {
        //     SetState(UnitStates.Charging, MovementTargetPosition, _formationLookDirection);
        // }
        // else
        // {
        //     TryStartCombat();
        // }
    }

    private bool IsFormationSpacingMaintained(Unit frontAlly)
    {
        var distanceToAlly = GlobalPosition.DistanceSquaredTo(frontAlly.GlobalPosition);
        var spacingSqr = FormationData.Spacing * FormationData.Spacing;
        if (distanceToAlly > spacingSqr)
        {
            return false;
        }
        return true;
    }

    private IUnit GetFrontAlly()
    {
        return _combat.GetFrontAlly(_formationLookDirection, FormationData.Spacing, CheckAllyRayMultiplicator);
    }

    private void OnReachDestination()
    {
        _movement.Stop();
        if (CurrentState == UnitStates.Charging)
        {
            SetState(UnitStates.BattleReady);
        }
        else if (CurrentState != UnitStates.Attacking && CurrentState != UnitStates.BattleReady)
        {
            SetState(UnitStates.Idle);
        }
    }
}
