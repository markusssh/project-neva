class_name GameActionHandler extends Resource

signal sync_player_list_complete

func handle(action: GameAction) -> void:
	match action.type:
		GameAction.ActionType.SYNC_PLAYER_LIST:
			sync_player_list(action.content)

func sync_player_list(content: Array) -> void:
	#TODO: FIX LOGIC? TRY ADDING MISSING PLAYERS
	WebManager.room.players = []
	for player_data in content:
		WebManager.room.players.append(player_data.get("name"))
	sync_player_list_complete.emit()
