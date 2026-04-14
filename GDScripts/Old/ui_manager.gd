extends CanvasLayer
class_name UI_Manager

@onready var squad_panel = %SquadPanel
@onready var squad_label = %CountLabel
@onready var container = %PlayerUnits

func _process(delta: float) -> void:
	show_squad_label()
	
func show_squad_label():
	var selected_units = container.get_children().filter(func(u): return u.is_selected)
	if selected_units.size() > 0:
		squad_panel.visible = true
		
		var center_pos = Vector2.ZERO
		var move_points = 0
		for unit in selected_units:
			move_points = int(unit.move_points)
			center_pos += unit.global_position

		center_pos /= selected_units.size()
		squad_panel.global_position = center_pos - (squad_panel.size / 2) + Vector2(0, -70)
		squad_label.text = str(move_points)
	else:
		squad_panel.visible = false
