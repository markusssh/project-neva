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

func _init(p_from: String = "", p_action_type: ActionType = ActionType.NONE, p_content: Array = []) -> void:
	type = WebSocketRequest.RequestType.GAME_ACTION
	from = p_from
	action_type = p_action_type
	content = p_content
