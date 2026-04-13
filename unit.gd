extends CharacterBody2D
class_name Unit

@onready var animation_unit = $AnimationPlayer
var target_position: Vector2
@export var speed = 200.0
@export var move_points = 1.0
@export var move_cooldown = 3.0
@export var vision_radius:int = 0

var squad_id: int = -1
var is_done: bool = false
var can_move: bool = true
var last_position = Vector2.ZERO
var logical_cell: Vector2i

@export_group("Formation Settings")
@export var formation_cols: int = 3
@export var formation_rows: int = 3
@export var formation_spacing: int = 20
var my_col: int = 0
var my_row:  int = 0
@onready var sprite_2D = %Sprite2D

func _ready() -> void:
	target_position = global_position

func _process(delta: float) -> void:
	move_to_target(delta)
	
func move_to_target(delta: float):
	var distance = global_position.distance_to(target_position)
	
	if is_moving():
		var direction = global_position.direction_to(target_position)
		velocity = direction * speed
		
		if distance < 15.0: velocity *= (distance / 15.0)
		move_and_slide()
		
		if distance < 5.0:
			global_position = target_position
			velocity = Vector2.ZERO
			
		animation_unit.play("Run")
		sprite_2D.flip_h = direction.x < 0
	else:
		velocity = Vector2.ZERO
		global_position = target_position
		animation_unit.play("Idle")

func is_moving() -> bool:
	return global_position.distance_to(target_position) > 5.0
