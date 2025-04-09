class_name LobbyListPlayerItem
extends PanelContainer

signal kick_requested

func setup
(is_creator: bool, 
can_kick: bool,
player_name: String) -> void:
	if is_creator:
		%Creator.show()
	elif can_kick:
		%Kick.show()
	%Name.text = player_name

func _on_kick_pressed() -> void:
	kick_requested.emit()
