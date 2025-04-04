extends Control

var player_name: String:
	set(val):
		player_name = val
		update_buttons()

var code: String:
	set(val):
		code = val
		update_buttons()

func _on_create_button_pressed() -> void:
	get_tree().set_meta("player_name", player_name)
	shoot_create_scene()

func _on_join_button_pressed() -> void:
	shoot_room_scene()

func shoot_create_scene() -> void:
	get_tree().change_scene_to_file("res://Main/Menu/Scenes/create_lobby.tscn")

func shoot_room_scene() -> void:
	get_tree().change_scene_to_file("res://Main/Menu/Scenes/lobby_room.tscn")


func _on_name_edit_text_changed(new_text: String) -> void:
	player_name = new_text

func _on_code_edit_text_changed(new_text: String) -> void:
	code = new_text

func update_buttons() -> void:
	if player_name.length() > 0:
		enable_button(%CreateButton)
		if code.length() > 0:
			enable_button(%JoinButton)
		else:
			disable_button(%JoinButton)
	else:
		disable_button(%CreateButton)
		disable_button(%JoinButton)

func disable_button(button: Button) -> void:
	button.disabled = true
	button.focus_mode = Control.FOCUS_NONE

func enable_button(button: Button) -> void:
	button.disabled = false
	button.focus_mode = Control.FOCUS_ALL

func _on_lobby_create_request_completed(result, response_code, headers, body):
	var json = JSON.new()
