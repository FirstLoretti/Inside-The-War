extends Node

@onready var fog_unexplored = %FogUnexplored
@onready var fog_explored = %FogExplored

var explored_cells = []
var squad_vision: Dictionary = {}  # { id: { "cell": Vector2i, "vision": int } }

func _ready() -> void:
	#Signals.entity_spawned.connect(_on_entity_spawned)
	Signals.entity_moved.connect(_on_entity_moved)
	#Signals.entity_removed.connect(_on_entity_removed())

#func _on_entity_spawned(cell: Vector2i, vision: int, id: int):
	#squad_vision[id] = {"cell": cell, "vision": vision}
	#
	#reveal_and_remember(cell, vision)
	#
	#refresh_vision()

func _on_entity_moved(id: int, new_cell: Vector2i, vision: int):
	squad_vision[id] = {"cell": new_cell, "vision": vision}
	
	reveal_and_remember(new_cell, vision)
	
	refresh_vision()

func remove_fog(layer: TileMapLayer, cell: Vector2i, vision: int, source_id: int, atlas_coords: Vector2i):
	for x in range(-vision, vision + 1):
		for y in range(-vision, vision + 1):
			var target_cell = cell + Vector2i(x, y)
			
			layer.set_cell(target_cell, source_id, atlas_coords)

func refresh_vision():
	for cell in explored_cells:
		fog_explored.set_cell(cell, 0, Vector2i(0,0))
	
	for unit_id in squad_vision:
		var data = squad_vision[unit_id]
		remove_fog(fog_explored, data.cell, data.vision, -1, Vector2i(-1, -1))

func reveal_and_remember(target_center: Vector2i, vision: int):
	for x in range(-vision, vision + 1):
		for y in range(-vision, vision + 1):
			var target_cell = target_center + Vector2i(x, y)
			
			fog_unexplored.set_cell(target_cell, -1)
			
			if not explored_cells.has(target_cell):
				explored_cells.append(target_cell)
