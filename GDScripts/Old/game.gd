extends Node2D

@onready var grid_manager: GridManager = %GridManager
@onready var spawn_manager = %SpawnManager
@onready var select_manager = %SelectManager
@onready var player_unit_manager = %PlayerUnitManager

#@onready var player_units = %PlayerUnits
#@onready var enemy_units = %EnemyUnits

func request_spawn(mouse_pos: Vector2, unit: PackedScene, team: String):
	var target_cell = grid_manager.target_cell(mouse_pos)
	#var all_units = player_units.get_children() + enemy_units.get_children()
	#
	#for u in all_units:
		#if u.global_position.distance_to(mouse_pos) < 60.0:
			#print("Слишком тесно для спавна")
			#return
	
	if grid_manager.is_area_occupied(target_cell, 1):
		print("Занято")
		return
	
	var new_id = spawn_manager.spawn_squad(mouse_pos, unit, team)
	#if new_id != -1:
	grid_manager.occupied_tiles[target_cell] = new_id
		
func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed:
		var mouse_pos = get_global_mouse_position()
		if event.button_index == MOUSE_BUTTON_LEFT:
			if Input.is_key_pressed(KEY_SHIFT):
				select_manager.select_squad(mouse_pos)
			if Input.is_key_pressed(KEY_CTRL):
				request_spawn(mouse_pos, spawn_manager.unit_blue, "Enemy")
			else:
				request_spawn(mouse_pos, spawn_manager.unit_red, "Player")
		elif event.button_index == MOUSE_BUTTON_RIGHT:
			player_unit_manager.move_squad_to_cell(mouse_pos)
