extends Control

@export var room_code_line: LineEdit

func _on_create_room_button_pressed() -> void:
	var err = WebClient.create_room()
	if err != OK:
		OS.alert("Couldn't connect to the server!", "Network Error")

func _on_join_room_button_pressed() -> void:
	var id = room_code_line.text
	WebClient.connect_to_room(room_code_line.text)
