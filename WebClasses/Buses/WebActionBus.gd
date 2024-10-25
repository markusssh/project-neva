extends Node

var actions: Array[GameAction] = []

func add_message_action(message: String) -> WebActionBus:
	actions.append(GameAction.new(Room.my_name, GameAction.ActionType.MESSAGE, [message]))
	return self

func add_game_start_action() -> WebActionBus:
	actions.append(GameAction.new(Room.my_name, GameAction.ActionType.START_GAME, []))
	return self

func add_sync_player_list_action(players: Array[String]) -> WebActionBus:
	actions.append(GameAction.new(Room.my_name, GameAction.ActionType.SYNC_PLAYER_LIST, players))
	return self

func add_sync_theme_action(theme_id: int) -> WebActionBus:
	actions.append(GameAction.new(Room.my_name, GameAction.ActionType.SYNC_THEME, [theme_id]))
	return self

func empty() -> bool:
	return actions.is_empty()

func clear() -> void:
	actions.clear()
