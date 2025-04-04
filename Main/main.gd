class_name Main extends Node

# Список используемых плагинов и утилит:
# 1. Script-IDE https://github.com/Maran23/script-ide
# 2. rcedit https://github.com/electron/rcedit?tab=readme-ov-file
# 3. 

var PlayerLabelTemplate = preload("res://Main/MainUI/player_label_template.tscn")
var player_labels: Dictionary

func _ready() -> void:
	if OS.has_feature("debug"):
		_place_windows_layout()

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
				placement = Vector2i(1, -1)
			"top_left":
				placement = -Vector2i.ONE
			"bottom_left":
				placement = Vector2i(-1, 1)
			"bottom_right":
				placement = Vector2i.ONE
			"center":
				placement = Vector2i.ZERO
		var offset_pxl = 30
		get_window().size = DisplayServer.screen_get_size()
		get_window().size.x = get_window().size.x / 2 if abs(placement.x) > 0 else get_window().size.x
		get_window().size.y = get_window().size.y / 2 if abs(placement.y) > 0 else get_window().size.y - offset_pxl * 3
		get_window().position.x = 0 if placement.x <= 0 else get_window().size.x
		get_window().position.y = offset_pxl if placement.y <= 0 else get_window().size.y + offset_pxl

func _on_join_game_button_pressed() -> void:
	%JoinGameButton.disabled = true
	if await Networking.JoinGame() > OK:
		OS.alert("Cannot connect to server!")
		%JoinGameButton.disabled = false
	else:
		%JoinGameContainer.hide()
		%PlayerCountLabel.show()
		%PlayerNamesContainer.show()
		MultiplayerController.PlayerJoinedLobby.connect(_on_player_joined_room)
		MultiplayerController.PlayerLeftLobby.connect(_on_player_left_room)

func _update_player_num() -> void:
	%PlayerCountLabel.text = "Ожидаем игроков: %d/%d" % [
		MultiplayerController.Client_Players.size(),
		MultiplayerController.Client_MaxPlayers
	]

func _on_player_joined_room(player_id: int) -> void:
	var player_label = PlayerLabelTemplate.instantiate()
	player_label.text = MultiplayerController.Client_Players[player_id].PlayerName
	player_labels[player_id] = player_label
	%PlayerNamesContainer.add_child(player_label)
	_update_player_num()

func _on_player_left_room(player_id: int) -> void:
	if player_labels.has(player_id):
		var to_delete = player_labels[player_id]
		player_labels.erase(player_id)
		to_delete.queue_free()
		_update_player_num()
