extends Node2D

@onready var grid_manager: GridManager = %GridManager
@onready var container = %EnemyUnits

func movement_logic(delta: float):
	var all_enemy_units = container.get_children()
	if all_enemy_units.is_empty(): return
	
	var squads = group_by_squad(all_enemy_units)
	
	for squad_id in squads:
		var squad_members = squads[squad_id]
		var leader: AI_Unit = squad_members[0]
		
		if leader.move_points >= 1.0 and leader.is_ready_to_move:
				var directions = [Vector2i.UP, Vector2i.DOWN, Vector2i.LEFT, Vector2i.RIGHT]
				var random_dir = directions.pick_random()
			
				var target_cell = leader.logical_cell + random_dir
		
				if not grid_manager.is_cell_occupied(target_cell, squad_id):
					move_squad_to(squad_members, target_cell, squad_id)
					
					for unit in squad_members:
						unit.is_ready_to_move = false
						unit.decision_move_timer = -1.0
					
func move_squad_to(units: Array, target_cell: Vector2i, squad_id: int):
	var target_center = grid_manager.target_cell_center(target_cell)
	
	grid_manager.update_occupation(units[0].global_position, target_center, units[0])	
			
	for unit in units:
		unit.logical_cell = target_cell
		
		var offset = calculate_formation_offset(unit)
		unit.target_position = target_center + offset
		
		unit.move_points -= 1.0
		
func calculate_formation_offset(unit) -> Vector2:
	var c = unit.my_col
	var r = unit.my_row
	var s = unit.formation_spacing
	var cols_count = unit.formation_cols
	var rows_count = unit.formation_rows
	var offset_x = (c - (cols_count - 1.0) / 2.0) * s
	var offset_y = (r - (rows_count - 1.0) / 2.0) * s
	return Vector2(offset_x, offset_y)

func group_by_squad(units: Array) -> Dictionary:
	var squads = {}
	
	for unit in units:
		if not squads.has(unit.squad_id): squads[unit.squad_id] = []
		squads[unit.squad_id].append(unit)
		
	return squads

func _on_timer_timeout() -> void:
	var delta = get_process_delta_time()
	movement_logic(delta)
