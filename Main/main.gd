class_name Main extends Node

## Список используемых плагинов и утилит:
## 1. Script-IDE https://github.com/Maran23/script-ide
## 2. rcedit https://github.com/electron/rcedit?tab=readme-ov-file

var PlayerLabelTemplate = preload("res://Main/MainUI/player_label_template.tscn")
var player_labels: Dictionary

func _ready() -> void:
	pass


func _on_join_game_button_pressed() -> void:
	%JoinGameButton.disabled = true
	if await Networking.JoinGame() > OK:
		OS.alert("Cannot connect to server!")
		%JoinGameButton.disabled = false
	else:
		%JoinGameContainer.hide()
		%PlayerCountLabel.show()
		%PlayerNamesContainer.show()
		MultiplayerController.PeerJoinedRoom.connect(_on_player_joined_room)
		MultiplayerController.PeerLeftRoom.connect(_on_player_left_room)

func _on_player_joined_room(player_id: int) -> void:
	var player_label = PlayerLabelTemplate.instantiate()
	player_label.text = MultiplayerController.CurrentRoomPeers[player_id].PlayerName
	player_labels[player_id] = player_label
	%PlayerNamesContainer.add_child(player_label)

func _on_player_left_room(player_id: int) -> void:
	if player_labels.has(player_id):
		var to_delete = player_labels[player_id]
		player_labels.erase(player_id)
		to_delete.queue_free()
