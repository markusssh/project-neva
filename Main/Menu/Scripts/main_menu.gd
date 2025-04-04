extends Control

@export var disappear_anim_curve: Curve

const DELAY_AFTER_START_LENGTH = 0.2

var disappear_progress: float = 0
var delay_progress: float = 0.0

func _ready() -> void:
	%AfterIntro.hide()
	set_process(false)

func _on_start_pressed() -> void:
	set_process(true)

func _process(delta: float) -> void:
	if disappear_progress < 1:
		disappear_progress += delta * 0.75
		%Start.material.set_shader_parameter(
			"progress", disappear_anim_curve.sample(disappear_progress))
	else:
		if delay_progress < DELAY_AFTER_START_LENGTH:
			delay_progress += delta
		else:
			shoot_next_scene()
			set_process(false)

func shoot_next_scene() -> void:
	get_tree().change_scene_to_file("res://Main/Menu/Scenes/enter_lobby.tscn")
