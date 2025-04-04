class_name LobbyListPlayerItem
extends PanelContainer

signal kick_requested(player_id: int)

@export var is_host: bool = false
@export var can_kick: bool = false

var player_id: int
var player_name: String = "Name"

func _ready() -> void:
	%Name.text = player_name
	if is_host:
		%Host.show()
	elif can_kick:
		%Kick.show()

func _on_kick_pressed() -> void:
	kick_requested.emit(player_id)
