extends Control

var creator_id: int
var is_creator: bool
var player_list_items: Dictionary

func _ready() -> void:
	MultiplayerController.PlayerJoinedLobby.connect(_on_player_joined)
	MultiplayerController.PlayerLeftLobby.connect(_on_player_left)
	
	creator_id = MultiplayerController.Client_CreatorId
	is_creator = MultiplayerController.Client_IsCreator
	%CopyCode.text = MultiplayerController.Client_LobbyCode
	
	for player in MultiplayerController.Client_Players.values:
		var player_id = player.Id
		var item_is_creator = creator_id == player_id
		var item_can_kick = false
		
		if is_creator:
			if player_id != MultiplayerController.Client_Id:
				item_can_kick = true
		
		var p_item := LobbyListPlayerItem.create(
			item_is_creator,
			item_can_kick,
			player.Name
		)
		player_list_items[player_id] = p_item
		%PlayerList.add_child(p_item)
		p_item.kick_requested.connect(_on_kick_requested.bind(player_id))

func _on_player_joined(player_id: int):
	var item_can_kick = false
	if is_creator:
			if player_id != MultiplayerController.Client_Id:
				item_can_kick = true
	var item_player_name = MultiplayerController \
		.Client_Players[player_id].Name
	var p_item := LobbyListPlayerItem.create(
			false,
			item_can_kick,
			item_player_name
		)
	player_list_items[player_id] = p_item
	%PlayerList.add_child(p_item)
	p_item.kick_requested.connect(_on_kick_requested.bind(player_id))

func _on_player_left(player_id: int):
	var item = player_list_items.get(player_id)
	if item == null:
		return
	item.queue_free()
	player_list_items.erase(player_id)

func _on_kick_requested(player_id: int):
	MultiplayerController.Client_RequestKick(player_id)

func _on_copy_code_pressed() -> void:
	DisplayServer.clipboard_set(%CopyCode.text)
