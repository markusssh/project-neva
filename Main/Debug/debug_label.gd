extends Label

func _ready() -> void:
	text = str(multiplayer.multiplayer_peer.get_unique_id())
