extends Control

func _on_join_room_button_pressed() -> void:
	WebHandler.start_connection()
