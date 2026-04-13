extends Node2D
class_name Player_Unit_Manager

@onready var grid_manager: GridManager = %GridManager
@onready var container = %PlayerUnits

var last_cell: Vector2i

func _ready() -> void:
	Signals.player_leader_position_changed.connect(_on_leader_position_changed)
	
func move_squad_to_cell(mouse_pos: Vector2):
	var all_units = container.get_children()
	var selected_units = container.get_children().filter(func(u): return u.is_selected)
	
	if selected_units.is_empty(): return
	
	for unit in all_units:
		if unit.global_position.distance_to(mouse_pos) < 25.0:
			print("Клик по юниту")
			return
			
	var leader = selected_units[0]
	
	var squad_center = Vector2.ZERO
	for unit in selected_units:
		squad_center += unit.global_position
	squad_center /= selected_units.size()
	
	if squad_center.distance_to(mouse_pos) < 50.0:
		return
		
	var target_cell = grid_manager.target_cell(mouse_pos) #Только проверка занятости или тумана
	var first_unit_cell = grid_manager.target_cell(leader.global_position)
	
	if first_unit_cell == target_cell:
		print("We are already here")
		return
	
	if grid_manager.is_cell_occupied(target_cell,leader.squad_id):
		print("Тайл занят")
		return
		
		
	grid_manager.clear_squad_occupation(leader.squad_id)
	grid_manager.update_occupation(leader.global_position, mouse_pos, leader)
	
	for unit in selected_units:
		if unit.move_points >= 1.0:
			#var old_cell = grid_manager.target_cell(leader.global_position)
			var offset = calculate_formation_offset(unit)
			
			unit.target_position = mouse_pos + offset
			
func calculate_formation_offset(unit) -> Vector2:
	var c = unit.my_col
	var r = unit.my_row
	var s = unit.formation_spacing
	var cols_count = unit.formation_cols
	var rows_count = unit.formation_rows
	var offset_x = (c - (cols_count - 1.0) / 2.0) * s
	var offset_y = (r - (rows_count - 1.0) / 2.0) * s
	return Vector2(offset_x, offset_y)

func _on_leader_position_changed(pos: Vector2, vision: int, squad_id: int):
	var current_cell = grid_manager.target_cell(pos)
	
	if current_cell != last_cell:
		Signals.entity_moved.emit(squad_id, current_cell, vision)
		
		last_cell = current_cell
