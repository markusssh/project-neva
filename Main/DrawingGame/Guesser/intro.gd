class_name Intro extends Control

func _init() -> void:
	set_physics_process(false)
	show()
	MultiplayerController.RoundStarted.connect(_on_round_start)

func _physics_process(delta: float) -> void:
	if modulate.a > 0.6:
		set_modulate(lerp(get_modulate(), Color(1,1,1,0), 0.005))
	elif modulate.a > 0:
		set_modulate(lerp(get_modulate(), Color(1,1,1,0), 0.05))
	else:
		hide()
		set_physics_process(false)

func _on_round_start() -> void:
	set_physics_process(true)
