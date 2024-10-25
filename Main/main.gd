extends Node2D

@onready var lobby_id = $LobbyID

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_host_pressed() -> void:
	Network.create_lobby()


func _on_join_pressed() -> void:
	var id: int = int(lobby_id.text)
	Network.join_lobby(id)
