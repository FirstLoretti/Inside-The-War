extends Node2D
class_name GridManager

@onready var tile_map = get_tree().get_first_node_in_group("main_map")
var occupied_tiles = {}

func target_cell_center(target_cell: Vector2i) -> Vector2:
	return tile_map.map_to_local(target_cell)
	
func target_cell(mouse_pos: Vector2) -> Vector2i:
	return tile_map.local_to_map(mouse_pos)
	
func is_cell_occupied(cell_coords: Vector2i, my_squad_id: int) -> bool:
	if not occupied_tiles.has(cell_coords):
		return false
	return occupied_tiles[cell_coords] != my_squad_id
	
func update_occupation(old_pos: Vector2, new_pos: Vector2, unit: Node):
	if old_pos.length() < 10:
		var new_cell = tile_map.local_to_map(new_pos)
		occupied_tiles[new_cell] = unit.squad_id
		return
		
	var old_cell = tile_map.local_to_map(old_pos)
	var new_cell = tile_map.local_to_map(new_pos)
	
	occupied_tiles.erase(old_cell)
	occupied_tiles[new_cell] = unit.squad_id

func set_occupied(cell: Vector2i, squad_id: int):
	occupied_tiles[cell] = squad_id

func _draw() -> void:
	var cell_size = 64
	var grid_width = 20
	var grid_height = 20
	var color = Color(1, 1, 1, 0.20)
	
	for i in range(grid_width + 1):
		draw_line(Vector2(i * cell_size, 0), Vector2(i * cell_size, grid_height * cell_size), color)
	for j in range(grid_height + 1):
		draw_line(Vector2(0, j * cell_size), Vector2(grid_width * cell_size, j * cell_size), color) 

func clear_squad_occupation(id: int):
	for cell in occupied_tiles.keys():
		if occupied_tiles[cell] == id:
			occupied_tiles.erase(cell)

func is_area_occupied(cell: Vector2i, radius: int) -> bool:
	for x in range(-radius, radius + 1):
		for y in range(-radius, radius + 1):
			var check_cell = cell + Vector2i(x, y)
			if occupied_tiles.has(check_cell):
				return true
	return false
	
