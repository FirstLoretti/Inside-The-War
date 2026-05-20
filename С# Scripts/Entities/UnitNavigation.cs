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
            if (TickTryStartCombat(deltaFloat))
            {
                return;
            }
            TickFormationAdvance(deltaFloat);
            return;
        }

        if (CurrentState == UnitStates.Charging)
        {
            if (TickTryStartCombat(deltaFloat))
            {
                return;
            }
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

    public virtual void MoveTo(Vector2 targetPosition) => SetState(UnitStates.Moving, targetPosition);

    private void TickFormationAdvance(float delta)
    {
        _checkAttackQueueTimer -= delta;
        if (_checkAttackQueueTimer > Constants.Zero) { return; }

        _checkAttackQueueTimer = _timer;
        if (GetFrontAlly() is Unit frontAlly)
        {
            if (frontAlly.CurrentState == UnitStates.Attacking || frontAlly.CurrentState == UnitStates.BattleReady) { return; }
        }

        var distanceToTarget = GlobalPosition.DistanceSquaredTo(MovementTargetPosition);
        if (distanceToTarget > _stoppingDistanceSqr)
        {
            SetState(UnitStates.Charging, MovementTargetPosition);
        }
        else
        {
            TickTryStartCombat(Constants.Zero);
        }
    }
    public IUnit GetFrontAlly()
    {
        return _combat.GetFrontAlly(MovementTargetPosition, FormationData.Spacing, CheckAllyRayMultiplicator);
    }

    private void OnReachDestination()
    {
        _movement.Stop();

        if (CurrentState == UnitStates.Charging)
        {
            var enemy = _combat.FindNearestAvailibleEnemy();
            _combat.TrySetPersonalTarget(enemy);
            if (_personalAttackTarget != null)
            {
                StartCombat(_personalAttackTarget);
            }
            else
            {
                SetState(UnitStates.BattleReady);
            }
        }
        else
        {
            SetState(UnitStates.Idle);
        }
    }
}
