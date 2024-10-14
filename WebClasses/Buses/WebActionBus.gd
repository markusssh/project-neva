class_name GameActionWebBus extends Resource

var actions: Array[GameAction] = []

func add_message_action(message: String) -> void:
	actions.append(GameAction.new(WebClient.room.my_name, GameAction.ActionType.MESSAGE, [message]))

func add_game_start_action() -> void:
	actions.append(GameAction.new(WebClient.room.my_name, GameAction.ActionType.START_GAME, []))

func empty() -> bool:
	return actions == []

func clear() -> void:
	actions.clear()

func _to_string() -> String:
	var res = "{\"actions\":["
	for action in actions:
		res += action.to_string() + ","
	res = res.left(-1) + "]}"
	return res
