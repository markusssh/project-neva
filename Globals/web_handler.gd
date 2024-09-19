extends Node

const WEBSOCKET_URL = "ws://localhost:8080/ws"

var socket = WebSocketPeer.new()

func _ready():
	set_process(false)

func start_connection() -> void:
	var err = socket.connect_to_url(WEBSOCKET_URL)
	if err != OK:
		print("Unable to connect")
		set_process(false)
	else:
		await get_tree().create_timer(2).timeout
		socket.send_text("Test packet")

func _process(_delta):
	socket.poll()
	var state = socket.get_ready_state()
	match state:
		WebSocketPeer.STATE_OPEN:
			print("Got data from server: ", socket.get_packet().get_string_from_utf8())
		WebSocketPeer.STATE_CLOSING:
			pass
		WebSocketPeer.STATE_CLOSED:
			var code = socket.get_close_code()
			print("WebSocket closed with code: %d. Clean: %s" % [code, code != -1])
			set_process(false)
