class_name GameAction extends WebSocketRequest

enum ActionType {
	SYNC_PLAYER_LIST,
	START_GAME,
	CONNECTED,
	MESSAGE,
	IMAGE
}

var from: String
var action_type: ActionType
var content: Array

func _init(from: String, action_type: ActionType, content: Array) -> void:
	type = WebSocketRequest.RequestType.GAME_ACTION
	self.from = from
	self.action_type = action_type
	self.content = content

func _to_string() -> String:
	var a = "{\"type\":\"GAME_ACTION\",\"from\":\"%s\",\"actionType\":\"%s\",\"content\":%s}" % [
				from,
				GameAction.ActionType.keys()[action_type],
				str(content)
			]
	return a
