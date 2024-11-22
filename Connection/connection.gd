extends Control

var ls: LabelSettings = preload("res://Assets/label_settings.tres")

func _ready() -> void:
	if OS.has_feature("debug"):
		_place_windows_layout()
	GameActionController.sync_player_list_complete.connect(update_player_list)
	GameActionController.game_started.connect(on_game_joined)

func _place_windows_layout():
	var arguments = {}
	for argument in OS.get_cmdline_args():
			# Parse valid command-line arguments into a dictionary
			if argument.find("=") > -1:
					var key_value = argument.split("=")
					arguments[key_value[0].lstrip("--")] = key_value[1]
	if arguments.has("name"):
		%PlayerName.text = arguments.get("name")
	if arguments.has("layout"):
		var placement: Vector2i
		match arguments.get("layout"):
			"left":
				placement = Vector2i.LEFT
			"right":
				placement = Vector2i.RIGHT
			"top":
				placement = Vector2i.UP
			"bottom":
				placement = Vector2i.DOWN
			"top_right":
				placement = Vector2i.ONE
			"top_left":
				placement = Vector2i(-1, 1)
			"bottom_left":
				placement = -Vector2i.ONE
			"bottom_right":
				placement = Vector2i(1, -1)
			"center":
				placement = Vector2i.ZERO
		var offset_pxl = 30
		get_window().size = DisplayServer.screen_get_size()
		get_window().size.x = get_window().size.x / 2 if abs(placement.x) > 0 else get_window().size.x
		get_window().size.y = get_window().size.y / 2 if abs(placement.y) > 0 else get_window().size.y - offset_pxl * 3
		get_window().position.x = 0 if placement.x <= 0 else get_window().size.x
		get_window().position.y = offset_pxl if placement.y <= 0 else get_window().size.y + offset_pxl

func _on_create_room_button_pressed() -> void:
	if OS.has_feature("debug"):
		WebClient.join_room(%PlayerName.text, WebClient.DEBUG_CODE)
		WebClient.hosting = true
	else:
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
	for player in Room.players:
		var l := Label.new()
		l.text = player.name
		l.label_settings = ls
		%PlayersVBoxContainer.add_child(l)

func _on_start_game_button_pressed() -> void:
	WebActionBus.add_game_start_action()
	get_tree().change_scene_to_file("res://DrawingGame/hosted_drawing_game.tscn")

func on_game_joined() -> void:
	get_tree().change_scene_to_file("res://DrawingGame/guest_drawing_game.tscn")
