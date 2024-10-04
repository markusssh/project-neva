class_name GameAction extends Resource

enum ActionType {
	SYNC_PLAYER_LIST,
	CONNECTED,
	MESSAGE,
	IMAGE
}

const ACTION_MAP = {
	"sync_player_list": ActionType.SYNC_PLAYER_LIST,
	ActionType.SYNC_PLAYER_LIST: "sync_player_list",
	"connected": ActionType.CONNECTED,
	ActionType.CONNECTED: "connected",
	"message": ActionType.MESSAGE,
	ActionType.MESSAGE: "message",
	"image": ActionType.IMAGE,
	ActionType.IMAGE: "image"
}

var from: String
var type: ActionType
var content: Array

func _init(from: String, type: ActionType, content: Array) -> void:
	self.from = from
	self.type = type
	self.content = content

func get_string_type() -> String:
	return ACTION_MAP[type]

static func parse_type_enum(type_str: String) -> ActionType:
	return ACTION_MAP[type_str]

static func parse_type_string(type_en: ActionType) -> String:
	return ACTION_MAP[type_en]
