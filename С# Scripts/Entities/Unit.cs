using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Data;
using InsideTheWar.Interfaces;

namespace InsideTheWar.Entities;

public partial class Unit : CharacterBody2D, IUnit, IDamageable
{
    private bool TryStartCombat()
    {
        if (FindAndSetTarget())
        {
            StartCombat(_personalAttackTarget);
            return true;
        }
        return false;
    }

    private bool FindAndSetTarget()
    {
        var enemy = _combat.FindEnemyInAttackDistance();
        if (_combat.TrySetPersonalTarget(enemy))
        {
            return true;
        }
        return false;
    }

    public void SetState(UnitStates unitState, Vector2? targetPosition = null, Vector2? lookDirectionInBattle = null)
    {
        CurrentState = unitState;
        Velocity = Vector2.Zero;

        switch (unitState)
        {
            case UnitStates.Idle:
                _animationPlayer.Play(_idleAnimation);
                break;
            case UnitStates.BattleReady:
                _movement.LookAt(_formationLookDirection);
                _animationPlayer.Play(_idleAnimation);
                break;
            case UnitStates.WaitingOrder:
                _animationPlayer.Play(_idleAnimation);
                break;
            case UnitStates.Moving:
                _movement.MoveTo(targetPosition.Value);
                _animationPlayer.Play(_runAnimation);
                break;
            case UnitStates.Charging:
                _formationLookDirection = lookDirectionInBattle.Value;
                _movement.MoveTo(targetPosition.Value);
                _animationPlayer.Play(_runAnimation);
                break;
            case UnitStates.Attacking:
                var enemyDir = (targetPosition.Value - GlobalPosition).Normalized();
                _movement.LookAt(enemyDir);
                _animationPlayer.Play(_attackAnimation);
                break;
        }
    }

    private void Attack(IDamageable target)
    {
        if (_personalAttackTarget == target && CurrentState == UnitStates.Attacking) { return; }

        SetState(UnitStates.Attacking, target.GlobalPosition, _formationLookDirection);
        _isAttacker = true;
    }

    private void StartCombat(IDamageable target)
    {
        Attack(target);
        HealthComponent.AddAttacker(target);
        AttackedCounterattack(target, this);
    }

    private void AttackedCounterattack(IDamageable attacked, IDamageable attacker)
    {
        if (attacked is Unit enemyUnit)
        {
            enemyUnit._combat.TrySetPersonalTarget(attacker);
            enemyUnit.Attack(attacker);
            enemyUnit.HealthComponent.AddAttacker(attacker);
            enemyUnit._isAttacker = false;
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
        _animationPlayer.Stop();

        var enemyInAttackDistance = _combat.FindEnemyInAttackDistance();
        if (enemyInAttackDistance != null)
        {
            StartCombat(enemyInAttackDistance);
            return;
        }

        var enemyInVision = _combat.FindEnemyInVision();
        GD.Print(enemyInVision);
        if (enemyInVision != null)
        {
            var enemyPosition = enemyInVision.GlobalPosition;
            var directionToEnemy = (enemyPosition - GlobalPosition).Normalized();
            SetState(UnitStates.Charging, enemyPosition, directionToEnemy);
            return;
        }

        SetState(UnitStates.Idle);

        // if (_isAttacker)
        // {
        //     var targetPosition = GlobalPosition + _movement.LookDirection * FormationData.Spacing;
        //     SetState(UnitStates.Charging, targetPosition, _formationLookDirection);
        // }
        // else
        // {
        //     SetState(UnitStates.BattleReady);
        // }
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
