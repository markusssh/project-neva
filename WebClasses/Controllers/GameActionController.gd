class_name GameActionController extends Resource

signal sync_player_list_complete
signal image_recieved(i: Image)
signal game_started

func handle(action: GameAction) -> void:
	match action.action_type:
		GameAction.ActionType.SYNC_PLAYER_LIST:
			sync_player_list(action.content)
		GameAction.ActionType.IMAGE:
			recieve_image(action.content[0])
			image_recieved.emit(action.content[0])
		GameAction.ActionType.START_GAME:
			game_started.emit()

func sync_player_list(content) -> void:
	WebClient.room.players.clear()
	for name in content:
		if name is String:
			WebClient.room.players.append(Player.new(name))
		else:
			#TODO: LOG
			printerr("GameActionController error: Wrong Player Data")
	sync_player_list_complete.emit()

func recieve_image(content) -> void:
	if content is Image:
		image_recieved.emit(content)
	else:
		#TODO: LOG
		printerr("GameActionController error: Not An Image")
