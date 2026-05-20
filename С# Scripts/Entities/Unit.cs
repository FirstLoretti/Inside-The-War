using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Data;
using InsideTheWar.Interfaces;

namespace InsideTheWar.Entities;

public partial class Unit : CharacterBody2D, IUnit, IDamageable
{
    private bool TickTryStartCombat(float delta)
    {
        if (TickFindAndSetTarget(delta))
        {
            StartCombat(_personalAttackTarget);
            return true;
        }
        return false;
    }

    public void SetState(UnitStates unitState, Vector2? target = null)
    {
        CurrentState = unitState;
        Velocity = Vector2.Zero;

        switch (unitState)
        {
            case UnitStates.Idle:
                _animationPlayer.Play(_idleAnimation);
                break;
            case UnitStates.BattleReady:
                _animationPlayer.Play(_idleAnimation);
                break;
            case UnitStates.WaitingOrder:
                _animationPlayer.Play(_idleAnimation);
                break;
            case UnitStates.Moving:
                if (target.HasValue)
                {
                    _movement.MoveTo(target.Value);
                    _animationPlayer.Play(_runAnimation);
                }
                break;
            case UnitStates.Charging:
                if (target.HasValue)
                {
                    _movement.MoveTo(target.Value);
                    _animationPlayer.Play(_runAnimation);
                }
                break;
            case UnitStates.Attacking:
                if (target.HasValue)
                {
                    _movement.LookAt(target.Value);
                    _animationPlayer.Play(_attackAnimation);
                }
                break;
        }
    }

    private bool TickFindAndSetTarget(float delta)
    {
        var enemy = _combat.TickFindEnemy(delta);
        if (_combat.TrySetPersonalTarget(enemy))
        {
            return true;
        }
        return false;
    }

    private void Attack(IDamageable target)
    {
        if (_personalAttackTarget == target && CurrentState == UnitStates.Attacking) { return; }

        SetState(UnitStates.Attacking, target.GlobalPosition);
    }

    private void StartCombat(IDamageable target)
    {
        Attack(target);
        HealthComponent.AddAttacker(target);
        TargetCounterattack(target, this);
    }

    private void TargetCounterattack(IDamageable target, IDamageable attacker)
    {
        if (target is Unit enemyUnit)
        {
            enemyUnit._combat.TrySetPersonalTarget(attacker);
            enemyUnit.Attack(attacker);
            enemyUnit.HealthComponent.AddAttacker(attacker);
        }
    }

    public void DoDamage() // Animation Event
    {
        var damage = (int)GameMath.GetRandomNumber(Data.MinDamage, Data.MaxDamage);
        if (!IsInstanceValid((CollisionObject2D)_personalAttackTarget))
        {
            OnTargetLost();
            return;
        }

        _personalAttackTarget.HealthComponent.TakeDamage(damage);
    }

    public void OnTargetLost()
    {
        _combat.ClearTarget();

        var nextTarget = _combat.FindNearestAvailibleEnemy();
        if (nextTarget != null)
        {
            StartCombat(nextTarget);
            return;
        }

        var targetPosition = GlobalPosition + _movement.LookDirection * FormationData.Spacing;
        SetState(UnitStates.Charging, targetPosition);
    }

    private void Dying()
    {
        SetState(UnitStates.Dead);
        foreach (var attacker in HealthComponent.Attackers)
        {
            if (attacker is Unit unit)
            {
                if (IsInstanceIdValid(unit.Id))
                {
                    unit.OnTargetLost();
                }
            }
        }
        HealthComponent.ClearAttackersList();
        Die?.Invoke(this);
        HealthComponent.HealthDepleted -= Dying;
        QueueFree();
    }
}
