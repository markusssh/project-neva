class_name JSONRequestWrapper extends Resource

static func wrap_actions(actions: Array[GameAction]) -> String:
	var from := WebManager.room.my_name
	var res := "{\"actions\":["
	for action in actions:
		res += "{\"from\":\"%s\",\"type\":\"%s\",\"content\":%s}," % [
				from,
				action.get_string_type(), 
				str(action.content)
			]
	res = res.left(-1)
	res += "]}"
	return res
