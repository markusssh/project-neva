extends Node

@export var handshake_headers: PackedStringArray
@export var supported_protocols: PackedStringArray
var tls_options: TLSOptions = null

var socket := WebSocketPeer.new()
var last_state := WebSocketPeer.STATE_CLOSED
var image_buff: Array = []
var buff_collecting: bool = false

signal connected_to_server()
signal connection_closed()
#signal message_received(message: Variant)
signal action_recieved(action: GameAction)

func connect_to_url(url: String) -> int:
	handshake_headers.append("roomId: " + WebClient.room.id)
	
	socket.supported_protocols = supported_protocols
	socket.handshake_headers = handshake_headers

	var err := socket.connect_to_url(url, tls_options)
	if err != OK:
		return err

	last_state = socket.get_ready_state()
	return OK


func send(message: String) -> int:
	if typeof(message) == TYPE_STRING:
		return socket.send_text(message)
	return socket.send(var_to_bytes(message))


func get_message() -> Variant:
	if socket.get_available_packet_count() < 1:
		return null
	var pkt := socket.get_packet()
	if socket.was_string_packet():
		return pkt.get_string_from_utf8()
	return bytes_to_var(pkt)


func close(code: int = 1000, reason: String = "") -> void:
	socket.close(code, reason)
	last_state = socket.get_ready_state()


func clear() -> void:
	socket = WebSocketPeer.new()
	last_state = socket.get_ready_state()


func get_socket() -> WebSocketPeer:
	return socket


func poll() -> void:
	if socket.get_ready_state() != socket.STATE_CLOSED:
		socket.poll()

	var state := socket.get_ready_state()

	if last_state != state:
		last_state = state
		if state == socket.STATE_OPEN:
			connected_to_server.emit()
		elif state == socket.STATE_CLOSED:
			connection_closed.emit()
	while socket.get_ready_state() == socket.STATE_OPEN and socket.get_available_packet_count():
		var message = get_message()
		var game_data = JSON.parse_string(str(message))
		if game_data != null:
			_handle_game_data(game_data)

func _handle_game_data(d):
	var type = d.get("type")
	if type == "IMAGE_PACKET":
		_handle_image_packet(ImagePacket.new(
			d.get("packet_num"),
			d.get("total_packets"),
			d.get("data")
		))
	elif type == "GAME_ACTION":
		action_recieved.emit(
			GameAction.new(
				d.get("from"),
				GameAction.ActionType.get(d.get("actionType")),
				d.get("content")
			)
		)

func _handle_image_packet(packet: ImagePacket):
	if not buff_collecting:
		buff_collecting = true
		image_buff.resize(packet.total_packets)
	image_buff[packet.packet_num] = packet.data
	if not image_buff.has(null):
		_assemble_image()
		buff_collecting = false
		image_buff.clear()

func _assemble_image() -> void:
	var full_image_data := PackedByteArray()
	for chunk in image_buff:
		full_image_data.append_array(chunk)
	full_image_data = full_image_data.decompress(0, FileAccess.CompressionMode.COMPRESSION_ZSTD)
	action_recieved.emit(
		GameAction.new(
			"host",
			GameAction.ActionType.IMAGE,
			[Image.create_from_data(
				Params.CANV_W, 
				Params.CANV_H, 
				false, 
				Image.FORMAT_BPTC_RGBA, 
				full_image_data
			)]
		)
	)

func _process(_delta: float) -> void:
	poll()
