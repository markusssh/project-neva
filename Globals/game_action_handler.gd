class_name GameActionHandler extends Resource

signal sync_player_list_complete

func handle(action: GameAction) -> void:
	match action.type:
		GameAction.ActionType.SYNC_PLAYER_LIST:
			sync_player_list(action.content)

func sync_player_list(content: Array) -> void:
	for player_data in content:
		if player_data is not String:
			printerr("Wrong player data!")
		WebManager.room.players.append(player_data)
	sync_player_list_complete.emit
