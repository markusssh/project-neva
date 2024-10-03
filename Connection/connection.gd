extends Control

@export var player_name_label_template : Label

func _ready() -> void:
	WebManager.action_handler.sync_player_list_complete.connect(update_player_list)

func _on_create_room_button_pressed() -> void:
	WebManager.create_room(%PlayerName.text)

func _on_join_room_button_pressed() -> void:
	WebManager.join_room(%PlayerName.text, %RoomCode.text)

func update_player_list():
	%PlayersVBoxContainer.add_child()
