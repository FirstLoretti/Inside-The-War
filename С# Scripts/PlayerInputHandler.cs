using Godot;
using InsideTheWar.Managers;
using InsideTheWar.Singletons;

namespace InsideTheWar;

public partial class PlayerInputHandler : Node2D
{
    [Export] private SelectionManager _selectionManager;
    [Export] private SpawnManager _spawnManager;
    [Export] private PlayerUnitManager _playerUnitManager;


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            var mousePos = GetGlobalMousePosition();
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                GD.Print("КЛИК ПОЛУЧЕН СИСТЕМОЙ");
                GlobalSignals.Instance.EmitSignal(GlobalSignals.SignalName.RequestSpawn, mousePos, "Player");

                if (Input.IsKeyPressed(Key.Shift))
                {
                    _selectionManager.SelectSquad(mousePos);
                }
                else if (Input.IsKeyPressed(Key.Ctrl))
                {
                    GlobalSignals.Instance.EmitSignal(GlobalSignals.SignalName.RequestSpawn, mousePos, "AI");
                }
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                _playerUnitManager.MoveSquadTo(mousePos);
            }
        }
    }
}
