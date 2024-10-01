class_name JSONActionWrapper extends Resource

static func wrap_actions(actions: Array[Dictionary]) -> String:
	var res := "{\"from\":\"%s\",\"actions\":[" % WebManager.room.my_name
	for action in actions:
		res = _add_action(res, action.get("type"), action.get("content"))
	return res

static func _add_action(r: String, t: String, c: String) -> String:
	return r + "{\"type\":\"%s\",\"content\":%s}," % [t, c]
