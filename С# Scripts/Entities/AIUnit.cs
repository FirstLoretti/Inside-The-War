using Godot;
using InsideTheWar.Helpers;
using System;

namespace InsideTheWar.Entities;

public partial class AIUnit : Unit
{
    [Export] private float _movementRadiusMin = 50.0f;
    [Export] private float _movementRadiusMax = 200.0f;

    private void SetRandomDestination()
    {
        TargetPosition = GameMath.GetRandomPointInCircle(GlobalPosition, _movementRadiusMin, _movementRadiusMax);
    }
}
