extends Unit

var is_selected: bool = false

@onready var player_units_container = get_tree().get_first_node_in_group("player_units_container")

func _process(delta: float) -> void:
	super(delta)
	
	if is_squad_leader() and is_moving():
		Signals.player_leader_position_changed.emit(global_position, vision_radius, squad_id)
		
	on_select()

func on_select():
	if is_selected:
		%Sprite2D.modulate = Color(1.25, 1.25, 1.25)
	else:
		%Sprite2D.modulate = Color(1, 1, 1)

func is_squad_leader() -> bool:
	var my_squad = player_units_container.get_children().filter(func(u): return u.squad_id == self.squad_id)
	
	return my_squad.size() > 0 and my_squad[0] == self
