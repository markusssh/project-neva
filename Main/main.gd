extends Node

func _process(_delta: float) -> void:
	if not OS.has_feature("dedicated_server"):
		get_tree().change_scene_to_file("res://Main/Menu/Scenes/main_menu.tscn")
	set_process(false)
