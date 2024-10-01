extends Control

func _on_create_room_button_pressed() -> void:
	WebManager.create_room(%PlayerName.text)

func _on_join_room_button_pressed() -> void:
	WebManager.join_room(%PlayerName.text, %RoomCode.text)
