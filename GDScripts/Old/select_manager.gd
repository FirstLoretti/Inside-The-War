extends Node
@onready var container = %PlayerUnits

func select_squad(mouse_pos: Vector2):
	var player_units = %PlayerUnits.get_children()
	for unit in player_units:
		unit.is_selected = false
	for unit in player_units:
		if unit.global_position.distance_to(mouse_pos) < 30:
			var target_id = unit.squad_id
			for squad_member in player_units:
				if squad_member.squad_id == target_id:
					squad_member.is_selected = true
			break
