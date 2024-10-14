extends Control

@export var player_name_label_template : Label
var ls: LabelSettings = preload("res://Assets/label_settings.tres")

func _ready() -> void:
	WebClient.action_controller.sync_player_list_complete.connect(update_player_list)
	WebClient.action_controller.game_started.connect(on_game_joined)

func _on_create_room_button_pressed() -> void:
	WebClient.create_room(%PlayerName.text)
	view_lobby()

func _on_join_room_button_pressed() -> void:
	WebClient.join_room(%PlayerName.text, %RoomCode.text)
	view_lobby()

func view_lobby() -> void:
	await WebClient.connected
	%RoomVBoxContainer.hide()
	%StartGameContainer.show()
	if WebClient.hosting:
		%StartGameButton.show()

func update_player_list():
	#ALERT: FIX LOGIC
	for child in %PlayersVBoxContainer.get_children():
		child.queue_free()
	for player in WebClient.room.players:
		var l := Label.new()
		l.text = player
		l.label_settings = ls
		%PlayersVBoxContainer.add_child(l)

func _on_start_game_button_pressed() -> void:
	WebClient.action_bus.add_game_start_action()
	get_tree().change_scene_to_file("res://DrawingGame/hosted_drawing_game.tscn")

func on_game_joined() -> void:
	get_tree().change_scene_to_file("res://DrawingGame/guest_drawing_game.tscn")
