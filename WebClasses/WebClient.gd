extends Node

signal connected

const HTTP_URL = "http://localhost:8080/" #в конфиг
const WS_URL = "ws://localhost:8080/ws" #в конфиг

const DEBUG_CODE = "XDEBUG"

var hosting: bool = false
var last_tick: int
var debug_code

func _ready() -> void:
	end_ticks()
	MyWebSocketClient.action_recieved.connect(GameActionController.handle)

func throw_web_alert() -> void:
	OS.alert("Web err!")

func name_is_valid(n: String) -> bool:
	return n != ""

func id_is_valid(i: String) -> bool:
	return i != ""

func create_room(n: String) -> void:
	if name_is_valid(n):
		var err = MyWebHTTPClient.create_room()
		if err != OK:
			OS.alert("Couldn't connect to the server!", "Network Error")
		else:
			await MyWebHTTPClient.room_created
			Room.my_name = n
			web_connect()
	else:
		throw_web_alert()

func join_room(n: String, i: String) -> void:
	if name_is_valid(n) and id_is_valid(i):
		Room.id = i
		Room.my_name = n
		web_connect()
	else:
		throw_web_alert()

func web_connect() -> void:
	assert(Room.id != "" and Room.my_name != "")
	MyWebSocketClient.connect_to_url(WS_URL)
	await MyWebSocketClient.connected_to_server
	var connected_action = GameAction.new(
		Room.my_name,
		GameAction.ActionType.CONNECTED,
		[Room.id]
	)
	MyWebHTTPClient.send_user_connected(str(WebRequestSerializer.to_dict(connected_action)))
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
		if not WebActionBus.empty():
			MyWebSocketClient.send_action_bus()
			WebActionBus.clear()
		if hosting and not WebImageBus.empty():
			MyWebSocketClient.send_image_bus()
			WebImageBus.clear()
