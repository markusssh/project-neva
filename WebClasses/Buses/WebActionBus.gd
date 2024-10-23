extends Node

var actions: Array[GameAction] = []

func add_message_action(message: String) -> void:
	actions.append(GameAction.new(WebClient.room.my_name, GameAction.ActionType.MESSAGE, [message]))

func add_game_start_action() -> void:
	actions.append(GameAction.new(WebClient.room.my_name, GameAction.ActionType.START_GAME, []))

func add_sync_player_list_action(players: Array[String]) -> void:
	actions.append(GameAction.new(WebClient.room.my_name, GameAction.ActionType.SYNC_PLAYER_LIST, players))

func add_sync_theme_action(theme_id: int) -> void:
	actions.append(GameAction.new(WebClient.room.my_name, GameAction.ActionType.SYNC_THEME, [theme_id]))

func empty() -> bool:
	return actions.is_empty()

func clear() -> void:
	actions.clear()
