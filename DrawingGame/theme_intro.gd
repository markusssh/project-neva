extends Control

func _process(delta: float) -> void:
	if modulate != Color(1, 1, 1, 0.2):
		set_modulate(lerp(get_modulate(), Color(1,1,1,0), 0.01))
	else:
		set_modulate(Color(1, 1, 1, 0))
		set_process(false)
