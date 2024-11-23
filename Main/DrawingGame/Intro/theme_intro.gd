extends Control

func _ready() -> void:
	set_physics_process(false)

func _physics_process(delta: float) -> void:
	%ThemeNameLabel.text = Params.drawing_theme.name
	if modulate.a > 0.6:
		set_modulate(lerp(get_modulate(), Color(1,1,1,0), 0.005))
	elif modulate.a > 0:
		set_modulate(lerp(get_modulate(), Color(1,1,1,0), 0.05))
	else:
		hide()
		set_physics_process(false)

func announce_new_theme() -> void:
	show()
	modulate = Color(1, 1, 1, 1)
	set_physics_process(true)
