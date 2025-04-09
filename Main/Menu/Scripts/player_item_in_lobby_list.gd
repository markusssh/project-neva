class_name LobbyListPlayerItem
extends PanelContainer

signal kick_requested

@export var is_creator: bool = false:
	set(value):
		is_creator = not can_kick and value # ИЛИ хост ИЛИ можно кикнуть
@export var can_kick: bool = false:
	set(value):
		can_kick = not is_creator and value

var player_name: String = "Name"

func _ready() -> void:
	%Name.text = player_name
	if is_creator:
		%Host.show()
	elif can_kick:
		%Kick.show()

static func create
(p_is_creator: bool, 
p_can_kick: bool,
p_player_name: String) -> LobbyListPlayerItem:
	var res := LobbyListPlayerItem.new()
	res.is_creator = p_is_creator
	res.can_kick = p_can_kick
	res.player_name = p_player_name
	return res

func _on_kick_pressed() -> void:
	kick_requested.emit()
