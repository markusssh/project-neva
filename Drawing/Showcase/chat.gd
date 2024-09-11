extends Node2D

@export var address: String = "127.0.0.1"
@export var port: int = 8910
var peer

func _ready():
	multiplayer.peer_connected.connect(peer_connected)
	multiplayer.peer_disconnected.connect(peer_disconnected)
	multiplayer.connected_to_server.connect(connected_to_server)
	multiplayer.connection_failed.connect(connection_failed)

func peer_connected(id):
	print("Player Connected " + str(id))
	start_chat()

func peer_disconnected(id):
	print("Player Disconnected " + str(id))

func connected_to_server():
	print("Connected To Sever!")
	send_player_info.rpc_id(1, multiplayer.get_unique_id(), _get_name_entered())
	start_chat()

func connection_failed():
	print("Couldnt Connect")

func host_chat():
	peer = ENetMultiplayerPeer.new()
	var error = peer.create_server(port, 2)
	if error:
		print("cannot host")
		return
	peer.get_host().compress(ENetConnection.COMPRESS_RANGE_CODER)
	multiplayer.set_multiplayer_peer(peer)
	send_player_info(multiplayer.get_unique_id(), _get_name_entered())
	print("Waiting For Players!")
	_freeze_start_ui()

func start_chat():
	%StartUI.hide()
	%ChatUI.show()

@rpc("any_peer")
func send_player_info(p_id, p_name) -> void:
	if not GameManager.players.has(p_id):
		GameManager.players[p_id] = {
			"name": p_name
		}
	if multiplayer.is_server():
		for id in GameManager.players:
			send_player_info.rpc(id, GameManager.players[id].name)

func _on_host_button_pressed() -> void:
	host_chat()

func _on_join_button_pressed() -> void:
	peer = ENetMultiplayerPeer.new()
	peer.create_client(address, port)
	peer.get_host().compress(ENetConnection.COMPRESS_RANGE_CODER)
	multiplayer.set_multiplayer_peer(peer)
	_freeze_start_ui()

func _freeze_start_ui() -> void:
	%HostButton.disabled = true
	%JoinButton.disabled = true
	%NameEdit.editable = false

func _get_name_entered() -> String:
	return %NameEdit.text
