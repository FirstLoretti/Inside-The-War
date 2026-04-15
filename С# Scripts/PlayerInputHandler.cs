using Godot;
using InsideTheWar.Managers;

namespace InsideTheWar.PlayerInput;

public partial class PlayerInputHandler : Node2D
{
    [Export] private SelectionManager _selectManager;
    [Export] private SpawnManager _spawnManager;
    [Export] private PlayerUnitManager _playerUnitManager;


    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            var mousePos = GetGlobalMousePosition();
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (Input.IsKeyPressed(Key.Shift))
                {
                    _selectManager.SelectSquad(mousePos);
                }
                else if (Input.IsKeyPressed(Key.Ctrl))
                {
                    _spawnManager.RequestSpawn(mousePos, "Enemy");
                }
                else
                {
                    _spawnManager.RequestSpawn(mousePos, "Player");
                }
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                _playerUnitManager.MoveSquadTo(mousePos);
            }

        }
    }

}
