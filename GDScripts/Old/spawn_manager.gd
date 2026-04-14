extends Node
class_name Spawn_Manager

@export var unit_red: PackedScene
@export var unit_blue: PackedScene

var last_squad_id = 0

@onready var containers = {
	"PlayerUnits": %PlayerUnits,
	"EnemyUnits": %EnemyUnits
}

func spawn_squad(spawn_pos: Vector2, unit: PackedScene, team: String ) -> int:
	last_squad_id += 1
	
	var current_id = last_squad_id
	var leader: Unit = null
	
	var parent_node = containers.get(team + "Units")
	
	var dobby = unit.instantiate()
	var rows = dobby.formation_rows
	var cols = dobby.formation_cols
	var spacing = dobby.formation_spacing
	var vision = dobby.vision_radius
	dobby.free()

	for row in range(rows):
		for col in range(cols):
			var new_unit = unit.instantiate()
			new_unit.my_col = col
			new_unit.my_row = row
			new_unit.formation_cols = cols
			new_unit.formation_rows = rows
			new_unit.formation_spacing = spacing
			new_unit.squad_id = current_id
			#new_unit.logical_cell = target_cell
			
			new_unit.add_to_group(team + "Units")
			
			var offset_x = (float(col) - (float(cols) - 1.0) / 2.0) * spacing
			var offset_y = (float(row) - (float(rows) - 1.0) / 2.0) * spacing
			var final_offset = Vector2(offset_x, offset_y)
			
			new_unit.global_position = spawn_pos + final_offset
			
			parent_node.add_child(new_unit)
			
			#new_unit.force_update_transform()
			
			if leader == null:
				leader = new_unit
				
	if team == "Player":
		Signals.entity_moved.emit(leader.squad_id, spawn_pos, vision)
	
	return current_id
