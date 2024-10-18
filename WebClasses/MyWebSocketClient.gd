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

func send_action_bus(bus: GameActionWebBus):
	for action in bus.actions:
		socket.send(WebRequestSerializer.to_bytes(action))

func send_image_bus(bus: ImageWebBus):
	for packet in bus.packetize_image():
		socket.send(WebRequestSerializer.to_bytes(packet))

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
		var packet = socket.get_packet()
		print(packet.get_string_from_utf8())
		var req = WebRequestSerializer.from_bytes(packet)
		if req is GameAction:
			handle_game_action(req)
		elif req is ImagePacket:
			handle_image_packet(req)
		else:
			#TODO: логирование
			printerr("Wrong WebSocketRequest Type Fromat")

func handle_game_action(action: GameAction):
	action_recieved.emit(action)

func handle_image_packet(packet: ImagePacket):
	if not buff_collecting:
		buff_collecting = true
		image_buff.resize(packet.total_packets)
	image_buff[packet.packet_num] = packet.data
	if not image_buff.has(null):
		assemble_image(packet.original_size)
		buff_collecting = false
		image_buff.clear()

func assemble_image(original_size: int) -> void:
	var full_image_data := PackedByteArray()
	for chunk in image_buff:
		full_image_data.append_array(chunk)
	full_image_data = full_image_data.decompress(original_size, FileAccess.CompressionMode.COMPRESSION_ZSTD)
	action_recieved.emit(
		GameAction.new(
			"host",
			GameAction.ActionType.IMAGE,
			[Image.create_from_data(
				Params.CANV_W, 
				Params.CANV_H, 
				false, 
				Image.FORMAT_RGBA8, 
				full_image_data
			)]
		)
	)

func _process(_delta: float) -> void:
	poll()
