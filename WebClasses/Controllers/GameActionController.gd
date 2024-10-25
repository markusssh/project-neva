extends Node

signal sync_player_list_complete
signal sync_theme_complete
signal image_recieved(i: Image)
signal message_recieved(f: String, m: String)
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
		GameAction.ActionType.MESSAGE:
			recieve_message(action.from, action.content[0])
		GameAction.ActionType.SYNC_THEME:
			sync_theme(action.content[0])

func sync_player_list(content) -> void:
	Room.players.clear()
	for p_name in content:
		if p_name is String:
			var p := Player.new(p_name)
			Room.players.append(p)
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

func recieve_message(from, content) -> void:
	if content is String and from is String:
		message_recieved.emit(from, content)
	else:
		#TODO: LOG
		printerr("GameActionController error: Not An Image")

func sync_theme(content) -> void:
	if content is int:
		Params.set_theme_by_idx(content)
		sync_theme_complete.emit()
	else:
		#TODO: LOG
		printerr("GameActionController error: Not An Integer")
