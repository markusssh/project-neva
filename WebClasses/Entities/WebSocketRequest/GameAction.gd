class_name GameAction extends WebSocketRequest

enum ActionType {
	NONE,
	SYNC_PLAYER_LIST,
	START_GAME,
	CONNECTED,
	MESSAGE,
	IMAGE
}

var from: String
var action_type: ActionType
var content: Array

func _init(from: String = "", action_type: ActionType = ActionType.NONE, content: Array = []) -> void:
	type = WebSocketRequest.RequestType.GAME_ACTION
	self.from = from
	self.action_type = action_type
	self.content = content
