class_name GameActionController extends Resource

signal sync_player_list_complete
signal image_recieved(i: Image)
signal game_started

func handle(action: GameAction) -> void:
	match action.action_type:
		GameAction.ActionType.SYNC_PLAYER_LIST:
			sync_player_list(action.content)
		GameAction.ActionType.IMAGE:
			image_recieved.emit(action.content[0])
		GameAction.ActionType.START_GAME:
			game_started.emit()

func sync_player_list(content: Array) -> void:
	#TODO: FIX LOGIC? TRY ADDING MISSING PLAYERS
	WebClient.room.players = []
	for player_data in content:
		WebClient.room.players.append(player_data.get("name"))
	sync_player_list_complete.emit()
