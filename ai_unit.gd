extends Unit
class_name AI_Unit

@export_group("AI Settings")
@export var decision_time_min: float = 1.0
@export var decision_time_max: float = 4.0

var decision_move_timer: float = -1.0
var is_ready_to_move: bool = false
var is_thinking: bool = false
#@export var move_range: Vector2i = Vector2i(1, 4)
#@export var rest_chance: float = 0.3
#var is_selected: bool = false

func _ready():
	super._ready()
	
func _process(delta: float) -> void:
	super(delta)
	
	if is_ready_to_move:
		return
		
	move_decision(delta)
	
func move_decision(delta: float):
	if not is_moving() and move_points >= 1.0:
		if decision_move_timer < 0.0:
			decision_move_timer = randf_range(decision_time_min, decision_time_max)
			print("Юнит ", squad_id, " начал думать на ", decision_move_timer)
			
		elif decision_move_timer > 0.0:
			decision_move_timer -= delta
			
			if decision_move_timer <= 0.0:
				decision_move_timer = 0.0
				is_ready_to_move = true
				print("Юнит ", squad_id, " ГОТОВ")
