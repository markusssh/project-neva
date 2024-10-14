extends Node

signal connected

@export var HTTP_URL = "http://localhost:8080/"
@export var WS_URL = "ws://localhost:8080/ws"

var room: Room
var hosting: bool = false
var last_tick: int

var action_bus: GameActionWebBus = GameActionWebBus.new()
var image_bus: ImageWebBus = ImageWebBus.new()
var action_controller: GameActionController = GameActionController.new()
var latest_image: Image

func _ready() -> void:
	end_ticks()
	MyWebSocketClient.action_recieved.connect(action_controller.handle)

func throw_web_alert() -> void:
	OS.alert("Web err!")

func name_is_valid(n: String) -> bool:
	return n != ""

func id_is_valid(i: String) -> bool:
	return i != ""

func create_room(n: String) -> void:
	if name_is_valid(n):
		var err = MyHTTPClient.create_room()
		if err != OK:
			OS.alert("Couldn't connect to the server!", "Network Error")
		else:
			await MyHTTPClient.room_created
			room.my_name = n
			web_connect()
	else:
		throw_web_alert()

func join_room(n: String, i: String) -> void:
	if name_is_valid(n) and id_is_valid(i):
		room = Room.new()
		room.id = i
		room.my_name = n
		web_connect()
	else:
		throw_web_alert()

func web_connect() -> void:
	assert(room.id != "" and room.my_name != "")
	MyWebSocketClient.connect_to_url(WS_URL)
	await MyWebSocketClient.connected_to_server
	var connected_action = GameAction.new(
		room.my_name,
		GameAction.ActionType.CONNECTED,
		[room.id]
	)
	MyHTTPClient.send_user_connected(connected_action.to_string())
	connected.emit()
	start_ticks()

func start_ticks() -> void:
	last_tick = Time.get_ticks_msec()
	set_process(true)

func end_ticks() -> void:
	set_process(false)

func _process(_delta: float) -> void:
	if Time.get_ticks_msec() - last_tick >= 5:
		last_tick = Time.get_ticks_msec()
		if not action_bus.empty():
			send_action_bus()
		if hosting and not image_bus.empty():
			send_image_bus()

func send_action_bus() -> void:
	MyWebSocketClient.send(action_bus.to_string())
	action_bus.clear()

func send_image_bus() -> void:
	for packet in image_bus.packetize_image():
		MyWebSocketClient.send(packet.to_string())
	image_bus.clear()
