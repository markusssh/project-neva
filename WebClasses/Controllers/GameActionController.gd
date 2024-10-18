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

func sync_player_list(players: Array[Player]) -> void:
	WebClient.room.players.clear()
	for player in players:
		WebClient.room.players.append(player)
	sync_player_list_complete.emit()
